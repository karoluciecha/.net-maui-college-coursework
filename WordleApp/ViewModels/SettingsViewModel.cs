using System.ComponentModel;

namespace WordleApp.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    private bool _isDarkModeEnabled;
    public bool IsDarkModeEnabled
    {
        get => _isDarkModeEnabled;
        set
        {
            if (_isDarkModeEnabled != value)
            {
                _isDarkModeEnabled = value;
                OnPropertyChanged(nameof(IsDarkModeEnabled));
            }
        }
    }

    private bool _isWordExistenceCheckEnabled;
    public bool IsWordExistenceCheckEnabled
    {
        get => _isWordExistenceCheckEnabled;
        set
        {
            if (_isWordExistenceCheckEnabled != value)
            {
                _isWordExistenceCheckEnabled = value;
                OnPropertyChanged(nameof(IsWordExistenceCheckEnabled));
            }
        }
    }

    private bool _isKeyDisableEnabled;
    public bool IsKeyDisableEnabled
    {
        get => _isKeyDisableEnabled;
        set
        {
            if (_isKeyDisableEnabled != value)
            {
                _isKeyDisableEnabled = value;
                OnPropertyChanged(nameof(IsKeyDisableEnabled));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}