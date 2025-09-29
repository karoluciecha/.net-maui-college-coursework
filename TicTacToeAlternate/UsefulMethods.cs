
using Microsoft.Maui.Controls.Shapes;
using Path = Microsoft.Maui.Controls.Shapes.Path;


namespace TicTacToe2024
{
    public class UsefulMethods
    {
        public static Ellipse DrawEllipse(double height) {
            return new Ellipse()
            {
                Stroke = Color.FromRgb(0, 0, 0),
                StrokeThickness = 6,
                Fill = Color.FromRgb(0, 255, 0),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                HeightRequest = height - 5,
                WidthRequest = height - 5
            };
        }
        public static Path MakeCrossUsingPath(double dim, int stroke, Color color) {
            Path pth = new Path()
            {
                Stroke = color,
                StrokeThickness = stroke,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center

            };
            pth.Data = new PathGeometry
            {
                Figures = new PathFigureCollection
                        {
                            new PathFigure
                            {
                                StartPoint = new Point(0,0),
                                Segments = new PathSegmentCollection
                                {
                                    new LineSegment(new Point(dim-10, dim-10))
                                }
                            },
                           new PathFigure
                            {
                                StartPoint = new Point(0 , dim - 10),
                                Segments = new PathSegmentCollection
                                {
                                    new LineSegment(new Point(dim-10, 0))
                                }
                            }
                        }
            };
            return pth;
        }

        public static bool SearchDiagonalComplete(int[,] ints, int size, int which) {
            //Top left to bottom right
            bool foundit = true;
            for (int i = 0; i < size; i++) {
                if (ints[i, i] != which) {
                    foundit = false;
                    break;
                }
            }
            if (foundit)
                return true;
            //Top right to bottom left
            foundit = true;
            for (int i = size - 1; i >= 0; i--) {
                if (ints[i, size - 1 - i] != which) {
                    foundit = false;
                    break;
                }
            }
            if (foundit)
                return true;
            return false;
        }

        public static bool SearchColsComplete(int[,] ints, int size, int which) {
            //Search for a completed column for the specified player
            for (int i = 0; i < size; i++) {
                bool found = true;
                for (int j = 0; j < size; j++) {
                    if (ints[j, i] != which) {
                        found = false;
                        break;
                    }
                }
                if (found) {
                    return true;
                }
            }
            //If a completed column has not been found here, then return false
            return false;
        }

        public static bool SearchRowsComplete(int[,] ints, int size, int which) {
            for (int i = 0; i < size; i++) {
                bool found = true;
                for (int j = 0; j < size; j++) {
                    if (ints[i, j] != which) {
                        found = false;
                        break;
                    }
                }
                if (found)
                    return true;
            }
            //If a completed row has not been found here, then return false
            return false;
        }

        public static bool FindinArray(int[,] ints, int size, int which) {
            bool foundit = false;
            for (int i = 0; i < size; ++i) {
                for (int j = 0; j < size; j++) {
                    if (ints[i, j] == which) {
                        foundit = true;
                        break;
                    }
                }
                if (foundit)
                    break;
            }
            return foundit;
        }
    }
}
