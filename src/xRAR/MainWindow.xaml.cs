using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using xRAR.Models;
using xRAR.Services;
using xRAR.Views;

namespace xRAR
{
    public partial class MainWindow : Window
    {
        private readonly ArchiveService _archiveService = new();
        private readonly ObservableCollection<FileSystemItem> _items = new();
        private string _currentPath = "";
        private bool _browsingArchive = false;
        private string _currentArchivePath = "";
        private List<ArchiveEntry> _archiveEntries = new();

        public MainWindow()
        {
            InitializeComponent();
            FileListView.ItemsSource = _items;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            IconExtractor.PreloadCommonIcons();
            LoadDrives();

            // If launched by double-clicking an archive file, open it directly
            if (App.StartupFilePath != null)
            {
                var ext = Path.GetExtension(App.StartupFilePath).ToLowerInvariant();
                if (FileSystemItem.IsArchiveExtension(ext))
                {
                    OpenArchive(App.StartupFilePath);
                    return;
                }
            }

            NavigateTo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        }

        #region Navigation

        private void LoadDrives()
        {
            AddressBar.Items.Clear();
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (!drive.IsReady) continue;
                string label;
                try
                {
                    label = drive.VolumeLabel;
                    if (string.IsNullOrWhiteSpace(label)) label = "Local Disk";
                }
                catch { label = "Local Disk"; }

                string display = drive.Name.Length > 0 && drive.Name[1] == ':'
                    ? $"{label} ({drive.Name[0]}:)"
                    : $"{label} ({drive.Name})";

                AddressBar.Items.Add(new ComboBoxItem
                {
                    Content = display,
                    Tag = drive.Name
                });
            }
        }

