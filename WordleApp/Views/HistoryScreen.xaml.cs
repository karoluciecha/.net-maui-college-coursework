using WordleApp.Models;
using WordleApp.Services;

namespace WordleApp.Views;

public partial class HistoryScreen : ContentPage
{
    public List<GameRecord> HistoryRecords { get; set; }

    public HistoryScreen()
    {
        InitializeComponent();
        LoadHistory();
    }

    private void LoadHistory()
    {
        HistoryRecords = GameHistoryService.LoadRecords();
        BindingContext = this; // Bind the ViewModel to the page
    }
    private async void OnBackButtonClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//StartScreen");
    }
}