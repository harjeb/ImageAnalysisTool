using System;
using System.Runtime.InteropServices;

class Program
{
    [DllImport("ImageProcessor.dll", CallingConvention = CallingConvention.Cdecl)]
    static extern int ProcessImage_Canny(
        byte[] inputImageData,
        int width,
        int height,
        byte[] outputImageData,
        out int outputWidth,
        out int outputHeight,
        double threshold1,
        double threshold2,
        int grayscaleThreshold,
        int roiX,
        int roiY,
        int roiWidth,
        int roiHeight
    );

    static void Main()
    {
        try
        {
            // Create a simple test image (100x100 pixels, BGRA format)
            int width = 100;
            int height = 100;
            byte[] inputData = new byte[width * height * 4]; // BGRA format
            
            // Fill with some test data
            for (int i = 0; i < inputData.Length; i += 4)
            {
                inputData[i] = 128;     // B
                inputData[i + 1] = 128; // G
                inputData[i + 2] = 128; // R
                inputData[i + 3] = 255; // A
            }
            
            // Create output buffer
            byte[] outputData = new byte[50 * 50]; // ROI size
            
            // Call the function
            int result = ProcessImage_Canny(
                inputData,
                width,
                height,
                outputData,
                out int outputWidth,
                out int outputHeight,
                50.0,   // threshold1
                150.0,  // threshold2
                128,    // grayscaleThreshold
                25,     // roiX
                25,     // roiY
                50,     // roiWidth
                50      // roiHeight
            );
            
            Console.WriteLine($"Function returned: {result}");
            Console.WriteLine($"Output dimensions: {outputWidth}x{outputHeight}");
            
            if (result == 0)
            {
                Console.WriteLine("SUCCESS: Function call completed successfully");
            }
            else
            {
                Console.WriteLine($"ERROR: Function returned error code {result}");
            }
        }
        catch (DllNotFoundException)
        {
            Console.WriteLine("ERROR: ImageProcessor.dll not found");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
