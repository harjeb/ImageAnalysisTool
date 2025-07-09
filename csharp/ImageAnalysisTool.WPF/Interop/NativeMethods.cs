using System.Runtime.InteropServices;

namespace ImageAnalysisTool.WPF.Interop
{
    internal static class NativeMethods
    {
        private const string DllPath = "ImageProcessor.dll";

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int ProcessImage_Canny(
            byte[] inputImageData,
            byte[] outputImageData,
            int width,
            int height,
            double threshold1,
            double threshold2,
            int grayscaleThreshold,
            int roiX,
            int roiY,
            int roiWidth,
            int roiHeight
        );
    }
}