        private void NavigateTo(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path)) return;

                if (File.Exists(path) && FileSystemItem.IsArchiveExtension(Path.GetExtension(path)))
                {
                    OpenArchive(path);
                    return;
                }

                if (!Directory.Exists(path))
                {
                    MessageBox.Show("Cannot find path: " + path, "xRAR", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _currentPath = path;
                _browsingArchive = false;
                _currentArchivePath = "";

                var di = new DirectoryInfo(path);
                string titlePath;
                if (di.Parent == null)
                {
                    // Drive root
                    string label;
                    try
                    {
                        var drive = new DriveInfo(di.FullName);
                        label = string.IsNullOrWhiteSpace(drive.VolumeLabel) ? "Local Disk" : drive.VolumeLabel;
                    }
                    catch { label = "Local Disk"; }
                    titlePath = $"{label} ({di.FullName.TrimEnd('\\')})";
                }
                else
                {
                    titlePath = di.Name;
                }
                Title = $"{titlePath} - xRAR";

                // Update address bar
                AddressBar.Text = titlePath;
                AddressBar.SelectedItem = null;

                _items.Clear();
                LoadDirectoryContents(di);
                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "xRAR Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDirectoryContents(DirectoryInfo di)
        {
            // Folders first
            try
            {
                foreach (var dir in di.GetDirectories())
                {
                    if ((dir.Attributes & FileAttributes.Hidden) != 0 && !_showHidden) continue;
                    var item = FileSystemItem.FromDirectoryInfo(dir);
                    item.Icon = IconExtractor.GetIcon("", true);
                    _items.Add(item);
                }
            }
            catch (UnauthorizedAccessException) { }

            // Then files
            try
            {
                foreach (var file in di.GetFiles())
                {
                    if ((file.Attributes & FileAttributes.Hidden) != 0 && !_showHidden) continue;
                    var item = FileSystemItem.FromFileInfo(file);
                    item.Icon = IconExtractor.GetIcon(file.FullName, false);
                    _items.Add(item);
                }
            }
            catch (UnauthorizedAccessException) { }
        }

        private bool _showHidden = false;

        private async void OpenArchive(string archivePath)
        {
            try
            {
                _currentArchivePath = archivePath;
                _browsingArchive = true;
                _currentPath = Path.GetDirectoryName(archivePath) ?? "";

                Title = $"{Path.GetFileName(archivePath)} - xRAR";
                AddressBar.Text = archivePath;

                _items.Clear();
                _archiveEntries = await _archiveService.ListArchiveAsync(archivePath);

                foreach (var entry in _archiveEntries)
                {
                    var item = new FileSystemItem
                    {
                        Name = entry.Name,
                        Size = entry.Size,
                        Type = entry.Type,
                        Modified = entry.Modified,
                        FullPath = archivePath + "\\" + entry.Name,
                        IsDirectory = entry.IsDirectory,
                        IsArchive = !entry.IsDirectory && FileSystemItem.IsArchiveExtension(Path.GetExtension(entry.Name)),
                        PackedSize = entry.PackedSize > 0 ? FileSystemItem.FormatBytes(entry.PackedSize) : "",
                        CRC32 = entry.CRC32,
                        Attributes = entry.Attributes,
                        ArchiveName = archivePath
                    };
                    item.Icon = entry.IsDirectory
                        ? IconExtractor.GetIcon("", true)
                        : IconExtractor.GetIcon(Path.GetExtension(entry.Name), false);
                    _items.Add(item);
                }
                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening archive: {ex.Message}", "xRAR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnUp_Click(object sender, RoutedEventArgs e)
        {
            if (_browsingArchive)
            {
                NavigateTo(_currentPath);
                return;
            }
            if (string.IsNullOrEmpty(_currentPath)) return;
            var parent = Directory.GetParent(_currentPath);
            if (parent != null)
                NavigateTo(parent.FullName);
        }

        private void AddressBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var path = AddressBar.Text.Trim();
                if (File.Exists(path) || Directory.Exists(path))
                    NavigateTo(path);
                else
                    MessageBox.Show($"Cannot find '{path}'", "xRAR", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AddressBar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AddressBar.SelectedItem is ComboBoxItem item && item.Tag is string drivePath)
            {
                NavigateTo(drivePath);
            }
        }

        private void FileListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSelected();
        }

        private void OpenSelected()
        {
            if (FileListView.SelectedItem is not FileSystemItem item) return;

            if (item.IsDirectory)
            {
                if (_browsingArchive)
                {
                    // Navigate into archive folder - filter entries
                    FilterArchiveFolder(item.Name);
                }
                else
                {
                    NavigateTo(item.FullPath);
                }
            }
            else if (item.IsArchive && !_browsingArchive)
            {
                OpenArchive(item.FullPath);
            }
            else
            {
                // Open file with default app
                try
                {
                    if (_browsingArchive)
                    {
                        // Extract to temp and open
                        var tempDir = Path.Combine(Path.GetTempPath(), "xRAR_temp");
                        Directory.CreateDirectory(tempDir);
                        _ = _archiveService.ExtractAsync(_currentArchivePath, tempDir,
                            new List<string> { item.Name });
                    }
                    else
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(item.FullPath) { UseShellExecute = true });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "xRAR", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void FilterArchiveFolder(string folderName)
        {
            _items.Clear();
            var prefix = folderName + "\\";
            foreach (var entry in _archiveEntries.Where(e => e.Name.StartsWith(prefix)))
            {
                var rel = entry.Name[prefix.Length..];
                if (string.IsNullOrEmpty(rel)) continue;
                // Only show direct children
                var nextSlash = rel.IndexOf('\\');
                var displayName = nextSlash >= 0 ? rel[..nextSlash] : rel;
                var isDir = nextSlash >= 0;

                if (_items.Any(x => x.Name == displayName)) continue;

                _items.Add(new FileSystemItem
                {
                    Name = displayName,
                    Size = isDir ? 0 : entry.Size,
                    Type = isDir ? "File folder" : entry.Type,
                    Modified = entry.Modified,
                    FullPath = _currentArchivePath + "\\" + prefix + displayName,
                    IsDirectory = isDir,
                    IsArchive = !isDir && FileSystemItem.IsArchiveExtension(Path.GetExtension(displayName)),
                    PackedSize = entry.PackedSize > 0 ? FileSystemItem.FormatBytes(entry.PackedSize) : "",
                    CRC32 = entry.CRC32,
                    ArchiveName = _currentArchivePath,
                    Icon = isDir
                        ? IconExtractor.GetIcon("", true)
                        : IconExtractor.GetIcon(Path.GetExtension(displayName), false)
                });
            }
            UpdateStatusBar();
        }

        private void FileListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateStatusBar();
        }

        #endregion

        #region Status Bar

        private void UpdateStatusBar()
        {
            int folderCount = _items.Count(x => x.IsDirectory);
            int fileCount = _items.Count(x => !x.IsDirectory);
            long totalBytes = _items.Where(x => !x.IsDirectory).Sum(x => x.Size);

            string folderWord = folderCount == 1 ? "folder" : "folders";
            string fileWord = fileCount == 1 ? "file" : "files";
            string total = $"Total {folderCount} {folderWord} and {FileSystemItem.FormatBytesExact(totalBytes)} bytes in {fileCount} {fileWord}";

            // Show selected info + total on the same line
            var selected = FileListView.SelectedItems.Cast<FileSystemItem>().ToList();
            if (selected.Count > 0)
            {
                long selBytes = selected.Where(x => !x.IsDirectory).Sum(x => x.Size);
                int selFolders = selected.Count(x => x.IsDirectory);
                int selFiles = selected.Count(x => !x.IsDirectory);

                string selPart;
                if (selFolders > 0 && selFiles > 0)
                    selPart = $"Selected {selFolders} folder(s) and {selFiles} file(s), {FileSystemItem.FormatBytesExact(selBytes)} bytes";
                else if (selFolders > 0)
                    selPart = $"Selected {selFolders} folder(s)";
                else
                {
                    string fw = selFiles == 1 ? "file" : "files";
                    selPart = $"Selected {selFiles} {fw}, {FileSystemItem.FormatBytesExact(selBytes)} bytes";
                }
                StatusText.Text = $"{selPart}    |    {total}";
            }
            else
            {
                StatusText.Text = total;
            }
        }

        #endregion

        #region Toolbar Commands

        private async void ToolbarAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new AddDialog(_currentPath, _browsingArchive ? _currentArchivePath : null)
                {
                    Owner = this
                };
                if (dlg.ShowDialog() == true)
                {
                    var files = dlg.SelectedFiles;
                    if (files.Count == 0) return;

                    var archivePath = dlg.ArchivePath;
                    var format = dlg.Format;
                    var password = dlg.Password;
                    var level = dlg.CompressionLevel;

                    bool ok = await _archiveService.CreateArchiveAsync(archivePath, files, format, level, password);
                    if (ok)
                    {
                        NavigateTo(Path.GetDirectoryName(archivePath) ?? _currentPath);
                        OpenArchive(archivePath);
                    }
                    else
                    {
                        MessageBox.Show("Failed to create archive.", "xRAR", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "xRAR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ToolbarExtractTo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_browsingArchive)
                {
                    MessageBox.Show("Please open an archive first.", "xRAR", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var selected = FileListView.SelectedItems.Cast<FileSystemItem>()
                    .Where(x => !x.IsDirectory)
                    .Select(x => x.Name)
                    .ToList();
                var dlg = new ExtractDialog(_currentArchivePath, selected.Count > 0 ? selected : null)
                {
                    Owner = this
                };
                if (dlg.ShowDialog() == true)
                {
                    await _archiveService.ExtractAsync(_currentArchivePath, dlg.DestinationPath,
                        selected.Count > 0 ? selected : null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "xRAR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ToolbarTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_browsingArchive)
                {
                    MessageBox.Show("Please open an archive first.", "xRAR", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                var result = await _archiveService.TestArchiveAsync(_currentArchivePath);
                MessageBox.Show(result, "xRAR - Test Results", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "xRAR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToolbarView_Click(object sender, RoutedEventArgs e)
        {
            if (FileListView.SelectedItem is FileSystemItem item && !item.IsDirectory)
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(item.FullPath) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "xRAR", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ToolbarDelete_Click(object sender, RoutedEventArgs e)
        {
            var selected = FileListView.SelectedItems.Cast<FileSystemItem>().ToList();
            if (selected.Count == 0) return;

            if (_browsingArchive)
            {
                MessageBox.Show("Deleting from archives requires RAR.exe with the 'd' command.\nUse the command line: Rar d archive.rar files...",
                    "xRAR", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Delete {selected.Count} item(s)?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm == MessageBoxResult.Yes)
            {
                foreach (var item in selected)
                {
                    try
                    {
                        if (item.IsDirectory)
                            Directory.Delete(item.FullPath, true);
                        else
                            File.Delete(item.FullPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to delete {item.Name}: {ex.Message}", "xRAR", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                NavigateTo(_currentPath);
            }
        }

        private void ToolbarFind_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new FindDialog(_currentPath) { Owner = this };
                dlg.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "xRAR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToolbarWizard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new WizardDialog { Owner = this };
                dlg.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "xRAR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ToolbarInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_browsingArchive)
                {
                    if (FileListView.SelectedItem is FileSystemItem sel)
                    {
                        var dlg = new InfoDialog(sel.FullPath) { Owner = this };
                        dlg.ShowDialog();
                    }
                    return;
                }

                var info = await _archiveService.GetArchiveInfoAsync(_currentArchivePath);
                var dlg2 = new InfoDialog(_currentArchivePath, info) { Owner = this };
                dlg2.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "xRAR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToolbarRepair_Click(object sender, RoutedEventArgs e)
        {
            if (!_browsingArchive)
            {
                MessageBox.Show("Please open an archive first.", "xRAR", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            MessageBox.Show(
                "Repair uses the recovery record embedded in the archive.\n" +
                "Command line: Rar r archive.rar\n" +
                "This requires Rar.exe to be available.",
                "xRAR - Repair", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Menu Commands

        private void MenuOpenArchive_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "Open archive",
                    Filter = "All archives|*.rar;*.zip;*.zipx;*.7z;*.tar;*.gz;*.bz2;*.xz;*.cab;*.arj;*.lzh;*.zst;*.iso;*.001|RAR archives|*.rar|ZIP archives|*.zip;*.zipx|7Z archives|*.7z|All files|*.*",
                    InitialDirectory = _currentPath
                };
                if (dlg.ShowDialog() == true)
                    OpenArchive(dlg.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "xRAR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new SettingsDialog { Owner = this };
                dlg.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "xRAR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new AboutDialog { Owner = this };
                dlg.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "xRAR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ToolbarExtract_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_browsingArchive)
                {
                    MessageBox.Show("Please open an archive first.", "xRAR", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                // Extract to same folder as archive (WinRAR default behavior)
                string destDir = System.IO.Path.GetDirectoryName(_currentArchivePath) ?? "";
                var selected = FileListView.SelectedItems.Cast<FileSystemItem>()
                    .Where(x => !x.IsDirectory)
                    .Select(x => x.Name)
                    .ToList();
                _ = _archiveService.ExtractAsync(_currentArchivePath, destDir,
                    selected.Count > 0 ? selected : null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "xRAR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToolbarComment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_browsingArchive)
                {
                    MessageBox.Show("Please open an archive first.", "xRAR", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                MessageBox.Show("Add archive comment.", "xRAR",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "xRAR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToolbarProtect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_browsingArchive)
                {
                    MessageBox.Show("Please open an archive first.", "xRAR", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                MessageBox.Show("Protecting is supported only for RAR archives.", "xRAR",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "xRAR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToolbarVirusScan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_browsingArchive)
                {
                    MessageBox.Show("Please open an archive first.", "xRAR", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                MessageBox.Show("You need to specify a virus scanner program.", "xRAR",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "xRAR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToolbarConvert_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_browsingArchive)
                {
                    MessageBox.Show("Please open an archive first.", "xRAR", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                MessageBox.Show("Convert archive to self-extracting.", "xRAR",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "xRAR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToolbarLock_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_browsingArchive)
                {
                    MessageBox.Show("Please open an archive first.", "xRAR", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                MessageBox.Show("Locking is supported only for RAR archives.", "xRAR",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "xRAR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToolbarReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_browsingArchive)
                {
                    MessageBox.Show("Please open an archive first.", "xRAR", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                MessageBox.Show("Generate report.", "xRAR",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "xRAR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToolbarBenchmark_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Benchmark.", "xRAR", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ToolbarPrint_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Print file list.", "xRAR", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Context Menu

        private void CtxOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenSelected();
        }

        private void CtxProperties_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (FileListView.SelectedItem is FileSystemItem item)
                {
                    var dlg = new InfoDialog(item.FullPath) { Owner = this };
                    dlg.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "xRAR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Drag and Drop

        private Point _dragStartPoint;

        private void FileListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
        }

        private async void FileListView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _dragStartPoint == default)
                return;

            if (!_browsingArchive) return;

            var pos = e.GetPosition(null);
            var diff = pos - _dragStartPoint;
            if (Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance)
                return;

            var selected = FileListView.SelectedItems.Cast<FileSystemItem>()
                .Where(x => !x.IsDirectory)
                .Select(x => x.Name)
                .ToList();

            if (selected.Count == 0) return;

            _dragStartPoint = default;

            // Extract selected files to a temp dir so Explorer/external apps can receive them
            var tempDir = Path.Combine(Path.GetTempPath(), "xRAR_drag_" + Guid.NewGuid().ToString("N")[..8]);
            try
            {
                Directory.CreateDirectory(tempDir);
                bool ok = await _archiveService.ExtractAsync(_currentArchivePath, tempDir, selected);
                if (!ok)
                {
                    MessageBox.Show("Failed to extract files for drag operation.", "xRAR",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Collect actual extracted file paths
                var extractedFiles = new List<string>();
                foreach (var name in selected)
                {
                    var filePath = Path.Combine(tempDir, name);
                    if (File.Exists(filePath))
                        extractedFiles.Add(filePath);
                    else
                    {
                        // Might be inside a subfolder
                        var found = Directory.GetFiles(tempDir, Path.GetFileName(name), SearchOption.AllDirectories);
                        extractedFiles.AddRange(found);
                    }
                }

                if (extractedFiles.Count == 0) return;

                var data = new DataObject();
                // Set FileDrop so Explorer, Desktop, any app can receive the files
                var fileList = new System.Collections.Specialized.StringCollection();
                foreach (var f in extractedFiles) fileList.Add(f);
                data.SetFileDropList(fileList);
                // Also keep custom format for internal use
                data.SetData("xRAR.ArchivePath", _currentArchivePath);
                data.SetData("xRAR.FileNames", selected.ToArray());

                DragDrop.DoDragDrop(FileListView, data, DragDropEffects.Copy);

                // Schedule temp cleanup after a delay so the drop target has time to copy
                _ = Task.Run(async () =>
                {
                    await Task.Delay(5000);
                    try { Directory.Delete(tempDir, true); } catch { }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Drag error: {ex.Message}", "xRAR", MessageBoxButton.OK, MessageBoxImage.Error);
                try { Directory.Delete(tempDir, true); } catch { }
            }
        }

        private void FileListView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void FileListView_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void FileListView_DragLeave(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private async void FileListView_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
                if (e.Data.GetData(DataFormats.FileDrop) is not string[] files || files.Length == 0) return;

                // Determine target directory
                string targetDir = _currentPath;
                if (string.IsNullOrEmpty(targetDir))
                    targetDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                // If browsing inside an archive, extract dropped files into the archive's folder
                if (_browsingArchive && !string.IsNullOrEmpty(_currentArchivePath))
                {
                    targetDir = Path.GetDirectoryName(_currentArchivePath) ?? targetDir;
                }

                // Filter to only archive files
                var archiveFiles = files.Where(f => FileSystemItem.IsArchiveExtension(Path.GetExtension(f))).ToArray();
                if (archiveFiles.Length == 0) return;

                if (_browsingArchive)
                {
                    // Dropping files INTO an archive — add them
                    foreach (var file in archiveFiles)
                    {
                        await _archiveService.CreateArchiveAsync(_currentArchivePath, new List<string> { file }, "rar", 3, null);
                    }
                    OpenArchive(_currentArchivePath);
                }
                else
                {
                    // Dropping archives onto the file list — open the first one
                    OpenArchive(archiveFiles[0]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Drag-drop error: {ex.Message}", "xRAR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        private void MainToolbar_Loaded(object sender, RoutedEventArgs e)
        {
            // Hide the overflow button and grip for clean WinRAR look
            var tray = MainToolbar.TemplatedParent as FrameworkElement;
            if (MainToolbar.Template.FindName("OverflowGrid", MainToolbar) is UIElement overflow)
                overflow.Visibility = Visibility.Collapsed;
            if (MainToolbar.Template.FindName("MainPanelBorder", MainToolbar) is FrameworkElement border2)
                border2.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");
        }
    }
}
