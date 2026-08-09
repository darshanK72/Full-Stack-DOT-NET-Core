/*
 * =============================================================================
 * 01. FILE AND DIRECTORY OPERATIONS — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Working with the file system through static helpers (File, Directory)
 *        and instance wrappers (FileInfo, DirectoryInfo) — create, copy, move,
 *        delete, inspect metadata, and enumerate files and folders.
 *
 * WHY IT MATTERS:
 *   Nearly every app reads or writes disk: logs, uploads, config, exports.
 *   .NET groups common operations into static classes so you do not reinvent
 *   OS calls. FileInfo and DirectoryInfo add cached metadata and instance
 *   methods when you touch the same path repeatedly.
 *
 * WHAT YOU WILL LEARN:
 *   1.  File handling overview — static vs instance, paths, existence checks
 *   2.  Directory static methods — create, enumerate, delete folder trees
 *   3.  File static methods — text helpers, copy, move, delete, timestamps
 *   4.  FileInfo — Length, timestamps, attributes, instance copy/move/delete
 *   5.  DirectoryInfo — subfolders, recursive enumeration, instance delete
 *   6.  File attributes and timestamp APIs on File and FileInfo
 *   7.  using blocks and IDisposable — File.Create, Open, OpenRead, OpenWrite
 *   8.  Common pitfalls — missing paths, sharing violations, relative paths
 *
 * =============================================================================
 */

using System;
using System.IO;
using System.Text;

namespace FileAndDirectoryOperations;

/*
 * =========================================================================
 * SECTION 1: FILE HANDLING OVERVIEW
 * =========================================================================
 *
 * A path is a string pointing at a file or folder on disk. .NET abstracts
 * the OS through System.IO. Two styles:
 *
 *   Static (File, Directory)           → one-off calls; no object to hold state
 *   Instance (FileInfo, DirectoryInfo) → reuse metadata; chain operations
 *
 * Always check Exists before read/delete when the path comes from user input.
 * Path building (Combine, GetExtension, special folders) is covered in
 * ch.04 Path and Environment Classes.
 *
 * Stream-based reading/writing (StreamReader, FileStream) is covered in
 * ch.02 StreamReader and StreamWriter and ch.03 FileStream and Binary Files.
 * -------------------------------------------------------------------------
 */
public class Program
{
    /*
     * =========================================================================
     * SECTION 2: DIRECTORY STATIC METHODS
     * =========================================================================
     *
     * Directory wraps folder operations. Common members:
     *
     *   Exists(path)                         → bool
     *   CreateDirectory(path)                → creates full path chain; returns DirectoryInfo
     *   GetFiles(path)                       → string[] of file paths (non-recursive)
     *   GetDirectories(path)                 → string[] of immediate subfolders
     *   EnumerateFiles(path, pattern, option) → lazy IEnumerable<string>
     *   EnumerateDirectories(path, pattern)  → lazy IEnumerable<string>
     *   Delete(path, recursive)              → recursive:true removes contents
     *
     * GetFiles loads every path into a string[] immediately.
     * EnumerateFiles yields paths as you foreach — better for large folders.
     * -------------------------------------------------------------------------
     */
    public static void DemoDirectoryStaticMethods(string workspaceRoot)
    {
        string reportsFolder = Path.Combine(workspaceRoot, "reports", "2026");
        DirectoryInfo reportsInfo = Directory.CreateDirectory(reportsFolder); // creates full chain

        string archiveFolder = Path.Combine(workspaceRoot, "archive");
        Directory.CreateDirectory(archiveFolder);

        Console.WriteLine();
        Console.WriteLine("--- Directory.CreateDirectory ---");
        Console.WriteLine($"Reports folder: {reportsInfo.FullName}");
        Console.WriteLine($"Parent of reports: {reportsInfo.Parent?.Name}");

        File.WriteAllText(Path.Combine(reportsFolder, "q1-summary.txt"), "Q1 revenue up 12%.");
        File.WriteAllText(Path.Combine(reportsFolder, "q2-summary.txt"), "Q2 revenue up 8%.");
        File.WriteAllText(Path.Combine(archiveFolder, "readme.txt"), "Archived documents.");

        string[] reportFiles = Directory.GetFiles(reportsFolder);
        string[] topLevelDirs = Directory.GetDirectories(workspaceRoot);

        Console.WriteLine();
        Console.WriteLine($"Files in reports ({reportFiles.Length}):");
        foreach (string filePath in reportFiles)
        {
            Console.WriteLine($"  {Path.GetFileName(filePath)}");
        }

        Console.WriteLine($"Top-level folders under workspace ({topLevelDirs.Length}):");
        foreach (string dirPath in topLevelDirs)
        {
            Console.WriteLine($"  {Path.GetFileName(dirPath)}");
        }

        Console.WriteLine();
        Console.WriteLine("--- Directory.EnumerateFiles (lazy) ---");
        foreach (string filePath in Directory.EnumerateFiles(reportsFolder, "*.txt"))
        {
            Console.WriteLine($"  {Path.GetFileName(filePath)}");
        }

        Console.WriteLine();
        Console.WriteLine("--- Directory.EnumerateDirectories (lazy) ---");
        foreach (string dirPath in Directory.EnumerateDirectories(workspaceRoot))
        {
            Console.WriteLine($"  {Path.GetFileName(dirPath)}");
        }
    }

