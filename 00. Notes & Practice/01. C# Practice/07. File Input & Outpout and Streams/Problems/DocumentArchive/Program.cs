/*
 * PROBLEM: Document Archive
 *
 * Legal archives signed contract files from an inbox into a dated folder tree
 * without overwriting existing copies. Auditors need file sizes and inbox listing.
 *
 * This exercise covers:
 *   ch01 — Directory.CreateDirectory, File.Copy, FileInfo/DirectoryInfo enumeration
 *   ch01 — File.Exists guards, Directory.Delete non-recursive empty folder
 */

using System;
using System.Collections.Generic;
using System.IO;

namespace LegalDocumentArchive
{
    /*
     * Archives inbox documents into archive\{yyyy}\{MM}\ folders.
     *
     * No Console calls in this class.
     */
    class DocumentArchiveService
    {
        private readonly string _inboxRoot;
        private readonly string _archiveRoot;

        /*
         * Stores inbox and archive root paths for later operations.
         */
        public DocumentArchiveService(string inboxRoot, string archiveRoot)
        {
            _inboxRoot = inboxRoot;
            _archiveRoot = archiveRoot;
        }

        /*
         * Creates the inbox directory if it does not exist.
         */
        public void EnsureInbox()
        {
            // TODO: Directory.CreateDirectory when inbox missing
            throw new NotImplementedException();
        }

        /*
         * Copies one inbox file into a UTC year/month archive folder.
         *
         * Returns false when source missing or destination file already exists.
         * Returns true on successful copy (overwrite: false).
         */
        public bool ArchiveDocument(string fileName)
        {
            // TODO: combine paths, guard source Exists, build dated folder, File.Copy overwrite false
            throw new NotImplementedException();
        }

        /*
         * Returns file names (not full paths) currently in the inbox.
         */
        public IReadOnlyList<string> ListInboxFiles()
        {
            // TODO: Directory.GetFiles + Path.GetFileName for each
            throw new NotImplementedException();
        }

        /*
         * Finds archived copy by file name anywhere under archiveRoot.
         *
         * Returns FileInfo.Length of first match, or -1 when not found.
         */
        public long GetArchivedSize(string fileName)
        {
            // TODO: DirectoryInfo.EnumerateFiles(fileName, AllDirectories), return Length or -1
            throw new NotImplementedException();
        }

        /*
         * Deletes inbox folder only when it exists and has no files or subdirectories.
         */
        public void PurgeEmptyInbox()
        {
            // TODO: check Directory.Exists, count files/dirs, Directory.Delete recursive false
            throw new NotImplementedException();
        }
    }

    /*
     * Entry point. Owns all Console I/O and temp folder lifecycle.
     */
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: create temp inbox/archive under AppContext.BaseDirectory
            // TODO: write two inbox files, archive each, retry duplicate → false
            // TODO: print ListInboxFiles and GetArchivedSize
            // TODO: delete inbox files, PurgeEmptyInbox, print inbox Exists
            // TODO: delete archive tree recursively
            throw new NotImplementedException();
        }
    }
}
