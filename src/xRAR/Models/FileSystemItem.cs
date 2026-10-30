using System;
using System.IO;

namespace xRAR.Models
{
    public enum ItemType
    {
        File,
        Folder,
        Archive
    }

    public class FileSystemItem : System.ComponentModel.INotifyPropertyChanged
    {
        private string _name = "";
        private long _size;
        private string _type = "";
        private DateTime _modified;
        private DateTime _created;
        private DateTime _accessed;
        private string _fullPath = "";
        private bool _isDirectory;
        private bool _isArchive;
        private string _archiveName = "";
        private string _packedSize = "";
        private string _crc32 = "";
        private string _attributes = "";
        private System.Windows.Media.ImageSource? _icon;

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public long Size
        {
            get => _size;
            set { _size = value; OnPropertyChanged(nameof(Size)); OnPropertyChanged(nameof(SizeDisplay)); }
        }

        public string SizeDisplay
        {
            get
            {
                if (_isDirectory) return "";
                return FormatBytes(_size);
            }
        }

        public string PackedSize
        {
            get => _packedSize;
            set { _packedSize = value; OnPropertyChanged(nameof(PackedSize)); OnPropertyChanged(nameof(PackedDisplay)); }
        }

        public string PackedDisplay => string.IsNullOrEmpty(_packedSize) ? "" : _packedSize;

        public string CRC32
        {
            get => _crc32;
            set { _crc32 = value; OnPropertyChanged(nameof(CRC32)); OnPropertyChanged(nameof(Checksum)); }
        }

        public string Checksum => string.IsNullOrEmpty(_crc32) ? "" : _crc32;