    /*
     * =========================================================================
     * SECTION 3: FILE STATIC METHODS
     * =========================================================================
     *
     * File exposes convenience methods for common tasks:
     *
     *   Exists(path)                    → bool
     *   WriteAllText / ReadAllText      → whole file as one string
     *   WriteAllLines / ReadAllLines    → whole file as string[]
     *   ReadLines(path)                 → lazy IEnumerable<string> per line
     *   AppendAllText / AppendAllLines  → add to end without overwriting
     *   Copy(source, dest, overwrite)   → duplicate file
     *   Move(source, dest)              → rename or relocate (source gone)
     *   Delete(path)                    → remove file
     *   Create(path)                    → creates empty file; returns FileStream
     *   GetCreationTime / GetLastWriteTime / GetLastAccessTime
     *
     * Move vs Copy: Copy leaves the original; Move is atomic rename on same volume.
     * ReadAllLines loads every line at once — fine for small files; use ReadLines
     * or ch.02 StreamReader for large logs.
     * -------------------------------------------------------------------------
     */
    public static string DemoFileStaticMethods(string workspaceRoot, string archiveFolder)
    {
        string draftPath = Path.Combine(workspaceRoot, "draft.txt");
        File.WriteAllText(draftPath, "Draft contract v1.", Encoding.UTF8); // optional encoding overload

        Console.WriteLine();
        Console.WriteLine($"Draft exists: {File.Exists(draftPath)}");
        Console.WriteLine($"Draft content: {File.ReadAllText(draftPath)}");
        Console.WriteLine($"Draft created (local): {File.GetCreationTime(draftPath):yyyy-MM-dd HH:mm}");
        Console.WriteLine($"Draft last write (UTC): {File.GetLastWriteTimeUtc(draftPath):yyyy-MM-dd HH:mm}");

        string draftCopyPath = Path.Combine(archiveFolder, "draft-backup.txt");
        File.Copy(draftPath, draftCopyPath, overwrite: true);
        Console.WriteLine($"Copy created: {File.Exists(draftCopyPath)}");

        string finalPath = Path.Combine(workspaceRoot, "contract-final.txt");
        File.Move(draftPath, finalPath); // source path no longer exists after Move
        Console.WriteLine($"After move — draft exists: {File.Exists(draftPath)}");
        Console.WriteLine($"Final exists: {File.Exists(finalPath)}");

        string todoPath = Path.Combine(workspaceRoot, "todo.txt");
        string[] initialTasks = { "Review contract", "Send invoice", "Archive reports" };
        File.WriteAllLines(todoPath, initialTasks);

        string[] loadedTasks = File.ReadAllLines(todoPath);
        Console.WriteLine();
        Console.WriteLine($"--- File.ReadAllLines ({loadedTasks.Length} lines) ---");
        foreach (string task in loadedTasks)
        {
            Console.WriteLine($"  {task}");
        }

        File.AppendAllText(todoPath, Environment.NewLine + "# appended note");
        File.AppendAllLines(todoPath, new[] { "Follow up with client" });

        Console.WriteLine();
        Console.WriteLine("--- File.ReadLines (lazy, one line at a time) ---");
        foreach (string line in File.ReadLines(todoPath))
        {
            Console.WriteLine($"  {line}");
        }

        return finalPath;
    }

