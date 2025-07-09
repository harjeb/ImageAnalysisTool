# System Patterns *Optional*

This file documents recurring patterns and standards used in the project.
It is optional, but recommended to be updated as the project evolves.
2025-07-09 20:29:04 - Log of updates made.

*

## Coding Patterns

*   **P/Invoke:** C# code will use `[DllImport]` to call exported C functions from the C++ DLL. Data will be marshaled primarily as byte arrays (`byte[]`) for images.
*   **INotifyPropertyChanged:** ViewModels will implement this interface to notify the View of property changes, enabling data binding.

## Architectural Patterns

*   **MVVM (Model-View-ViewModel):** The primary pattern for the WPF application.
    *   **Model:** Represents the application's data and business logic (e.g., image data, processing parameters).
    *   **View:** The UI (XAML). It is responsible for displaying data and routing user commands to the ViewModel.
    *   **ViewModel:** The intermediary between the View and the Model. It handles UI logic, state management, and exposes data and commands to the View.
*   **Native Library Interop:** A C++ DLL acts