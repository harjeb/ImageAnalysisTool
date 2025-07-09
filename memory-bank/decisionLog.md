# Decision Log

This file records architectural and implementation decisions using a list format.
2025-07-09 20:28:54 - Log of updates made.

*

---
### Decision
[2025-07-09 20:28:54] - Adopt a two-project structure (C# WPF App, C++ DLL) and the MVVM pattern for the WPF application.

**Rationale:**
This separation isolates the user interface from the core image processing logic. C++ is chosen for performance-critical image processing with OpenCV, while C# WPF is ideal for building a modern desktop UI. The MVVM pattern is a standard for WPF that enhances testability, maintainability, and separation of concerns between the UI (View), the UI logic (ViewModel), and the data (Model).

**Implications/Details:**
- The C# project will handle all UI elements, user interactions, and application state management.
- The C++ project will contain all OpenCV-dependent code and will be compiled into a DLL.
- Communication between the two projects will be handled via P/Invoke, requiring a stable C-style API exported from the DLL.

---
### 2025-07-09: 功能增强 - ROI 与灰度阈值

**决策:**
1.  **C++/C# 接口变更**:
    *   **决定**: 修改 `ProcessImage_Canny` 函数签名，增加 `grayscaleThreshold` 和 ROI (`roiX`, `roiY`, `roiWidth`, `roiHeight`) 参数。
    *   **理由**: 这是将新的控制参数从 UI (C#) 传递到后端处理逻辑 (C++) 的最直接方法。避免了复杂的结构体或多次调用，保持了接口的扁平化和高效性。输入图像数据依然是完整的原图，由 C++ 负责根据 ROI 参数进行裁剪，这样可以简化 C# 端的逻辑。

2.  **ROI 交互实现**:
    *   **决定**: ROI 的位置和大小 (`Rect`) 在 `MainViewModel` 中作为属性进行管理。而具体的鼠标拖动和缩放逻辑，建议在 View 层（`MainWindow.xaml.cs` 或附加行为）实现。
    *   **理由**: 遵循 MVVM 模式。ViewModel 不应关心具体的 UI 事件（如 `MouseDown`, `MouseMove`），它只负责状态（`RoiRect`）的管理和响应状态变化。View 层更适合处理与 UI 控件和坐标转换相关的复杂交互。

3.  **数据流**:
    *   **决定**: `UpdateProcessedImage` 方法现在将根据 `RoiRect` 的尺寸来创建 `outputBytes` 数组，并将 ROI 参数传递给 C++。返回的处理结果图（`ProcessedImage`）的尺寸将与 ROI 的尺寸一致。
    *   **理由**: 这样可以节省内存和处理时间，因为 C++ 只需要返回感兴趣区域的数据。UI 效果图区域也自然地只显示 ROI 的处理结果，符合用户预期。

4.  **UI 控件**:
    *   **决定**: 在原图上叠加一个半透明的 `Rectangle` 来可视化 ROI。使用 `Canvas` 作为容器，以便通过 `Canvas.Left/Top` 轻松定位。
    *   **理由**: `Canvas` 提供了最简单的绝对定位机制，非常适合实现这种覆盖层的效果。数据绑定到 ViewModel 的 `RoiRect` 属性可以确保 UI 和数据状态的同步。

---
### Decision
[2025-07-09 22:15:00] - Architecturally define the ROI user interaction logic to be implemented within the View layer (e.g., `MainWindow.xaml.cs` or an attached behavior), not in the ViewModel.

**Rationale:**
This decision strictly adheres to the MVVM pattern's principle of separation of concerns.
1.  **ViewModel Purity:** The `MainViewModel` should remain a pure representation of the application's state and logic, free from any knowledge of UI-specific implementation details like mouse clicks, drags, or control coordinates. This makes the ViewModel easier to test, maintain, and reason about.
2.  **View's Responsibility:** The View is responsible for presentation and user interaction. Handling raw input events (`MouseDown`, `MouseMove`, `MouseUp`), performing coordinate transformations (from screen space to image space), and managing visual feedback are classic View-layer responsibilities.
3.  **Decoupling:** It decouples the ViewModel from any specific UI framework's eventing system (e.g., WPF's routed events). The only contract between the View and ViewModel is the `RoiRect` property, which the View updates when the interaction is complete.

**Implications/Details:**
- The `MainWindow.xaml.cs` (or a dedicated interaction service/behavior) will contain the event handlers for mouse interactions on the image display area.
- This code will be responsible for creating and modifying a visual rectangle and, upon completion (e.g., `MouseUp`), updating the `MainViewModel.RoiRect` property via its data binding.
- The `MainViewModel` simply reacts to the change in the `RoiRect` property to trigger the image processing logic.

---
### Decision (Code)
[2025-07-09 22:33:00] - Implement ROI visual handles using a custom `Adorner`.

**Rationale:**
Using the `AdornerLayer` is the standard and most appropriate WPF mechanism for overlaying visual elements on top of a `UIElement` without interfering with its layout or logic. It's more flexible and powerful than manually adding and positioning shapes on a `Canvas`. This approach cleanly separates the core ROI rectangle (defined in XAML) from its decorative handles (drawn in code).

**Details:**
- A new class `RoiAdorner` inheriting from `System.Windows.Documents.Adorner` was created.
- The adorner is responsible for drawing four small, green rectangles at the corners of the ROI `Rect`.
- The `MainWindow.xaml.cs` code-behind manages the lifecycle of the adorner: it is removed and re-added in the `UpdateRoiAdorner` method, which is called during `MouseMove` and `MouseUp` events to ensure the handles are always in the correct position.
- File Reference: `csharp/ImageAnalysisTool.WPF/Views/MainWindow.xaml.cs`

---
### Decision (Code)
[2025-07-09 22:40:21] - Fix bug where ROI adjustment did not trigger image analysis.

**Rationale:**
The `ImageCanvas_MouseUp` event handler in `MainWindow.xaml.cs` was not updating the `ViewModel.RoiRect` property after the user finished dragging the ROI. This meant the property's setter, which contains the call to `UpdateProcessedImage()`, was never invoked upon completion of the drag gesture. The fix involves adding logic to the `MouseUp` handler to calculate the final `Rect` and explicitly set the `ViewModel.RoiRect` property, ensuring the analysis is triggered as expected.

**Details:**
File Reference: `csharp/ImageAnalysisTool.WPF/Views/MainWindow.xaml.cs`