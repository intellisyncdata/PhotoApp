using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace DemoPhotoBooth.Common
{
    public static class SvgToBitmapHelper
    {
        public static BitmapSource ConvertSvgToBitmap(string svgPath, int width, int height)
        {
            if (!File.Exists(svgPath))
                throw new FileNotFoundException("SVG file not found!", svgPath);

            // Load the SVG file
            var settings = new WpfDrawingSettings
            {
                IncludeRuntime = true,
                TextAsGeometry = true
            };
            var reader = new FileSvgReader(settings);

            DrawingGroup drawingGroup;
            using (var stream = new FileStream(svgPath, FileMode.Open, FileAccess.Read))
            {
                drawingGroup = reader.Read(stream);
            }

            // Create a drawing visual to render the SVG
            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawDrawing(drawingGroup);
            }

            // Convert to BitmapSource
            var bitmap = new RenderTargetBitmap(
                width,
                height,
                72, // DPI X
                72, // DPI Y
                PixelFormats.Pbgra32);
            bitmap.Render(drawingVisual);

            return bitmap;
        }
    }
}
