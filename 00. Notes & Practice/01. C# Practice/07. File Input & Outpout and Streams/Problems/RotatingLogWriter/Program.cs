/*
 * PROBLEM: Rotating Log Writer
 *
 * Ops appends UTF-8 log lines and support tools analyze logs via TextReader helpers
 * without loading entire files into memory.
 *
 * This exercise covers:
 *   ch02 — StreamWriter append, UTF-8 encoding, Flush
 *   ch02 — StreamReader ReadLine, TextReader polymorphism with StringReader
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ApplicationLogging
{
    /*
     * Append-only writer for timestamped log entries.
     *
     * No Console calls in this class.
     */
    class ApplicationLogWriter
    {
        private readonly string _logFilePath;

        /*
         * Stores the target log file path.
         */
        public ApplicationLogWriter(string logFilePath)
        {
            _logFilePath = logFilePath;
        }

        /*
         * Appends one line: yyyy-MM-dd HH:mm:ss LEVEL message (UTC).
         *
         * Uses UTF-8 without BOM; flushes after each entry.
         */
        public void WriteEntry(string level, string message)
        {
            // TODO: StreamWriter append true, UTF8Encoding(false), WriteLine, Flush, using
            throw new NotImplementedException();
        }
    }

    /*
     * TextReader-based log analytics helpers.
     *
     * No Console calls in this class.
     */
    static class LogAnalytics
    {
        /*
         * Counts lines where ReadLine returns non-empty strings.
         */
        public static int CountNonEmptyLines(TextReader reader)
        {
            // TODO: while ReadLine != null, increment when line.Length > 0
            throw new NotImplementedException();
        }

        /*
         * Returns up to count trailing lines from a forward-only reader.
         *
         * Single pass — may return fewer lines when file is shorter than count.
         */
        public static IReadOnlyList<string> ReadLastLines(TextReader reader, int count)
        {
            // TODO: sliding window / queue of last count lines
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: temp log path, write INFO/WARN/INFO entries
            // TODO: StreamReader → CountNonEmptyLines and ReadLastLines(2)
            // TODO: StringReader inline text → CountNonEmptyLines
            // TODO: delete temp log
            throw new NotImplementedException();
        }
    }
}
