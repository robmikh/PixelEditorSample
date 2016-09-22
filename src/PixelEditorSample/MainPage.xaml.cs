using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PixelEditorSample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Color[,] _colors;
        private Color _currentColor;
        private bool _isMouseDown;

        public MainPage()
        {
            this.InitializeComponent();

            // Initialization
            _colors = new Color[200, 200];
            for(int i = 0; i < _colors.GetLength(0); i++)
            {
                for (int j = 0; j < _colors.GetLength(1); j++)
                {
                    _colors[i, j] = Colors.Transparent;
                }
            }

            // While this stays red the entire time in this sample, you
            // could create a little color picker to change the color
            // that you want to draw with.
            _currentColor = Colors.Red;

            _isMouseDown = false;
        }

        private void EditorCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            Draw(args.DrawingSession);
        }

        private void EditorCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _isMouseDown = true;
            ChangeColorAtPoint(e.GetCurrentPoint(EditorCanvas).Position);
        }

        private void EditorCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_isMouseDown)
            {
                ChangeColorAtPoint(e.GetCurrentPoint(EditorCanvas).Position);
            }
        }

        private void EditorCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _isMouseDown = false;
        }

        private void EditorCanvas_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            _isMouseDown = false;
        }

        private void ChangeColorAtPoint(Point point)
        {
            // Change the color according to the current color
            int x = (int)point.X;
            int y = (int)point.Y;
            _colors[y, x] = _currentColor;

            // This forces us to redraw
            EditorCanvas.Invalidate(); 
        }

        // The main workhorse for our drawing operations
        private void Draw(CanvasDrawingSession drawingSession)
        {
            for (int i = 0; i < _colors.GetLength(0); i++)
            {
                for (int j = 0; j < _colors.GetLength(1); j++)
                {
                    var color = _colors[i, j];

                    if (color != Colors.Transparent)
                    {
                        drawingSession.FillRectangle(new Rect(j, i, 1, 1), color);
                    }
                }
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Here we have to create a CanvasRenderTarget and then draw
            // our content there. Then we can save that as an image. This
            // is why we abstracted the draw away from our CanvasControl's
            // event handler.

            // Get our storage file
            var file = await GetTargetFileAsync();

            // If the user cancels or does not wish to save the image, then
            // our file object should be null. Don't continue if that's the
            // case.
            if (file == null) return;

            // Get the CanvasDevice (You can think of this like a Direct3D device)
            var canvasDevice = CanvasDevice.GetSharedDevice();

            // Create our render target
            using (var renderTarget = new CanvasRenderTarget(canvasDevice, // Device
                                                             200,          // Width
                                                             200,          // Height
                                                             96))          // DPI (this doesn't matter 
                                                                           // as much sicne we aren't 
                                                                           // displaying this to the 
                                                                           // screen and are instead 
                                                                           // saving directly to an image)
            {
                // Draw to our render target
                using (var drawingSession = renderTarget.CreateDrawingSession())
                {
                    Draw(drawingSession);
                }

                // Save to an image
                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await renderTarget.SaveAsync(stream, CanvasBitmapFileFormat.Png);
                }
            }
        }

        private async Task<StorageFile> GetTargetFileAsync()
        {
            var filePicker = new FileSavePicker();
            filePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            filePicker.FileTypeChoices.Add("PNG", new List<string>() { ".png" });

            return await filePicker.PickSaveFileAsync();
        }

        // Don't forget to do this! You'll leak memory otherwise!
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            EditorCanvas.RemoveFromVisualTree();
            EditorCanvas = null;
        }
    }
}
