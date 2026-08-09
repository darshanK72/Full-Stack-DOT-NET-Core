/*
 * PROBLEM: Deployment Path Planner
 *
 * Installer tooling resolves roaming settings, cache, logs, and bundled assets
 * using Path and Environment APIs — no hard-coded drive letters.
 *
 * This exercise covers:
 *   ch04 — Path.Combine, GetFullPath, ChangeExtension, GetTempPath
 *   ch04 — Environment.SpecialFolder, AppContext.BaseDirectory, MachineName
 */

using System;
using System.IO;

namespace DesktopDeployment
{
    /*
     * Cross-platform path resolution for app deployment layout.
     *
     * Path strings only — no file I/O. No Console calls.
     */
    static class DeploymentPathPlanner
    {
        /*
         * Roaming settings under ApplicationData\company\appName.
         */
        public static string GetRoamingSettingsDirectory(string company, string appName)
        {
            // TODO: Combine GetFolderPath(ApplicationData), company, appName
            throw new NotImplementedException();
        }

        /*
         * Local cache under LocalApplicationData\company\appName.
         */
        public static string GetLocalCacheDirectory(string company, string appName)
        {
            // TODO: Combine GetFolderPath(LocalApplicationData), company, appName
            throw new NotImplementedException();
        }

        /*
         * Absolute path to a bundled asset relative to the running assembly folder.
         */
        public static string ResolveBundledAsset(string relativeAssetPath)
        {
            // TODO: Combine AppContext.BaseDirectory + GetFullPath
            throw new NotImplementedException();
        }

        /*
         * Per-machine log folder under local cache: ...\logs\{MachineName}.
         */
        public static string GetMachineLogDirectory(string company, string appName)
        {
            // TODO: start from GetLocalCacheDirectory, append logs and MachineName
            throw new NotImplementedException();
        }

        /*
         * Unique temp export path with given extension (must start with '.').
         *
         * Does not create the file.
         */
        public static string CreateTempExportPath(string extension)
        {
            // TODO: Combine GetTempPath() with export-{Guid}{extension}
            throw new NotImplementedException();
        }

        /*
         * Changes log file extension via Path.ChangeExtension.
         */
        public static string ChangeLogExtension(string logPath, string newExtension)
        {
            // TODO: Path.ChangeExtension(logPath, newExtension)
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: print roaming, cache, machine log for Contoso/Warehouse
            // TODO: print ResolveBundledAsset for config\defaults.json
            // TODO: print CreateTempExportPath(".csv")
            // TODO: print ChangeLogExtension sample
            throw new NotImplementedException();
        }
    }
}
