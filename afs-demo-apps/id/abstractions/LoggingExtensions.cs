using System;

namespace afs.jwt.abstractions
{
    public static class LoggingExtensions
    {
        public static string ToLogMessage(this Exception ex)
            => ex.Message.Replace("\r\n", "<EOL>").Replace("\n", "<EOL>");
    }
}