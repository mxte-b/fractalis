using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace fractalis.Core.Numbers
{
    /// <summary>
    /// Platform-agnostic 256-bit vector of four <see cref="double"/> values.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 32)]
    public struct Vec256d
    {
        /// <summary>x86/AVX view: one 256-bit YMM register.</summary>
        [FieldOffset(0)] internal Vector256<double> _v256;

        /// <summary>ARM64/generic low half: lanes 0–1 (bytes 0–15).</summary>
        [FieldOffset(0)] internal Vector128<double> _lo;

        /// <summary>ARM64/generic high half: lanes 2–3 (bytes 16–31).</summary>
        [FieldOffset(16)] internal Vector128<double> _hi;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vec256d FromV256(Vector256<double> v)
        {
            Vec256d r = default;
            r._v256 = v;
            return r;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vec256d FromLoHi(Vector128<double> lo, Vector128<double> hi)
        {
            Vec256d r = default;
            r._lo = lo;
            r._hi = hi;
            return r;
        }
    }

    /// <summary>
    /// Hardware-agnostic SIMD operations over <see cref="Vec256d"/> (4 × <see cref="double"/>).
    /// </summary>
    public static class SimdAgnostic
    {

        #region Constants
        /// <summary>
        /// All lanes zero.
        /// A default-initialised <see cref="Vec256d"/> is all-zero on every platform;
        /// no instruction is emitted.
        /// </summary>
        public static Vec256d Zero
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => default;
        }

        /// <summary>
        /// All bits set in every lane.
        /// </summary>
        public static Vec256d AllBitsSet
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (Avx.IsSupported)
                    return Vec256d.FromV256(Vector256<double>.AllBitsSet);

                var half = Vector128<double>.AllBitsSet;
                return Vec256d.FromLoHi(half, half);
            }
        }

        /// <summary>Whether the current hardware supports AVX or NEON.</summary>
        public static bool IsSupported => Avx.IsSupported || AdvSimd.IsSupported;
        #endregion

        #region Create methods
        /// <summary>
        /// Broadcasts <paramref name="value"/> into all four lanes.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec256d Create(double value)
        {
            if (Avx.IsSupported)
                return Vec256d.FromV256(Vector256.Create(value));

            var half = Vector128.Create(value);
            return Vec256d.FromLoHi(half, half);
        }

        /// <summary>
        /// Initialises each lane individually (lane 0 = <paramref name="e0"/>, … lane 3 = <paramref name="e3"/>).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec256d Create(double e0, double e1, double e2, double e3)
        {
            if (Avx.IsSupported)
                return Vec256d.FromV256(Vector256.Create(e0, e1, e2, e3));

            return Vec256d.FromLoHi(
                Vector128.Create(e0, e1),
                Vector128.Create(e2, e3));
        }
        #endregion

        #region Arithmetic
        /// <summary>
        /// Fused multiply-add: <c>a * b + c</c> with a single rounding step.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec256d MultiplyAdd(Vec256d a, Vec256d b, Vec256d c)
        {
            if (Fma.IsSupported)
            {
                return Vec256d.FromV256(Fma.MultiplyAdd(a._v256, b._v256, c._v256));
            }

            if (AdvSimd.Arm64.IsSupported)
            {
                return Vec256d.FromLoHi(
                    AdvSimd.Arm64.FusedMultiplyAdd(c._lo, a._lo, b._lo), 
                    AdvSimd.Arm64.FusedMultiplyAdd(c._hi, a._hi, b._hi));
            }

            // Fallback
            return Vec256d.FromLoHi(a._lo * b._lo + c._lo, a._hi * b._hi + c._hi);
        }

        /// <summary>
        /// Fused negate-multiply-add: <c>-(a * b) + c</c>  i.e. <c>c − a·b</c>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec256d MultiplyAddNegated(Vec256d a, Vec256d b, Vec256d c)
        {
            if (Fma.IsSupported)
            {
                return Vec256d.FromV256(Fma.MultiplyAddNegated(a._v256, b._v256, c._v256));
            }

            if (AdvSimd.Arm64.IsSupported)
            {
                return Vec256d.FromLoHi(
                    AdvSimd.Arm64.FusedMultiplySubtract(c._lo, a._lo, b._lo),
                    AdvSimd.Arm64.FusedMultiplySubtract(c._hi, a._hi, b._hi));
            }

            return Vec256d.FromLoHi(c._lo - a._lo * b._lo, c._hi - a._hi * b._hi);
        }

        /// <summary>
        /// Element-wise addition.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec256d Add(Vec256d a, Vec256d b)
        {
            if (Avx.IsSupported)
            {
                return Vec256d.FromV256(Avx.Add(a._v256, b._v256));
            }

            return Vec256d.FromV256(Vector256.Add(a._v256, b._v256));
        }

        /// <summary>
        /// Element-wise subtraction.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec256d Subtract(Vec256d a, Vec256d b)
        {
            if (Avx.IsSupported)
                return Vec256d.FromV256(Avx.Subtract(a._v256, b._v256));

            return Vec256d.FromV256(Vector256.Subtract(a._v256, b._v256));
        }

        /// <summary>
        /// Element-wise multiplication.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec256d Multiply(Vec256d a, Vec256d b)
        {
            return Vec256d.FromV256(a._v256 * b._v256);
        }

        /// <summary>
        /// Scalar broadcast-multiply: every lane of <paramref name="v"/> multiplied by <paramref name="scalar"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec256d Multiply(double scalar, Vec256d v)
        {
            return Vec256d.FromV256(Vector256.Create(scalar) * v._v256);
        }
        #endregion

        #region Bitwise operations
        /// <summary>
        /// Bitwise AND of every lane.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec256d And(Vec256d a, Vec256d b)
        {
            if (Avx.IsSupported)
                return Vec256d.FromV256(Avx.And(a._v256, b._v256));

            return Vec256d.FromV256(Vector256.BitwiseAnd(a._v256, b._v256));
        }

        /// <summary>
        /// Bitwise AND-NOT: <c>~a &amp; b</c>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec256d AndNot(Vec256d a, Vec256d b)
        {
            if (Avx.IsSupported)
                return Vec256d.FromV256(Avx.AndNot(a._v256, b._v256));

            return Vec256d.FromV256(Vector256.AndNot(b._v256, a._v256));
        }

        /// <summary>
        /// Bitwise OR of every lane.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec256d Or(Vec256d a, Vec256d b)
        {
            if (Avx.IsSupported)
                return Vec256d.FromV256(Avx.Or(a._v256, b._v256));

            return Vec256d.FromV256(Vector256.BitwiseOr(a._v256, b._v256));
        }

        /// <summary>
        /// Bitwise XOR of every lane.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec256d Xor(Vec256d a, Vec256d b)
        {
            if (Avx.IsSupported)
                return Vec256d.FromV256(Avx.Xor(a._v256, b._v256));

            return Vec256d.FromV256(Vector256.Xor(a._v256, b._v256));
        }

        /// <summary>
        /// Per-lane ordered less-than comparison.
        /// Returns all-1 bits in lanes where <c>a &lt; b</c>, all-0 bits otherwise.
        /// NaN inputs produce all-0.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec256d CompareLessThan(Vec256d a, Vec256d b)
        {
            if (Avx.IsSupported)
            {
                return Vec256d.FromV256(Avx.Compare(a._v256, b._v256, FloatComparisonMode.OrderedLessThanNonSignaling));
            }

            if (AdvSimd.Arm64.IsSupported)
            {
                return Vec256d.FromLoHi(
                    AdvSimd.Arm64.CompareLessThan(a._lo, b._lo),
                    AdvSimd.Arm64.CompareLessThan(a._hi, b._hi));
            }

            // Fallback
            return Vec256d.FromLoHi(
                Vector128.LessThan(a._lo, b._lo),
                Vector128.LessThan(a._hi, b._hi));
        }

        /// <summary>
        /// Per-lane conditional select.
        /// Returns <paramref name="b"/> in lanes where <paramref name="mask"/> is all-1 bits,
        /// <paramref name="a"/> in lanes where <paramref name="mask"/> is all-0 bits.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec256d BlendVariable(Vec256d a, Vec256d b, Vec256d mask)
        {
            if (Avx.IsSupported)
            {
                return Vec256d.FromV256(Avx.BlendVariable(a._v256, b._v256, mask._v256));
            }

            if (AdvSimd.IsSupported)
            {
                return Vec256d.FromLoHi(
                    AdvSimd.BitwiseSelect(mask._lo.AsByte(), b._lo.AsByte(), a._lo.AsByte()).AsDouble(),
                    AdvSimd.BitwiseSelect(mask._hi.AsByte(), b._hi.AsByte(), a._hi.AsByte()).AsDouble());
            }

            // Fallback
            return Vec256d.FromLoHi(
                Vector128.ConditionalSelect(mask._lo, b._lo, a._lo),
                Vector128.ConditionalSelect(mask._hi, b._hi, a._hi));
        }

        /// <summary>
        /// Returns <see langword="true"/> when <c>(a &amp; b)</c> is entirely zero — i.e. no bit is
        /// set in the intersection of the two masks.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TestZ(Vec256d a, Vec256d b)
        {
            if (Avx.IsSupported)
            {
                return Avx.TestZ(a._v256, b._v256);
            }

            var lo = a._lo.AsUInt64() & b._lo.AsUInt64();
            var hi = a._hi.AsUInt64() & b._hi.AsUInt64();
            var any = lo | hi;
            return (any.GetElement(0) | any.GetElement(1)) == 0UL;
        }
        #endregion

        /// <summary>
        /// Stores all four lanes to an unaligned memory location.
        /// <paramref name="ptr"/> must point to at least 32 bytes of valid writable memory.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Store(double* ptr, Vec256d v)
        {
            if (Avx.IsSupported)
            {
                Avx.Store(ptr, v._v256);
                return;
            }

            // ptr + 2 advances by 2 × sizeof(double) = 16 bytes, covering the high half.
            // The ARM64 JIT commonly fuses the two adjacent stores into a single STP.
            if (AdvSimd.IsSupported)
            {
                AdvSimd.Store(ptr, v._lo);
                AdvSimd.Store(ptr + 2, v._hi);
                return;
            }

            // Fallback
            Unsafe.WriteUnaligned(ptr, v._lo);
            Unsafe.WriteUnaligned(ptr + 2, v._hi);
        }
    }
}