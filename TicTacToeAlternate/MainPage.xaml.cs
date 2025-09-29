using Microsoft.Maui.Controls.Shapes;
using Path = Microsoft.Maui.Controls.Shapes.Path;

namespace TicTacToe2024
{
    public partial class MainPage : ContentPage
    {
        private bool gridcreated = false;
        private int player = 1;
        private int numbrows;
        private int oldnumbrows;
        private int[,] positions;
        private bool winner = false;
        private bool useShapes = false;

        public MainPage() {
            InitializeComponent();
            InitializeSettings();

            this.LayoutChanged += OnWindowChange;
        }

        private void InitializeSettings()
        {
            useShapes = Preferences.Default.Get("UseShapes", false);
            shapesSwitch.IsToggled = useShapes;

            numbrows = Preferences.Default.Get("NumbRows", 3);
            GridSize.Text = numbrows.ToString();
        }

        private void OnWindowChange (object sender, EventArgs e)
        {
            double windowWidth = this.Width - 20;
            double windowHeight = this.Height - TopGrid.Height - 10;

        }
        private void StartBtn_Clicked(object sender, EventArgs e) {
            //First get the size of the grid from the box
            //Try Parse will try to Parse the entry box, if it fails numbrows will be assigned 0
            oldnumbrows = numbrows;
            int.TryParse(GridSize.Text, out numbrows);
            if (!gridcreated) {

                //We don't want less than 3 rows in a grid
                if (numbrows < 3) {
                    numbrows = 3;
                }
                if (numbrows > 9)
                {
                    numbrows = 9;
                }
                positions = new int[numbrows, numbrows];
                //Disable the box for entering the grid size
                GridSize.IsEnabled = false;
                Preferences.Default.Set("NumbRows", numbrows);
                CreateTheGrid();
            }
            //If the grid has already been created, call the RestartGame method instead to reset the board
            else {
                RestartGame();
            }
        }

        private void RestartGame()
        {
            // Reset core variables
            winner = false;
            player = 1;
            whichplayerlabel.Text = "Player " + player + "'s Turn";

            // First, clear the old grid completely
            GridPageContent.Children.Clear();
            GridPageContent.RowDefinitions.Clear();
            GridPageContent.ColumnDefinitions.Clear();

            // Try parse new size from the input box
            oldnumbrows = numbrows;
            int.TryParse(GridSize.Text, out numbrows);

            // Validate size
            if (numbrows < 3)
                numbrows = 3;
            if (numbrows > 9)
                numbrows = 9;

            // Reset positions array with the new size
            positions = new int[numbrows, numbrows];

            // Save preference for next app launch
            Preferences.Default.Set("NumbRows", numbrows);

            // Build the new grid
            CreateTheGrid();

            // Disable the grid size entry again
            GridSize.IsEnabled = false;

            // Just in case, keep the start button disabled
            StartBtn.IsEnabled = false;
            shapesSwitch.IsEnabled = false;
        }

        private void CreateTheGrid() {
            //Create numbrows rows and numbrows columns 3x3, 4x4 etc.
            for (int i = 0; i < numbrows; ++i) {
                GridPageContent.AddRowDefinition(new RowDefinition());
                GridPageContent.AddColumnDefinition(new ColumnDefinition());
            }

            //Populate the grid with Borders
            for (int i = 0; i < numbrows; ++i) {
                for (int j = 0; j < numbrows; ++j) {
                    Border styledBorder = new Border
                    {
                        BackgroundColor = Colors.Red, // Set the background color
                        Stroke = Colors.Black,
                        StrokeThickness = 3

                    };
                    TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
                    tapGestureRecognizer.Tapped += OnBorderTapped;
                    styledBorder.GestureRecognizers.Add(tapGestureRecognizer);
                    GridPageContent.Add(styledBorder, j, i);
                }

            }
            //Make the Text say it is player 1's turn
            whichplayerlabel.Text = "Player " + player + "'s Turn";
            gridcreated = true;
            //Disable the start button
            StartBtn.IsEnabled = false;
            shapesSwitch.IsEnabled = false;
        }

        private void OnBorderTapped(object sender, TappedEventArgs e) {
            Border border = (Border)sender;
            if (border != null) {
                DoMove(border);
            }
        }


