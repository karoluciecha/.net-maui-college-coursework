using System.Text.Json.Serialization;
using static SimplePaint.MainPage;

/* LineDrawable class implementing IDrawable; handles drawing shapes, colors, and paths with custom shapes */
public class LineDrawable(Action invalidateAction) : IDrawable
{
    /* Minimum distance between points for smoother freehand drawing */
    private const float PointDistanceThreshold = 2.0f; // Adjust this threshold to control the minimum distance between points

    /* Variables to store current stroke size, color, shape, and starting/ending points */
    private float _currentStrokeSize = 1;
    private Color _currentColor = Colors.Black;
    private ShapeType _currentShape;
    private PointF _startPoint;
    private PointF _endPoint;
    private List<PointF> _freehandPoints = new(); // Store points for freehand drawing

    /* Action to store the callback for refreshing the canvas view */
    private Action _invalidateAction = invalidateAction;

    /* Lists to store strokes and redo operations for undo/redo functionality */
    private readonly List<Stroke> _strokes = new();
    private readonly Stack<Stroke> _redoStack = new();
    private PathF _currentPath = new PathF();

    /* Inner class Stroke to store each stroke's points, color, stroke size, and shape */
    public class Stroke
    {
        public List<PointF> Points { get; set; } = new List<PointF>(); // Stores the points of the stroke
        public string ColorHex { get; set; } = "#000000";  // Stores the color as a hex string for serialization
        public float StrokeSize { get; set; } // Stores the brush size for the stroke
        public ShapeType Shape { get; set; }  // Stores the shape type (e.g., Line, Circle, etc.)

        [JsonIgnore]  // Prevent this property from being serialized directly

        /* Property to convert color to/from hex string for JSON serialization purposes */
        public Color Color
        {
            get => Color.FromArgb(ColorHex); // Converts hex color to Color object
            set => ColorHex = value.ToHex(); // Converts Color to hex string for serialization
        }
    }

    /* Method to set the stroke size for future strokes */
    public void SetStrokeSize(float size)
    {
        _currentStrokeSize = size;
    }

    /* Initializes a new stroke with start point, color, shape, and stroke size */
    public void StartNewStroke(PointF start, Color color, ShapeType shape, float strokeSize)
    {
        _startPoint = start; // Capture the start point for the stroke
        _currentColor = color; // Set the current color for the stroke
        _currentShape = shape; // Set the current shape type
        _currentStrokeSize = strokeSize; // Set the current stroke size

        if (shape == ShapeType.Draw) // If freehand drawing, start tracking points
        {
            _freehandPoints.Clear(); // Clear points from previous freehand drawing
            _freehandPoints.Add(start);
        }
        else
        {
            _currentPath = new PathF(); // For other shapes, prepare a new pat
        }
    }

