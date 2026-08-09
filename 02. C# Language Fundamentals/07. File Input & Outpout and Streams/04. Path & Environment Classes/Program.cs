/*
 * =============================================================================
 * 04. PATH AND ENVIRONMENT CLASSES — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Building, parsing, and resolving file paths with Path, and locating
 *        system folders and runtime context with Environment — without hard-coding
 *        OS-specific strings.
 *
 * WHY IT MATTERS:
 *   Hard-coding "C:\\Users\\..." or stitching paths with "\\" breaks on Linux
 *   and macOS. Path.Combine picks the correct separator for the running OS.
 *   Environment.GetFolderPath exposes Desktop, Documents, ApplicationData, and
 *   other well-known locations so apps store user data in the right place.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Path.Combine — join segments without manual slash guessing
 *   2.  Parse paths — GetFileName, GetDirectoryName, GetExtension, ChangeExtension
 *   3.  Resolve paths — GetFullPath, IsPathRooted, relative vs absolute pitfalls
 *   4.  Separator characters — DirectorySeparatorChar and AltDirectorySeparatorChar
 *   5.  Temp paths — GetTempPath and GetTempFileName
 *   6.  Environment.GetFolderPath — SpecialFolder enum (Desktop, MyDocuments, …)
 *   7.  Runtime context — CurrentDirectory, MachineName, UserName, BaseDirectory
 *   8.  Cross-platform rules — Windows vs Unix paths, case sensitivity, drive roots
 *
 * =============================================================================
 */

using System;
using System.IO;

namespace PathAndEnvironmentClasses;

public class Program
{
    /*
     * =========================================================================
     * SECTION 1: Path.Combine — BUILD PATHS SAFELY
     * =========================================================================
     *
     * Path.Combine joins path segments with the correct directory separator
     * for the running OS. Extra separators in segments are normalized where
     * possible — but you should still pass clean segment names.
     *
     *   Path.Combine("reports", "2026", "summary.txt")
     *     → reports\2026\summary.txt  (Windows)
     *     → reports/2026/summary.txt  (Linux/macOS)
     *
     * Prefer Combine over string concatenation with "\\" or "/".
     *
     * Pitfall: Combine does NOT guarantee the path exists — it only builds
     * a string. Creating folders/files is ch.01 File and Directory Operations.
     * -------------------------------------------------------------------------
     */
    private static void DemonstratePathCombine()
    {
        string baseFolder = "exports";
        string yearFolder = "2026";
        string fileName = "sales.csv";
        string relativePath = Path.Combine(baseFolder, yearFolder, fileName); // OS-correct join

        Console.WriteLine("--- Path.Combine ---");
        Console.WriteLine($"Relative path: {relativePath}");

        // A segment may already contain a separator — Combine still works
        string segmentWithSlash = "logs/app.log";
        string nestedPath = Path.Combine("app-data", segmentWithSlash);
        Console.WriteLine($"Combine with slash inside segment: {nestedPath}");

        // Combine with a rooted segment resets to that root (Windows drive or Unix /)
        string rootedTail = Path.Combine("ignored", "prefix", "/etc", "hosts");
        Console.WriteLine($"Combine after rooted segment: {rootedTail}");
    }

    /*
     * =========================================================================
     * SECTION 2: PARSE PATHS — FILENAME, DIRECTORY, EXTENSION
     * =========================================================================
     *
     * These methods inspect a path string without touching the file system:
     *
     *   GetFileName(path)                 → last segment (file or folder name)
     *   GetFileNameWithoutExtension(path) → name minus extension
     *   GetExtension(path)                → ".pdf" including the dot (or empty)
     *   GetDirectoryName(path)            → parent folder path (or null)
     *   ChangeExtension(path, ".bak")     → swap extension without manual slicing
     *
     * --- 2a. Break apart a sample path ---
     *
     * On Windows, "C:" in Combine is a drive root. On Unix, "C:" is just a
     * relative folder name — use SpecialFolder (Section 6) instead of drives.
     * -------------------------------------------------------------------------
     */
    private static string DemonstratePathParsing()
    {
        string samplePath = Path.Combine("C:", "Projects", "Invoice", "inv-2026-0042.pdf");

        Console.WriteLine();
        Console.WriteLine("--- Path parsing ---");
        Console.WriteLine($"Sample path: {samplePath}");
        Console.WriteLine($"GetFileName: {Path.GetFileName(samplePath)}");
        Console.WriteLine($"GetFileNameWithoutExtension: {Path.GetFileNameWithoutExtension(samplePath)}");
        Console.WriteLine($"GetExtension: {Path.GetExtension(samplePath)}");
        Console.WriteLine($"GetDirectoryName: {Path.GetDirectoryName(samplePath)}");

        /*
         * --- 2b. ChangeExtension — safer than path[..^4] string slicing ---
         */
        string archivePath = Path.ChangeExtension(samplePath, ".archive.pdf");
        Console.WriteLine($"ChangeExtension: {archivePath}");

        // Empty extension removes it; null extension keeps the current one
        string noExtension = Path.ChangeExtension(samplePath, string.Empty);
        Console.WriteLine($"ChangeExtension (remove): {noExtension}");

        return samplePath;
    }

