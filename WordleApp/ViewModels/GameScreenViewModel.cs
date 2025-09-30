using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace WordleApp.ViewModels
{
    public class GameScreenViewModel : BindableObject
    {
        public ICommand GoBackCommand { get; }

        public GameScreenViewModel()
        {
            GoBackCommand = new Command(GoBack);
        }

        private async void GoBack()
        {
            // Navigate back to the Start Screen
            await Shell.Current.GoToAsync("..");
        }
    }
}