    /* Updates the path for the current shape as the user drags; creates real-time drawing effect */
    public void UpdateCurrentShape(PointF endPoint)
    {
        _endPoint = endPoint; // Update the current endpoint of the shape

        if (_currentShape == ShapeType.Draw) // For freehand drawing
        {
            if (_freehandPoints.Count == 0 || Distance(_freehandPoints[^1], endPoint) >= PointDistanceThreshold)
            {
                _freehandPoints.Add(endPoint); // Add point if it meets distance threshold
                _currentPath = new PathF(); // Update the path with new points
                _currentPath.MoveTo(_freehandPoints[0].X, _freehandPoints[0].Y);

                for (int i = 1; i < _freehandPoints.Count; i++)
                {
                    _currentPath.LineTo(_freehandPoints[i].X, _freehandPoints[i].Y); // Draw path lines
                }

                _invalidateAction?.Invoke();  // Refresh canvas for real-time drawing
            }
            return;
        }

        _currentPath = new PathF();
    
        // Shape calculations are sourced from ChatGPT
        switch (_currentShape)
        {
            case ShapeType.Rectangle:
                _currentPath.MoveTo(_startPoint.X, _startPoint.Y);
                _currentPath.LineTo(_endPoint.X, _startPoint.Y); // Top edge
                _currentPath.LineTo(_endPoint.X, _endPoint.Y);   // Right edge
                _currentPath.LineTo(_startPoint.X, _endPoint.Y); // Bottom edge
                _currentPath.LineTo(_startPoint.X, _startPoint.Y); // Back to start to close explicitly
                _currentPath.Close(); // Ensure the rectangle is properly closed
                break;

            case ShapeType.Square:
                float sideLength = Math.Min(Math.Abs(_endPoint.X - _startPoint.X), Math.Abs(_endPoint.Y - _startPoint.Y));
                float xDirection = _endPoint.X >= _startPoint.X ? 1 : -1;
                float yDirection = _endPoint.Y >= _startPoint.Y ? 1 : -1;

                PointF topRight = new PointF(_startPoint.X + (sideLength * xDirection), _startPoint.Y);
                PointF bottomRight = new PointF(topRight.X, _startPoint.Y + (sideLength * yDirection));
                PointF bottomLeft = new PointF(_startPoint.X, bottomRight.Y);

                _currentPath.MoveTo(_startPoint.X, _startPoint.Y);
                _currentPath.LineTo(topRight.X, topRight.Y);
                _currentPath.LineTo(bottomRight.X, bottomRight.Y);
                _currentPath.LineTo(bottomLeft.X, bottomLeft.Y);
                _currentPath.Close(); // Explicitly close the square path
                break;

            case ShapeType.Circle:
                float radius = Distance(_startPoint, _endPoint) / 2;
                if (radius <= 0)
                {
                    _currentPath = null;
                    return;
                }

                PointF center = new PointF((_startPoint.X + _endPoint.X) / 2, (_startPoint.Y + _endPoint.Y) / 2);
                int segments = 100;
                float angleStep = 2 * MathF.PI / segments;

                _currentPath.MoveTo(center.X + radius, center.Y);

                for (int i = 1; i <= segments; i++)
                {
                    float angle = i * angleStep;
                    float x = center.X + radius * MathF.Cos(angle);
                    float y = center.Y + radius * MathF.Sin(angle);
                    _currentPath.LineTo(x, y);
                }

                _currentPath.Close(); // Ensure the circle is properly closed
                break;

            case ShapeType.Triangle:
                float sideLengthTriangle = Distance(_startPoint, _endPoint);

                PointF vertex1 = _startPoint;
                PointF vertex2 = new PointF(_startPoint.X + sideLengthTriangle, _startPoint.Y);
                PointF vertex3 = new PointF(_startPoint.X + sideLengthTriangle / 2, _startPoint.Y - (float)(Math.Sqrt(3) * sideLengthTriangle / 2));

                _currentPath.MoveTo(vertex1.X, vertex1.Y);
                _currentPath.LineTo(vertex2.X, vertex2.Y);
                _currentPath.LineTo(vertex3.X, vertex3.Y);
                _currentPath.Close(); // Explicitly close the triangle
                break;

            case ShapeType.Hexagon:
                float hexRadius = Distance(_startPoint, _endPoint) / 2;
                PointF hexCenter = new PointF((_startPoint.X + _endPoint.X) / 2, (_startPoint.Y + _endPoint.Y) / 2);

                for (int i = 0; i < 6; i++)
                {
                    float angle = MathF.PI / 3 * i;
                    float x = hexCenter.X + hexRadius * MathF.Cos(angle);
                    float y = hexCenter.Y + hexRadius * MathF.Sin(angle);

                    if (i == 0)
                        _currentPath.MoveTo(x, y);
                    else
                        _currentPath.LineTo(x, y);
                }
                _currentPath.Close(); // Explicitly close the hexagon
                break;

            case ShapeType.Line:
                _currentPath.MoveTo(_startPoint.X, _startPoint.Y);
                _currentPath.LineTo(_endPoint.X, _endPoint.Y);
                break;
        }

        _invalidateAction?.Invoke(); // Refresh the canvas to show the updated shape
    }