    /*
     * =========================================================================
     * SECTION 3: RESOLVE PATHS — GetFullPath, IsPathRooted, RELATIVE VS ABSOLUTE
     * =========================================================================
     *
     * Relative path — resolved from a starting directory (usually CurrentDirectory).
     * Absolute path — fully qualified from a root (drive letter or leading /).
     *
     *   Path.GetFullPath("docs\\readme.md")  → absolute path from current directory
     *   Path.IsPathRooted(path)              → true when path starts at a root
     *   Path.GetPathRoot(path)               → "C:\\" on Windows, "/" on Unix
     *
     * Pitfalls:
     *   • GetFullPath throws if the path is invalid for the OS.
     *   • Relative paths follow Environment.CurrentDirectory — not always the .exe folder.
     *   • Deployed apps often anchor relative paths to AppContext.BaseDirectory instead.
     * -------------------------------------------------------------------------
     */
    private static void DemonstratePathResolution(string relativeSample, string absoluteSample)
    {
        string relativeDoc = Path.Combine("docs", "readme.md");
        string fullDocPath = Path.GetFullPath(relativeDoc); // resolves against CurrentDirectory

        Console.WriteLine();
        Console.WriteLine("--- GetFullPath and rooted checks ---");
        Console.WriteLine($"Environment.CurrentDirectory: {Environment.CurrentDirectory}");
        Console.WriteLine($"Relative input: {relativeDoc}");
        Console.WriteLine($"GetFullPath: {fullDocPath}");
        Console.WriteLine($"IsPathRooted(relative): {Path.IsPathRooted(relativeSample)}");
        Console.WriteLine($"IsPathRooted(full): {Path.IsPathRooted(fullDocPath)}");
        Console.WriteLine($"GetPathRoot(full): {Path.GetPathRoot(fullDocPath)}");
        Console.WriteLine($"GetPathRoot(absolute sample): {Path.GetPathRoot(absoluteSample)}");
    }

