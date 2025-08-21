using BepInEx.Logging;
using System;

namespace MoreEyes.Core;
internal class Loggers
{
    private static void Log(LogLevel logLevel, object data)
    {
        Plugin.Logger?.Log(logLevel, data);
    }

    internal static void Spam(object data)
    {
        Log(LogLevel.Debug, data);
    }
    internal static void Info(object data)
    {
        Log(LogLevel.Info, data);
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
