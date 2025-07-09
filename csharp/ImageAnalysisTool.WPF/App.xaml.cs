using System.Windows;
using ImageAnalysisTool.WPF.ViewModels;
using ImageAnalysisTool.WPF.Views;

namespace ImageAnalysisTool.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var viewModel = new MainViewModel();
            var view = new MainWindow
            {
                DataContext = viewModel
            };

            view.Show();
        }
    }
}
