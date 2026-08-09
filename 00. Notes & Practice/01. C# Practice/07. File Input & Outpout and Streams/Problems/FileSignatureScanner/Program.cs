/*
 * PROBLEM: File Signature Scanner
 *
 * Upload screening detects PNG/PDF files by magic bytes at the start of each file
 * while walking a directory tree without loading whole files.
 *
 * This exercise covers:
 *   ch03 — FileStream read header bytes, partial Read return value
 *   ch01 — Directory.EnumerateFiles SearchOption.AllDirectories
 */

using System;
using System.Collections.Generic;
using System.IO;

namespace UploadScreening
{
    /*
     * Detects file types by leading magic byte signatures.
     *
     * No Console calls in this class.
     */
    static class FileSignatureScanner
    {
        private static readonly byte[] PngSignature = { 0x89, 0x50, 0x4E, 0x47 };
        private static readonly byte[] PdfSignature = { 0x25, 0x50, 0x44, 0x46 };

        /*
         * Reads signature.Length bytes from file start and compares to expected bytes.
         *
         * Returns false when file is shorter than signature or bytes differ.
         */
        public static bool MatchesSignature(string filePath, byte[] signature)
        {
            // TODO: FileStream Open Read, Read exact length, compare bytes read
            throw new NotImplementedException();
        }

        /*
         * Returns "PNG", "PDF", or null when no known signature matches.
         */
        public static string? DetectType(string filePath)
        {
            // TODO: try PNG then PDF via MatchesSignature
            throw new NotImplementedException();
        }

        /*
         * Scans folder tree and returns "{fileName} => {label}" for each match.
         */
        public static IReadOnlyList<string> ScanFolder(string folderPath)
        {
            // TODO: EnumerateFiles AllDirectories, DetectType, format results
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: create scan folder with PNG bytes, PDF bytes, and text file
            // TODO: print DetectType for each
            // TODO: print ScanFolder results
            // TODO: MatchesSignature false on text file
            // TODO: delete scan folder
            throw new NotImplementedException();
        }
    }
}
