# C++ Implementation Notes

This document records specific details and decisions made during the implementation of the C++ `ImageProcessor` DLL.

---

### [2025-07-09 20:33:41] - Initial Project Setup & OpenCV Configuration

**Decision:**
The Visual Studio project files (`ImageProcessor.vcxproj`, `ImageProcessor.sln`) were created manually instead of relying on Visual Studio to generate them.

**Rationale:**
This approach provides explicit, version-controllable definitions of the build configuration. It makes project setup independent of a specific Visual Studio installation and ensures that anyone can build the project with the correct settings by simply having the MSBuild tools.

**Details:**
- **OpenCV Path:** The path to the OpenCV library (`D:\work\xreal\OpenCV454`) has been hard-coded into the `.vcxproj` file for both include and library directories. This is a temporary measure for initial setup. For a more robust solution, this should be replaced with an environment variable or a property sheet (`.props` file).
- **OpenCV Libraries:** The project links against `opencv_world454d.lib` for Debug builds and `opencv_world454.lib` for Release builds. Using the "world" library simplifies linking by including all OpenCV modules in a single file.
- **Preprocessor Definition:** `IMAGEPROCESSOR_EXPORTS` is defined in the project settings, which correctly triggers the `__declspec(dllexport)` macro in `ImageProcessor.h`, making the C-style functions visible to external callers like a C# application.

---

### [2025-07-09 20:33:41] - Image Data Handling

**Decision:**
The `ProcessImage_Canny` function handles raw `unsigned char*` pointers for image data.

**Rationale:**
This is the most direct and efficient way to interface with C# via P/Invoke. C# can easily pin a `byte[]` array in memory and pass its pointer to the C++ function, avoiding complex marshalling or data copying across the managed/unmanaged boundary.

**Details:**
- **Input Format:** The function expects the input data (`inputImageData`) to be in a 4-channel BGRA format, which is a common format for `BitmapSource` in WPF.
- **Output Format:** The output (`outputImageData`) is a single-channel grayscale image, which is the direct result of the Canny algorithm. The C# client will be responsible for converting this grayscale byte array back into a displayable `BitmapSource`.
- **Memory Safety:** The implementation uses `memcpy` to copy the final Canny edge data into the output buffer. It relies on the calling C# code to allocate a sufficiently large buffer for both the input and output images. The size of the output buffer should be `width * height` bytes.

---

### [2025-07-09 22:19:12] - ROI (Region of Interest) Handling and Bounds Checking

**Decision:**
Implemented robust bounds checking for the Region of Interest (ROI) to prevent crashes or undefined behavior from invalid ROI coordinates.

**Rationale:**
The `ProcessImage_Canny` function now accepts ROI parameters (`roiX`, `roiY`, `roiWidth`, `roiHeight`) from the client. It is crucial that the C++ DLL does not trust these inputs blindly. An invalid ROI (e.g., one that extends beyond the source image dimensions) could cause OpenCV to throw an exception or attempt to access invalid memory. To ensure stability, a defensive check is necessary.

**Details:**
- A `cv::Rect` is created from the incoming ROI parameters.
- The intersection of this `cv::Rect` and the image's bounding rectangle (`cv::Rect(0, 0, width, height)`) is calculated using the bitwise AND operator (`&=`). This operation effectively clamps the ROI to be entirely within the valid image area.
- A check is performed to ensure the resulting `roiRect` has a positive width and height. If the ROI is completely outside the image, the resulting intersection will have zero or negative dimensions. In this case, the function returns a specific error code (`-2`) to signal an invalid ROI to the caller.
- All subsequent processing (grayscale conversion, thresholding, Canny) is performed only on this validated `roiImage`.