    /*
     * =========================================================================
     * SECTION 4: FileInfo — METADATA AND INSTANCE METHODS
     * =========================================================================
     *
     * FileInfo wraps one file path. Properties refresh on access (not a live watch):
     *
     *   Name, FullName, Extension, DirectoryName
     *   Length (bytes), CreationTime, LastWriteTime, LastAccessTime
     *   Attributes (FileAttributes flags)
     *
     * Instance methods mirror File static methods: CopyTo, MoveTo, Delete, OpenRead.
     * Call Refresh() after another process changes the file on disk.
     * Use FileInfo when you iterate files and call several operations per path.
     * -------------------------------------------------------------------------
     */
    public static void DemoFileInfo(string finalPath, string archiveFolder)
    {
        FileInfo contractInfo = new FileInfo(finalPath);

        Console.WriteLine();
        Console.WriteLine("--- FileInfo metadata ---");
        Console.WriteLine($"Name: {contractInfo.Name}");
        Console.WriteLine($"Extension: {contractInfo.Extension}");
        Console.WriteLine($"Directory: {contractInfo.DirectoryName}");
        Console.WriteLine($"Length: {contractInfo.Length} bytes");
        Console.WriteLine($"Creation (UTC): {contractInfo.CreationTimeUtc:yyyy-MM-dd HH:mm}");
        Console.WriteLine($"Last write (UTC): {contractInfo.LastWriteTimeUtc:yyyy-MM-dd HH:mm}");
        Console.WriteLine($"Last access (UTC): {contractInfo.LastAccessTimeUtc:yyyy-MM-dd HH:mm}");

        string signedCopyPath = Path.Combine(archiveFolder, "contract-signed.txt");
        contractInfo.CopyTo(signedCopyPath, overwrite: true);

        FileInfo signedInfo = new FileInfo(signedCopyPath);
        Console.WriteLine($"Signed copy length: {signedInfo.Length} bytes");

        string movedPath = Path.Combine(archiveFolder, "contract-archived.txt");
        signedInfo.MoveTo(movedPath); // instance MoveTo — same idea as File.Move
        Console.WriteLine($"After MoveTo — signed at old path: {File.Exists(signedCopyPath)}");
        Console.WriteLine($"Archived at new path: {File.Exists(movedPath)}");
    }

    /*
     * =========================================================================
     * SECTION 5: DirectoryInfo — ENUMERATE AND NESTED WORKSPACE
     * =========================================================================
     *
     * DirectoryInfo represents one folder. Useful members:
     *
     *   CreateSubdirectory(name)     → new folder under this one
     *   GetFiles() / GetDirectories() → immediate children
     *   EnumerateFiles("*", SearchOption.AllDirectories) → lazy recursive scan
     *   EnumerateDirectories(pattern, SearchOption)       → lazy folder scan
     *   Delete(recursive)            → remove folder tree
     *
     * SearchOption.TopDirectoryOnly vs AllDirectories controls recursion depth.
     * -------------------------------------------------------------------------
     */
    public static void DemoDirectoryInfo(string workspaceRoot)
    {
        DirectoryInfo workspaceInfo = new DirectoryInfo(workspaceRoot);
        DirectoryInfo invoicesFolder = workspaceInfo.CreateSubdirectory("invoices");
        File.WriteAllText(Path.Combine(invoicesFolder.FullName, "inv-001.txt"), "Invoice 001");

        Console.WriteLine();
        Console.WriteLine("--- DirectoryInfo.GetFiles (immediate children) ---");
        foreach (FileInfo file in invoicesFolder.GetFiles("*.txt"))
        {
            Console.WriteLine($"  {file.Name} ({file.Length} bytes)");
        }

        Console.WriteLine();
        Console.WriteLine("--- Recursive file enumeration (DirectoryInfo) ---");
        foreach (FileInfo file in workspaceInfo.EnumerateFiles("*", SearchOption.AllDirectories))
        {
            string relative = file.FullName
                .Replace(workspaceRoot, "")
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            Console.WriteLine($"  {relative} ({file.Length} bytes)");
        }

        Console.WriteLine();
        Console.WriteLine("--- DirectoryInfo.EnumerateDirectories (recursive) ---");
        foreach (DirectoryInfo folder in workspaceInfo.EnumerateDirectories("*", SearchOption.AllDirectories))
        {
            string relative = folder.FullName
                .Replace(workspaceRoot, "")
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            Console.WriteLine($"  {relative}/");
        }
    }