        /* Taking out BtnMove_Clicked as clicking on the squares is better anyway
        private void BtnMove_Clicked(object sender, EventArgs e) {
            int row, column;
            //Try Parse is another way to convert from string to integer, it checks whether the parse can work first instead of just crashing
            //int.TryParse(string, out) is the form of it
            //If it can parse it returns true and assigns the integer to the output variable
            //If it cannot parse it returns false and assigns 0 to the output variable
            if (!int.TryParse(EntryC.Text, out column) || !int.TryParse(EntryR.Text, out row)) {
                //If either entry cannot be parsed, we exit out of the method by just using return;
                //No feedback will be given to the user
                return;
            }
            //We need to subtract one from each of column and row if it has got this far
            --column;
            --row;

            //Make sure we are within the limits of the grid
            if (column > numbrows || column < 0 || row > numbrows || row < 0)
                return;

            //We are going to do a loop over all the Children of the grid finding all the objects that are there, looking for a match
            foreach (var item in GridPageContent.Children) {
                //We only want to search Borders, so ignore all other types of items
                if (item.GetType() == typeof(Border)) {
                    //Cast the object to type Frame so we can use all the Frame attributes and methods
                    Border border = (Border)item;

                    //Search for a match, if we find one, do the move and exit out of the loop with break
                    if (column == Convert.ToInt32(border.GetValue(Grid.ColumnProperty).ToString()) && row == Convert.ToInt32(border.GetValue(Grid.RowProperty).ToString())) {
                        DoMove(border);
                        break;
                    }
                }
            }
        }
        */
        private void FinishGame(int which) {
            if (which != 3) {
                whichplayerlabel.Text = "Player " + player + " wins";
            }
            else {
                whichplayerlabel.Text = "It's a Draw";
            }
            //Set winner to be true to prevent any more moves
            winner = true;
            //Enable the start game button so we can reset the board
            StartBtn.IsEnabled = true;
            shapesSwitch.IsEnabled = true;
            GridSize.IsEnabled = true;
        }
        private void DoMove(Border border) {
            //if winner is blocking DoMove from running if winner is set to true
            if (winner)
                return;

            int column = Convert.ToInt32(border.GetValue(Grid.ColumnProperty).ToString());
            int row = Convert.ToInt32(border.GetValue(Grid.RowProperty).ToString());
            if (positions[row, column] == 0) {
                positions[row, column] = player;
                double height = border.Height;
                int result = CheckWinner(player);
                bool update = true;
                if (result == player || result == 3) {
                    FinishGame(result);
                    update = false;
                }

                //Draw Cross's (X's) for player 1, remembering to change player after the cross is drawn
                if (player == 1) {

                    if (useShapes) {
                        Path cross = UsefulMethods.MakeCrossUsingPath(height, 6, Color.FromRgb(0, 0, 0));
                        GridPageContent.Add(cross, column, row);
                    }
                    else {
                        border.BackgroundColor = Colors.Green;
                    }
                    player = 2;
                }
                //Draw an ellipse for player 2
                else {
                    if (useShapes) {
                        Ellipse ell = UsefulMethods.DrawEllipse(height);
                        GridPageContent.Add(ell, column, row);
                    }
                    else {
                        border.BackgroundColor = Colors.Blue;
                    }
                    player = 1;
                }
                //Only update the player label text if there has not been a winner or a draw
                if (update) whichplayerlabel.Text = "Player " + player + "'s Turn";
            }

        }

        private int CheckWinner(int player) {
            //If a row, column or diagonal is complete, we return the player number to indicate they have won
            if (UsefulMethods.SearchRowsComplete(positions, numbrows, player))
                return player;
            if (UsefulMethods.SearchColsComplete(positions, numbrows, player))
                return player;
            if (UsefulMethods.SearchDiagonalComplete(positions, numbrows, player))
                return player;
            //Check if Draw and if it is a draw return 3
            if (!UsefulMethods.FindinArray(positions, numbrows, 0))
                return 3;
            //If game can continue return 0
            return 0;
        }

        private void shapesSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            if (sender is Switch shapesSwitch)
            {
                useShapes = shapesSwitch.IsToggled;
                Preferences.Default.Set("UseShapes", useShapes);
            }

        }
    }

}