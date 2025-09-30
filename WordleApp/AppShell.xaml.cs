using WordleApp.Views;

namespace WordleApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("GameScreen", typeof(GameScreen));
            Routing.RegisterRoute("HistoryScreen", typeof(HistoryScreen));
            Routing.RegisterRoute("SettingsScreen", typeof(SettingsScreen));
        }
    }
}