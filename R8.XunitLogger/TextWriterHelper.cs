using System;
using System.IO;

namespace R8.XunitLogger
{
    internal static class TextWriterHelper
    {
        private const string defaultForegroundColor = "\x1B[39m\x1B[22m";
        private const string defaultBackgroundColor = "\x1B[49m";

        public static void WriteConsole(this TextWriter textWriter, string message, ConsoleColor? background, ConsoleColor? foreground, bool colorize = true)
        {
            if (colorize)
            {
                // Order:
                //   1. background color
                //   2. foreground color
                //   3. message
                //   4. reset foreground color
                //   5. reset background color

                var backgroundColor = background.HasValue ? GetBackgroundColorEscapeCode(background.Value) : null;
                var foregroundColor = foreground.HasValue ? GetForegroundColorEscapeCode(foreground.Value) : null;

                if (backgroundColor != null)
                {
                    textWriter.Write(backgroundColor);
                }

                if (foregroundColor != null)
                {
                    textWriter.Write(foregroundColor);
                }

                textWriter.Write(message);

                if (foregroundColor != null)
                {
                    textWriter.Write(defaultForegroundColor);
                }

                if (backgroundColor != null)
                {
                    textWriter.Write(defaultBackgroundColor);
                }
            }
            else
            {
                textWriter.Write(message);
            }
        }

        private static string GetForegroundColorEscapeCode(ConsoleColor color) =>
            color switch
            {
                ConsoleColor.Black => "\x1B[30m",
                ConsoleColor.DarkRed => "\x1B[31m",
                ConsoleColor.DarkGreen => "\x1B[32m",
                ConsoleColor.DarkYellow => "\x1B[33m",
                ConsoleColor.DarkBlue => "\x1B[34m",
                ConsoleColor.DarkMagenta => "\x1B[35m",
                ConsoleColor.DarkCyan => "\x1B[36m",
                ConsoleColor.Gray => "\x1B[37m",
                ConsoleColor.Red => "\x1B[1m\x1B[31m",
                ConsoleColor.Green => "\x1B[1m\x1B[32m",
                ConsoleColor.Yellow => "\x1B[1m\x1B[33m",
                ConsoleColor.Blue => "\x1B[1m\x1B[34m",
                ConsoleColor.Magenta => "\x1B[1m\x1B[35m",
                ConsoleColor.Cyan => "\x1B[1m\x1B[36m",
                ConsoleColor.White => "\x1B[1m\x1B[37m",

                _ => defaultForegroundColor
            };

        private static string GetBackgroundColorEscapeCode(ConsoleColor color) =>
            color switch
            {
                ConsoleColor.Black => "\x1B[40m",
                ConsoleColor.DarkRed => "\x1B[41m",
                ConsoleColor.DarkGreen => "\x1B[42m",
                ConsoleColor.DarkYellow => "\x1B[43m",
                ConsoleColor.DarkBlue => "\x1B[44m",
                ConsoleColor.DarkMagenta => "\x1B[45m",
                ConsoleColor.DarkCyan => "\x1B[46m",
                ConsoleColor.Gray => "\x1B[47m",

                _ => defaultBackgroundColor
            };
    }
}