using fractalis.Core.Numbers;
using Spectre.Console;
using System.Reflection;
using System.Text;

namespace fractalis.Core.Miscellaneous
{
    /// <summary>
    /// Provides a set of reusable console prompt helpers built on top of Spectre.Console.
    /// </summary>
    /// <remarks>
    /// This class standardizes user input and console UI patterns such as selections,
    /// text input, validation, confirmations, and formatted status messages.
    /// It is designed to keep interactive CLI flows consistent across the application.
    /// </remarks>
    public static class Prompts
    {
        /// <summary>
        /// A custom console writer that keeps track of the number of lines written to the console.
        /// This class was created because Spectre.NET doesn't have consistent clearing behaviour.
        /// </summary>
        private sealed class LineCountingWriter(TextWriter inner) : TextWriter
        {
            public int Lines { get; private set; }
            public override Encoding Encoding => inner.Encoding;

            public override void Write(char value)
            {
                if (value == '\n') Lines++;
                inner.Write(value);
            }

            public override void Write(string? value)
            {
                if (value is not null)
                    foreach (char c in value)
                        if (c == '\n') Lines++;
                inner.Write(value);
            }

            public override void WriteLine(string? value) { Lines++; inner.WriteLine(value); }
            public override void WriteLine() { Lines++; inner.WriteLine(); }
            public override void Flush() => inner.Flush();
            protected override void Dispose(bool disposing) { } // don't close the underlying writer
        }

        private sealed class LineCountingOutput(LineCountingWriter writer) : IAnsiConsoleOutput
        {
            public TextWriter Writer => writer;
            public bool IsTerminal => true;

            public int Width => Console.WindowWidth;

            public int Height => Console.WindowHeight;

            public void SetEncoding(Encoding encoding) => Console.OutputEncoding = encoding;
        }

        /// <summary>
        /// A method that displays a prompt to the console and clears it after a valid input,
        /// adhering to console scrolling.
        /// </summary>
        /// <typeparam name="T">The return type of the prompt</typeparam>
        /// <param name="action">The console action.</param>
        /// <returns>The return value of the prompt.</returns>
        private static T PromptAndClear<T>(Func<IAnsiConsole, T> action)
        {
            var writer = new LineCountingWriter(Console.Out);
            var console = AnsiConsole.Create(new AnsiConsoleSettings { Out = new LineCountingOutput(writer) });

            T result = action(console);

            if (writer.Lines > 0) Console.Write($"\u001b[{writer.Lines}A");
            Console.Write("\u001b[J"); // erase cursor to end of screen

            return result;
        }

        private enum FileBrowserActions
        {
            Up,
            SelectFolder
        }

        private static readonly Dictionary<string, FileBrowserActions> _browserActions = new() {
            { $"[{ThemeColor.Accent}].. (up)[/]", FileBrowserActions.Up },
            { $"[{ThemeColor.Accent}][[SELECT FOLDER]][/]", FileBrowserActions.SelectFolder }
        };

        /// <summary>
        /// Displays a selection prompt and returns the user's chosen value.
        /// </summary>
        /// <typeparam name="T">The type of the selectable items.</typeparam>
        /// <param name="title">The title displayed above the selection prompt (optional).</param>
        /// <param name="choices">The collection of available options.</param>
        /// <param name="converter">
        /// Optional function used to convert each item into a display string.
        /// </param>
        /// <param name="searchable">
        /// If true, enables search functionality within the selection list.
        /// </param>
        /// <returns>The value selected by the user.</returns>
        public static T Selection<T>(string? title, IEnumerable<T> choices, Func<T, string>? converter = null, bool searchable = false)
            where T : notnull
        {
            var prompt = new SelectionPrompt<T>()
                .HighlightStyle(Theme.Selection)
                .AddChoices(choices);

            if (title is not null) prompt.Title = title;
            if (converter is not null) prompt.UseConverter(converter);
            if (searchable) prompt.EnableSearch();

            return AnsiConsole.Prompt(prompt);
        }

