using log4net.Appender;
using log4net.Core;

namespace CrealityKE2MQTT;

internal class ColorConsoleAppender : ConsoleAppender
{
    protected override void Append(LoggingEvent loggingEvent)
    {
        var color = Console.ForegroundColor;

        try
        {
            if (loggingEvent.Level >= Level.Error)
                Console.ForegroundColor = ConsoleColor.Red;
            else if (loggingEvent.Level >= Level.Warn)
                Console.ForegroundColor = ConsoleColor.Yellow;
            else if (loggingEvent.Level >= Level.Info)
                Console.ForegroundColor = ConsoleColor.White;

            base.Append(loggingEvent);
        }
        finally
        {
            Console.ForegroundColor = color;
        }
    }
}