    /*
     * =========================================================================
     * SECTION 4: SEPARATOR CHARACTERS — OS-SPECIFIC SLASHES
     * =========================================================================
     *
     *   DirectorySeparatorChar     → primary (\ on Windows, / on Linux/macOS)
     *   AltDirectorySeparatorChar  → alternate (often / on Windows too)
     *
     * Use Path.DirectorySeparatorChar when you must insert exactly one character
     * manually — still prefer Combine for multi-segment paths.
     *
     * Cross-platform note: never assume DirectorySeparatorChar is '\\'.
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateSeparators()
    {
        Console.WriteLine();
        Console.WriteLine("--- Separator characters ---");
        Console.WriteLine($"DirectorySeparatorChar: '{Path.DirectorySeparatorChar}'");
        Console.WriteLine($"AltDirectorySeparatorChar: '{Path.AltDirectorySeparatorChar}'");
        Console.WriteLine($"Running on Windows: {OperatingSystem.IsWindows()}");
        Console.WriteLine($"Running on Linux: {OperatingSystem.IsLinux()}");
        Console.WriteLine($"Running on macOS: {OperatingSystem.IsMacOS()}");

        // Manual char insert — rare; Combine is the default choice
        char separator = Path.DirectorySeparatorChar;
        string manualPath = "data" + separator + "cache" + separator + "index.dat";
        Console.WriteLine($"Manual separator join: {manualPath}");
    }

    /*
     * =========================================================================
     * SECTION 5: TEMP PATHS — GetTempPath AND GetTempFileName
     * =========================================================================
     *
     *   Path.GetTempPath()      → folder for short-lived files (OS-managed temp dir)
     *   Path.GetTempFileName()  → creates a unique zero-byte file and returns its path
     *
     * GetTempFileName is convenient for scratch files but leaves a file on disk until
     * you delete it. For reusable temp names without creating a file, combine GetTempPath
     * with Guid.NewGuid() or your own naming scheme.
     *
     * File delete/cleanup patterns → ch.01 File and Directory Operations.
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateTempPaths()
    {
        string tempFolder = Path.GetTempPath();
        string tempFile = Path.GetTempFileName(); // creates an empty file immediately

        Console.WriteLine();
        Console.WriteLine("--- Temp paths ---");
        Console.WriteLine($"GetTempPath: {tempFolder}");
        Console.WriteLine($"GetTempFileName (created file): {tempFile}");
        Console.WriteLine($"Temp file exists: {File.Exists(tempFile)}");

        File.Delete(tempFile); // cleanup — full File API in ch.01

        string scratchName = Path.Combine(tempFolder, $"scratch-{Guid.NewGuid():N}.txt");
        Console.WriteLine($"Custom scratch path (no auto-create): {scratchName}");
    }

    /*
     * =========================================================================
     * SECTION 6: Environment.GetFolderPath — SpecialFolder ENUM
     * =========================================================================
     *
     * Environment.GetFolderPath(SpecialFolder) returns OS-standard user and system
     * folders. Always combine results with Path.Combine — never assume drive letters
     * exist on every platform.
     *
     * Common SpecialFolder values:
     *
     *   Desktop                  → user's Desktop folder
     *   MyDocuments / Personal   → Documents (Personal is the older alias)
     *   ApplicationData          → roaming profile app settings (syncs across PCs)
     *   LocalApplicationData     → local-only app cache/settings
     *   UserProfile              → home directory root on Windows; profile on Unix
     *   CommonApplicationData    → machine-wide shared app data
     *   Templates, MyPictures, … → other well-known user folders
     *
     * Optional second argument: SpecialFolderOption (Create, DoNotVerify).
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateSpecialFolders()
    {
        string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string personal = Environment.GetFolderPath(Environment.SpecialFolder.Personal); // alias on Windows
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string commonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

        Console.WriteLine();
        Console.WriteLine("--- Environment.SpecialFolder ---");
        Console.WriteLine($"Desktop: {desktop}");
        Console.WriteLine($"MyDocuments: {documents}");
        Console.WriteLine($"Personal (alias): {personal}");
        Console.WriteLine($"ApplicationData: {appData}");
        Console.WriteLine($"LocalApplicationData: {localAppData}");
        Console.WriteLine($"UserProfile: {userProfile}");
        Console.WriteLine($"CommonApplicationData: {commonAppData}");

        // Typical app config path — roaming settings under ApplicationData
        string appConfigDir = Path.Combine(appData, "MyCompany", "MyApp");
        Console.WriteLine($"Typical roaming config folder: {appConfigDir}");

        // Create if missing — Directory.CreateDirectory is ch.01; shown here as path use case
        string demoConfigDir = Path.Combine(localAppData, "PathTutorialDemo");
        Directory.CreateDirectory(demoConfigDir);
        Console.WriteLine($"Created demo folder under LocalApplicationData: {demoConfigDir}");
        Directory.Delete(demoConfigDir);
    }

    /*
     * =========================================================================
     * SECTION 7: RUNTIME CONTEXT — CurrentDirectory, MachineName, UserName
     * =========================================================================
     *
     *   Environment.CurrentDirectory  → process working directory (shell/IDE CWD)
     *   AppContext.BaseDirectory      → folder containing the running assembly
     *   Environment.MachineName       → computer name (useful in log path segments)
     *   Environment.UserName          → interactive user name (audit/log folders)
     *
     * Pitfall: CurrentDirectory changes when the process starts from different folders.
     * Relative GetFullPath and File.Open on relative paths follow CurrentDirectory —
     * not BaseDirectory. Deployed apps often build paths from BaseDirectory instead.
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateRuntimeContext()
    {
        Console.WriteLine();
        Console.WriteLine("--- Runtime context ---");
        Console.WriteLine($"Environment.CurrentDirectory: {Environment.CurrentDirectory}");
        Console.WriteLine($"AppContext.BaseDirectory: {AppContext.BaseDirectory}");
        Console.WriteLine($"Environment.MachineName: {Environment.MachineName}");
        Console.WriteLine($"Environment.UserName: {Environment.UserName}");

        // Example: per-machine/per-user log folder under the app base directory
        string logRoot = Path.Combine(
            AppContext.BaseDirectory,
            "logs",
            Environment.MachineName,
            Environment.UserName);
        Console.WriteLine($"Example log path root: {logRoot}");
    }

    /*
     * =========================================================================
     * SECTION 8: CROSS-PLATFORM PATH RULES AND PITFALLS
     * =========================================================================
     *
     * Windows                          | Linux / macOS
     * ---------------------------------|----------------------------------
     * Drive roots (C:\)                | Single root per mount (/home, /var)
     * Usually case-insensitive paths   | Case-sensitive paths
     * \ primary, / often accepted      | / only
     *
     * Rules:
     *   • Use Path.Combine — never hard-code "\\" or assume "C:".
     *   • Compare paths carefully on Unix — "Report.pdf" ≠ "report.pdf".
     *   • Use SpecialFolder + Combine for user locations, not string literals.
     *   • Prefer BaseDirectory for deployed relative assets; document CWD assumptions.
     *
     * End-to-end demo: build a path under BaseDirectory, create one file, verify,
     * then delete — minimal File/Directory usage (full API in ch.01).
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateCrossPlatformDemo()
    {
        string demoRoot = Path.Combine(AppContext.BaseDirectory, "path-demo");
        if (Directory.Exists(demoRoot))
        {
            Directory.Delete(demoRoot, recursive: true);
        }

        string settingsFile = Path.Combine(demoRoot, "config", "settings.json");
        Directory.CreateDirectory(Path.GetDirectoryName(settingsFile)!);
        File.WriteAllText(settingsFile, "{ \"theme\": \"dark\" }");

        Console.WriteLine();
        Console.WriteLine("--- Cross-platform demo ---");
        Console.WriteLine($"Created settings at: {settingsFile}");
        Console.WriteLine($"File.Exists: {File.Exists(settingsFile)}");
        Console.WriteLine($"IsPathRooted(settings): {Path.IsPathRooted(settingsFile)}");
        Console.WriteLine($"Case-sensitive filesystem: {OperatingSystem.IsLinux() || OperatingSystem.IsMacOS()}");

        Directory.Delete(demoRoot, recursive: true);
    }

    /*
     * =========================================================================
     * SECTION 9: DEMONSTRATION — Main ORCHESTRATES THE CHAPTER
     * =========================================================================
     *
     * Main runs each helper in reading order. Scroll up to the section comment
     * above each method for the full explanation and tables.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        Console.WriteLine("=== 04. Path and Environment Classes ===\n");

        DemonstratePathCombine();
        string absoluteSample = DemonstratePathParsing();
        string relativeSample = Path.Combine("exports", "2026", "sales.csv");
        DemonstratePathResolution(relativeSample, absoluteSample);
        DemonstrateSeparators();
        DemonstrateTempPaths();
        DemonstrateSpecialFolders();
        DemonstrateRuntimeContext();
        DemonstrateCrossPlatformDemo();

        Console.WriteLine();
        Console.WriteLine("=== End of chapter demo ===");
    }
}

/*
 * =============================================================================
 * QUICK REFERENCE — PATH AND ENVIRONMENT
 * =============================================================================
 *
 * --- Path ---
 *
 *   Path.Combine(a, b, …)                  OS-correct join; prefer over "\\" or "/"
 *   Path.GetFileName(path)                 Last segment (e.g. "report.pdf")
 *   Path.GetFileNameWithoutExtension(path) Name without extension
 *   Path.GetExtension(path)                ".pdf" including dot (or empty)
 *   Path.GetDirectoryName(path)            Parent folder (null if none)
 *   Path.ChangeExtension(path, ".bak")     Swap or remove extension safely
 *   Path.GetFullPath(relative)             Absolute path from CurrentDirectory
 *   Path.IsPathRooted(path)                true when absolute (drive or / root)
 *   Path.GetPathRoot(path)                 "C:\\" or "/" prefix
 *   Path.GetTempPath()                     OS temp directory
 *   Path.GetTempFileName()                 Creates unique temp file; returns path
 *   Path.DirectorySeparatorChar            \ on Windows, / on Linux/macOS
 *   Path.AltDirectorySeparatorChar         Alternate separator char
 *
 * --- Environment ---
 *
 *   Environment.GetFolderPath(SpecialFolder.Desktop)
 *   Environment.GetFolderPath(SpecialFolder.MyDocuments)
 *   Environment.GetFolderPath(SpecialFolder.ApplicationData)       roaming profile
 *   Environment.GetFolderPath(SpecialFolder.LocalApplicationData)  local cache
 *   Environment.GetFolderPath(SpecialFolder.UserProfile)
 *   Environment.GetFolderPath(SpecialFolder.CommonApplicationData)
 *   Environment.CurrentDirectory           Process working directory
 *   Environment.MachineName                Computer name
 *   Environment.UserName                   Interactive user name
 *   AppContext.BaseDirectory               Folder containing running assembly
 *
 * --- Common mistakes ---
 *
 *  Mistake                               | Result
 *  --------------------------------------|----------------------------------
 *  "C:\\Users\\" + name + "\\file.txt"   | Breaks on Linux/macOS
 *  Assuming drive letters everywhere     | Use SpecialFolder + Combine
 *  Confusing CurrentDirectory vs Base    | Relative paths follow CurrentDirectory
 *  Manual extension slice (path[..^4])   | Use GetExtension / ChangeExtension
 *  Forgetting to delete GetTempFileName  | Orphan temp files accumulate
 *
 * --- Related chapters ---
 *
 *   01. File and Directory Operations     File.Exists, Directory.CreateDirectory
 *   02. StreamReader and StreamWriter     Open streams at paths built here
 *   05. Working with CSV and Text Files   Read text files at combined paths
 *
 * =============================================================================
 */
