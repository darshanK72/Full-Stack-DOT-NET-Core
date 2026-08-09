/*
 * PROBLEM: Log Retention Janitor
 *
 * Deletes stale .log files across a folder tree and tails the newest log for
 * quick diagnostics using enumeration and streaming reads.
 *
 * This exercise covers:
 *   ch01 — Directory.EnumerateFiles, FileInfo.LastWriteTimeUtc, File.Delete
 *   ch02 — StreamReader tail without ReadAllText
 */

using System;
using System.Collections.Generic;
using System.IO;

namespace LogOperations
{
    /*
     * Prunes old logs and tails the newest file under a root folder.
     *
     * No Console calls in this class.
     */
    class LogRetentionJanitor
    {
        private readonly string _logsRoot;

        /*
         * Stores the logs root directory path.
         */
        public LogRetentionJanitor(string logsRoot)
        {
            _logsRoot = logsRoot;
        }

        /*
         * Deletes *.log files under logsRoot older than maxAge (UTC comparison).
         *
         * Returns count of files deleted.
         */
        public int DeleteOlderThan(TimeSpan maxAge)
        {
            // TODO: EnumerateFiles AllDirectories, compare LastWriteTimeUtc, File.Delete
            throw new NotImplementedException();
        }

        /*
         * Returns full path of the newest *.log by LastWriteTimeUtc, or null.
         */
        public string? FindNewestLog()
        {
            // TODO: enumerate and track max LastWriteTimeUtc path
            throw new NotImplementedException();
        }

        /*
         * Reads last lineCount lines from the newest log file.
         *
         * Returns empty list when no logs exist.
         */
        public IReadOnlyList<string> TailNewestLog(int lineCount)
        {
            // TODO: FindNewestLog, StreamReader with using, delegate to TailReader
            throw new NotImplementedException();
        }

        /*
         * Single forward pass tail — shared algorithm for any TextReader.
         */
        public static IReadOnlyList<string> TailReader(TextReader reader, int lineCount)
        {
            // TODO: keep last lineCount lines while ReadLine != null
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: create nested logs with three files
            // TODO: backdate one file LastWriteTimeUtc > 10 days
            // TODO: DeleteOlderThan(7 days) → print count
            // TODO: append lines to newest, TailNewestLog(2)
            // TODO: delete log tree
            throw new NotImplementedException();
        }
    }
}
