# C# Implementation Notes

This document records key decisions and technical details related to the C# WPF application implementation.

---

### Decision (C#)
[2025-07-09 21:04:00] - Standardize Bitmap/Image Data Handling for P/Invoke

**Rationale:**
To ensure stable and performant data exchange between the C# application and the C++ DLL, a standardized approach for handling image data is crucial. The chosen method involves converting all loaded images to a consistent pixel format (`Bgra32`) before processing and carefully managing the conversion of the processed data back into a displayable format.

**Details:**
1.  **Input Image Conversion:**
    *   When an image is loaded via the `LoadImage` method in `MainViewModel.cs`, it is immediately converted to a `FormatConvertedBitmap` with `PixelFormats.Bgra32`.
    *   **Reasoning:** This guarantees that the byte array sent to the C++ DLL always has a predictable 4-bytes-per-pixel structure (Blue, Green, Red, Alpha), simplifying the marshalling and C++-side processing. It avoids complexities from handling various source image formats (e.g., indexed color, CMYK, different RGB orderings).
    *   Reference: `MainViewModel.cs`, `LoadImage()` method.

2.  **Output Image Conversion:**
    *   The C++ `ProcessImage_Canny` function returns a single-channel, 8-bit grayscale image (`byte[]`).
    *   This byte array is converted into a `BitmapSource` using `BitmapSource.Create()`.
    *   The `PixelFormats.Gray8` format is specified, which directly maps the single-channel data to a grayscale image.
    *   Reference: `MainViewModel.cs`, `UpdateProcessedImage()` method.

3.  **Performance Optimization (`.Freeze()`):**
    *   The `ProcessedImage` `BitmapSource` has its `.Freeze()` method called immediately after creation.
    *   **Reasoning:** Freezing the object makes it immutable and allows it to be accessed from different threads without requiring synchronization. Since the `ProcessedImage` is generated in the ViewModel and displayed by the UI thread, this is a critical performance optimization that prevents potential cross-thread access exceptions and improves UI responsiveness.

4.  **Error Handling:**
    *   A `try-catch` block is wrapped around the entire `UpdateProcessedImage` logic.
    *   A specific `catch` for `DllNotFoundException` provides a user-friendly error message if the C++ DLL is missing, which is a common issue in P/Invoke scenarios.
    *   A general `catch` for other exceptions prevents the application from crashing due to unexpected errors during processing.
    *   Reference: `MainViewModel.cs`, `UpdateProcessedImage()` method.

---

### Decision (C#)
[2025-07-09 22:24:00] - Implement ROI Mouse Interaction in View's Code-Behind

**Rationale:**
Following the architectural decision outlined in `architecture_overview.md` and `decisionLog.md`, the logic for handling direct user manipulation of the ROI (Region of Interest) rectangle is placed in the `MainWindow.xaml.cs` file. This adheres strictly to the MVVM pattern.

**Details:**
1.  **Responsibility:** The View (`MainWindow.xaml.cs`) is responsible for capturing and interpreting raw mouse events (`MouseDown`, `MouseMove`, `MouseUp`) that occur on the image `Canvas`.
2.  **State Management:** The code-behind manages the temporary state required for the drag operation, such as the starting point (`_startPoint`) and whether a drag is in progress (`_isDragging`).
3.  **Coordinate Handling:** It handles the logic of calculating the rectangle's dimensions based on the mouse position and, crucially, clamps these values to the boundaries of the original image to prevent invalid ROI definitions.
4.  **ViewModel Update:** Upon modification, the code-behind directly updates the `RoiRect` property on the `MainViewModel`. The data binding mechanism ensures the ViewModel is notified, which in turn triggers the `UpdateProcessedImage` method.
5.  **Decoupling:** This approach keeps the `MainViewModel` clean of any UI-specific event handling logic. The ViewModel's only responsibility is to own the `RoiRect` state and react when it changes, making it more portable and testable.
6.  **Reference:** `csharp/ImageAnalysisTool.WPF/Views/MainWindow.xaml.cs`