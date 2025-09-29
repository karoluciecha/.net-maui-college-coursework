namespace TicTacToeApp
{
    public partial class MainPage : ContentPage
    {
        private int gridSize;
        private int currentPlayer = 1;
        private Button[,] buttons;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnStartClicked(object sender, EventArgs e)
        {
            if (int.TryParse(GridSizeEntry.Text, out gridSize) && gridSize >= 3 && gridSize <= 9)
            {
                CreateGrid(gridSize);
                StartButton.IsEnabled = false;
                ResetButton.IsEnabled = true;
                PlayerTurnLabel.Text = "Player 1's turn";
            }
            else
            {
                DisplayAlert("Invalid Input", "Please enter a valid grid size (between 3 and 9).", "OK");
            }
        }

        private void OnResetClicked(object sender, EventArgs e)
        {
            GameGrid.Children.Clear();
            StartButton.IsEnabled = true;
            ResetButton.IsEnabled = false;
            PlayerTurnLabel.Text = "";
        }

        private void CreateGrid(int size)
        {
            buttons = new Button[size, size];
            GameGrid.RowDefinitions.Clear();
            GameGrid.ColumnDefinitions.Clear();

            for (int i = 0; i < size; i++)
            {
                GameGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                GameGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    var button = new Button
                    {
                        BackgroundColor = Colors.White,
                        FontSize = 32
                    };
                    button.Clicked += OnButtonClicked;
                    buttons[row, col] = button;
                    GameGrid.Add(button, col, row);
                }
            }
        }

        private void OnButtonClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;

            if (button.Text == null) // Only allow clicking on empty buttons
            {
                button.Text = currentPlayer == 1 ? "X" : "O";
                button.BackgroundColor = currentPlayer == 1 ? Colors.LightGreen : Colors.LightBlue;
                if (CheckWin())
                {
                    DisplayAlert($"Player {currentPlayer} wins!", $"Player {currentPlayer} has won the game!", "OK");
                    DisableButtons();
                }
                else if (IsBoardFull())
                {
                    DisplayAlert("Draw", "The game is a draw!", "OK");
                    DisableButtons();
                }
                else
                {
                    currentPlayer = currentPlayer == 1 ? 2 : 1;
                    PlayerTurnLabel.Text = $"Player {currentPlayer}'s turn";
                }
            }
        }

        private bool IsBoardFull()
        {
            foreach (var button in buttons)
            {
                if (string.IsNullOrEmpty(button.Text))
                {
                    return false;
                }
            }
            return true;
        }

        private void DisableButtons()
        {
            foreach (var button in buttons)
            {
                button.IsEnabled = false;
            }
        }
        private bool CheckWin()
        {
            // Check rows for a win
            for (int row = 0; row < gridSize; row++)
            {
                bool rowWin = true;
                for (int col = 1; col < gridSize; col++)
                {
                    if (buttons[row, col].Text != buttons[row, 0].Text || string.IsNullOrEmpty(buttons[row, col].Text))
                    {
                        rowWin = false;
                        break;
                    }
                }
                if (rowWin) return true;
            }
            
            // Check columns for a win
            for (int col = 0; col < gridSize; col++)
            {
                bool colWin = true;
                for (int row = 1; row < gridSize; row++)
                {
                    if (buttons[row, col].Text != buttons[0, col].Text || string.IsNullOrEmpty(buttons[row, col].Text))
                    {
                        colWin = false;
                        break;
                    }
                }
                if (colWin) return true;
            }

            // Check the primary diagonal (top-left to bottom-right)
            bool diagonal1Win = true;
            for (int i = 1; i < gridSize; i++)
            {
                if (buttons[i, i].Text != buttons[0, 0].Text || string.IsNullOrEmpty(buttons[i, i].Text))
                {
                    diagonal1Win = false;
                    break;
                }
            }
            if (diagonal1Win) return true;

            // Check the secondary diagonal (top-right to bottom-left)
            bool diagonal2Win = true;
            for (int i = 1; i < gridSize; i++)
            {
                if (buttons[i, gridSize - i - 1].Text != buttons[0, gridSize - 1].Text || string.IsNullOrEmpty(buttons[i, gridSize - i - 1].Text))
                {
                    diagonal2Win = false;
                    break;
                }
            }
            if (diagonal2Win) return true;

            // If no win found, return false
            return false;
        }
    }

}