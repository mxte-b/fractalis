using Spectre.Console;

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
            var top = Console.CursorTop;
            var prompt = new TextPrompt<T>(title);

            if (defaultValue is not null)
                prompt.DefaultValue(defaultValue).DefaultValueStyle(Theme.Muted);

            T result = AnsiConsole.Prompt(prompt);

            ClearLines(top, Console.CursorTop);

            return result;
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
            var top = Console.CursorTop;

            if (hint is not null) AnsiConsole.MarkupLine(hint + "\n");

            var prompt = new TextPrompt<T>(title).Validate(validator);

            if (defaultValue is not null)
                prompt.DefaultValue(defaultValue).DefaultValueStyle(Theme.Muted);

            T result = AnsiConsole.Prompt(prompt);

            ClearLines(top, Console.CursorTop);

            return result;
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

            bool result = AnsiConsole.Prompt(prompt);

            ClearLines(top, Console.CursorTop);

            return result;
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

        /// <summary>
        /// Clears the console lines in a specified range.
        /// </summary>
        /// <param name="from">The first line to clear.</param>
        /// <param name="to">The last line to clear.</param>
        public static void ClearLines(int from, int to)
        {
            for (int i = from; i <= to; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.Write(new string(' ', Console.WindowWidth));
            }

            Console.SetCursorPosition(0, from);
        }
    }
}
