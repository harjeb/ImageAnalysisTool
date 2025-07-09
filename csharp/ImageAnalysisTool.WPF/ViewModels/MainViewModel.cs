using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageAnalysisTool.WPF.Interop;
using Microsoft.Win32;

namespace ImageAnalysisTool.WPF.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private BitmapSource? _originalImage;
        public BitmapSource? OriginalImage
        {
            get => _originalImage;
            set
            {
                if (SetField(ref _originalImage, value))
                {
                    UpdateProcessedImage();
                }
            }
        }

        private BitmapSource? _processedImage;
        public BitmapSource? ProcessedImage
        {
            get => _processedImage;
            set => SetField(ref _processedImage, value);
        }

        private double _threshold1 = 50;
        public double Threshold1
        {
            get => _threshold1;
            set
            {
                if (SetField(ref _threshold1, value))
                {
                    UpdateProcessedImage();
                }
            }
        }

        private double _threshold2 = 150;
        public double Threshold2
        {
            get => _threshold2;
            set
            {
                if (SetField(ref _threshold2, value))
                {
                    UpdateProcessedImage();
                }
            }
        }

        private int _grayscaleThreshold = 128;
        public int GrayscaleThreshold
        {
            get => _grayscaleThreshold;
            set
            {
                if (SetField(ref _grayscaleThreshold, value))
                {
                    UpdateProcessedImage();
                }
            }
        }

        private Rect _roiRect;
        public Rect RoiRect
        {
            get => _roiRect;
            set
            {
                if (_roiRect == value) return;
                _roiRect = value;
                OnPropertyChanged();
                UpdateProcessedImage();
            }
        }

        public ICommand LoadImageCommand { get; }
        public ICommand MaximizeRoiCommand { get; }

        public MainViewModel()
        {
            LoadImageCommand = new RelayCommand(LoadImage);
            MaximizeRoiCommand = new RelayCommand(MaximizeRoi, () => OriginalImage != null);
        }

        private void LoadImage()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg;*.bmp)|*.png;*.jpeg;*.jpg;*.bmp|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var uri = new Uri(openFileDialog.FileName);
                    // Load the image into a format that's easy to work with (Bgra32)
                    var bitmap = new BitmapImage(uri);
                    OriginalImage = new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0);
                    // Reset ROI when a new image is loaded
                    MaximizeRoi();
                }
                catch (Exception ex)
                {
                    // Handle potential file loading errors
                    System.Windows.MessageBox.Show($"Failed to load image: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    OriginalImage = null;
                }
            }
        }

        private void MaximizeRoi()
        {
            if (OriginalImage != null)
            {
                // Set the ROI to the full image size.
                // This will trigger an update via the RoiRect setter.
                RoiRect = new Rect(0, 0, OriginalImage.PixelWidth, OriginalImage.PixelHeight);
            }
        }

        private void UpdateProcessedImage()
        {
            if (OriginalImage == null || RoiRect.Width <= 0 || RoiRect.Height <= 0)
            {
                ProcessedImage = null;
                return;
            }

            try
            {
                int fullWidth = OriginalImage.PixelWidth;
                int fullHeight = OriginalImage.PixelHeight;
                int fullStride = fullWidth * 4; // 4 bytes per pixel for Bgra32
                byte[] inputBytes = new byte[fullHeight * fullStride];
                OriginalImage.CopyPixels(inputBytes, fullStride, 0);

                // The output buffer size now depends on the ROI size.
                // The C++ code will tell us the exact output dimensions.
                int outputWidth, outputHeight;
                byte[] outputBytes = new byte[(int)(RoiRect.Width * RoiRect.Height)];

                // Call the updated C++ function with all parameters
                NativeMethods.ProcessImage_Canny(
                    inputBytes,
                    fullWidth,
                    fullHeight,
                    outputBytes,
                    out outputWidth,
                    out outputHeight,
                    Threshold1,
                    Threshold2,
                    GrayscaleThreshold,
                    (int)RoiRect.X,
                    (int)RoiRect.Y,
                    (int)RoiRect.Width,
                    (int)RoiRect.Height
                );

                if (outputWidth > 0 && outputHeight > 0)
                {
                    // Convert the grayscale byte array back to a BitmapSource
                    ProcessedImage = BitmapSource.Create(
                        outputWidth,
                        outputHeight,
                        OriginalImage.DpiX,
                        OriginalImage.DpiY,
                        PixelFormats.Gray8,
                        null,
                        outputBytes,
                        outputWidth // Stride for grayscale is just the width
                    );
                    ProcessedImage.Freeze(); // Freeze for performance, as it's used across threads
                }
                else
                {
                    ProcessedImage = null;
                }
            }
            catch (DllNotFoundException)
            {
                System.Windows.MessageBox.Show("Error: ImageProcessor.dll not found.\nPlease ensure the C++ project has been built and the DLL is in the correct location.", "DLL Load Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                ProcessedImage = null;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An error occurred during image processing: {ex.Message}", "Processing Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                ProcessedImage = null;
            }
        }
    }

    // Basic RelayCommand implementation
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute();

        public void Execute(object? parameter) => _execute();
    }
}