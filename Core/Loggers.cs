using BepInEx.Logging;
using MoreEyes.Utility;
using static MoreEyes.Utility.Enums;

namespace MoreEyes.Core;
internal class Loggers
{ 
    private static bool ShouldLog(LogLevel level)
    {
        var modLogLevel = ModConfig.ClientLogLevel.Value;
        if (level > LogLevel.Warning && modLogLevel == ModLogLevel.WarningsOnly)
            return false;

        if ((level == LogLevel.Debug || level == LogLevel.Info) && modLogLevel != ModLogLevel.Debug)
            return false;

        return true;
    }

    private static void Log(LogLevel logLevel, object data)
    {
        if(!ShouldLog(logLevel)) 
            return;

        Plugin.Logger?.Log(logLevel, data);
    }

    internal static void Info(object data)
    {
        Log(LogLevel.Info, data);
    }

    internal static void Debug(object data)
    {
        Log(LogLevel.Debug, data);
    }

    internal static void Message(object data)
    {
        Log(LogLevel.Message, data);
    }
    internal static void Warning(object data)
    {
        Log(LogLevel.Warning, data);
    }
    internal static void Error(object data)
    {
        Log(LogLevel.Debug, data);
    }
    internal static void Fatal(object data)
    {
        Log(LogLevel.Debug, data);
    }
}
