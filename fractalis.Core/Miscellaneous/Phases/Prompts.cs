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
        /// <param name="title">The title displayed above the selection prompt.</param>
        /// <param name="choices">The collection of available options.</param>
        /// <param name="converter">
        /// Optional function used to convert each item into a display string.
        /// </param>
        /// <param name="searchable">
        /// If true, enables search functionality within the selection list.
        /// </param>
        /// <returns>The value selected by the user.</returns>
        public static T Selection<T>(string title, IEnumerable<T> choices, Func<T, string>? converter = null, bool searchable = false)
            where T : notnull
        {
            var prompt = new SelectionPrompt<T>()
                .Title(title)
                .HighlightStyle(Theme.Selection)
                .AddChoices(choices);

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
            var prompt = new TextPrompt<T>(title).ClearOnFinish();

            if (defaultValue is not null)
                prompt.DefaultValue(defaultValue).DefaultValueStyle(Theme.Muted);

            return AnsiConsole.Prompt(prompt);
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
            T? defaultValue = default
        ) where T : notnull
        {
            var prompt = new TextPrompt<T>(title)
                .ClearOnFinish()
                .Validate(validator);

            if (defaultValue is not null)
                prompt.DefaultValue(defaultValue).DefaultValueStyle(Theme.Muted);

            return AnsiConsole.Prompt(prompt);
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
            var prompt = new ConfirmationPrompt(message)
                .DefaultValueStyle(Theme.Muted)
                .ChoicesStyle(Theme.Accent);

            prompt.DefaultValue = defaultValue;
            return AnsiConsole.Prompt(prompt);
        }

        /// <summary>
        /// Writes a formatted section header to the console.
        /// </summary>
        /// <param name="title">The title of the section.</param>
        /// <param name="index">The phase or section index displayed in the header.</param>
        public static void Section(string title, int index)
        {
            AnsiConsole.Write(
                new Rule($"[bold {ThemeColor.Primary}]Phase {index}: {title}[/]")
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
    }
}