    /*
     * =========================================================================
     * SECTION 6: FILE ATTRIBUTES AND TIMESTAMPS
     * =========================================================================
     *
     * FileAttributes is a flags enum describing OS-level file properties:
     *
     *   ReadOnly, Hidden, Archive, System, …
     *
     * Static helpers on File:
     *   GetAttributes(path)  → FileAttributes
     *   SetAttributes(path, attributes)
     *
     * FileInfo.Attributes reads/writes the same flags on the wrapped path.
     * Timestamps can be read/written via File.Get/SetCreationTime and FileInfo
     * properties (CreationTime, LastWriteTime, LastAccessTime).
     * -------------------------------------------------------------------------
     */
    public static void DemoFileAttributesAndTimestamps(string filePath)
    {
        FileAttributes originalAttributes = File.GetAttributes(filePath);
        Console.WriteLine();
        Console.WriteLine("--- File attributes ---");
        Console.WriteLine($"Original attributes: {originalAttributes}");

        File.SetAttributes(filePath, originalAttributes | FileAttributes.ReadOnly);
        Console.WriteLine($"After SetAttributes (ReadOnly): {File.GetAttributes(filePath)}");

        File.SetAttributes(filePath, originalAttributes); // restore for later delete demos

        FileInfo info = new FileInfo(filePath);
        DateTime originalWrite = info.LastWriteTimeUtc;
        info.LastWriteTimeUtc = originalWrite.AddMinutes(-5); // instance timestamp write
        Console.WriteLine($"Adjusted last write (UTC): {info.LastWriteTimeUtc:yyyy-MM-dd HH:mm}");
        info.LastWriteTimeUtc = originalWrite; // restore
    }

    /*
     * =========================================================================
     * SECTION 7: using BLOCKS AND IDisposable — FILE HANDLES
     * =========================================================================
     *
     * File.Create, File.Open, File.OpenRead, and File.OpenWrite return
     * FileStream — a handle that locks the file until disposed.
     *
     *   File.Create(path)              → new/truncate file; read+write stream
     *   File.Open(path, mode, access)  → general-purpose open
     *   File.OpenRead(path)            → read-only stream
     *   File.OpenWrite(path)           → write-only; creates or truncates
     *
     * Always wrap streams in using so Dispose() runs — especially on Windows,
     * where an open handle blocks File.Delete (IOException: sharing violation).
     *
     * COVERED IN DETAIL LATER → 03. FileStream and Binary Files
     * (byte-level read/write, seeking, buffering, BinaryReader/Writer)
     * -------------------------------------------------------------------------
     */
    public static void DemoFileStreamHandles(string archiveFolder)
    {
        string markerPath = Path.Combine(archiveFolder, "placeholder.dat");

        using (FileStream created = File.Create(markerPath))
        {
            byte[] header = Encoding.UTF8.GetBytes("HDR");
            created.Write(header, 0, header.Length); // write bytes through the stream
            Console.WriteLine();
            Console.WriteLine($"File.Create length after write: {created.Length} bytes");
        }

        using (FileStream readOnly = File.OpenRead(markerPath))
        {
            Console.WriteLine($"File.OpenRead CanRead={readOnly.CanRead}, CanWrite={readOnly.CanWrite}");
        }

        using (FileStream writeOnly = File.OpenWrite(Path.Combine(archiveFolder, "append-log.txt")))
        {
            byte[] line = Encoding.UTF8.GetBytes("log entry" + Environment.NewLine);
            writeOnly.Write(line, 0, line.Length); // OpenWrite truncates existing file
            Console.WriteLine($"File.OpenWrite wrote {line.Length} bytes");
        }

        using (FileStream general = File.Open(
            Path.Combine(archiveFolder, "config.bin"),
            FileMode.CreateNew,
            FileAccess.ReadWrite))
        {
            Console.WriteLine($"File.Open (CreateNew) — new empty file, length {general.Length}");
        }

        FileInfo markerInfo = new FileInfo(markerPath);
        using (Stream instanceRead = markerInfo.OpenRead()) // FileInfo.OpenRead mirrors File.OpenRead
        {
            Console.WriteLine($"FileInfo.OpenRead length: {instanceRead.Length} bytes");
        }
    }

