using System.Diagnostics;

namespace BlueLine;

public static class Utilities
{
    public static void Log(string msg)
    {
        Trace.WriteLine($"BlueLine - {msg}");
    }
}