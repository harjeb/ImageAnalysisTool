# Product Context

This file provides a high-level overview of the project and the expected product that will be created. Initially it is based upon projectBrief.md (if provided) and all other available project-related information in the working directory. This file is intended to be updated as the project evolves, and should be used to inform all other modes of the project's goals and context.
2025-07-09 20:28:21 - Log of updates made will be appended as footnotes to the end of this file.

*

## Project Goal

*   Develop a C# WPF desktop application for image analysis, leveraging a high-performance C++ backend for core processing tasks. The initial feature will be Canny edge detection with real-time parameter adjustments.

## Key Features

*   Load local image files (.jpg, .png, .bmp).
*   Display original and processed images side-by-side.
*   Real-time Canny edge detection with adjustable thresholds via UI sliders.
*   Clear separation between UI (WPF) and processing logic (C++ DLL).

## Overall Architecture

*   The solution will use the Model-View-ViewModel (MVVM) pattern for the WPF application to ensure a clean separation of concerns.
*   A C++ DLL will encapsulate all image processing logic (initially OpenCV-based Canny edge detection), exposing a C-style interface for P/Invoke from C#.