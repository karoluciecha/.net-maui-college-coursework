using System.Text;
using CommunityToolkit.Maui.Storage;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimplePaint
{
    /* MainPage class definition, inheriting from ContentPage which represents a page in the app */
    public partial class MainPage : ContentPage
    {
        /* Private fields to manage drawing state, color, shape, drawable instance, and file paths */
        private bool _isDrawing;
        private Color _currentColor = Colors.Black;
        private ShapeType _currentShape = ShapeType.Draw;
        private LineDrawable _drawable;
        private readonly string _saveFilePath;

        /* Constant to define the file name for storing user settings in JSON format */
        private const string SettingsFileName = "settings.json";
        private string _settingsFilePath;

        /* Inner Settings class to store user preferences like color, brush size, and shape */
        private class Settings
        {
            public string ColorHex { get; set; } = "#000000";
            public float BrushSize { get; set; }
            public ShapeType Shape { get; set; }
        }

        /* Enum ShapeType to define possible drawing shapes */
        public enum ShapeType
        {
            Draw,
            Line,
            Rectangle,
            Circle,
            Triangle,
            Square,
            Hexagon
        }

        /* Constructor for MainPage; initializes components, sets up drawable canvas and paths, and loads settings/canvas */
        public MainPage()
        {
            InitializeComponent();
            _drawable = new LineDrawable(CanvasView.Invalidate);
            CanvasView.Drawable = _drawable;

            // Determine the save file path (e.g., application data folder)
            _saveFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "canvasData.json");
            _settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SettingsFileName);

            // Apply default values only if loading fails
            if (!LoadSettings())
            {
                _currentColor = Colors.Black;
                _drawable.SetStrokeSize(1.0f);
                _currentShape = ShapeType.Draw;
            }

            // Load saved strokes if available
            LoadCanvasState();
        }

        /* Overridden method called when page is disappearing; saves settings and canvas state */
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            SaveSettings();
            SaveCanvasState(_saveFilePath);
        }

        /* Method to save user settings like color, brush size, and shape to a JSON file */
        private void SaveSettings()
        {
            try
            {
                var settings = new Settings
                {
                    ColorHex = _currentColor.ToHex(),
                    BrushSize = _drawable.GetStrokeSize(),
                    Shape = _currentShape
                };
                var json = JsonSerializer.Serialize(settings);
                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception) { }
        }

        /* Method to load settings from a JSON file; if not found or fails, default settings are applied */
        private bool LoadSettings()
        {
            if (File.Exists(_settingsFilePath))
            {
                try
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    var settings = JsonSerializer.Deserialize<Settings>(json);

                    if (settings != null)
                    {
                        _currentColor = Color.FromArgb(settings.ColorHex);
                        _drawable.SetStrokeSize(settings.BrushSize);
                        _currentShape = settings.Shape;
                        BrushSizeSlider.Value = settings.BrushSize; // Update slider to reflect saved brush size
                        return true; // Settings loaded successfully
                    }
                }
                catch (Exception) { }
            }
            return false;
        }

        /* Event handler for start of drawing interaction; initializes new stroke with starting point and color */
        private void CanvasView_StartInteraction(object? sender, TouchEventArgs e)
        {
            PointF startPoint = e.Touches[0];
            _drawable.StartNewStroke(startPoint, _currentColor, _currentShape, _drawable.GetStrokeSize());
            _isDrawing = true;
        }

        /* Event handler for drawing while dragging; updates stroke and refreshes canvas view */
        private void CanvasView_DragInteraction(object? sender, TouchEventArgs e)
        {
            if (_isDrawing)
            {
                PointF currentPoint = e.Touches[0];
                _drawable.UpdateCurrentShape(currentPoint);
                CanvasView.Invalidate(); // Refresh to show the new line in real time
            }
        }

        /* Event handler for end of drawing interaction; finalizes stroke and refreshes canvas */
        private void CanvasView_EndInteraction(object? sender, TouchEventArgs e)
        {
            _isDrawing = false;
            _drawable.EndCurrentStroke();
            CanvasView.Invalidate();
        }

        /* Method to set the current drawing color based on button selection */
        private void SetColor(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                _currentColor = button.BackgroundColor;
                ColorMenu.IsVisible = false;
            }
        }

        /* Event handler to clear all drawings from the canvas */
        private void ClearButton_Clicked(object sender, EventArgs e)
        {
            _drawable.Clear();
            CanvasView.Invalidate();
        }

        /* Event handler to update brush size based on slider value */
        private void BrushSizeSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            _drawable.SetStrokeSize((float)e.NewValue);
        }

        /* Event handler for Undo/Redo actions; undoes or redoes the last stroke */
        private void UndoRedoButton_Clicked(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Text == "Undo")
                    _drawable.Undo();
                else if (button.Text == "Redo")
                    _drawable.Redo();
            }
            CanvasView.Invalidate();
        }

        /* Event handler for Save/Load actions; saves or loads canvas data to/from a JSON file */
        private async void SaveLoadButton_Clicked(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Text == "Save")
                {
                    try
                    {
                        // Predefine the file name without user input
                        string fileName = "canvasData.json";

                        // Serialize the canvas data to JSON
                        var strokes = _drawable.GetStrokes();
                        var options = new JsonSerializerOptions
                        {
                            ReferenceHandler = ReferenceHandler.Preserve,
                            WriteIndented = true
                        };
                        var json = JsonSerializer.Serialize(strokes, options);

                        /* Use MemoryStream to save data
                        Importing necessary namespaces for functionalities like storage, serialization, and UI components */
                        using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                        {
                            // Use FileSaver to save the file to the user-specified location
                            var saveResult = await FileSaver.Default.SaveAsync(fileName, memoryStream);

                            if (saveResult != null)
                            {
                                await DisplayAlert("Save", "Canvas state saved successfully!", "OK");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Error", $"Failed to save canvas state: {ex.Message}", "OK");
                    }
                }
                else if (button.Text == "Load")
                {
                    try
                    {
                        var fileResult = await FilePicker.Default.PickAsync(new PickOptions
                        {
                            PickerTitle = "Select a canvas file to load",
                            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.WinUI, new[] { ".json" } },
                        { DevicePlatform.MacCatalyst, new[] { ".json" } },
                        { DevicePlatform.iOS, new[] { ".json" } },
                        { DevicePlatform.Android, new[] { "application/json" } }
                    })
                        });

                        if (fileResult != null && File.Exists(fileResult.FullPath))
                        {
                            LoadCanvasState(fileResult);
                            await DisplayAlert("Load", "Canvas state loaded successfully!", "OK");
                        }
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Error", $"Failed to load canvas state: {ex.Message}", "OK");
                    }
                }
            }
        }


        /* Method to toggle visibility of color or shape selection menus */
        private void ToggleMenuVisibility(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Text == "Colors") ColorMenu.IsVisible = !ColorMenu.IsVisible;
                else if (button.Text == "Shapes") ShapeMenu.IsVisible = !ShapeMenu.IsVisible;
            }
        }

        /* Method to set the current shape for drawing based on button selection */
        private void SetShape(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                string shapeText = button.Text;

                _currentShape = shapeText switch
                {
                    "Draw" => ShapeType.Draw,
                    "Rectangle" => ShapeType.Rectangle,
                    "Circle" => ShapeType.Circle,
                    "Triangle" => ShapeType.Triangle,
                    "Square" => ShapeType.Square,
                    "Hexagon" => ShapeType.Hexagon,
                    _ => ShapeType.Line
                };

                ShapeMenu.IsVisible = false;
            }
        }

        /* Method to save current canvas strokes to a JSON file */
        private void SaveCanvasState(string filePath)
        {
            try
            {
                var strokes = _drawable.GetStrokes();
                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.Preserve,
                    WriteIndented = true
                };
                var json = JsonSerializer.Serialize(strokes, options);
                File.WriteAllText(filePath, json);
            }
            catch (Exception) { }
        }


        /* Method to load saved canvas strokes from the default file path */
        private void LoadCanvasState()
        {
            if (File.Exists(_saveFilePath))
            {
                try
                {
                    var json = File.ReadAllText(_saveFilePath);
                    var options = new JsonSerializerOptions
                    {
                        ReferenceHandler = ReferenceHandler.Preserve
                    };
                    var strokes = JsonSerializer.Deserialize<List<LineDrawable.Stroke>>(json, options);
                    if (strokes != null)
                    {
                        _drawable.SetStrokes(strokes);
                        CanvasView.Invalidate();
                    }
                }
                catch { }
            }
        }

        /* Method to load saved canvas strokes from a user-selected file */
        private void LoadCanvasState(FileResult fileResult)
        {
            try
            {
                var json = File.ReadAllText(fileResult.FullPath);
                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.Preserve
                };
                var strokes = JsonSerializer.Deserialize<List<LineDrawable.Stroke>>(json, options);
                if (strokes != null)
                {
                    _drawable.SetStrokes(strokes);
                    CanvasView.Invalidate();
                }
            }
            catch { }
        }
    }
}