    /*
     * =========================================================================
     * SECTION 8: COMMON PITFALLS — GUARDS AND PATH BEHAVIOR
     * =========================================================================
     *
     *  Mistake                              | Result
     *  -------------------------------------|----------------------------------
     *  Read/Delete without Exists check       | FileNotFoundException
     *  Delete file while stream still open  | IOException (sharing violation)
     *  ReadAllLines on huge files           | OutOfMemoryException — use ReadLines or ch.02
     *  Directory.Delete without recursive   | IOException if folder not empty
     *  Relative path from wrong working dir | FileNotFoundException — see ch.04 Path
     *
     * Relative paths resolve against Environment.CurrentDirectory (process CWD),
     * not necessarily the folder containing your .exe. Prefer absolute paths built
     * from AppContext.BaseDirectory or Path APIs from ch.04 when paths must be stable.
     * -------------------------------------------------------------------------
     */
    public static void DemoCommonPitfalls(string workspaceRoot)
    {
        string missingPath = Path.Combine(workspaceRoot, "does-not-exist.txt");

        Console.WriteLine();
        Console.WriteLine("--- Guard before read (avoid FileNotFoundException) ---");
        if (File.Exists(missingPath))
        {
            Console.WriteLine(File.ReadAllText(missingPath));
        }
        else
        {
            Console.WriteLine($"Skipped read — file missing: {Path.GetFileName(missingPath)}");
        }

        string relativeName = "relative-demo.txt";
        string absolutePath = Path.Combine(workspaceRoot, relativeName);
        File.WriteAllText(absolutePath, "Prefer absolute paths built from a known root.");
        Console.WriteLine();
        Console.WriteLine("--- Relative vs absolute (preview → ch.04 Path and Environment) ---");
        Console.WriteLine($"Relative segment: {relativeName}");
        Console.WriteLine($"Resolved absolute: {Path.GetFullPath(absolutePath)}");
    }

    /*
     * =========================================================================
     * SECTION 9: CLEANUP — DELETE FILES AND FOLDERS
     * =========================================================================
     *
     * Two delete styles mirror the static vs instance pattern:
     *
     *   File.Delete(path)                    → remove one file (throws if missing)
     *   Directory.Delete(path, recursive)    → remove folder; recursive:true wipes children
     *   fileInfo.Delete() / dirInfo.Delete() → same ops on the wrapped path
     *
     * Dispose all streams before delete. Guard user-supplied paths with Exists.
     * -------------------------------------------------------------------------
     */
    public static void DemoCleanup(string workspaceRoot, string archiveFolder)
    {
        string placeholderPath = Path.Combine(archiveFolder, "placeholder.dat");
        if (File.Exists(placeholderPath))
        {
            File.Delete(placeholderPath);
        }

        Console.WriteLine();
        Console.WriteLine($"Placeholder deleted: {File.Exists(placeholderPath)}");

        DirectoryInfo workspaceInfo = new DirectoryInfo(workspaceRoot);
        workspaceInfo.Delete(recursive: true); // instance delete removes entire tree
        Console.WriteLine();
        Console.WriteLine($"Workspace removed. Exists: {Directory.Exists(workspaceRoot)}");
    }