        /// <summary>
        /// Displays a text input prompt and returns the entered value.
        /// </summary>
        /// <typeparam name="T">The type to parse the input into.</typeparam>
        /// <param name="title">The prompt message shown to the user.</param>
        /// <param name="defaultValue">
        /// Optional default value pre-filled in the prompt.
        /// </param>
        /// <returns>The parsed user input.</returns>
        public static T Text<T>(string title, T? defaultValue = default) where T : notnull
        {
            var prompt = new TextPrompt<T>(title);

            if (defaultValue is not null)
                prompt.DefaultValue(defaultValue).DefaultValueStyle(Theme.Muted);

            return PromptAndClear(c => c.Prompt(prompt));
        }

        /// <summary>
        /// Displays a validated text input prompt and returns the entered value.
        /// </summary>
        /// <typeparam name="T">The type to parse the input into.</typeparam>
        /// <param name="title">The prompt message shown to the user.</param>
        /// <param name="validator">
        /// Function used to validate the input and return a validation result.
        /// </param>
        /// <param name="defaultValue">
        /// Optional default value pre-filled in the prompt.
        /// </param>
        /// <returns>The validated user input.</returns>
        public static T TextValidated<T>(
            string title,
            Func<T, ValidationResult> validator,
            T? defaultValue = default,
            string? hint = null
        ) where T : notnull
        {
            return PromptAndClear(c =>
            {
                if (hint is not null) c.MarkupLine(hint + "\n");

                var prompt = new TextPrompt<T>(title).Validate(validator);

                if (defaultValue is not null)
                    prompt.DefaultValue(defaultValue).DefaultValueStyle(Theme.Muted);

                return c.Prompt(prompt);
            });
        }

        /// <summary>
        /// Displays a confirmation prompt and returns the user's decision.
        /// </summary>
        /// <param name="message">The confirmation message shown to the user.</param>
        /// <param name="defaultValue">
        /// The default selected value if the user provides no input.
        /// </param>
        /// <returns><see langword="true"/> if confirmed; otherwise <see langword="false"/>.</returns>
        public static bool Confirm(string message, bool defaultValue = false)
        {
            var top = Console.CursorTop;

            var prompt = new TextPrompt<bool>(message)
                .AddChoice(true)
                .AddChoice(false)
                .DefaultValue(defaultValue)
                .WithConverter(value => value ? "y" : "n")
                .DefaultValueStyle(Theme.Muted)
                .ChoicesStyle(Theme.Accent);

            return PromptAndClear(c => c.Prompt(prompt));
        }

        /// <summary>
        /// Displays an interactive prompt and returns a selected file path or resource identifier.
        /// </summary>
        /// <param name="title">The prompt message to show to the user.</param>
        /// <param name="allowResources">
        /// Whether to allow embedded assembly resources as valid input,
        /// using the "resource:" prefix.
        /// </param>
        /// <param name="defaultValue">A default path pre-filled in the manual entry prompt.</param>
        /// <param name="hint">An optional hint displayed below the title to guide the user.</param>
        /// <param name="alsoAccept">
        /// An optional set of values to accept in addition to valid file paths.
        /// </param>
        /// <param name="allowedFormats">
        /// An optional set of file extensions (e.g. ".png", ".jpg") to restrict selection to.
        /// Files with other extensions are hidden in the browser and rejected in manual entry.
        /// </param>
        public static string FilePath(
            string title,
            bool allowResources = false,
            string? defaultValue = null,
            string? hint = null,
            IEnumerable<string>? alsoAccept = null,
            IEnumerable<string>? allowedFormats = null)
        {
            // Helper local function to validate a given path p
            ValidationResult validator(string p)
            {
                if (alsoAccept?.Contains(p) == true) return ValidationResult.Success();

                if (allowedFormats is not null &&
                    !allowedFormats.Any(e => Path.GetExtension(p).Equals(e, StringComparison.OrdinalIgnoreCase)))
                    return ValidationResult.Error($"File must be one of: {string.Join(", ", allowedFormats)}");

                bool valid = false;

                if (allowResources && p.Contains("resource:"))
                {
                    valid = Assembly.GetExecutingAssembly().GetManifestResourceNames().Contains(p.Replace("resource:", ""));
                }
                else
                {
                    valid = File.Exists(p);
                }

                return valid ? ValidationResult.Success() : ValidationResult.Error("Invalid file or resource path.");
            }

            // Helper local function to select an active drive
            string SelectDrive()
            {
                var drives = DriveInfo.GetDrives()
                            .Where(d => d.IsReady)
                            .Select(d => d.Name);

                return AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .AddChoices(drives)
                        .Title($"Select [{ThemeColor.Accent}]drive[/]:")
                        .HighlightStyle(Theme.Selection));
            }

