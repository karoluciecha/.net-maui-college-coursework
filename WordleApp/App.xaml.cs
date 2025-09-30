using Microsoft.Maui.Controls;

namespace WordleApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Set the AppShell as the main page
            MainPage = new AppShell();
        }
        protected override void OnStart()
        {
            base.OnStart();

            // Get the theme preference from Preferences
            bool isDarkMode = Preferences.Get("IsDarkModeEnabled", true);

            // Apply the theme
            Application.Current.Resources.MergedDictionaries.Clear();
            if (isDarkMode)
            {
                Application.Current.Resources.MergedDictionaries.Add(new WordleApp.Resources.Styles.DarkTheme());
            }
            else
            {
                Application.Current.Resources.MergedDictionaries.Add(new WordleApp.Resources.Styles.LightTheme());
            }
        }
    }
}