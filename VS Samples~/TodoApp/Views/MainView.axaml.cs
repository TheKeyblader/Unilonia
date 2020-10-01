using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TodoApp.ViewModels;

namespace TodoApp.Views
{
    public class MainView : UserControl
    {
        public MainView()
        {
            DataContext = new MainViewModel();
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
