using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WordleApp.Services;
using WordleApp.Models;
using Microsoft.Maui.Graphics.Text;

namespace WordleApp.Views
{
    [QueryProperty(nameof(PlayerName), "playerName")]
    public partial class GameScreen : ContentPage
    {
        private string _playerName;

        public string PlayerName
        {
            get => _playerName;
            set
            {
                _playerName = value;
                OnPropertyChanged();
                PlayerNameLabel.Text = $"Current Player: {_playerName}"; // Display the player name
            }
        }

        private DateTime _startTime;
        private System.Timers.Timer _gameTimer;
        private int Rows; // Number of attempts
        private readonly int Columns = 5; // Number of letters in the word
        private string TargetWord = "";
        private HashSet<string> WordList = new(); // Set of valid words
        private int CurrentRow = 0;
        private int CurrentColumn = 0;

        private Dictionary<string, Button> KeyboardButtons = new Dictionary<string, Button>();

        public GameScreen()
        {
            InitializeComponent();
            Rows = Math.Min(Math.Max(Preferences.Get("NumberOfAttempts", 6), 1), 10);
            LoadWordListAsync();
            BuildLetterGrid();
            BuildKeyboard();
            StartGameTimer();
        }

        // Download or load the word list
        private async void LoadWordListAsync()
        {
            string wordListUrl = "https://raw.githubusercontent.com/DonH-ITS/jsonfiles/main/words.txt";
            string localFilePath = Path.Combine(FileSystem.CacheDirectory, "words.txt");

            // Check if the file exists locally
            if (!File.Exists(localFilePath))
            {
                // Download the file
                try
                {
                    using HttpClient client = new();
                    string wordListContent = await client.GetStringAsync(wordListUrl);
                    File.WriteAllText(localFilePath, wordListContent);
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to download word list: {ex.Message}", "OK");
                    return;
                }
            }

            // Load words into a HashSet for quick lookups
            WordList = new HashSet<string>(File.ReadAllLines(localFilePath).Select(word => word.Trim().ToUpper()));

            // Randomly choose a target word
            if (WordList.Count > 0)
            {
                Random random = new();
                TargetWord = WordList.ElementAt(random.Next(WordList.Count));
                //await DisplayAlert("Word", $"TargetWord: {TargetWord}", "OK"); // Debugging
            }
            else
            {
                await DisplayAlert("Error", "Word list is empty. Please reload the app.", "OK");
            }
        }
        // Build the letter grid dynamically
        private void BuildLetterGrid()
        {
            // Clear the existing grid
            LetterGrid.Children.Clear();
            LetterGrid.RowDefinitions.Clear();

            // Dynamically define rows
            for (int i = 0; i < Rows; i++)
            {
                LetterGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    // Create the Label with dynamic font size adjustment
                    var label = new Label
                    {
                        Text = "",
                        FontSize = 18, // Initial font size
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                        TextColor = (Color)Application.Current.Resources["BoxTextColor"],
                        LineBreakMode = LineBreakMode.NoWrap
                    };

                    // Create the Frame that holds the Label
                    var box = new Frame
                    {
                        BackgroundColor = (Color)Application.Current.Resources["CellBackgroundColor"],
                        BorderColor = (Color)Application.Current.Resources["BorderColor"],
                        CornerRadius = 0, // To make it look square
                        WidthRequest = 50,
                        HeightRequest = 50,
                        Padding = 0, // Remove extra padding
                        Content = label
                    };

                    // Set the row and column in the grid
                    Grid.SetRow(box, row);
                    Grid.SetColumn(box, col);
                    LetterGrid.Children.Add(box);
                }
            }
        }
        private void BuildKeyboard()
        {
            // Rows of letters and the special buttons row
            string[] rows = { "QWERTYUIOP", "ASDFGHJKL" };

            // Create a stack layout for the keyboard
            var keyboardLayout = new VerticalStackLayout
            {
                Spacing = 10,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End
            };

            // Build the first two rows of letters
            for (int rowIndex = 0; rowIndex < rows.Length; rowIndex++)
            {
                string row = rows[rowIndex];

                var rowLayout = new HorizontalStackLayout
                {
                    Spacing = 5,
                    HorizontalOptions = LayoutOptions.Center
                };

                foreach (char letter in row)
                {
                    string letterString = letter.ToString();

                    // Create a local copy to avoid closure issues
                    var localLetterString = letterString;

                    var keyButton = new Button
                    {
                        Text = letterString,
                        FontSize = 18,
                        Style = (Style)Application.Current.Resources["GrayEnabledButton"],
                        CornerRadius = 5,
                        WidthRequest = 40,
                        HeightRequest = 50,
                        Margin = new Thickness(2),
                        Command = new Command(() =>
                        {
                            // OnKeyPressed should handle the key validation and input logic
                            OnKeyPressed(localLetterString);
                        })
                    };

                    KeyboardButtons[localLetterString] = keyButton;
                    rowLayout.Children.Add(keyButton);
                }

                keyboardLayout.Children.Add(rowLayout);
            }

            // Add the last row with "Enter", letters, and "Delete"
            var lastRowLayout = new HorizontalStackLayout
            {
                Spacing = 5,
                HorizontalOptions = LayoutOptions.Center
            };

            // Add the "Enter" button
            var enterButton = new Button
            {
                Text = "Enter",
                FontSize = 16,
                Style = (Style)Application.Current.Resources["EnterButton"],
                CornerRadius = 5,
                WidthRequest = 80, // Adjusted width for better fit
                HeightRequest = 50,
                Margin = new Thickness(2),
                IsEnabled = false, // Initially disabled
                Command = new Command(OnEnterPressed)
            };
            KeyboardButtons["Enter"] = enterButton;
            lastRowLayout.Children.Add(enterButton);

            // Add the letter keys for the last row
            string lastRowLetters = "ZXCVBNM";
            foreach (char letter in lastRowLetters)
            {
                string letterString = letter.ToString();

                var keyButton = new Button
                {
                    Text = letterString,
                    FontSize = 18,
                    Style = (Style)Application.Current.Resources["GrayEnabledButton"],
                    CornerRadius = 5,
                    WidthRequest = 40,
                    HeightRequest = 50,
                    Margin = new Thickness(2),
                    Command = new Command(() =>
                    {
                        OnKeyPressed(letterString); // Directly pass the letter to the method
                    })
                };

                KeyboardButtons[letterString] = keyButton;
                lastRowLayout.Children.Add(keyButton);
            }

            // Add the "Delete" button
            var deleteButton = new Button
            {
                Text = "Delete",
                FontSize = 16,
                Style = (Style)Application.Current.Resources["DeleteButton"],
                CornerRadius = 5,
                WidthRequest = 80, // Adjusted width for better fit
                HeightRequest = 50,
                Margin = new Thickness(2),
                IsEnabled = false, // Initially disabled
                Command = new Command(OnDeletePressed)
            };
            KeyboardButtons["Delete"] = deleteButton;
            lastRowLayout.Children.Add(deleteButton);

            keyboardLayout.Children.Add(lastRowLayout);

            // Add the keyboard layout to the main content
            KeyboardGrid.Children.Clear(); // Clear any existing grid children
            KeyboardGrid.Add(keyboardLayout);
        }
        private async void OnEnterPressed()
        {
            // Collect the word from the current row
            var currentWord = string.Concat(
                LetterGrid.Children
                    .OfType<Frame>()
                    .Where(frame => Grid.GetRow(frame) == CurrentRow)
                    .Select(frame => (frame.Content as Label)?.Text ?? "") // Get the Label text or empty string
            );

            // Check if the word is incomplete
            if (currentWord.Length != Columns || currentWord.Any(c => string.IsNullOrEmpty(c.ToString())))
            {
                await DisplayAlert("Error", "Complete the word before submitting.", "OK");
                return;
            }

            bool isWordExistenceCheckEnabled = Preferences.Get("IsWordExistenceCheckEnabled", true);
            // Check if the word exists in the word list
            if (isWordExistenceCheckEnabled && !WordList.Contains(currentWord))
            {
                await DisplayAlert("Invalid Word", "This word is not in the word list. Try again.", "OK");
                return;
            }

            // Compare the word or perform your game logic
            CheckWord();
            CurrentRow++;
            CurrentColumn = 0;

            // Disable the "Enter" button after submission
            KeyboardButtons["Enter"].IsEnabled = false;
            KeyboardButtons["Delete"].IsEnabled = false;
        }
        private void OnDeletePressed()
        {
            if (CurrentColumn > 0)
            {
                // Move to the previous box
                CurrentColumn--;

                // Get the target frame and its content (Label)
                var targetFrame = LetterGrid.Children
                    .OfType<Frame>()
                    .FirstOrDefault(frame => Grid.GetRow(frame) == CurrentRow && Grid.GetColumn(frame) == CurrentColumn);

                if (targetFrame != null && targetFrame.Content is Label targetLabel)
                {
                    // Clear the text in the label
                    targetLabel.Text = string.Empty;

                    // If no more input to delete, reset the "Delete" button color
                    if (CurrentColumn == 0)
                    {
                        KeyboardButtons["Delete"].IsEnabled = false;
                    }

                }
            }

            // Always reset the "Enter" button's color and disable it if the word is incomplete
            if (CurrentColumn < Columns)
            {
                KeyboardButtons["Enter"].IsEnabled = false;
            }
        }
        // Handle keyboard button presses
        private void OnKeyPressed(string letter)
        {
            // Check if the button is disabled
            if (KeyboardButtons.TryGetValue(letter, out Button button) && !button.IsEnabled)
            {
                return; // Do nothing if the button is disabled
            }

            if (CurrentRow >= Rows || CurrentColumn >= Columns) return;

            // Get the target frame for the current cell
            var targetFrame = LetterGrid.Children
                .OfType<Frame>()
                .FirstOrDefault(frame => Grid.GetRow(frame) == CurrentRow && Grid.GetColumn(frame) == CurrentColumn);

            if (targetFrame != null && targetFrame.Content is Label targetLabel)
            {
                // Update the text in the label
                targetLabel.Text = letter.ToUpper();
                CurrentColumn++;

                // Enable "Enter" if the word is complete
                if (CurrentColumn == Columns)
                {
                    KeyboardButtons["Enter"].IsEnabled = true;
                }

                // Enable and change the "Delete" button color to red if there's input to delete
                if (CurrentColumn > 0)
                {
                    KeyboardButtons["Delete"].IsEnabled = true;
                }

            }
        }
        private void CheckWord()
        {
            // Get the guessed word from the current row
            string guessedWord = string.Concat(
                LetterGrid.Children
                    .OfType<Frame>()
                    .Where(frame => Grid.GetRow(frame) == CurrentRow)
                    .Select(frame => (frame.Content as Label)?.Text ?? "")
            );

            if (guessedWord.Equals(TargetWord, StringComparison.OrdinalIgnoreCase))
            {
                HandleGameWin();
                return;
            }

            if (CurrentRow + 1 >= Rows)
            {
                HandleGameOver();
                return;
            }

            // Create a copy of the target word to track matches
            char[] targetWordArray = TargetWord.ToUpper().ToCharArray();
            bool[] matched = new bool[Columns]; // Track matched letters to prevent duplicate marking
            bool[] guessedMatched = new bool[Columns]; // Track guessed word letters already matched

            // First pass: Check for correct letters in the correct positions (Green)
            for (int i = 0; i < guessedWord.Length; i++)
            {
                if (i < targetWordArray.Length && guessedWord[i] == targetWordArray[i])
                {
                    matched[i] = true;
                    guessedMatched[i] = true;

                    // Update grid box to Green
                    var targetFrame = LetterGrid.Children
                        .OfType<Frame>()
                        .FirstOrDefault(frame => Grid.GetRow(frame) == CurrentRow && Grid.GetColumn(frame) == i);

                    if (targetFrame != null)
                    {
                        targetFrame.BackgroundColor = (Color)Application.Current.Resources["EnterGreenActive"];
                    }

                    // Update the corresponding keyboard button's color if not already Green
                    if (KeyboardButtons.TryGetValue(guessedWord[i].ToString(), out Button button) && button.Style != (Style)Application.Current.Resources["EnterButton"])
                    {
                        button.Style = (Style)Application.Current.Resources["EnterButton"];
                    }
                }
            }

            // Second pass: Check for correct letters in the wrong positions (Golden)
            for (int i = 0; i < guessedWord.Length; i++)
            {
                if (i < targetWordArray.Length && !guessedMatched[i]) // Skip already matched letters
                {
                    for (int j = 0; j < targetWordArray.Length; j++)
                    {
                        if (!matched[j] && guessedWord[i] == targetWordArray[j])
                        {
                            matched[j] = true;
                            guessedMatched[i] = true;

                            // Update grid box to Golden
                            var targetFrame = LetterGrid.Children
                                .OfType<Frame>()
                                .FirstOrDefault(frame => Grid.GetRow(frame) == CurrentRow && Grid.GetColumn(frame) == i);

                            if (targetFrame != null)
                            {
                                targetFrame.BackgroundColor = (Color)Application.Current.Resources["LetterGoldActive"];
                            }

                            // Update the corresponding keyboard button's color if not already Green
                            if (KeyboardButtons.TryGetValue(guessedWord[i].ToString(), out Button button) && button.Style != (Style)Application.Current.Resources["EnterButton"])
                            {
                                button.Style = (Style)Application.Current.Resources["GoldButton"];
                            }
                            break;
                        }
                    }
                }
            }

            // Third pass: Mark incorrect letters (DarkGray)
            bool isKeyDisableEnabled = Preferences.Get("IsKeyDisableEnabled", true);

            for (int i = 0; i < guessedWord.Length; i++)
            {
                if (!guessedMatched[i]) // If the letter wasn't matched
                {
                    var targetFrame = LetterGrid.Children
                        .OfType<Frame>()
                        .FirstOrDefault(frame => Grid.GetRow(frame) == CurrentRow && Grid.GetColumn(frame) == i);

                    if (targetFrame != null)
                    {
                        targetFrame.BackgroundColor = (Color)Application.Current.Resources["LetterIncorrect"];
                    }

                    // Update the corresponding keyboard button's color to DarkGray and disable the button
                    if (KeyboardButtons.TryGetValue(guessedWord[i].ToString(), out Button button))
                    {
                        if (button.Style != (Style)Application.Current.Resources["EnterButton"] && button.Style != (Style)Application.Current.Resources["GoldButton"])
                        {
                            button.BackgroundColor = (Color)Application.Current.Resources["KeyboardBackgroundColorInactive"];

                            if (isKeyDisableEnabled && button.IsEnabled != false)
                            {
                                button.IsEnabled = false; // Disable incorrect keys
                            }
                        }
                    }
                }
            }
        }
        private async void HandleGameWin()
        {
            // Stop the timer
            _gameTimer?.Stop();

            // Disable all keyboard inputs
            foreach (var button in KeyboardButtons.Values)
            {
                button.IsEnabled = false;
            }

            // Update the timer label to "YOU WIN"
            MainThread.BeginInvokeOnMainThread(() =>
            {
                PlayerNameLabel.Text = "YOU WON!";
                ElapsedTimeLabel.IsVisible = false;
                PlayAgainButton.IsVisible = true;
            });

            // Change the background of all boxes in the current row to green
            foreach (var frame in LetterGrid.Children.OfType<Frame>()
                     .Where(f => Grid.GetRow(f) == CurrentRow))
            {
                frame.BackgroundColor = (Color)Application.Current.Resources["EnterGreenActive"];
            }

            // Show congratulations popup
            TimeSpan elapsedTime = DateTime.Now - _startTime;
            string message = $"Congratulations, {PlayerName}!\n" +
                 $"Time: {elapsedTime:mm\\:ss}\n" +
                 $"Attempts: {CurrentRow + 1}";

            await DisplayAlert("You Won!", message, "OK");

            // Save the record
            GameHistoryService.SaveRecord(new GameRecord
            {
                PlayerName = PlayerName,
                ElapsedTime = elapsedTime,
                Attempts = CurrentRow + 1,
                PlayedAt = DateTime.Now
            });
        }
        private async void HandleGameOver()
        {
            // Stop the timer
            _gameTimer?.Stop();

            // Disable all keyboard inputs
            foreach (var button in KeyboardButtons.Values)
            {
                button.IsEnabled = false;
            }

            // Update the timer label to "YOU LOST"
            MainThread.BeginInvokeOnMainThread(() =>
            {
                PlayerNameLabel.Text = "YOU LOST!";
                ElapsedTimeLabel.IsVisible = false;
                PlayAgainButton.IsVisible = true;
            });

            // Apply coloring logic to the last row
            string guessedWord = string.Concat(
                LetterGrid.Children
                    .OfType<Frame>()
                    .Where(frame => Grid.GetRow(frame) == CurrentRow)
                    .Select(frame => (frame.Content as Label)?.Text ?? "")
            );

            // Create a copy of the target word to track matches
            char[] targetWordArray = TargetWord.ToUpper().ToCharArray();
            bool[] matched = new bool[Columns];
            bool[] guessedMatched = new bool[Columns];

            // First pass: Correct letters in correct positions (Green)
            for (int i = 0; i < guessedWord.Length; i++)
            {
                if (i < targetWordArray.Length && guessedWord[i] == targetWordArray[i])
                {
                    matched[i] = true;
                    guessedMatched[i] = true;

                    var frame = LetterGrid.Children
                        .OfType<Frame>()
                        .FirstOrDefault(f => Grid.GetRow(f) == CurrentRow && Grid.GetColumn(f) == i);

                    if (frame != null)
                    {
                        frame.BackgroundColor = (Color)Application.Current.Resources["EnterGreenActive"];
                    }
                }
            }

            // Second pass: Correct letters in wrong positions (Gold)
            for (int i = 0; i < guessedWord.Length; i++)
            {
                if (!guessedMatched[i] && i < targetWordArray.Length)
                {
                    for (int j = 0; j < targetWordArray.Length; j++)
                    {
                        if (!matched[j] && guessedWord[i] == targetWordArray[j])
                        {
                            matched[j] = true;
                            guessedMatched[i] = true;

                            var frame = LetterGrid.Children
                                .OfType<Frame>()
                                .FirstOrDefault(f => Grid.GetRow(f) == CurrentRow && Grid.GetColumn(f) == i);

                            if (frame != null)
                            {
                                frame.BackgroundColor = (Color)Application.Current.Resources["LetterGoldActive"];
                            }

                            break;
                        }
                    }
                }
            }

            // Third pass: Incorrect letters (DarkGray)
            for (int i = 0; i < guessedWord.Length; i++)
            {
                if (!guessedMatched[i])
                {
                    var frame = LetterGrid.Children
                        .OfType<Frame>()
                        .FirstOrDefault(f => Grid.GetRow(f) == CurrentRow && Grid.GetColumn(f) == i);

                    if (frame != null)
                    {
                        frame.BackgroundColor = (Color)Application.Current.Resources["LetterIncorrect"];
                    }
                }
            }

            // Show the "Game Over" popup
            string message = $"Game Over!\nThe correct word was: {TargetWord}.\nBetter luck next time.";
            await DisplayAlert("You Lost!", message, "OK");
        }
        private void OnPlayAgainClicked(object sender, EventArgs e)
        {
            // Reset the game
            ResetGame();

            // Show the elapsed time and hide the "Play Again" button
            ElapsedTimeLabel.IsVisible = true;
            PlayAgainButton.IsVisible = false;
        }
        private void ResetGame()
        {
            // Reset game state
            Rows = Math.Min(Math.Max(Preferences.Get("NumberOfAttempts", 6), 1), 10);
            CurrentRow = 0;
            CurrentColumn = 0;
            TargetWord = WordList.ElementAt(new Random().Next(WordList.Count));
            //DisplayAlert("Word", $"TargetWord: {TargetWord}", "OK"); // Debugging

            // Clear the grid
            foreach (var frame in LetterGrid.Children.OfType<Frame>())
            {
                if (frame.Content is Label label)
                {
                    label.Text = string.Empty;
                }

                frame.BackgroundColor = Colors.Black;
            }

            // Reset the keyboard
            foreach (var button in KeyboardButtons.Values)
            {
                button.IsEnabled = true;
                button.Style = (Style)Application.Current.Resources["GrayEnabledButton"];
            }

            // Restart the timer
            StartGameTimer();
        }
        private void StartGameTimer()
        {
            _startTime = DateTime.Now;

            _gameTimer = new System.Timers.Timer(1000); // Update every second
            _gameTimer.Elapsed += OnTimerElapsed;
            _gameTimer.Start();
        }
        private void OnTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            TimeSpan elapsed = DateTime.Now - _startTime;

            // Update the UI on the main thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ElapsedTimeLabel.Text = $"Elapsed Time: {elapsed:mm\\:ss}";
            });
        }
    }
}