using Microsoft.Maui.Controls;
using WordleApp.ViewModels;

namespace WordleApp.Views
{
    public partial class StartScreen : ContentPage
    {
        public StartScreen()
        {
            InitializeComponent();
            BindingContext = new StartScreenViewModel();

        }
    }
}