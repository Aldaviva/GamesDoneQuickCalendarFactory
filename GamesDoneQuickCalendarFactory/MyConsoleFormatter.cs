using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace GamesDoneQuickCalendarFactory;

[ExcludeFromCodeCoverage]
public class MyConsoleFormatter(IOptions<MyConsoleFormatter.MyConsoleOptions> options): ConsoleFormatter(NAME) {

    public const  string NAME                = "myConsoleFormatter";
    private const string DEFAULT_DATE_FORMAT = "yyyy'-'MM'-'dd' 'HH':'mm':'ss.fff";
    private const string PADDING             = "                                ";
    private const string ANSI_RESET          = "\u001b[0m";

    private static readonly int MAX_PADDED_CATEGORY_LENGTH = PADDING.Length;

    private readonly MyConsoleOptions options = options.Value;

    private int maxCategoryLength;

    public override void Write<STATE>(in LogEntry<STATE> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter) {
        DateTimeOffset now       = DateTimeOffset.Now;
        string?        message   = logEntry.State?.ToString();
        Exception?     exception = logEntry.Exception;
        if (message is not null || exception is not null) {

            textWriter.Write(formatLevel(logEntry.LogLevel));
            textWriter.Write(options.columnSeparator);
            textWriter.Write(formatTime(now));
            textWriter.Write(options.columnSeparator);
            writeCategory(logEntry, textWriter);
            textWriter.Write(options.columnSeparator);

            if (message is not null) {
                textWriter.Write(message);
            }

            if (message is not null && exception is not null) {
                textWriter.Write("\n   ");
            }

            if (exception is not null) {
                textWriter.Write(exception.ToString().Replace("\n", "\n   "));
            }

            textWriter.WriteLine(ANSI_RESET);
        }
    }

    private void writeCategory<STATE>(LogEntry<STATE> logEntry, TextWriter textWriter) {
        int lastSeparatorPosition = options.includeNamespaces ? -1 : logEntry.Category.LastIndexOf('.', logEntry.Category.Length - 2);

        ReadOnlySpan<char> category = lastSeparatorPosition != -1 ? logEntry.Category.AsSpan(lastSeparatorPosition + 1) : logEntry.Category.AsSpan();

        int categoryLength = category.Length;
        maxCategoryLength = Math.Max(maxCategoryLength, categoryLength);
        textWriter.Write(category);

        if (categoryLength >= maxCategoryLength) {
            maxCategoryLength = categoryLength;
        } else {
            textWriter.Write(PADDING.AsSpan(0, Math.Max(0, Math.Min(maxCategoryLength, MAX_PADDED_CATEGORY_LENGTH) - categoryLength)));
        }
    }

    private string formatTime(DateTimeOffset now) => now.ToString(options.TimestampFormat ?? DEFAULT_DATE_FORMAT);

    private static string formatLevel(LogLevel level) => level switch {
        LogLevel.Trace       => "\u001b[0;90m t",
        LogLevel.Debug       => " d",
        LogLevel.Information => "\u001b[0;36m i",
        LogLevel.Warning     => "\u001b[30;43m W",
        LogLevel.Error       => "\u001b[97;41m E",
        LogLevel.Critical    => "\u001b[97;41m C",
        _                    => "  "
    };

    public class MyConsoleOptions: ConsoleFormatterOptions {

        public bool includeNamespaces { get; set; }
        public string columnSeparator { get; set; } = " | ";

    }

}

public static class MyConsoleFormatterExtensions {

    public static ILoggingBuilder addMyCustomFormatter(this ILoggingBuilder builder, Action<MyConsoleFormatter.MyConsoleOptions>? configure = null) {
        builder.AddConsole(options => options.FormatterName = MyConsoleFormatter.NAME);
        if (configure != null) {
            builder.AddConsoleFormatter<MyConsoleFormatter, MyConsoleFormatter.MyConsoleOptions>(configure);
        } else {
            builder.AddConsoleFormatter<MyConsoleFormatter, MyConsoleFormatter.MyConsoleOptions>();
        }
        return builder;
    }

}