
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WinForms = System.Windows.Forms;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ImageConvertResize.WPF
{
    public partial class MainWindow : Window
    {
        private CancellationTokenSource? _cts;
        private System.Windows.Threading.DispatcherTimer? _previewTimer;
        private double _originalAspectRatio = 0;
        private bool _isUpdatingDimensions = false;
        private string? _currentImagePath;
        private (string path, int w, int h, int mode)? _lastPreviewParams;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors de l'initialisation XAML:\n\n{ex.GetType().Name}\n{ex.Message}\n\n{ex.StackTrace}", 
                    "Erreur d'initialisation", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }

            FormatComboBox.ItemsSource = new[] { "jpg", "png", "webp", "avif", "bmp", "tiff", "ico" };
            FormatComboBox.SelectedIndex = 0;
            FormatComboBox.SelectionChanged += (_, __) => UpdateQualityEnabled();

            BrowseFileButton.Click += BrowseFileButton_Click;
            BrowseFolderButton.Click += BrowseFolderButton_Click;
            BrowseOutputButton.Click += BrowseOutputButton_Click;
            StartButton.Click += StartButton_Click;
            CancelButton.Click += CancelButton_Click;
            ResetButton.Click += ResetButton_Click;

            _previewTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            _previewTimer.Tick += (_, __) => { _previewTimer.Stop(); RefreshPreview(); };

            // Ne pas attacher TextChanged ici pour éviter les boucles, on le fait dans Dimension_TextChanged
            FormatComboBox.SelectionChanged += (_, __) => RefreshPreview();
            QualityTextBox.TextChanged += (_, __) => RefreshPreview();

            UpdateQualityEnabled();
        }

        private void UpdateQualityEnabled()
        {
            var fmt = (FormatComboBox.SelectedItem?.ToString() ?? "").ToLowerInvariant();
            QualityTextBox.IsEnabled = fmt is "jpg" or "jpeg" or "webp" or "avif";
        }

        // Préréglages de tailles
        private void PresetButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is string tag)
            {
                var parts = tag.Split('x');
                if (parts.Length == 2)
                {
                    _isUpdatingDimensions = true;
                    WidthTextBox.Text = parts[0];
                    HeightTextBox.Text = parts[1];
                    _isUpdatingDimensions = false;
                    
                    _originalAspectRatio = double.Parse(parts[0]) / double.Parse(parts[1]);
                    _previewTimer?.Stop();
                    RefreshPreview();
                }
            }
        }

        // Verrouillage du ratio d'aspect
        private void LockRatio_Changed(object sender, RoutedEventArgs e)
        {
            if (LockRatioCheckBox.IsChecked == true && !string.IsNullOrEmpty(WidthTextBox.Text) && !string.IsNullOrEmpty(HeightTextBox.Text))
            {
                if (int.TryParse(WidthTextBox.Text, out int w) && int.TryParse(HeightTextBox.Text, out int h) && h > 0)
                {
                    _originalAspectRatio = (double)w / h;
                }
            }
        }

        // Gestion des changements de dimensions avec verrouillage du ratio
        private void Dimension_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingDimensions) return;

            if (LockRatioCheckBox.IsChecked == true && _originalAspectRatio > 0)
            {
                _isUpdatingDimensions = true;
                
                if (sender == WidthTextBox && int.TryParse(WidthTextBox.Text, out int w) && w > 0)
                {
                    int newH = (int)Math.Round(w / _originalAspectRatio);
                    HeightTextBox.Text = newH.ToString();
                }
                else if (sender == HeightTextBox && int.TryParse(HeightTextBox.Text, out int h) && h > 0)
                {
                    int newW = (int)Math.Round(h * _originalAspectRatio);
                    WidthTextBox.Text = newW.ToString();
                }
                
                _isUpdatingDimensions = false;
            }

            // Rafraîchir immédiatement l'aperçu
            _previewTimer?.Stop();
            RefreshPreview();
        }

        private void ResizeMode_Changed(object sender, SelectionChangedEventArgs e)
        {
            // Rafraîchir immédiatement quand le mode change
            RefreshPreview();
        }

        private void BrowseFileButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Choisir un fichier image",
                Filter = "Images|*.jpg;*.jpeg;*.png;*.webp;*.avif;*.bmp;*.tiff;*.tif;*.gif|Tous les fichiers|*.*",
                CheckFileExists = true
            };
            if (dlg.ShowDialog(this) == true)
            {
                InputPathTextBox.Text = dlg.FileName;
                LoadPreviewOriginal(dlg.FileName);
            }
        }

        private void BrowseFolderButton_Click(object sender, RoutedEventArgs e)
        {
            using var dlg = new WinForms.FolderBrowserDialog { Description = "Choisir un dossier d'entrée" };
            if (dlg.ShowDialog() == WinForms.DialogResult.OK)
            {
                InputPathTextBox.Text = dlg.SelectedPath;
                PreviewOriginal.Source = null;
                PreviewProcessed.Source = null;
            }
        }

        private void BrowseOutputButton_Click(object sender, RoutedEventArgs e)
        {
            using var dlg = new WinForms.FolderBrowserDialog { Description = "Choisir le dossier de sortie" };
            if (dlg.ShowDialog() == WinForms.DialogResult.OK)
            {
                OutputDirTextBox.Text = dlg.SelectedPath;
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            // Réinitialiser les champs d'entrée/sortie
            InputPathTextBox.Text = string.Empty;
            OutputDirTextBox.Text = "C:\\Mes Documents\\";
            
            // Réinitialiser les dimensions
            _isUpdatingDimensions = true;
            WidthTextBox.Text = "1920";
            HeightTextBox.Text = "1080";
            _isUpdatingDimensions = false;
            _originalAspectRatio = 1920.0 / 1080.0;
            
            // Réinitialiser le mode de redimensionnement
            ResizeModeComboBox.SelectedIndex = 0;
            
            // Réinitialiser le format et la qualité
            FormatComboBox.SelectedIndex = 0; // JPG
            QualityTextBox.Text = "85";
            DpiTextBox.Text = "96";
            
            // Réinitialiser les options
            LockRatioCheckBox.IsChecked = false;
            OverwriteCheckBox.IsChecked = false;
            KeepNameCheckBox.IsChecked = false;
            RecursiveCheckBox.IsChecked = false;
            PreserveExifCheckBox.IsChecked = true;
            AutoRotateCheckBox.IsChecked = true;
            
            // Réinitialiser les aperçus
            PreviewOriginal.Source = null;
            PreviewProcessed.Source = null;
            OriginalPlaceholder.Visibility = Visibility.Visible;
            ProcessedPlaceholder.Visibility = Visibility.Visible;
            OriginalInfoPanel.Visibility = Visibility.Collapsed;
            ProcessedInfoPanel.Visibility = Visibility.Collapsed;
            _currentImagePath = null;
            _lastPreviewParams = null;
            
            // Réinitialiser le statut
            StatusText.Text = "Prêt à traiter vos images";
            StatusSubText.Text = "Cliquez sur Démarrer pour commencer le traitement";
            ProgressBar.Value = 0;
            ExportProgressBar.Value = 0;
            LogTextBox.Text = string.Empty;
            
            AppendLog("✨ Paramètres réinitialisés aux valeurs par défaut");
        }

        // Drag & drop
        private void Preview_DragOver(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop)) e.Effects = System.Windows.DragDropEffects.Copy; else e.Effects = System.Windows.DragDropEffects.None;
            e.Handled = true;
        }
        private void InputPathTextBox_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop)) return;
            var dropped = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop)!;
            if (dropped.Length == 0) return;
            InputPathTextBox.Text = dropped[0];
            if (File.Exists(dropped[0])) LoadPreviewOriginal(dropped[0]); else { PreviewOriginal.Source = null; PreviewProcessed.Source = null; }
        }
        private void PreviewOriginal_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop)) return;
            var dropped = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop)!;
            if (dropped.Length == 0) return;
            InputPathTextBox.Text = dropped[0];
            LoadPreviewOriginal(dropped[0]);
        }

        // Aperçus
        private void LoadPreviewOriginal(string path)
        {
            if (!File.Exists(path)) return;
            
            try
            {
                _currentImagePath = path;
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(path);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.DecodePixelWidth = 600;
                bmp.EndInit();
                bmp.Freeze();
                PreviewOriginal.Source = bmp;

                // Mémoriser le ratio réel de l'image chargée pour le verrouillage
                _originalAspectRatio = bmp.PixelWidth > 0 && bmp.PixelHeight > 0
                    ? (double)bmp.PixelWidth / bmp.PixelHeight
                    : 0;
                
                // Masquer le placeholder
                if (FindName("OriginalPlaceholder") is System.Windows.Controls.TextBlock originalPlaceholder)
                    originalPlaceholder.Visibility = Visibility.Collapsed;
                
                // Afficher les statistiques de l'image originale
                UpdateOriginalImageStats(path, bmp.PixelWidth, bmp.PixelHeight);
                
                // Générer l'aperçu traité
                GeneratePreviewProcessed(path);
            }
            catch
            {
                PreviewOriginal.Source = null;
                PreviewProcessed.Source = null;
                _currentImagePath = null;
                
                // Réafficher les placeholders
                if (FindName("OriginalPlaceholder") is System.Windows.Controls.TextBlock originalPlaceholder)
                    originalPlaceholder.Visibility = Visibility.Visible;
                if (FindName("ProcessedPlaceholder") is System.Windows.Controls.TextBlock processedPlaceholder)
                    processedPlaceholder.Visibility = Visibility.Visible;
                if (FindName("OriginalInfoPanel") is System.Windows.Controls.Border originalInfoPanel)
                    originalInfoPanel.Visibility = Visibility.Collapsed;
                if (FindName("ProcessedInfoPanel") is System.Windows.Controls.Border processedInfoPanel)
                    processedInfoPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateOriginalImageStats(string path, int width, int height)
        {
            try
            {
                var fileInfo = new FileInfo(path);
                var fileSize = FormatFileSize(fileInfo.Length);
                
                if (FindName("OriginalDimensionsText") is System.Windows.Controls.TextBlock dimText)
                    dimText.Text = $"📏 Dimensions : {width} x {height} pixels";
                
                if (FindName("OriginalSizeText") is System.Windows.Controls.TextBlock sizeText)
                    sizeText.Text = $"💾 Taille : {fileSize}";
                
                if (FindName("OriginalInfoPanel") is System.Windows.Controls.Border infoPanel)
                    infoPanel.Visibility = Visibility.Visible;
            }
            catch
            {
                if (FindName("OriginalInfoPanel") is System.Windows.Controls.Border infoPanel)
                    infoPanel.Visibility = Visibility.Collapsed;
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private void GeneratePreviewProcessed(string path)
        {
            if (!File.Exists(path)) return;

            try
            {
                // Lire les paramètres
                int maxW = int.TryParse(WidthTextBox.Text, out var w) && w > 0 ? w : 800;
                int maxH = int.TryParse(HeightTextBox.Text, out var h) && h > 0 ? h : 800;
                int modeIndex = ResizeModeComboBox.SelectedIndex;

                // Vérifier le cache simple
                if (_lastPreviewParams == (path, maxW, maxH, modeIndex))
                    return; // Déjà calculé avec ces paramètres

                using var img = ImageHelper.LoadImage(path).CloneAs<Rgba32>();
                int originalWidth = img.Width;
                int originalHeight = img.Height;

                ImageHelper.AutoOrient(img);

                // Déterminer le mode
                var resizeMode = modeIndex switch
                {
                    1 => SixLabors.ImageSharp.Processing.ResizeMode.Crop,
                    2 => SixLabors.ImageSharp.Processing.ResizeMode.Stretch,
                    _ => SixLabors.ImageSharp.Processing.ResizeMode.Max
                };

                // Calculer la taille cible
                int finalW = maxW;
                int finalH = maxH;

                if (resizeMode == SixLabors.ImageSharp.Processing.ResizeMode.Max)
                {
                    double scale = Math.Min((double)maxW / img.Width, (double)maxH / img.Height);
                    finalW = Math.Max(1, (int)Math.Round(img.Width * scale));
                    finalH = Math.Max(1, (int)Math.Round(img.Height * scale));
                }

                // Appliquer le redimensionnement avec Box (plus rapide que Lanczos3)
                img.Mutate(x => x.Resize(new SixLabors.ImageSharp.Processing.ResizeOptions
                {
                    Mode = resizeMode,
                    Size = new SixLabors.ImageSharp.Size(finalW, finalH),
                    Sampler = SixLabors.ImageSharp.Processing.KnownResamplers.Box,
                    Position = SixLabors.ImageSharp.Processing.AnchorPositionMode.Center
                }));

                // Construire le BitmapImage depuis un buffer en mémoire
                byte[] pngData;
                using (var ms = new MemoryStream())
                {
                    img.SaveAsPng(ms);
                    pngData = ms.ToArray();
                }

                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = new MemoryStream(pngData);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();

                PreviewProcessed.Source = bmp;

                if (FindName("ProcessedPlaceholder") is System.Windows.Controls.TextBlock placeholder)
                    placeholder.Visibility = Visibility.Collapsed;

                UpdateProcessedImageStats(originalWidth, originalHeight, finalW, finalH);
                
                // Mémoriser le cache
                _lastPreviewParams = (path, maxW, maxH, modeIndex);
            }
            catch (Exception ex)
            {
                PreviewProcessed.Source = null;
                if (FindName("ProcessedInfoPanel") is System.Windows.Controls.Border panel)
                    panel.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateProcessedImageStats(int origWidth, int origHeight, int newWidth, int newHeight)
        {
            try
            {
                var reductionPercent = 100.0 - ((double)(newWidth * newHeight) / (origWidth * origHeight) * 100.0);
                
                if (FindName("ProcessedDimensionsText") is System.Windows.Controls.TextBlock dimText)
                    dimText.Text = $"📏 Nouvelles dimensions : {newWidth} x {newHeight} pixels";
                
                if (FindName("ProcessedReductionText") is System.Windows.Controls.TextBlock reductionText)
                {
                    if (reductionPercent > 0)
                        reductionText.Text = $"📉 Réduction : {reductionPercent:F1}% (pixels)";
                    else
                        reductionText.Text = "✅ Aucun redimensionnement nécessaire";
                }
                
                if (FindName("ProcessedInfoPanel") is System.Windows.Controls.Border infoPanel)
                    infoPanel.Visibility = Visibility.Visible;
            }
            catch
            {
                if (FindName("ProcessedInfoPanel") is System.Windows.Controls.Border infoPanel)
                    infoPanel.Visibility = Visibility.Collapsed;
            }
        }
        private void RefreshPreview()
        {
            if (!IsLoaded) return;
            try
            {
                // Chercher l'image à afficher
                string imagePath = null;
                
                // D'abord utiliser _currentImagePath si disponible
                if (!string.IsNullOrEmpty(_currentImagePath) && File.Exists(_currentImagePath))
                    imagePath = _currentImagePath;
                // Sinon essayer InputPathTextBox (si initialisé)
                else if (InputPathTextBox != null && !string.IsNullOrEmpty(InputPathTextBox.Text) && File.Exists(InputPathTextBox.Text))
                    imagePath = InputPathTextBox.Text;
                
                // Si on a une image, générer l'aperçu
                if (!string.IsNullOrEmpty(imagePath))
                {
                    GeneratePreviewProcessed(imagePath);
                }
                else
                {
                    // Pas d'image disponible
                    if (PreviewProcessed != null)
                        PreviewProcessed.Source = null;
                    if (FindName("ProcessedInfoPanel") is System.Windows.Controls.Border panel)
                        panel.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RefreshPreview error: {ex.Message}");
            }
        }

        // Lot
        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            var inputPath = InputPathTextBox.Text.Trim();
            var outputDir = OutputDirTextBox.Text.Trim();
            if (string.IsNullOrEmpty(inputPath) || (!File.Exists(inputPath) && !Directory.Exists(inputPath)))
            { System.Windows.MessageBox.Show(this, "Entrez un chemin d'entrée valide (fichier ou dossier).", "Entrée invalide", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (string.IsNullOrEmpty(outputDir))
            { System.Windows.MessageBox.Show(this, "Choisissez un dossier de sortie.", "Sortie manquante", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            
            // Créer un sous-dossier "image converter" par défaut
            outputDir = Path.Combine(outputDir, "image converter");
            Directory.CreateDirectory(outputDir);
            if (!TryParsePositiveInt(WidthTextBox.Text, out int maxW)) { WarnInvalid("Largeur max"); return; }
            if (!TryParsePositiveInt(HeightTextBox.Text, out int maxH)) { WarnInvalid("Hauteur max"); return; }
            var format = (FormatComboBox.SelectedItem?.ToString() ?? "jpg").ToLowerInvariant();
            int quality = 85; if (QualityTextBox.IsEnabled && !TryParsePositiveInt(QualityTextBox.Text, out quality)) { WarnInvalid("Qualité"); return; }
            int dpi; if (!TryParsePositiveInt(DpiTextBox.Text, out dpi)) { WarnInvalid("DPI"); return; }
            bool overwrite = OverwriteCheckBox.IsChecked == true;
            bool keepName = KeepNameCheckBox.IsChecked == true;
            bool recursive = RecursiveCheckBox.IsChecked == true;

            var files = ImageHelper.GetInputFiles(inputPath, recursive).ToList();
            if (files.Count == 0) { System.Windows.MessageBox.Show(this, "Aucune image trouvée à traiter.", "Info", MessageBoxButton.OK, MessageBoxImage.Information); return; }

            ToggleUi(false);
            LogTextBox.Clear();
            ProgressBar.Value = 0;
            _cts = new CancellationTokenSource();

            AppendLog($"📁 {files.Count} fichier(s) à traiter.");
            AppendLog($"→ Sortie : {outputDir}");
            AppendLog("");
            
            // Mise à jour du statut visuel
            if (FindName("StatusText") is System.Windows.Controls.TextBlock statusText)
                statusText.Text = $"Traitement en cours... ({files.Count} fichier(s))";
            if (FindName("StatusSubText") is System.Windows.Controls.TextBlock statusSubText)
                statusSubText.Text = "Veuillez patienter pendant le traitement des images";

            var progress = new Progress<(int current, int total, string message)>(p =>
            {
                ProgressBar.Value = Math.Min(100, (int)(p.current * 100.0 / Math.Max(1, p.total)));
                if (!string.IsNullOrWhiteSpace(p.message)) AppendLog(p.message);
            });

            try
            {
                var result = await Task.Run(() => ProcessFiles(files, outputDir, maxW, maxH, format, quality, dpi, overwrite, keepName, _cts!.Token, progress), _cts.Token);
                AppendLog("");
                AppendLog($"✅ Terminé. Succès: {result.ok}, Échecs: {result.fail}");
                System.Windows.MessageBox.Show(this, $"Terminé.\nSuccès : {result.ok}\nÉchecs : {result.fail}", "Fin du traitement", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (OperationCanceledException) 
            { 
                AppendLog("⛔ Traitement annulé."); 
                if (FindName("StatusText") is System.Windows.Controls.TextBlock st)
                    st.Text = "Traitement annulé";
            }
            catch (Exception ex) 
            { 
                AppendLog("❌ Erreur inattendue : " + ex.Message); 
                System.Windows.MessageBox.Show(this, ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error); 
                if (FindName("StatusText") is System.Windows.Controls.TextBlock st)
                    st.Text = "Erreur lors du traitement";
            }
            finally 
            { 
                ToggleUi(true); 
                _cts?.Dispose(); 
                _cts = null; 
                
                // Restaurer le statut par défaut
                if (FindName("StatusText") is System.Windows.Controls.TextBlock statusTextBlock)
                    statusTextBlock.Text = "Prêt à traiter vos images";
                if (FindName("StatusSubText") is System.Windows.Controls.TextBlock statusSubTextBlock)
                    statusSubTextBlock.Text = "Cliquez sur Démarrer pour commencer le traitement";
            }

            void WarnInvalid(string field) => System.Windows.MessageBox.Show(this, $"{field} invalide.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => _cts?.Cancel();

        // Gestionnaires du menu Aide
        private void MenuItem_UserGuide_Click(object sender, RoutedEventArgs e)
        {
            var helpWindow = new HelpWindow();
            helpWindow.Owner = this;
            helpWindow.ShowDialog();
        }

        private void MenuItem_About_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show(
                "Image Converter Pro v1.0.0\n\n" +
                "Application professionnelle de conversion et redimensionnement d'images\n\n" +
                "Technologies : .NET 8, WPF, SixLabors.ImageSharp, ComputeSharp\n" +
                "GPU : DirectX 12 (fallback WARP)\n\n" +
                "© 2025 Image Converter Pro\n" +
                "Auteur: C.L (Skill teams)\n" +
                "Licence : MIT",
                "À propos d'Image Converter Pro",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void ToggleUi(bool enable)
        {
            InputPathTextBox.IsEnabled = enable;
            BrowseFileButton.IsEnabled = enable;
            BrowseFolderButton.IsEnabled = enable;
            OutputDirTextBox.IsEnabled = enable;
            BrowseOutputButton.IsEnabled = enable;
            WidthTextBox.IsEnabled = enable;
            HeightTextBox.IsEnabled = enable;
            FormatComboBox.IsEnabled = enable;
            QualityTextBox.IsEnabled = enable && (FormatComboBox.SelectedItem?.ToString() is "jpg" or "webp" or "avif");
            DpiTextBox.IsEnabled = enable;
            OverwriteCheckBox.IsEnabled = enable;
            KeepNameCheckBox.IsEnabled = enable;
            RecursiveCheckBox.IsEnabled = enable;
            StartButton.IsEnabled = enable;
            CancelButton.IsEnabled = !enable;
        }
        private void AppendLog(string line) { LogTextBox.AppendText(line + Environment.NewLine); LogTextBox.ScrollToEnd(); }
        private static bool TryParsePositiveInt(string? s, out int v) { if (int.TryParse(s, out v) && v > 0) return true; v = 0; return false; }

        // Traitement parallèle + GPU/CPU
        private (int ok, int fail) ProcessFiles(
            List<string> files,
            string outputDir,
            int maxW,
            int maxH,
            string format,
            int quality,
            int dpi,
            bool overwrite,
            bool keepName,
            CancellationToken ct,
            IProgress<(int current, int total, string message)> progress)
        {
            int ok = 0, fail = 0, done = 0;
            int total = files.Count;
            object lockLog = new(), lockCounter = new();

            bool useGpu = GpuResizer.IsAvailable;
            var useEncoder = format != "ico";
            var encoder = useEncoder ? ImageHelper.GetEncoder(format, quality) : null;
            int[] icoSizes = new[] { 16, 32, 48, 64, 128, 256 };

            System.Threading.Tasks.Parallel.ForEach(
                files,
                new ParallelOptions { CancellationToken = ct, MaxDegreeOfParallelism = Environment.ProcessorCount },
                file =>
                {
                    ct.ThrowIfCancellationRequested();
                    string msgPrefix;
                    try
                    {
                        using var baseImg = ImageHelper.LoadImage(file).CloneAs<Rgba32>();
                        ImageHelper.AutoOrient(baseImg);
                        baseImg.Metadata.HorizontalResolution = dpi;
                        baseImg.Metadata.VerticalResolution = dpi;

                        var target = ImageHelper.ComputeTargetSize(baseImg.Width, baseImg.Height, maxW, maxH);
                        string name = System.IO.Path.GetFileNameWithoutExtension(file);

                        if (format == "ico")
                        {
                            string outName = keepName ? $"{name}.ico" : $"{name}_icon.ico";
                            string outPath = System.IO.Path.Combine(outputDir, outName);
                            if (File.Exists(outPath) && !overwrite)
                            {
                                lock (lockCounter) ok++;
                                lock (lockLog) { msgPrefix = $"[{done + 1}/{total}]"; progress.Report((done, total, $"{msgPrefix} ⏭️ Existe déjà : {outName}")); }
                            }
                            else
                            {
                                var pngBlobs = new Dictionary<int, byte[]>();
                                foreach (var sz in icoSizes)
                                {
                                    ct.ThrowIfCancellationRequested();
                                    byte[] png = useGpu ? GpuResizer.ResizePadToPng(baseImg, sz) : CpuPadToPng(baseImg, sz);
                                    pngBlobs[sz] = png;
                                }
                                Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
                                using var fs = File.Open(outPath, FileMode.Create, FileAccess.Write);
                                IcoHelper.WriteIcoFromPngBlobs(pngBlobs, fs);
                                lock (lockCounter) ok++;
                                lock (lockLog) { msgPrefix = $"[{done + 1}/{total}]"; progress.Report((done, total, $"{msgPrefix} 🟦 ICO → {outName}")); }
                            }
                        }
                        else
                        {
                            using Image<Rgba32> outImg = useGpu ? GpuResizer.Resize(baseImg, target.width, target.height) : CpuResize(baseImg, target.width, target.height);
                            string outName = keepName ? $"{name}.{format}" : $"{name}_{target.width}x{target.height}.{format}";
                            string outPath = Path.Combine(outputDir, outName);
                            if (File.Exists(outPath) && !overwrite)
                            {
                                lock (lockCounter) ok++;
                                lock (lockLog) { msgPrefix = $"[{done + 1}/{total}]"; progress.Report((done, total, $"{msgPrefix} ⏭️ Existe déjà : {outName}")); }
                            }
                            else
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
                                using var fs = File.Open(outPath, FileMode.Create, FileAccess.Write);
                                
                                // Utiliser AvifHelper pour AVIF, sinon ImageSharp
                                if (format == "avif")
                                {
                                    AvifHelper.SaveAvif(outImg, fs, quality);
                                }
                                else
                                {
                                    outImg.Save(fs, encoder!);
                                }
                                
                                lock (lockCounter) ok++;
                                lock (lockLog) { msgPrefix = $"[{done + 1}/{total}]"; progress.Report((done, total, $"{msgPrefix} ✅ {Path.GetFileName(file)} → {outName}")); }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (lockCounter) fail++;
                        lock (lockLog)
                        {
                            msgPrefix = $"[{done + 1}/{total}]";
                            progress.Report((done, total, $"{msgPrefix} ❌ {Path.GetFileName(file)} : {ex.Message}"));
                        }
                    }
                    finally
                    {
                        lock (lockCounter)
                        {
                            done++;
                            progress.Report((done, total, string.Empty));
                        }
                    }
                });

            return (ok, fail);

            static Image<Rgba32> CpuResize(Image<Rgba32> src, int w, int h)
            {
                var resized = src.Clone(x => x.Resize(new SixLabors.ImageSharp.Processing.ResizeOptions
                {
                    Mode = SixLabors.ImageSharp.Processing.ResizeMode.Max,
                    Size = new SixLabors.ImageSharp.Size(w, h),
                    Sampler = SixLabors.ImageSharp.Processing.KnownResamplers.Lanczos3
                }));
                return resized;
            }
            static byte[] CpuPadToPng(Image<Rgba32> src, int size)
            {
                var target = ImageHelper.ComputeTargetSize(src.Width, src.Height, size, size);
                using var resized = src.Clone(x => x.Resize(new SixLabors.ImageSharp.Processing.ResizeOptions
                {
                    Mode = SixLabors.ImageSharp.Processing.ResizeMode.Max,
                    Size = new SixLabors.ImageSharp.Size(target.width, target.height),
                    Sampler = SixLabors.ImageSharp.Processing.KnownResamplers.Lanczos3
                }));
                using var canvas = new Image<Rgba32>(size, size);
                int ox = (size - resized.Width) / 2; int oy = (size - resized.Height) / 2;
                canvas.Mutate(c => c.DrawImage(resized, new SixLabors.ImageSharp.Point(ox, oy), 1f));
                using var ms = new MemoryStream(); canvas.SaveAsPng(ms); return ms.ToArray();
            }
        }

        // Export aperçu (PNG/JPG/WebP/TIFF/BMP/ICO)
        private async void SavePreviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (PreviewProcessed.Source == null)
            { System.Windows.MessageBox.Show("Aucune image traitée à enregistrer.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Enregistrer l’image traitée",
                Filter = "PNG (*.png)|*.png|JPEG (*.jpg)|*.jpg|WebP (*.webp)|*.webp|AVIF (*.avif)|*.avif|TIFF (*.tiff)|*.tiff|BMP (*.bmp)|*.bmp|ICO (*.ico)|*.ico",
                FileName = "image_traitée"
            };
            if (dlg.ShowDialog() != true) return;
            ExportProgressBar.Value = 0; SavePreviewButton.IsEnabled = false;
            try
            {
                var pngStream = new MemoryStream();
                var bmpEncoder = new PngBitmapEncoder();
                bmpEncoder.Frames.Add(BitmapFrame.Create((BitmapSource)PreviewProcessed.Source));
                bmpEncoder.Save(pngStream); pngStream.Position = 0;
                var ext = System.IO.Path.GetExtension(dlg.FileName).ToLowerInvariant();
                if (ext == ".ico")
                {
                    var sizes = new[] { 16, 32, 48, 64, 128, 256 };
                    await Task.Run(() =>
                    {
                        var pngBlobs = new Dictionary<int, byte[]>(); int step = 0;
                        foreach (var sz in sizes)
                        {
                            pngStream.Position = 0;
                            using var img = SixLabors.ImageSharp.Image.Load<Rgba32>(pngStream);
                            img.Mutate(x => x.Resize(new SixLabors.ImageSharp.Processing.ResizeOptions
                            {
                                Mode = SixLabors.ImageSharp.Processing.ResizeMode.Pad,
                                Size = new SixLabors.ImageSharp.Size(sz, sz),
                                Sampler = SixLabors.ImageSharp.Processing.KnownResamplers.Lanczos3
                            }));
                            using var msPng = new MemoryStream(); img.SaveAsPng(msPng); pngBlobs[sz] = msPng.ToArray();
                            step++; int progress = (int)(step * 80.0 / sizes.Length);
                            Dispatcher.Invoke(() => ExportProgressBar.Value = progress);
                        }
                        using var fs = File.Open(dlg.FileName, FileMode.Create, FileAccess.Write);
                        IcoHelper.WriteIcoFromPngBlobs(pngBlobs, fs);
                        Dispatcher.Invoke(() => ExportProgressBar.Value = 100);
                    });
                }
                else
                {
                    await Task.Run(() =>
                    {
                        using var img = SixLabors.ImageSharp.Image.Load<Rgba32>(pngStream);
                        var format = ext.TrimStart('.');
                        using var fs = File.Open(dlg.FileName, FileMode.Create, FileAccess.Write);
                        
                        // Gérer AVIF séparément
                        if (format == "avif")
                        {
                            AvifHelper.SaveAvif(img, fs, 90);
                        }
                        else
                        {
                            var encoder = ImageHelper.GetEncoder(format, 90);
                            img.Save(fs, encoder!);
                        }
                        
                        Dispatcher.Invoke(() => ExportProgressBar.Value = 100);
                    });
                }
                System.Windows.MessageBox.Show("Image enregistrée avec succès.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) { System.Windows.MessageBox.Show($"Erreur lors de l'enregistrement : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error); }
            finally { SavePreviewButton.IsEnabled = true; await Task.Delay(300); ExportProgressBar.Value = 0; }
        }

        private void OpenPreviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (PreviewProcessed.Source == null)
            {
                System.Windows.MessageBox.Show("Aucun aperçu à afficher.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var window = new Window
            {
                Title = "Aperçu - Image Traitée",
                Width = 1000,
                Height = 700,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Background = System.Windows.Media.Brushes.White
            };

            var scrollViewer = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            var image = new System.Windows.Controls.Image
            {
                Source = PreviewProcessed.Source,
                Stretch = System.Windows.Media.Stretch.None,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                VerticalAlignment = System.Windows.VerticalAlignment.Top
            };

            scrollViewer.Content = image;
            window.Content = scrollViewer;
            window.Show();
        }
    }
}
