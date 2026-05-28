using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Miscellaneous
{
    /// <summary>Theme color definitions used throughout the UI.</summary>
    public static class ThemeColor
    {
        public static readonly string Title = "#FF5A5A";
        public static readonly string Primary = "#E14B4B";
        public static readonly string Accent = "#FF7A7A";

        public static readonly string Success = "DarkOliveGreen2";
        public static readonly string Info = "#3CA5FB";

        public static readonly string Text = "#F3EAEA";
        public static readonly string Muted = "#7A5A5A";

        public static readonly string SelectionForeground = "#FFFFFF";
        public static readonly string SelectionBackground = "#5C0B0B";

        public static readonly string Surface = "#0B0B0B";
    }

    /// <summary>Predefined terminal UI styles.</summary>
    public static class Theme
    {
        public static readonly Style Title      = new(Color.FromHex(ThemeColor.Title));

        public static readonly Style Primary    = new(Color.FromHex(ThemeColor.Primary));

        public static readonly Style Accent     = new(Color.FromHex(ThemeColor.Accent));

        public static readonly Style Muted      = new(Color.FromHex(ThemeColor.Muted));

        public static readonly Style Success    = new(Color.FromName("DarkOliveGreen2"));

        public static readonly Style Selection  = new(
            Color.FromHex(ThemeColor.SelectionForeground),
            Color.FromHex(ThemeColor.SelectionBackground)
        );

        public static readonly Style Phase      = new(
            foreground: Color.FromHex(ThemeColor.Primary),
            background: Color.FromHex("#1A0F0F")
        );
    }
}