    /* Finalizes the stroke by adding it to strokes list with its color and points */
    public void EndCurrentStroke()
    {
        if (_currentShape == ShapeType.Draw)
        {
            // Add all the points from the freehand drawing as a new Stroke
            if (_freehandPoints.Count > 1)
            {
                _strokes.Add(new Stroke
                {
                    Points = new List<PointF>(_freehandPoints), // Copy the points to the Stroke
                    Color = _currentColor,  // Use the current color
                    StrokeSize = _currentStrokeSize,
                    Shape = _currentShape
                });
            }
        }
        else
        {
            // For other shapes, calculate and add the key points
            // Shape calculations are sourced from ChatGPT
            var points = new List<PointF>();
            switch (_currentShape)
            {
                case ShapeType.Line:
                    points.Add(_startPoint);
                    points.Add(_endPoint);
                    break;

                case ShapeType.Rectangle:
                    points.Add(_startPoint);
                    points.Add(new PointF(_endPoint.X, _startPoint.Y)); // Top-right
                    points.Add(_endPoint); // Bottom-right
                    points.Add(new PointF(_startPoint.X, _endPoint.Y)); // Bottom-left
                    points.Add(_startPoint); // Closing the rectangle
                    break;

                case ShapeType.Square:
                    float sideLength = Math.Min(Math.Abs(_endPoint.X - _startPoint.X), Math.Abs(_endPoint.Y - _startPoint.Y));
                    float xDirection = _endPoint.X >= _startPoint.X ? 1 : -1;
                    float yDirection = _endPoint.Y >= _startPoint.Y ? 1 : -1;

                    PointF topRight = new PointF(_startPoint.X + (sideLength * xDirection), _startPoint.Y);
                    PointF bottomRight = new PointF(topRight.X, _startPoint.Y + (sideLength * yDirection));
                    PointF bottomLeft = new PointF(_startPoint.X, bottomRight.Y);

                    points.Add(_startPoint);
                    points.Add(topRight);
                    points.Add(bottomRight);
                    points.Add(bottomLeft);
                    points.Add(_startPoint); // Closing the square
                    break;

                case ShapeType.Circle:
                    // Representing a circle as multiple points along its circumference
                    float radius = Distance(_startPoint, _endPoint) / 2;
                    if (radius > 0)
                    {
                        PointF center = new PointF((_startPoint.X + _endPoint.X) / 2, (_startPoint.Y + _endPoint.Y) / 2);
                        int segments = 100; // Number of segments to represent the circle
                        float angleStep = 2 * MathF.PI / segments;

                        for (int i = 0; i < segments; i++)
                        {
                            float angle = i * angleStep;
                            float x = center.X + radius * MathF.Cos(angle);
                            float y = center.Y + radius * MathF.Sin(angle);
                            points.Add(new PointF(x, y));
                        }
                        points.Add(points[0]); // Closing the circle
                    }
                    break;

                case ShapeType.Triangle:
                    float sideLengthTriangle = Distance(_startPoint, _endPoint);
                    PointF vertex1 = _startPoint; // Bottom-left point (starting point of the drag)
                    PointF vertex2 = new PointF(_startPoint.X + sideLengthTriangle, _startPoint.Y); // Bottom-right point
                    PointF vertex3 = new PointF(_startPoint.X + sideLengthTriangle / 2, _startPoint.Y - (float)(Math.Sqrt(3) * sideLengthTriangle / 2)); // Top point

                    points.Add(vertex1);
                    points.Add(vertex2);
                    points.Add(vertex3);
                    points.Add(vertex1); // Closing the triangle
                    break;

                case ShapeType.Hexagon:
                    float hexRadius = Distance(_startPoint, _endPoint) / 2;
                    PointF hexCenter = new PointF((_startPoint.X + _endPoint.X) / 2, (_startPoint.Y + _endPoint.Y) / 2);
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathF.PI / 3 * i; // 60 degrees in radians for each vertex
                        float x = hexCenter.X + hexRadius * MathF.Cos(angle);
                        float y = hexCenter.Y + hexRadius * MathF.Sin(angle);
                        points.Add(new PointF(x, y));
                    }
                    points.Add(points[0]); // Closing the hexagon
                    break;
            }

            if (_currentShape != ShapeType.Draw)
            {
                _strokes.Add(new Stroke
                {
                    Points = points,
                    Color = _currentColor,  // Use the current color
                    StrokeSize = _currentStrokeSize,
                    Shape = _currentShape
                });
            }
        }

