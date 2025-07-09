# 图像分析工具

## 简介

本项目是一个桌面应用程序，旨在提供一套基础的图像处理和分析功能。它采用 C# WPF 作为前端界面，并调用一个用 C++ 编写的高性能图像处理后端。这种混合架构旨在将用户友好的界面与高效的计算密集型任务处理能力结合起来。

## 功能特性

*   **加载图像**: 支持从本地文件系统加载常见的图像格式（如 PNG, JPG, BMP）。
*   **Canny 边缘检测**: 对加载的图像应用 Canny 算法以检测边缘。
*   **实时参数调整**: 用户可以通过滑块实时调整 Canny 算法的 `threshold1` 和 `threshold2` 参数，并立即在界面上看到结果。

## 技术栈

*   **前端**: C# WPF (.NET 9.0)
*   **后端**: C++17
*   **图像处理库**: OpenCV 4.5.4
*   **构建工具**: MSBuild (for C++), .NET CLI (for C#)

## 如何构建

### 前提条件

在开始之前，请确保您已安装以下工具：

*   **Visual Studio 2022**: 确保已安装 "Desktop development with C++" 和 ".NET desktop development" 工作负载。
*   **.NET 9 SDK**: 用于编译和运行 C# WPF 应用程序。
*   **OpenCV 4.5.4**: 需要将其安装或解压到您的系统中，并设置 `OPENCV_DIR` 环境变量指向其 `build` 目录。

### 构建步骤

#### 步骤 1: 编译 C++ DLL

首先，需要编译 C++ 核心库 (`ImageProcessor.dll`)。

打开 "Developer Command Prompt for VS 2022" 或一个配置好 MSBuild 环境变量的终端，然后导航到项目根目录并执行以下命令：

```bash
msbuild cpp/ImageProcessor.sln /p:Configuration=Release /p:Platform=x64
```

编译成功后，`ImageProcessor.dll` 将会生成在 `cpp/x64/Release/` 目录下。

#### 步骤 2: 运行 C# 应用

C# 项目配置了一个构建后事件，它会自动将 `ImageProcessor.dll` 从 C++ 项目的输出目录复制到 C# 项目的运行目录中。这确保了前端应用在启动时总能找到所需的本地依赖。

要运行 WPF 应用程序，请在项目根目录打开一个终端并执行以下命令：

```bash
dotnet run --project csharp/ImageAnalysisTool.WPF/ImageAnalysisTool.WPF.csproj
```

应用程序窗口将会启动。

## 如何使用

1.  **加载图像**: 点击应用程序窗口中的 "Load Image" 按钮，然后选择一个图像文件。
2.  **调整参数**: 图像加载后，Canny 边缘检测将自动应用。您可以通过拖动下方的两个滑块来调整 `Threshold1` 和 `Threshold2` 的值。
3.  **查看结果**: 图像显示区域会实时更新，以反映您对参数的调整。

## 项目结构

*   `csharp/`: 包含 C# WPF 应用程序的源代码。这是项目的用户界面和前端逻辑部分。
*   `cpp/`: 包含 C++ 图像处理库的源代码。这部分负责所有计算密集型的图像处理任务，并被编译成一个 DLL 供 C# 端调用。
*   `memory-bank/`: 包含所有项目相关的设计文档、决策日志和实现说明。