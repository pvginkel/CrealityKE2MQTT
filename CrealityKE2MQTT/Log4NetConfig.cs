using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace CrealityKE2MQTT;

public static class Log4NetConfig
{
    static Log4NetConfig()
    {
        var hierarchy = (Hierarchy)LogManager.GetRepository();

        var patternLayout = new PatternLayout
        {
            ConversionPattern = "%date [%thread] %-5level %logger - %message%newline"
        };
        patternLayout.ActivateOptions();

        var consoleAppender = new ColorConsoleAppender { Layout = patternLayout };
        consoleAppender.ActivateOptions();
        hierarchy.Root.AddAppender(consoleAppender);

        var rollingFileAppender = new RollingFileAppender
        {
            AppendToFile = true,
            File = "Logs/CrealityKE2MQTT.log",
            Layout = patternLayout,
            MaxSizeRollBackups = 5,
            MaximumFileSize = "10MB",
            RollingStyle = RollingFileAppender.RollingMode.Size,
            StaticLogFileName = true
        };
        rollingFileAppender.ActivateOptions();
        hierarchy.Root.AddAppender(rollingFileAppender);

        hierarchy.Root.Level = Level.Debug;
        hierarchy.Configured = true;
    }

    public static void ConfigureLog4Net()
    {
        // Ignore. Logic is in the static constructor.
    }
}