        _currentPath = null;
        _redoStack.Clear();
    }

    /* Draw method to render all strokes on canvas and handle in-progress shape */
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        foreach (var stroke in _strokes) // Draw each saved stroke
        {
            canvas.StrokeColor = stroke.Color;
            canvas.StrokeSize = stroke.StrokeSize;

            if (stroke.Points.Count > 0)
            {
                var path = new PathF(); // Construct path for stroke points
                path.MoveTo(stroke.Points[0].X, stroke.Points[0].Y);

                for (int i = 1; i < stroke.Points.Count; i++)
                {
                    path.LineTo(stroke.Points[i].X, stroke.Points[i].Y);
                }

                // Explicitly close the shape, if needed
                if (stroke.Shape != ShapeType.Line && stroke.Shape != ShapeType.Draw)
                {
                    path.Close(); // Close path for shapes like rectangle
                }

                DrawShape(canvas, path, stroke.Shape); // Call DrawShape() for specific shape rendering
            }
        }

        // Draw the current shape in progress
        canvas.StrokeColor = _currentColor;
        canvas.StrokeSize = _currentStrokeSize;

        if (_currentShape == ShapeType.Draw && _currentPath != null)
        {
            canvas.DrawPath(_currentPath); // Draw freehand path
        }
        else if (_currentPath != null)
        {
            DrawShape(canvas, _currentPath, _currentShape); // Draw specific shapes in progress
        }
    }

    /* DrawShape to render specific shapes on the canvas (line, rectangle, etc.) */
    private void DrawShape(ICanvas canvas, PathF path, ShapeType shape)
    {
        if (shape == ShapeType.Draw || shape == ShapeType.Line || shape == ShapeType.Rectangle || shape == ShapeType.Triangle || shape == ShapeType.Square || shape == ShapeType.Hexagon || shape == ShapeType.Circle)
        {
            canvas.DrawPath(path); // Renders the path on the canvas for all shape types
        }
    }


    /* Utility method to calculate the distance between two points */
    private float Distance(PointF p1, PointF p2)
    {
        return (float)Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
    }

    /* Clears all strokes from the canvas and resets redo stack */
    public void Clear()
    {
        _strokes.Clear();
        _redoStack.Clear();
        _currentPath = null;
    }

    /* Undo the last stroke by moving it to the redo stack */
    public void Undo()
    {
        if (_strokes.Count > 0)
        {
            var lastStroke = _strokes[^1];
            _strokes.RemoveAt(_strokes.Count - 1);
            _redoStack.Push(lastStroke);
        }
    }

    /* Redo the last undone stroke by retrieving it from the redo stack */
    public void Redo()
    {
        if (_redoStack.Count > 0)
        {
            var strokeToRedo = _redoStack.Pop();
            _strokes.Add(strokeToRedo);
        }
    }

    /* Getter for the strokes list; used for saving or reloading */
    public List<Stroke> GetStrokes()
    {
        return _strokes;
    }
    /* Setter for the strokes list; clears and sets strokes for reloading saved state */
    public void SetStrokes(List<Stroke> strokes)
    {
        _strokes.Clear();
        foreach (var stroke in strokes)
        {
            // No need to recreate a PathF; just add the stroke as it is.
            _strokes.Add(stroke);
        }
        _redoStack.Clear();
    }

    /* Getter for the current stroke size */
    public float GetStrokeSize()
    {
        return _currentStrokeSize;
    }
}