        public string Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(nameof(Type)); }
        }

        public DateTime Modified
        {
            get => _modified;
            set { _modified = value; OnPropertyChanged(nameof(Modified)); OnPropertyChanged(nameof(ModifiedDisplay)); }
        }

        public string ModifiedDisplay => _modified.ToString("dd/MM/yy HH:mm");

        public DateTime Created
        {
            get => _created;
            set { _created = value; OnPropertyChanged(nameof(Created)); OnPropertyChanged(nameof(CreatedDisplay)); }
        }

        public string CreatedDisplay => _created.ToString("dd/MM/yy HH:mm");

        public DateTime Accessed
        {
            get => _accessed;
            set { _accessed = value; OnPropertyChanged(nameof(Accessed)); OnPropertyChanged(nameof(AccessedDisplay)); }
        }

        public string AccessedDisplay => _accessed.ToString("dd/MM/yy HH:mm");

        public string Path => _fullPath;

        public string FullPath
        {
            get => _fullPath;
            set { _fullPath = value; OnPropertyChanged(nameof(FullPath)); }
        }

        public bool IsDirectory
        {
            get => _isDirectory;
            set { _isDirectory = value; OnPropertyChanged(nameof(IsDirectory)); }
        }

        public bool IsArchive
        {
            get => _isArchive;
            set { _isArchive = value; OnPropertyChanged(nameof(IsArchive)); }
        }

        public string ArchiveName
        {
            get => _archiveName;
            set { _archiveName = value; OnPropertyChanged(nameof(ArchiveName)); }
        }

        public string Attributes
        {
            get => _attributes;
            set { _attributes = value; OnPropertyChanged(nameof(Attributes)); }
        }

        public string IconKey => IsDirectory ? "folder" : (IsArchive ? "archive" : "file");

        public System.Windows.Media.ImageSource? Icon
        {
            get => _icon;
            set { _icon = value; OnPropertyChanged(nameof(Icon)); }
        }

        public static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes}";
            if (bytes < 1024 * 1024) return $"{bytes / 1024:N0} KB";
            if (bytes < 1024L * 1024 * 1024) return $"{bytes / (1024.0 * 1024):N1} MB";
            return $"{bytes / (1024.0 * 1024 * 1024):N2} GB";
        }

        public static string FormatBytesExact(long bytes)
        {
            return bytes.ToString("N0");
        }

        public static FileSystemItem FromFileInfo(FileInfo fi)
        {
            return new FileSystemItem
            {
                Name = fi.Name,
                Size = fi.Length,
                Type = GetFileType(fi.Extension),
                Modified = fi.LastWriteTime,
                Created = fi.CreationTime,
                Accessed = fi.LastAccessTime,
                FullPath = fi.FullName,
                IsDirectory = false,
                IsArchive = IsArchiveExtension(fi.Extension),
                Attributes = GetAttrString(fi.Attributes)
            };
        }

        public static FileSystemItem FromDirectoryInfo(DirectoryInfo di)
        {
            return new FileSystemItem
            {
                Name = di.Name,
                Size = 0,
                Type = "File folder",
                Modified = di.LastWriteTime,
                Created = di.CreationTime,
                Accessed = di.LastAccessTime,
                FullPath = di.FullName,
                IsDirectory = true,
                Attributes = GetAttrString(di.Attributes)
            };
        }

        public static string GetFileType(string ext)
        {
            return ext.ToLowerInvariant() switch
            {
                ".rar" => "WinRAR archive",
                ".zip" or ".zipx" => "ZIP archive",
                ".7z" => "7Z archive",
                ".tar" => "TAR archive",
                ".gz" or ".tgz" => "GZIP archive",
                ".bz2" or ".tbz2" or ".taz" or ".tbz" => "BZIP2 archive",
                ".xz" or ".txz" => "XZ archive",
                ".cab" => "Cabinet archive",
                ".arj" => "ARJ archive",
                ".lzh" or ".lha" or ".lz" => "LZH archive",
                ".ace" => "ACE archive",
                ".zst" or ".tzst" => "Zstandard archive",
                ".z" => "Compressed archive",
                ".uue" => "UUEncoded archive",
                ".jar" => "Java archive",
                ".001" => "Split archive",
                ".iso" => "Disc image",
                ".exe" => "Application",
                ".txt" => "Text Document",
                ".doc" or ".docx" => "Microsoft Word Document",
                ".xls" or ".xlsx" => "Microsoft Excel Worksheet",
                ".ppt" or ".pptx" => "Microsoft PowerPoint Presentation",
                ".pdf" => "PDF Document",
                ".jpg" or ".jpeg" => "JPEG Image",
                ".png" => "PNG Image",
                ".gif" => "GIF Image",
                ".bmp" => "Bitmap Image",
                ".mp3" => "MP3 Audio",
                ".mp4" => "MP4 Video",
                ".avi" => "AVI Video",
                ".mkv" => "MKV Video",
                ".dll" => "Application Extension",
                ".sys" => "System File",
                ".ini" => "Configuration Settings",
                ".bat" or ".cmd" => "Windows Batch File",
                ".ps1" => "PowerShell Script",
                ".lnk" => "Shortcut",
                ".html" or ".htm" => "HTML Document",
                ".xml" => "XML Document",
                ".json" => "JSON Document",
                "" => "File",
                _ => ext.TrimStart('.').ToUpperInvariant() + " File"
            };
        }

        public static bool IsArchiveExtension(string ext)
        {
            return ext.ToLowerInvariant() switch
            {
                ".rar" or ".zip" or ".zipx" or ".7z" or ".tar" or ".gz" or ".tgz"
                or ".bz2" or ".tbz2" or ".xz" or ".txz" or ".cab" or ".arj"
                or ".lzh" or ".lha" or ".lz" or ".ace" or ".zst" or ".tzst"
                or ".uue" or ".z" or ".taz" or ".tbz" or ".iso" or ".jar"
                or ".001" => true,
                _ => false
            };
        }

        public static string GetAttrString(FileAttributes attrs)
        {
            var sb = new System.Text.StringBuilder();
            if ((attrs & FileAttributes.ReadOnly) != 0) sb.Append('R');
            if ((attrs & FileAttributes.Hidden) != 0) sb.Append('H');
            if ((attrs & FileAttributes.System) != 0) sb.Append('S');
            if ((attrs & FileAttributes.Archive) != 0) sb.Append('A');
            if ((attrs & FileAttributes.Compressed) != 0) sb.Append('C');
            if ((attrs & FileAttributes.Encrypted) != 0) sb.Append('E');
            if ((attrs & FileAttributes.Temporary) != 0) sb.Append('T');
            return sb.ToString();
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
    }
}
