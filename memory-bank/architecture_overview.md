# 图像分析工具 - 架构概览

本文档详细描述了“图像分析工具”的软件架构，包括项目结构、架构模式、核心组件及其交互。

---

## 1. 解决方案与项目结构

解决方案 (`ImageAnalysisTool.sln`) 将包含两个核心项目：

1.  **ImageAnalysisTool.WPF (C# WPF 项目)**: 负责用户界面和应用程序逻辑。
2.  **ImageProcessor.Native (C++ DLL 项目)**: 负责高性能的图像处理。

### 1.1 ImageAnalysisTool.WPF (C#) 文件结构

```
/ImageAnalysisTool.WPF
|
|-- /Assets           # 存放图标、图片等静态资源
|-- /Models           # 数据模型 (例如，图像信息)
|   |-- ImageInfo.cs
|
|-- /ViewModels       # 视图模型 (MVVM中的VM)
|   |-- MainViewModel.cs
|   |-- ViewModelBase.cs
|
|-- /Views            # 视图 (MVVM中的V)
|   |-- MainWindow.xaml
|   |-- MainWindow.xaml.cs
|
|-- /Services         # 服务 (例如，文件对话框、图像转换)
|   |-- ImageConverterService.cs
|   |-- FileDialogService.cs
|
|-- /Interop          # 与C++ DLL交互的封装
|   |-- NativeMethods.cs
|
|-- App.xaml
|-- App.xaml.cs
```

### 1.2 ImageProcessor.Native (C++) 文件结构

```
/ImageProcessor.Native
|
|-- /include          # 公共头文件
|   |-- ImageProcessor.h
|
|-- /src              # 源文件
|   |-- dllmain.cpp
|   |-- ImageProcessor.cpp
|
|-- ImageProcessor.Native.vcxproj
```

---

## 2. 架构模式 (MVVM)

WPF 应用将严格遵循 **Model-View-ViewModel (MVVM)** 模式。

*   **Model**:
    *   职责: 表示应用程序的数据和核心业务逻辑。在此项目中，Model 层相对简单，主要是数据容器，如 `ImageInfo.cs`，用于存储图像元数据。核心业务逻辑（图像处理）被委托给 C++ DLL。
*   **View**:
    *   职责: 定义 UI 的结构、布局和外观。由 XAML 文件 (`MainWindow.xaml`) 构成。它通过数据绑定与 ViewModel 交互，并将用户输入（如点击按钮、移动滑块）转发给 ViewModel 的命令。View 本身不包含任何业务逻辑。
*   **ViewModel**:
    *   职责: 作为 View 和 Model 之间的桥梁，处理所有 UI 逻辑。`MainViewModel.cs` 将：
        *   公开 `BitmapSource` 类型的属性 (`OriginalImage`, `ProcessedImage`) 供 View 绑定。
        *   公开 `int` 类型的属性 (`GrayscaleThreshold`) 供灰度阈值滑块绑定。
        *   公开 `Rect` 类型的属性 (`RoiRect`)，用于定义感兴趣区域，并与 View 中的 ROI 可视化元素绑定。
        *   提供 `ICommand` 类型的命令 (`LoadImageCommand`, `MaximizeRoiCommand`) 来处理用户操作。
        *   在属性（如 `GrayscaleThreshold` 或 `RoiRect`）发生变化时，调用 C++ DLL 进行图像处理，并更新 `ProcessedImage` 属性，从而自动更新 UI。

---

## 3. 核心组件关系图

```mermaid
graph TD
    subgraph "ImageAnalysisTool.WPF (C#)"
        A[View: MainWindow.xaml] -- DataBinding/Commands --> B["
            <b>MainViewModel</b>
            <hr>
            + GrayscaleThreshold: int
            + RoiRect: Rect
            <hr>
            + LoadImageCommand: ICommand
            + MaximizeRoiCommand: ICommand
        "];
        B -- Uses --> C[Service: ImageConverterService];
        B -- Calls --> D[Interop: NativeMethods];
        C -- Manipulates --> E[Model: Image Data (BitmapSource)];
    end

    subgraph "ImageProcessor.Native (C++)"
        F[ImageProcessor.cpp] -- Implements --> G["
            <b>Exported C-API</b>
            <hr>
            + ProcessImage_Threshold(...,<br>grayscaleThreshold, roiX, ...)";
        ];
        F -- Uses --> H[OpenCV Library];
    end

    D -- P/Invoke --> G;

    style A fill:#f9f,stroke:#333,stroke-width:2px
    style B fill:#ccf,stroke:#333,stroke-width:2px
    style F fill:#cfc,stroke:#333,stroke-width:2px
```

**组件说明:**

*   **MainWindow.xaml (View)**: 用户界面，包含图像显示区域、参数滑块和 ROI 交互层。负责捕获原始鼠标事件以进行 ROI 操作。
*   **MainViewModel (ViewModel)**: 驱动 UI 的核心逻辑。管理图像数据、灰度阈值 (`GrayscaleThreshold`) 和感兴趣区域 (`RoiRect`)。响应用户命令，并调用底层服务进行图像处理。
*   **NativeMethods (Interop)**: C# 静态类，使用 `DllImport` 定义对 C++ DLL 中 `ProcessImage_Threshold` 函数的 P/Invoke 调用，传递包括 ROI 和阈值在内的所有参数。
*   **ImageProcessor.cpp (C++)**: C++ 实现，使用 OpenCV 执行灰度阈值处理。它接收完整的图像和 ROI 坐标，仅在 ROI 内部执行处理。

---

## 4. UI 交互模式：ROI 定义

为了保持 ViewModel 的纯粹性并遵循 MVVM 模式，ROI 的绘制和修改逻辑在 **View 层** 中处理。

*   **职责划分**:
    *   **View (Code-Behind / Interaction Service)**: 负责直接处理 UI 事件（`MouseDown`, `MouseMove`, `MouseUp`）。它计算出新的 ROI 矩形（坐标和尺寸），然后更新 ViewModel 的 `RoiRect` 属性。View 层也负责处理坐标转换（例如，从鼠标在 `Image` 控件上的位置到实际图像的像素位置）。
    *   **ViewModel (`MainViewModel`)**: 仅负责持有和公开 `RoiRect` 属性。它不包含任何特定于 UI 事件的代码。当 `RoiRect` 属性被 View 更新时，ViewModel 的 `setter` 会触发图像处理流程。

*   **实现方式**:
    1.  在 `MainWindow.xaml` 中，一个覆盖在原始图像上的 `Canvas` 用于捕捉鼠标事件。
    2.  `MainWindow.xaml.cs` 中的事件处理器监听鼠标按下、移动和抬起事件。
    3.  在这些事件处理器中，代码计算出用户想要定义的矩形区域。
    4.  计算完成后，代码通过数据绑定更新 `MainViewModel.RoiRect` 属性。

这种方法将复杂的 UI 交互逻辑封装在 View 中，使 ViewModel 保持简洁和高度可测试性。

---

## 5. 数据流

### 5.1 加载图像

1.  **触发**: 用户点击“加载图像”按钮。
2.  **ViewModel**: `LoadImageCommand` 被触发。`MainViewModel` 使用服务打开文件对话框，加载图像文件为 `BitmapSource` 并赋值给 `OriginalImage` 属性。
3.  **UI 更新**: View 通过数据绑定显示原图。
4.  **初始化处理**: `MainViewModel` 触发一次全图的初始处理。

### 5.2 实时处理 (参数变化)

1.  **触发**: 用户在 UI 上与控件交互：
    *   拖动灰度阈值滑块。
    *   通过鼠标在 View 中完成一次 ROI 区域的绘制或修改。
2.  **ViewModel 更新**:
    *   滑块的值通过数据绑定更新 `MainViewModel.GrayscaleThreshold` 属性。
    *   View 层的交互逻辑更新 `MainViewModel.RoiRect` 属性。
3.  **调用处理逻辑**: `GrayscaleThreshold` 或 `RoiRect` 属性的 `setter` 在 `MainViewModel` 中被触发。
4.  **准备数据**: `MainViewModel` 从 `OriginalImage` 准备图像数据 (`byte[]`)。
5.  **P/Invoke 调用**: `MainViewModel` 调用 `NativeMethods.ProcessImage_Threshold`，将以下参数传递给 C++ DLL：
    *   输入图像数据 (`byte[]`)
    *   图像尺寸 (宽度、高度、步长)
    *   灰度阈值 (`GrayscaleThreshold`)
    *   ROI 矩形 (`RoiRect.X`, `RoiRect.Y`, `RoiRect.Width`, `RoiRect.Height`)
6.  **C++ 处理**: C++ DLL (`ImageProcessor.cpp`) 接收完整图像数据和参数。它使用 OpenCV 在指定的 ROI 内部应用灰度阈值处理，并将结果写入输出 `byte[]` 数组。
7.  **返回结果**: `MainViewModel` 接收到处理后的 `byte[]` 数据（其尺寸与 ROI 匹配）。
8.  **UI 更新**: `MainViewModel` 将 `byte[]` 转换回 `BitmapSource` 并更新 `ProcessedImage` 属性。View 上的效果图区域通过数据绑定自动更新，展示 ROI 的处理结果。