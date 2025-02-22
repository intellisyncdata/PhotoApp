using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace DemoPhotoBooth.Models
{
    public class BgLayout : INotifyPropertyChanged
    {
        [JsonPropertyName("id")]
        public int id { get; set; }
        [JsonPropertyName("width")]
        public int width { get; set; }
        [JsonPropertyName("height")]
        public int height { get; set; }
        [JsonPropertyName("frame_type")]
        public string frame_typed { get; set; }
        [JsonPropertyName("image_url")]
        public string image_url { get; set; }

        public override string ToString()
        {
            return string.Empty;
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ThemeType
    {
        [JsonPropertyName("id")]
        public int id { get; set; }
        [JsonPropertyName("name")]
        public string name { get; set; }
    }
    public class Theme
    {
        [JsonPropertyName("id")]
        public int id { get; set; }
        [JsonPropertyName("theme_type")]
        public ThemeType theme_type { get; set; }
        [JsonPropertyName("bg_layouts")]
        public List<BgLayout> bg_layouts { get; set; }
    }
    public class Layout
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; } = string.Empty; 

        [JsonPropertyName("frame_type")]
        public string FrameType { get; set; } = string.Empty;

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("bg_color")]
        public List<string> BackgroundColor { get; set; } = new List<string>();

        [JsonPropertyName("bg_layouts")]
        public List<Background> BgLayouts { get; set; } = new List<Background>(); 

        [JsonPropertyName("paper_size")]
        public string PaperSize { get; set; }

        [JsonPropertyName("themes")]
        public List<Theme> themes { get; set; }
    }
}
