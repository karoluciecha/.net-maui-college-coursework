using System;
using System.Timers;
using Microsoft.Maui.Controls;
using Timer = System.Timers.Timer;

namespace SlidingPuzzleGame
{
    public partial class MainPage : ContentPage
    {
        private Timer _timer;
        private TimeSpan _elapsedTime;
        private bool _isTimerRunning;
        static readonly int NUM = 3;
        GameSquare[,] squares = new GameSquare[NUM, NUM];
        int emptyRow = NUM - 1;
        int emptyCol = NUM - 1;
        double squareSize;
        bool isBusy;
        bool isPlaying;

        public MainPage()
        {
            InitializeComponent();
            _timer = new Timer(1000);
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = true;
            _isTimerRunning = false;
            _elapsedTime = TimeSpan.Zero;
            InitializeSquares();
        }

        private void InitializeSquares()
        {
            string text = "NET MAUI";
            string winText = "CONGRATS";
            int index = 0;

            for (int row = 0; row < NUM; row++)
            {
                for (int col = 0; col < NUM; col++)
                {
                    if (row == NUM - 1 && col == NUM - 1)
                        break;

                    GameSquare square = new GameSquare(text[index], winText[index], index)
                    {
                        Row = row,
                        Column = col
                    };

                    TapGestureRecognizer tap = new TapGestureRecognizer
                    {
                        Command = new Command(OnSquareTapped),
                        CommandParameter = square
                    };
                    square.GestureRecognizers.Add(tap);

                    squares[row, col] = square;
                    absoluteLayout.Add(square);
                    index++;
                }
            }
        }

        void OnStackSizeChanged(object sender, EventArgs e)
        {
            double width = stackLayout.Width;
            double height = stackLayout.Height;

            if (width <= 0 || height <= 0)
                return;

            stackLayout.Orientation = (width < height) ? StackOrientation.Vertical : StackOrientation.Horizontal;
            squareSize = Math.Min(width, height) / NUM;
            absoluteLayout.WidthRequest = NUM * squareSize;
            absoluteLayout.HeightRequest = NUM * squareSize;

            double multiplayer = DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS ? 0.4 : 0.5;

            foreach (View view in absoluteLayout)
            {
                GameSquare square = (GameSquare)view;
                square.SetLabelFont(multiplayer * squareSize, FontAttributes.Bold);

                AbsoluteLayout.SetLayoutBounds(square, new Rect(square.Column * squareSize,
                    square.Row * squareSize,
                    squareSize,
                    squareSize));
            }
        }

        async void OnSquareTapped(object parameter)
        {
            if (isBusy)
                return;

            isBusy = true;
            GameSquare tappedSquare = (GameSquare)parameter;
            await ShiftIntoEmpty(tappedSquare.Row, tappedSquare.Column);
            isBusy = false;

            if (isPlaying && CheckForWin())
            {
                isPlaying = false;
                StopTimer();
                await DoWinAnimation();
            }
        }

        async void OnRandomizeButtonClicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            button.IsEnabled = false;
            Random rand = new Random();

            isBusy = true;

            // Shuffle tiles with random moves
            for (int i = 0; i < 100; i++)
            {
                await ShiftIntoEmpty(rand.Next(NUM), emptyCol, 25);
                await ShiftIntoEmpty(emptyRow, rand.Next(NUM), 25);
            }

            button.IsEnabled = true;
            isBusy = false;
            isPlaying = true;
            StartTimer();
        }

        async Task ShiftIntoEmpty(int tappedRow, int tappedCol, uint length = 100)
        {
            if (tappedRow == emptyRow && tappedCol != emptyCol)
            {
                int inc = Math.Sign(tappedCol - emptyCol);
                for (int col = emptyCol + inc; col != tappedCol + inc; col += inc)
                {
                    await AnimateSquare(emptyRow, col, emptyRow, emptyCol, length);
                }
            }
            else if (tappedCol == emptyCol && tappedRow != emptyRow)
            {
                int inc = Math.Sign(tappedRow - emptyRow);
                for (int row = emptyRow + inc; row != tappedRow + inc; row += inc)
                {
                    await AnimateSquare(row, emptyCol, emptyRow, emptyCol, length);
                }
            }
        }

        async Task AnimateSquare(int row, int col, int newRow, int newCol, uint length)
        {
            GameSquare animaSquare = squares[row, col]; Rect rect = new Rect(squareSize * emptyCol, squareSize * emptyRow, squareSize, squareSize);
            await animaSquare.LayoutTo(rect, length);
            AbsoluteLayout.SetLayoutBounds(animaSquare, rect);
            squares[newRow, newCol] = animaSquare;
            animaSquare.Row = newRow;
            animaSquare.Column = newCol;
            squares[row, col] = null;
            emptyRow = row;
            emptyCol = col;
        }
        
        private bool CheckForWin()
        {
            // The puzzle is solved if all squares are in their original positions
            int index = 0;
            for (int row = 0; row < NUM; row++)
            {
                for (int col = 0; col < NUM; col++)
                {
                    if (row == NUM - 1 && col == NUM - 1)
                        continue; // Skip the empty square

                    GameSquare square = squares[row, col];
                    if (square == null || square.Index != index)
                        return false;
                    index++;
                }
            }
            return true;
        }
        
        async Task DoWinAnimation()
        {
            // Simple win animation: flash all tiles
            for (int i = 0; i < 3; i++)
            {
                foreach (View view in absoluteLayout)
                {
                    if (view is GameSquare square)
                        square.Opacity = 0.3;
                }
                await Task.Delay(150);
                foreach (View view in absoluteLayout)
                {
                    if (view is GameSquare square)
                        square.Opacity = 1.0;
                }
                await Task.Delay(150);
            }
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _elapsedTime = _elapsedTime.Add(TimeSpan.FromSeconds(1));
            MainThread.BeginInvokeOnMainThread(() =>
            {
                timeLabel.Text = $"Time: {_elapsedTime:hh\\:mm\\:ss}";
            });
        }

        private void StartTimer()
        {
            if (!_isTimerRunning)
            {
                _elapsedTime = TimeSpan.Zero;
                timeLabel.Text = "Time: 00:00:00";
                _timer.Start();
                _isTimerRunning = true;
            }
        }

        private void StopTimer()
        {
            if (_isTimerRunning)
            {
                _timer.Stop();
                _isTimerRunning = false;
            }
        }

        // Example usage: Call StartTimer() when the game starts, and StopTimer() when the game ends.
        // You can wire these to your game logic as needed.
    }
}
