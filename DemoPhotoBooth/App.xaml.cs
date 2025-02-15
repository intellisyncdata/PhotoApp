using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;

namespace DemoPhotoBooth
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IEventAggregator EventAggregator { get; } = new EventAggregator();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            RenderOptions.ProcessRenderMode = RenderMode.Default;
            // Cấu hình SharpVectors
            WpfDrawingSettings settings = new WpfDrawingSettings
            {
                IncludeRuntime = false,
                TextAsGeometry = false,
                OptimizePath = true,    
                EnsureViewboxSize = true 
            };
        }
    }

}