    /*
     * =========================================================================
     * SECTION 10: DEMONSTRATION — Main orchestrates the chapter demo
     * =========================================================================
     *
     * Each run builds a fresh workspace under AppContext.BaseDirectory, walks
     * through Directory/File static APIs, FileInfo/DirectoryInfo, attributes,
     * stream handles, pitfalls, then deletes the tree.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        string workspaceRoot = Path.Combine(AppContext.BaseDirectory, "workspace-demo");
        if (Directory.Exists(workspaceRoot))
        {
            Directory.Delete(workspaceRoot, recursive: true); // clean slate from prior run
        }

        Directory.CreateDirectory(workspaceRoot);
        string archiveFolder = Path.Combine(workspaceRoot, "archive");
        Directory.CreateDirectory(archiveFolder);

        Console.WriteLine($"Workspace root: {workspaceRoot}");
        Console.WriteLine($"Root exists: {Directory.Exists(workspaceRoot)}");

        DemoDirectoryStaticMethods(workspaceRoot);
        string finalPath = DemoFileStaticMethods(workspaceRoot, archiveFolder);
        DemoFileInfo(finalPath, archiveFolder);
        DemoDirectoryInfo(workspaceRoot);
        DemoFileAttributesAndTimestamps(finalPath);
        DemoFileStreamHandles(archiveFolder);
        DemoCommonPitfalls(workspaceRoot);
        DemoCleanup(workspaceRoot, archiveFolder);
    }
}

/*
 * =============================================================================
 * QUICK REFERENCE — FILE AND DIRECTORY OPERATIONS
 * =============================================================================
 *
 * --- Static helpers (one-off calls) ---
 *
 *   Directory.Exists(path)
 *   Directory.CreateDirectory(path)           → creates full chain; returns DirectoryInfo
 *   Directory.GetFiles(path)                  → string[] (immediate children only)
 *   Directory.GetDirectories(path)
 *   Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)  → lazy IEnumerable
 *   Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories)
 *   Directory.Delete(path, recursive: true)
 *
 *   File.Exists(path)
 *   File.WriteAllText(path, text) / ReadAllText(path)
 *   File.WriteAllLines(path, lines) / ReadAllLines(path)   → string[] (loads all lines)
 *   File.ReadLines(path)                                     → lazy IEnumerable<string>
 *   File.AppendAllText(path, text) / AppendAllLines(path, lines)
 *   File.Create(path) / Open(path, mode, access) / OpenRead / OpenWrite → FileStream
 *   File.GetCreationTime / GetLastWriteTime / GetLastAccessTime (+ Utc variants)
 *   File.GetAttributes(path) / SetAttributes(path, flags)
 *   File.Copy(source, dest, overwrite: true)
 *   File.Move(source, dest)                   → source gone; rename or relocate
 *   File.Delete(path)
 *
 * --- Instance wrappers (reuse same path) ---
 *
 *   FileInfo fi = new FileInfo(path);
 *   fi.Name, Extension, Length, CreationTime, LastWriteTimeUtc, Attributes
 *   fi.CopyTo(dest, overwrite), MoveTo(dest), Delete(), OpenRead()
 *   fi.Refresh() after external changes
 *
 *   DirectoryInfo di = new DirectoryInfo(path);
 *   di.CreateSubdirectory(name), GetFiles(), GetDirectories()
 *   di.EnumerateFiles("*", SearchOption.AllDirectories)
 *   di.EnumerateDirectories("*", SearchOption.AllDirectories)
 *   di.Delete(recursive: true)
 *
 * --- Static vs instance ---
 *
 *   Style          When to use
 *   -------------  ----------------------------------------------------------
 *   File / Directory   Single operation; path string is enough
 *   FileInfo / DirectoryInfo  Multiple ops on same path; cached metadata
 *
 * --- Common mistakes ---
 *
 *  Mistake                              | Result
 *  -------------------------------------|----------------------------------
 *  Delete file while stream still open  | IOException on Windows
 *  ReadAllLines on huge files           | OutOfMemoryException — use ReadLines or ch.02
 *  Directory.Delete without recursive   | IOException if folder not empty
 *  Relative path from wrong CWD         | FileNotFoundException — see ch.04 Path
 *  Assuming FileInfo metadata is live   | Refresh() or recreate after external change
 *
 * --- Related chapters ---
 *
 *   02. StreamReader and StreamWriter   line/chunk text I/O, encoding
 *   03. FileStream and Binary Files     byte-level read/write
 *   04. Path and Environment Classes    Combine, GetExtension, special folders
 *   05. Working with CSV and Text Files structured text parsing
 *
 * =============================================================================
 */
