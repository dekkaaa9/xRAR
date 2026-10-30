using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace xRAR.Services
{
    public class ArchiveEntry
    {
        public string Name { get; set; } = "";
        public long Size { get; set; }
        public long PackedSize { get; set; }
        public string Type { get; set; } = "";
        public DateTime Modified { get; set; }
        public string CRC32 { get; set; } = "";
        public string Attributes { get; set; } = "";
        public bool IsDirectory { get; set; }
        public string Ratio => Size > 0 ? $"{(1.0 - (double)PackedSize / Size) * 100:F1}%" : "";
    }

    public class ArchiveService
    {
        private readonly string _toolsDir;

        public ArchiveService()
        {
            // Look for Rar.exe / UnRAR.exe next to our exe, then in Program Files
            var exeDir = AppContext.BaseDirectory;
            _toolsDir = exeDir;

            if (!File.Exists(Path.Combine(_toolsDir, "UnRAR.exe")))
            {
                var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                var candidate = Path.Combine(pf, "WinRAR");
                if (File.Exists(Path.Combine(candidate, "UnRAR.exe")))
                    _toolsDir = candidate;
            }
        }

        public string ToolsDir => _toolsDir;

        public bool HasRarExe => File.Exists(Path.Combine(_toolsDir, "Rar.exe"));
        public bool HasUnRarExe => File.Exists(Path.Combine(_toolsDir, "UnRAR.exe"));

        /// <summary>List contents of an archive using UnRAR lb/vb.</summary>
        public async Task<List<ArchiveEntry>> ListArchiveAsync(string archivePath)
        {
            var entries = new List<ArchiveEntry>();
            if (!HasUnRarExe) return entries;

            var ext = Path.GetExtension(archivePath).ToLowerInvariant();
            if (ext == ".rar")
            {
                entries = await ListRarAsync(archivePath);
            }
            else if (ext == ".zip" || ext == ".zipx")
            {
                entries = await ListZipAsync(archivePath);
            }
            else if (ext == ".7z")
            {
                entries = await List7zAsync(archivePath);
            }
            else
            {
                // Try RAR lister as fallback, then zip
                entries = await ListRarAsync(archivePath);
                if (entries.Count == 0)
                    entries = await ListZipAsync(archivePath);
            }
            return entries;
        }

        private async Task<List<ArchiveEntry>> ListRarAsync(string archivePath)
        {
            var entries = new List<ArchiveEntry>();
            var output = await RunProcessAsync(Path.Combine(_toolsDir, "UnRAR.exe"),
                $"vb \"{archivePath}\"");

            foreach (var line in output.Split('\n'))
            {
                var trimmed = line.TrimEnd('\r');
                if (string.IsNullOrWhiteSpace(trimmed)) continue;

                var entry = new ArchiveEntry { Name = trimmed };
                if (trimmed.EndsWith("\\") || trimmed.EndsWith("/"))
                {
                    entry.IsDirectory = true;
                    entry.Name = trimmed.TrimEnd('\\', '/');
                    entry.Type = "File folder";
                }
                else
                {
                    entry.Type = Models.FileSystemItem.GetFileType(Path.GetExtension(trimmed));
                }
                entries.Add(entry);
            }

            // Get detailed info with 'lt' command
            var detail = await RunProcessAsync(Path.Combine(_toolsDir, "UnRAR.exe"),
                $"lt \"{archivePath}\"");
            ParseRarDetail(detail, entries);

            return entries;
        }

        private void ParseRarDetail(string detail, List<ArchiveEntry> entries)
        {
            // UnRAR lt output format:
            //       Name: filename
            //       Type: File
            //       Size: 12345
            //  Packed size: 6789
            //  ...
            //       mtime: 2020-01-01 12:00:00,000000000
            //  Attributes: ...
            //       CRC32: XXXXXXXX
            var lines = detail.Split('\n');
            ArchiveEntry? current = null;

            foreach (var raw in lines)
            {
                var line = raw.TrimEnd('\r');
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (line.Contains("Name:") && !line.Contains("Archive name"))
                {
                    var name = line.Substring(line.IndexOf("Name:") + 5).Trim();
                    current = entries.Find(e => e.Name == name || e.Name.EndsWith(name));
                }
                else if (current != null)
                {
                    if (line.Contains("Size:"))
                    {
                        var val = line.Substring(line.IndexOf("Size:") + 5).Trim();
                        if (long.TryParse(val, out var sz)) current.Size = sz;
                    }
                    else if (line.Contains("Packed size:"))
                    {
                        var val = line.Substring(line.IndexOf("Packed size:") + 11).Trim();
                        if (long.TryParse(val, out var ps)) current.PackedSize = ps;
                    }
                    else if (line.Contains("mtime:"))
                    {
                        var val = line.Substring(line.IndexOf("mtime:") + 6).Trim();
                        if (DateTime.TryParse(val.Split(',')[0], out var dt)) current.Modified = dt;
                    }
                    else if (line.Contains("CRC32:"))
                    {
                        current.CRC32 = line.Substring(line.IndexOf("CRC32:") + 6).Trim();
                    }
                    else if (line.Contains("Attributes:"))
                    {
                        current.Attributes = line.Substring(line.IndexOf("Attributes:") + 11).Trim();
                    }
                }
            }
        }

        private async Task<List<ArchiveEntry>> ListZipAsync(string archivePath)
        {
            var entries = new List<ArchiveEntry>();
            try
            {
                using var zip = System.IO.Compression.ZipFile.OpenRead(archivePath);
                foreach (var e in zip.Entries)
                {
                    var entry = new ArchiveEntry
                    {
                        Name = e.FullName.TrimEnd('/'),
                        Size = e.Length,
                        PackedSize = e.CompressedLength,
                        Modified = e.LastWriteTime.DateTime,
                        IsDirectory = e.FullName.EndsWith("/"),
                        Type = e.FullName.EndsWith("/") ? "File folder" : Models.FileSystemItem.GetFileType(Path.GetExtension(e.FullName)),
                        CRC32 = e.Crc32.ToString("X8")
                    };
                    entries.Add(entry);
                }
            }
            catch { }
            return entries;
        }

        private async Task<List<ArchiveEntry>> List7zAsync(string archivePath)
        {
            var entries = new List<ArchiveEntry>();
            var sevenZip = Path.Combine(_toolsDir, "7z.exe");
            if (!File.Exists(sevenZip))
            {
                // Try common paths
                foreach (var p in new[] {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "7-Zip", "7z.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "7-Zip", "7z.exe")
                })
                {
                    if (File.Exists(p)) { sevenZip = p; break; }
                }
            }
            if (!File.Exists(sevenZip)) return entries;

            var output = await RunProcessAsync(sevenZip, $"l -ba -slt \"{archivePath}\"");
            var blocks = Regex.Split(output, @"(?=^Path = )", RegexOptions.Multiline);
            foreach (var block in blocks)
            {
                if (!block.Contains("Path =")) continue;
                var entry = new ArchiveEntry();
                foreach (var line in block.Split('\n'))
                {
                    var l = line.TrimEnd('\r');
                    if (l.StartsWith("Path = ")) entry.Name = l[7..].TrimEnd('/', '\\');
                    else if (l.StartsWith("Size = ") && long.TryParse(l[7..], out var sz)) entry.Size = sz;
                    else if (l.StartsWith("Packed Size = ") && long.TryParse(l[14..], out var ps)) entry.PackedSize = ps;
                    else if (l.StartsWith("Modified = ")) { if (DateTime.TryParse(l[11..], out var dt)) entry.Modified = dt; }
                    else if (l.StartsWith("Folder = ")) entry.IsDirectory = l[9..].Trim() == "+";
                    else if (l.StartsWith("CRC = ")) entry.CRC32 = l[6..].Trim();
                }
                if (!string.IsNullOrEmpty(entry.Name))
                {
                    entry.Type = entry.IsDirectory ? "File folder" : Models.FileSystemItem.GetFileType(Path.GetExtension(entry.Name));
                    entries.Add(entry);
                }
            }
            return entries;
        }

        /// <summary>Extract files from archive. If filesToExtract is null/empty, extracts all.</summary>
        public async Task<bool> ExtractAsync(string archivePath, string destPath, List<string>? filesToExtract = null, string? password = null)
        {
            var ext = Path.GetExtension(archivePath).ToLowerInvariant();
            bool selective = filesToExtract != null && filesToExtract.Count > 0;

            if (ext == ".rar")
            {
                var args = new StringBuilder("x -y");
                if (!string.IsNullOrEmpty(password)) args.Append($" -p{password}");
                else args.Append(" -p-");
                args.Append($" \"{archivePath}\"");
                if (selective)
                {
                    foreach (var f in filesToExtract!)
                        args.Append($" \"{f}\"");
                }
                args.Append($" \"{destPath}\\\"");
                var result = await RunProcessAsync(Path.Combine(_toolsDir, "UnRAR.exe"), args.ToString());
                return !result.Contains("error", StringComparison.OrdinalIgnoreCase) || result.Contains("All OK");
            }
            else if (ext == ".zip" || ext == ".zipx")
            {
                try
                {
                    Directory.CreateDirectory(destPath);
                    using var zip = System.IO.Compression.ZipFile.OpenRead(archivePath);

                    if (!selective)
                    {
                        System.IO.Compression.ZipFile.ExtractToDirectory(archivePath, destPath, true);
                    }
                    else
                    {
                        var nameSet = new HashSet<string>(filesToExtract!, StringComparer.OrdinalIgnoreCase);
                        foreach (var entry in zip.Entries)
                        {
                            // Match exact name OR if a folder was selected, match all children
                            bool match = nameSet.Contains(entry.FullName);
                            if (!match)
                            {
                                foreach (var name in nameSet)
                                {
                                    if (entry.FullName.StartsWith(name.TrimEnd('/') + "/", StringComparison.OrdinalIgnoreCase))
                                    {
                                        match = true;
                                        break;
                                    }
                                }
                            }
                            if (!match) continue;

                            var target = Path.Combine(destPath, entry.FullName);
                            var dir = Path.GetDirectoryName(target);
                            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                            if (!entry.FullName.EndsWith("/"))
                                entry.ExtractToFile(target, true);
                        }
                    }
                    return true;
                }
                catch { return false; }
            }
            else if (ext == ".7z")
            {
                var sevenZip = Find7zExe();
                if (sevenZip == null) return false;
                var args = $"x -y -o\"{destPath}\" \"{archivePath}\"";
                if (selective)
                {
                    foreach (var f in filesToExtract!)
                        args += $" \"{f}\"";
                }
                await RunProcessAsync(sevenZip, args);
                return true;
            }

            // Fallback: try unrar then 7z
            if (HasUnRarExe)
            {
                var args = $"x -y -p- \"{archivePath}\"";
                if (selective)
                {
                    foreach (var f in filesToExtract!)
                        args += $" \"{f}\"";
                }
                args += $" \"{destPath}\\\"";
                await RunProcessAsync(Path.Combine(_toolsDir, "UnRAR.exe"), args);
                return true;
            }
            return false;
        }

        /// <summary>Create a new archive.</summary>
        public async Task<bool> CreateArchiveAsync(string archivePath, List<string> files, string format = "rar",
            int compressionLevel = 3, string? password = null, bool solid = false, int recoveryRecord = 0)
        {
            if (format.ToLowerInvariant() == "zip")
            {
                try
                {
                    if (File.Exists(archivePath)) File.Delete(archivePath);
                    using var zip = System.IO.Compression.ZipFile.Open(archivePath, System.IO.Compression.ZipArchiveMode.Create);
                    foreach (var f in files)
                    {
                        if (Directory.Exists(f))
                        {
                            foreach (var file in Directory.GetFiles(f, "*", SearchOption.AllDirectories))
                            {
                                var rel = Path.GetRelativePath(Path.GetDirectoryName(f) ?? "", file);
                                zip.CreateEntryFromFile(file, rel, (System.IO.Compression.CompressionLevel)Math.Clamp(compressionLevel, 0, 3));
                            }
                        }
                        else
                        {
                            zip.CreateEntryFromFile(f, Path.GetFileName(f), (System.IO.Compression.CompressionLevel)Math.Clamp(compressionLevel, 0, 3));
                        }
                    }
                    return true;
                }
                catch { return false; }
            }

            if (!HasRarExe) return false;
            var rarArgs = new StringBuilder("a");
            rarArgs.Append($" -m{compressionLevel}");
            if (solid) rarArgs.Append(" -s");
            if (recoveryRecord > 0) rarArgs.Append($" -rr{recoveryRecord}%");
            if (!string.IsNullOrEmpty(password)) rarArgs.Append($" -hp{password}");
            rarArgs.Append($" \"{archivePath}\"");
            foreach (var f in files) rarArgs.Append($" \"{f}\"");
            var result = await RunProcessAsync(Path.Combine(_toolsDir, "Rar.exe"), rarArgs.ToString());
            return result.Contains("Done") || !result.Contains("error", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Test archive integrity.</summary>
        public async Task<string> TestArchiveAsync(string archivePath)
        {
            var ext = Path.GetExtension(archivePath).ToLowerInvariant();
            if (ext == ".rar")
            {
                return await RunProcessAsync(Path.Combine(_toolsDir, "UnRAR.exe"), $"t \"{archivePath}\"");
            }
            else if (ext == ".zip")
            {
                try
                {
                    using var zip = System.IO.Compression.ZipFile.OpenRead(archivePath);
                    foreach (var e in zip.Entries)
                    {
                        using var s = e.Open();
                        using var ms = new MemoryStream();
                        s.CopyTo(ms);
                    }
                    return "All OK";
                }
                catch (Exception ex) { return "Error: " + ex.Message; }
            }
            else if (ext == ".7z")
            {
                var sz = Find7zExe();
                if (sz != null) return await RunProcessAsync(sz, $"t \"{archivePath}\"");
            }
            return "Unknown format";
        }

        /// <summary>Get archive info.</summary>
        public async Task<Dictionary<string, string>> GetArchiveInfoAsync(string archivePath)
        {
            var info = new Dictionary<string, string>();
            var fi = new FileInfo(archivePath);
            info["File name"] = fi.Name;
            info["File size"] = Models.FileSystemItem.FormatBytesExact(fi.Length) + " bytes";
            info["Modified"] = fi.LastWriteTime.ToString("dd/MM/yyyy HH:mm:ss");
            info["Path"] = fi.DirectoryName ?? "";

            var ext = Path.GetExtension(archivePath).ToLowerInvariant();
            if (ext == ".rar" && HasUnRarExe)
            {
                var lt = await RunProcessAsync(Path.Combine(_toolsDir, "UnRAR.exe"), $"lt \"{archivePath}\"");
                foreach (var line in lt.Split('\n'))
                {
                    var l = line.TrimEnd('\r');
                    if (l.Contains("Archive name")) info["Archive name"] = l.Split(':')[1].Trim();
                    else if (l.Contains("Type:")) info["Archive type"] = l.Split(':')[1].Trim();
                    else if (l.Contains("Solid:")) info["Solid"] = l.Split(':')[1].Trim();
                    else if (l.Contains("Recovery record:")) info["Recovery record"] = l.Split(':')[1].Trim();
                    else if (l.Contains("Comment:")) info["Comment"] = l.Split(':')[1].Trim();
                }
            }
            return info;
        }

        private string? Find7zExe()
        {
            foreach (var p in new[] {
                Path.Combine(_toolsDir, "7z.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "7-Zip", "7z.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "7-Zip", "7z.exe")
            })
            {
                if (File.Exists(p)) return p;
            }
            return null;
        }

        private async Task<string> RunProcessAsync(string exe, string args)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = exe,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    WorkingDirectory = _toolsDir
                };
                using var proc = Process.Start(psi);
                if (proc == null) return "";
                var output = await proc.StandardOutput.ReadToEndAsync();
                var error = await proc.StandardError.ReadToEndAsync();
                await proc.WaitForExitAsync();
                return output + error;
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }
    }
}
