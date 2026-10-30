using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace xRAR.Services
{
    public static class IconExtractor
    {
        private static readonly Dictionary<string, ImageSource> _iconCache = new();
        private static readonly object _lock = new();

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes,
            ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        private const uint SHGFI_ICON = 0x000000100;
        private const uint SHGFI_SMALLICON = 0x000000001;
        private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
        private const uint SHGFI_SYSICONINDEX = 0x000004000;
        private const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;

        /// <summary>Get a small icon ImageSource for a file path or extension.</summary>
        public static ImageSource? GetIcon(string pathOrExtension, bool isDirectory)
        {
            string cacheKey = isDirectory ? "__DIR__" : (pathOrExtension.ToLowerInvariant());

            lock (_lock)
            {
                if (_iconCache.TryGetValue(cacheKey, out var cached))
                    return cached;
            }

            ImageSource? icon = null;
            IntPtr hIcon = IntPtr.Zero;

            try
            {
                var shfi = new SHFILEINFO();
                uint flags = SHGFI_ICON | SHGFI_SMALLICON | SHGFI_USEFILEATTRIBUTES;
                uint attr = isDirectory ? FILE_ATTRIBUTE_DIRECTORY : FILE_ATTRIBUTE_NORMAL;

                // If it's a full path, use it directly; otherwise use extension with attributes
                string path = pathOrExtension;
                if (!isDirectory && !Path.IsPathRooted(pathOrExtension) && !File.Exists(pathOrExtension))
                {
                    // It's an extension like ".zip" - create a dummy path
                    path = "dummy" + (pathOrExtension.StartsWith(".") ? pathOrExtension : "." + pathOrExtension);
                }

                IntPtr result = SHGetFileInfo(path, attr, ref shfi, (uint)Marshal.SizeOf(typeof(SHFILEINFO)), flags);
                if (result != IntPtr.Zero && shfi.hIcon != IntPtr.Zero)
                {
                    hIcon = shfi.hIcon;
                    icon = Imaging.CreateBitmapSourceFromHIcon(
                        shfi.hIcon,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    icon.Freeze();
                }
            }
            catch
            {
                // Fallback: return null, will use default
            }
            finally
            {
                if (hIcon != IntPtr.Zero)
                    DestroyIcon(hIcon);
            }

            lock (_lock)
            {
                if (!_iconCache.ContainsKey(cacheKey))
                    _iconCache[cacheKey] = icon!;
            }

            return icon;
        }

        /// <summary>Get icon for a FileSystemItem.</summary>
        public static ImageSource? GetIconForItem(Models.FileSystemItem item)
        {
            if (item.IsDirectory)
                return GetIcon("", true);

            // Use full path if available for accurate icon
            if (!string.IsNullOrEmpty(item.FullPath) && File.Exists(item.FullPath))
                return GetIcon(item.FullPath, false);

            // Fallback to extension
            var ext = Path.GetExtension(item.Name);
            if (!string.IsNullOrEmpty(ext))
                return GetIcon(ext, false);

            return GetIcon(".txt", false);
        }

        /// <summary>Preload common icons to avoid UI stutter.</summary>
        public static void PreloadCommonIcons()
        {
            GetIcon("", true); // folder
            string[] exts = { ".rar", ".zip", ".zipx", ".7z", ".tar", ".gz", ".bz2", ".xz",
                              ".cab", ".arj", ".lzh", ".iso", ".zst", ".exe", ".dll", ".txt",
                              ".pdf", ".jpg", ".png", ".mp3", ".mp4", ".doc", ".docx",
                              ".xls", ".xlsx", ".bat", ".cmd", ".ini", ".lnk", ".sys" };
            foreach (var ext in exts)
                GetIcon(ext, false);
        }
    }
}
