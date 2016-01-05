using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhtView
{
    public static class Logger
    {
        public static void LogInfo(string info)
        {
            Log($"Info: {info}");
        }
        public static void LogException(Exception ex)
        {
            Log($"Exception: {ex.Message}: {ex.StackTrace}");
        }

        public static void Log(string message)
        {
            Debug.WriteLine($"({DateTime.UtcNow}) {message}");
        }
    }
}
