using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ImageAnalysisTool.WPF.ViewModels;

namespace ImageAnalysisTool.WPF.Views
{
    public partial class MainWindow : Window
    {
        private bool _isDragging;
        private Point _startPoint;
        private RoiAdorner? _roiAdorner;

        public MainWindow()
        {
            InitializeComponent();
        }

        private MainViewModel? GetViewModel() => DataContext as MainViewModel;

        private void ImageCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var canvas = sender as Canvas;
            if (canvas == null || GetViewModel()?.OriginalImage == null) return;

            _startPoint = e.GetPosition(canvas);
            _isDragging = true;
            canvas.CaptureMouse();
        }

        private void ImageCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging) return;

            var canvas = sender as Canvas;
            var viewModel = GetViewModel();
            if (canvas == null || viewModel?.OriginalImage == null) return;

            var currentPoint = e.GetPosition(canvas);

            var x = Math.Min(_startPoint.X, currentPoint.X);
            var y = Math.Min(_startPoint.Y, currentPoint.Y);
            var width = Math.Abs(_startPoint.X - currentPoint.X);
            var height = Math.Abs(_startPoint.Y - currentPoint.Y);

            // Clamp to image boundaries
            x = Math.Max(0, x);
            y = Math.Max(0, y);
            width = Math.Min(viewModel.OriginalImage.PixelWidth - x, width);
            height = Math.Min(viewModel.OriginalImage.PixelHeight - y, height);

            viewModel.RoiRect = new Rect(x, y, width, height);
            UpdateRoiAdorner(canvas, viewModel.RoiRect);
        }

        private void ImageCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDragging) return;

            var canvas = sender as Canvas;
            var viewModel = GetViewModel();
            if (canvas == null || viewModel?.OriginalImage == null)
            {
                _isDragging = false;
                canvas?.ReleaseMouseCapture();
                return;
            }

            canvas.ReleaseMouseCapture();
            _isDragging = false;

            // Final update to the RoiRect property
            var currentPoint = e.GetPosition(canvas);
            var x = Math.Min(_startPoint.X, currentPoint.X);
            var y = Math.Min(_startPoint.Y, currentPoint.Y);
            var width = Math.Abs(_startPoint.X - currentPoint.X);
            var height = Math.Abs(_startPoint.Y - currentPoint.Y);

            x = Math.Max(0, x);
            y = Math.Max(0, y);
            width = Math.Min(viewModel.OriginalImage.PixelWidth - x, width);
            height = Math.Min(viewModel.OriginalImage.PixelHeight - y, height);

            // This is the crucial step: setting the property will trigger the update in the ViewModel.
            viewModel.RoiRect = new Rect(x, y, width, height);

            UpdateRoiAdorner(canvas, viewModel.RoiRect);
        }

        private void UpdateRoiAdorner(Canvas canvas, Rect roiRect)
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
            if (adornerLayer == null) return;

            // Remove the old adorner if it exists
            if (_roiAdorner != null)
            {
                adornerLayer.Remove(_roiAdorner);
                _roiAdorner = null;
            }

            // Add the new adorner if the ROI is valid
            if (roiRect.Width > 0 && roiRect.Height > 0)
            {
                _roiAdorner = new RoiAdorner(canvas, roiRect);
                adornerLayer.Add(_roiAdorner);
            }
        }
    }

    // Adorner for drawing corner handles on the ROI
    public class RoiAdorner : Adorner
    {
        private readonly Rect _roiRect;
        private const double HandleSize = 8.0;

        public RoiAdorner(UIElement adornedElement, Rect roiRect) : base(adornedElement)
        {
            _roiRect = roiRect;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var renderBrush = new SolidColorBrush(Colors.Green);
            renderBrush.Freeze(); // Performance optimization
            var renderPen = new Pen(renderBrush, 2.0);
            renderPen.Freeze();

            double halfHandle = HandleSize / 2;

            // Top-left corner
            drawingContext.DrawRectangle(renderBrush, null, new Rect(_roiRect.TopLeft.X - halfHandle, _roiRect.TopLeft.Y - halfHandle, HandleSize, HandleSize));
            // Top-right corner
            drawingContext.DrawRectangle(renderBrush, null, new Rect(_roiRect.TopRight.X - halfHandle, _roiRect.TopRight.Y - halfHandle, HandleSize, HandleSize));
            // Bottom-left corner
            drawingContext.DrawRectangle(renderBrush, null, new Rect(_roiRect.BottomLeft.X - halfHandle, _roiRect.BottomLeft.Y - halfHandle, HandleSize, HandleSize));
            // Bottom-right corner
            drawingContext.DrawRectangle(renderBrush, null, new Rect(_roiRect.BottomRight.X - halfHandle, _roiRect.BottomRight.Y - halfHandle, HandleSize, HandleSize));
        }
    }
}