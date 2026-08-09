/*
 * PROBLEM: Document Archive Pipeline
 *
 * A legal archive classifies document batches by kind, slices file extensions
 * with ranges, writes summaries with using declarations, and caches folder
 * paths using null-coalescing assignment.
 *
 * This exercise covers:
 *   ch06 — nullable reference types on optional Notes
 *   ch06 — switch expressions for classification
 *   ch06 — indices and ranges on file names
 *   ch06 — using declarations for StreamWriter
 *   ch06 — ??= lazy initialization
 */

using System;
using System.Collections.Generic;
using System.IO;

namespace DocumentArchive
{
    enum DocumentKind
    {
        Text,
        Markup,
        Binary
    }

    /*
     * Immutable page span for archive indexing (readonly struct specimen).
     */
    readonly struct PageSpan
    {
        public int Start { get; }
        public int Length { get; }

        public PageSpan(int start, int length)
        {
            Start = start;
            Length = length;
        }

        public int End => Start + Length;
    }

    /*
     * Document row with non-nullable Id/FileName and optional Notes footnote.
     */
    class DocumentMetadata
    {
        public string Id { get; set; } = string.Empty;
        public DocumentKind Kind { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    /*
     * Pure classification helpers. No Console I/O in this class.
     */
    class ArchiveClassifier
    {
        /*
         * Maps document kind to storage folder via switch expression.
         */
        public string GetStorageFolder(DocumentMetadata doc)
        {
            // TODO: switch expression on doc.Kind
            throw new NotImplementedException();
        }

        /*
         * Returns substring after last dot using range syntax.
         *
         * Returns empty string when no dot in fileName.
         */
        public string GetExtension(string fileName)
        {
            // TODO: LastIndexOf and range slice
            throw new NotImplementedException();
        }

        /*
         * Formats "Id | Kind" plus optional " | Notes" when Notes has content.
         */
        public string BuildSummaryLine(DocumentMetadata doc)
        {
            // TODO: null/whitespace-safe Notes append
            throw new NotImplementedException();
        }
    }

    /*
     * Orchestrates cached folder resolution and file output.
     * No Console I/O except via WriteSummaries file write.
     */
    class ArchivePipeline
    {
        private readonly ArchiveClassifier _classifier = new ArchiveClassifier();
        private Dictionary<string, string>? _folderCache;

        /*
         * Returns cached folder path for doc.Id; populates cache on first call.
         *
         * Uses ??= to initialize _folderCache.
         */
        public string ResolveFolderCached(DocumentMetadata doc)
        {
            // TODO: ??= new dictionary; cache classifier result by Id
            throw new NotImplementedException();
        }

        /*
         * Writes one summary line per document using a using declaration.
         *
         * StreamWriter disposed automatically at end of method scope.
         */
        public void WriteSummaries(IReadOnlyList<DocumentMetadata> batch, string outputPath)
        {
            // TODO: using StreamWriter writer = ...; loop BuildSummaryLine
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: create 3 DocumentMetadata items (mixed kinds, one null Notes)
            // TODO: print folder and extension for each
            // TODO: WriteSummaries to temp path; read and print lines
            // TODO: ResolveFolderCached twice; show cache behavior
            throw new NotImplementedException();
        }
    }
}
