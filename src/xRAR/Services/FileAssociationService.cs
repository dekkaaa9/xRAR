using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace xRAR.Services
{
    public static class FileAssociationService
    {
        // Extensions extracted from WinRAR 7.13 binary (Setup\.* registry paths + filter strings)
        private static readonly string[] ArchiveExtensions = new[]
        {
            ".rar", ".zip", ".zipx", ".7z", ".tar", ".gz", ".bz2", ".xz",
            ".cab", ".arj", ".lzh", ".lz", ".iso", ".z", ".zst", ".uue",
            ".jar", ".001", ".tzst", ".tgz", ".tbz2", ".txz", ".taz", ".tbz"
        };

        private static string ExePath => Process.GetCurrentProcess().MainModule?.FileName
            ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "xRAR.exe");

        /// <summary>
        /// User-level classes key (HKCU\Software\Classes).
        /// No admin rights required — works for auto-register on first run.
        /// </summary>
        private static RegistryKey UserClasses =>
            Registry.CurrentUser.OpenSubKey("Software\\Classes", true)
            ?? Registry.CurrentUser.CreateSubKey("Software\\Classes");

        /// <summary>Check if xRAR is registered for the given extension.</summary>
        public static bool IsRegistered(string ext)
        {
            try
            {
                using var key = UserClasses.OpenSubKey(ext);
                if (key == null) return false;
                var progId = key.GetValue("") as string;
                if (string.IsNullOrEmpty(progId)) return false;

                using var cmdKey = UserClasses.OpenSubKey($@"{progId}\shell\open\command");
                if (cmdKey == null) return false;
                var cmd = cmdKey.GetValue("") as string;
                return cmd != null && cmd.Contains("xRAR", StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }

        /// <summary>Register xRAR as handler for all archive extensions.</summary>
        public static List<string> RegisterAll()
        {
            var registered = new List<string>();
            foreach (var ext in ArchiveExtensions)
            {
                try { RegisterExtension(ext); registered.Add(ext); }
                catch { }
            }
            return registered;
        }

        /// <summary>Register xRAR for a single extension (user-level, no admin needed).</summary>
        public static void RegisterExtension(string ext)
        {
            if (!ext.StartsWith(".")) ext = "." + ext;
            string progId = "xRAR" + ext;

            using var root = UserClasses;

            // .zip -> xRAR.zip
            using (var extKey = root.CreateSubKey(ext))
                extKey.SetValue("", progId);

            // xRAR.zip
            using (var progKey = root.CreateSubKey(progId))
            {
                progKey.SetValue("", $"{ext.TrimStart('.').ToUpperInvariant()} Archive");
                using var iconKey = progKey.CreateSubKey("DefaultIcon");
                iconKey.SetValue("", $"\"{ExePath}\",0");
            }

            // shell\open\command
            using (var cmdKey = root.CreateSubKey($@"{progId}\shell\open\command"))
                cmdKey.SetValue("", $"\"{ExePath}\" \"%1\"");

            // OpenWithProgids
            using (var openWithKey = root.CreateSubKey($@"{ext}\OpenWithProgids"))
                openWithKey.SetValue(progId, Array.Empty<byte>(), RegistryValueKind.Binary);

            SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
        }

        /// <summary>Unregister xRAR for all archive extensions.</summary>
        public static void UnregisterAll()
        {
            foreach (var ext in ArchiveExtensions)
            {
                try { UnregisterExtension(ext); }
                catch { }
            }
        }

        /// <summary>Unregister xRAR for a single extension.</summary>
        public static void UnregisterExtension(string ext)
        {
            if (!ext.StartsWith(".")) ext = "." + ext;
            string progId = "xRAR" + ext;

            try
            {
                using var root = UserClasses;

                using (var openWithKey = root.OpenSubKey($@"{ext}\OpenWithProgids", true))
                    openWithKey?.DeleteValue(progId, false);

                using (var extKey = root.OpenSubKey(ext, true))
                {
                    if (extKey?.GetValue("") as string == progId)
                        extKey.DeleteValue("", false);
                }

                root.DeleteSubKeyTree(progId, false);
            }
            catch { }

            SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
        }

        /// <summary>Get registration status for all extensions.</summary>
        public static Dictionary<string, bool> GetStatus()
        {
            var status = new Dictionary<string, bool>();
            foreach (var ext in ArchiveExtensions)
                status[ext] = IsRegistered(ext);
            return status;
        }

        /// <summary>
        /// Auto-register on first run — silent, non-blocking.
        /// Only registers if at least one extension is missing.
        /// </summary>
        public static int AutoRegisterIfNeeded()
        {
            int missing = 0;
            foreach (var ext in ArchiveExtensions)
            {
                if (!IsRegistered(ext)) missing++;
            }

            if (missing > 0)
            {
                try { RegisterAll(); }
                catch { }
            }
            return missing;
        }

        [System.Runtime.InteropServices.DllImport("shell32.dll")]
        private static extern void SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);
    }
}
