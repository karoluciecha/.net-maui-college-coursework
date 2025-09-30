using Microsoft.Maui.Controls;
using WordleApp.Resources.Styles;
using WordleApp.ViewModels;

namespace WordleApp.Views;
public partial class SettingsScreen : ContentPage
{
    public SettingsScreen()
    {
        InitializeComponent();
        BindingContext = new SettingsViewModel();

        // Retrieve the saved value or default to 6 if not set
        int attempts = Math.Min(Math.Max(Preferences.Get("NumberOfAttempts", 6), 1), 10);

        // Set the Slider value and Label text to the saved value
        AttemptsSlider.Value = attempts;
        AttemptsValueLabel.Text = "Number of Attempts: " + attempts.ToString();

        // Initialize the toggle state from Preferences
        WordExistenceSwitch.IsToggled = Preferences.Get("IsWordExistenceCheckEnabled", true);
        WordExistenceSwitch.Toggled += OnWordExistenceSwitchToggled;

        // Initialize the toggle state for disabling keys
        DisableKeySwitch.IsToggled = Preferences.Get("IsKeyDisableEnabled", true);
        DisableKeySwitch.Toggled += OnDisableKeySwitchToggled;

        // Initialize the toggle state from Preferences
        DarkModeSwitch.IsToggled = Preferences.Get("IsDarkModeEnabled", true);
        DarkModeSwitch.Toggled += OnDarkModeSwitchToggled;
    }
    private void OnWordExistenceSwitchToggled(object? sender, ToggledEventArgs e)
    {
        // Save the toggle state to Preferences
        Preferences.Set("IsWordExistenceCheckEnabled", e.Value);
    }
    private void OnDisableKeySwitchToggled(object? sender, ToggledEventArgs e)
    {
        Preferences.Set("IsKeyDisableEnabled", e.Value);
    }
    private async void OnBackButtonClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//StartScreen");
    }
    private void OnAttemptsSliderValueChanged(object? sender, ValueChangedEventArgs e)
    {
        int attempts = (int)Math.Round(e.NewValue); // Round slider value to integer
        AttemptsValueLabel.Text = "Number of Attempts: " + attempts.ToString();

        // Save the value to shared settings or a global service
        Preferences.Set("NumberOfAttempts", attempts);
    }
    private void OnDarkModeSwitchToggled(object? sender, ToggledEventArgs e)
    {
        Preferences.Set("IsDarkModeEnabled", e.Value);

        // Apply the selected theme
        Application.Current.Resources.MergedDictionaries.Clear();
        if (e.Value)
        {
            Application.Current.Resources.MergedDictionaries.Add(new WordleApp.Resources.Styles.DarkTheme());
        }
        else
        {
            Application.Current.Resources.MergedDictionaries.Add(new WordleApp.Resources.Styles.LightTheme());
        }
    }
}