            var top = Console.CursorTop;

            if (hint is not null) AnsiConsole.MarkupLine(hint + "\n");
            AnsiConsole.MarkupLine(title);

            var mode = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .HighlightStyle(Theme.Selection)
                .AddChoices(["Enter path manually", "Browse files"]));

            string path;
            switch (mode)
            {
                case "Enter path manually":
                    ClearFrom(top);
                    path = TextValidated(title, validator, defaultValue, hint);
                    break;

                case "Browse files":
                    var current = SelectDrive();
                    var directoryIdentifier = $"[{ThemeColor.Muted}][[DIR]][/]";

                    // Navigation loop
                    while (true)
                    {
                        // If the current directory is empty, prompt for a drive
                        current ??= SelectDrive();

                        var dirs = Directory.GetDirectories(current);
                        var files = Directory.GetFiles(current);

                        var dirMap = dirs.ToDictionary(
                            d => $"{directoryIdentifier} {Path.GetFileName(d)}",
                            d => d);

                        // Select a file, or directory, or navigation action
                        var choice = Selection(
                            $"Browsing: {current}, ",
                            dirMap.Keys
                                .Concat(
                                    files
                                        .Select(f => Path.GetFileName(f))
                                        .Where(f => allowedFormats?.Any(e => Path.GetExtension(f).Equals(e, StringComparison.OrdinalIgnoreCase)) ?? true))
                                .Prepend(_browserActions.First(a => a.Value == FileBrowserActions.Up).Key),
                            null,
                            true);

                        // Handle choice
                        if (_browserActions.TryGetValue(choice, out var action))
                        {
                            current = Directory.GetParent(current)?.FullName ?? null;
                        }
                        else if (dirMap.TryGetValue(choice, out var selectedDir))
                        {
                            current = selectedDir;
                        }
                        else
                        {
                            var p = Path.Combine(current, choice);

                            if (File.Exists(p))
                            {
                                path = p;
                                break;
                            }

                            current = p;
                        }
                    }

                    // Clear excess lines
                    ClearFrom(top);

                    break;

                default: throw new ArgumentException($"Unknown selection value: '{mode}'");
            }
            return path;
        }

        /// <summary>
        /// Displays an interactive prompt and returns a selected save path.
        /// </summary>
        /// <param name="title">The prompt message to show to the user.</param>
        /// <param name="defaultValue">A default path pre-filled in the manual entry prompt.</param>
        /// <param name="allowedFormats">
        /// An optional set of file extensions (e.g. ".png", ".mp4") to restrict selection to.
        /// Files with other extensions are hidden in the browser and rejected in manual entry.
        /// </param>
        public static string SavePath(string title, string? defaultValue = null, IEnumerable<string>? allowedFormats = null)
        {
            // Helper local function to validate a given path p
            ValidationResult validator(string p)
            {
                var resolved = Path.IsPathFullyQualified(p) ? p : Path.Combine(Directory.GetCurrentDirectory(), p);
                return Path.IsPathFullyQualified(resolved) && 
                    (allowedFormats?.Any(e => Path.GetExtension(resolved).Equals(e, StringComparison.OrdinalIgnoreCase)) ?? true)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("Invalid file path.");
            }

            // Helper local function to select an active drive
            string SelectDrive()
            {
                var drives = DriveInfo.GetDrives()
                            .Where(d => d.IsReady)
                            .Select(d => d.Name);

                return AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .AddChoices(drives)
                        .Title($"Select [{ThemeColor.Accent}]drive[/]:")
                        .HighlightStyle(Theme.Selection));
            }

            var top = Console.CursorTop;
            AnsiConsole.MarkupLine(title);

            var modePrompt = new SelectionPrompt<string>()
                .HighlightStyle(Theme.Selection)
                .AddChoices(["Enter path manually", "Browse files"]);

            string path;

            switch (AnsiConsole.Prompt(modePrompt))
            {
                case "Enter path manually":
                    ClearFrom(Console.CursorTop - 1);
                    path = TextValidated(title, validator, defaultValue);
                    break;

                case "Browse files":
                    var current = SelectDrive();

                    var directoryIdentifier = $"[{ThemeColor.Muted}][[DIR]][/]";

                    while (true)
                    {
                        current ??= SelectDrive();

                        var dirs = Directory.GetDirectories(current);
                        var files = Directory.GetFiles(current);

                        var choice = Selection(
                            $"Browsing: {current}, ",
                            _browserActions.Keys.Concat(
                                dirs
                                    .Select(d => $"{directoryIdentifier} {Path.GetFileName(d)}")
                                    .Concat(
                                        files
                                            .Select(f => Path.GetFileName(f))
                                            .Where(f => allowedFormats?.Any(e => Path.GetExtension(f).Equals(e, StringComparison.OrdinalIgnoreCase)) ?? true)
                                    )
                            ),
                            null,
                            true);

                        // If an action has been selected
                        if (_browserActions.TryGetValue(choice, out var action))
                        {
                            if (action == FileBrowserActions.Up)
                            {
                                current = Directory.GetParent(current)?.FullName ?? null;
                            }
                            else
                            {
                                ClearFrom(Console.CursorTop - 1);

                                var filename = TextValidated(
                                    $"What should the [{ThemeColor.Accent}]file name[/] be?", 
                                    p => validator(Path.Combine(current, p)), 
                                    defaultValue);

                                path = Path.Combine(current, filename);
                                break;
                            }
                        }
                        else
                        {
                            current = Path.Combine(current, choice.Replace(directoryIdentifier, "").Trim());
                        }
                    }
                    break;

                default: throw new ArgumentException($"Unknown selection value: '{modePrompt}'");
            }

            return path;
        }

        /// <summary>
        /// Displays an interactive prompt and returns a location.
        /// </summary>
        /// <param name="titleReal">The title of the prompt for the real part of the location.</param>
        /// <param name="titleImaginary">The title of the prompt for the imaginary part of the location.</param>
        /// <returns>The location as a <see cref="BigComplex"/>.</returns>
        public static BigComplex Location(string titleReal, string titleImaginary)
        {
            var real = Text<BigFloat>(
                titleReal,
                new(0)
            );

            var imaginary = Text<BigFloat>(
                titleImaginary,
                new(0)
            );

            return new(real, imaginary);
        }

        /// <summary>
        /// Writes a formatted section header to the console.
        /// </summary>
        /// <param name="title">The title of the section.</param>
        /// <param name="index">The phase or section index displayed in the header.</param>
        public static void Section(string title)
        {
            AnsiConsole.Write(
                new Rule($"[bold {ThemeColor.Primary}]{title}[/]")
                    .RuleStyle(ThemeColor.Muted)
                    .LeftJustified()
            );
        }

        /// <summary>
        /// Prints a success message to the console.
        /// </summary>
        /// <param name="message">The message to display.</param>
        public static void Success(string message)
            => AnsiConsole.MarkupLine($"[{ThemeColor.Success}]✓  {message}[/]");

        /// <summary>
        /// Prints a warning message to the console.
        /// </summary>
        /// <param name="message">The warning message to display.</param>
        public static void Warn(string message)
            => AnsiConsole.MarkupLine($"[yellow]⚠  {message}[/]");

        /// <summary>
        /// Prints a completion message indicating that an operation has finished.
        /// </summary>
        public static void Done()
        {
            AnsiConsole.MarkupLine("[DarkOliveGreen2]✓ Done[/]");
            AnsiConsole.WriteLine();
        }

        /// <summary>
        /// Prints an informational message to the console.
        /// </summary>
        /// <param name="message">The message to display.</param>
        public static void Info(string message) => AnsiConsole.MarkupLine($"[{ThemeColor.Info}]ℹ  {message}[/]");

        private const string ESC = "\u001b";
        private static void ClearFrom(int top)
        {
            Console.SetCursorPosition(0, top);
            Console.Write("\u001b[J");
        }
    }
}
