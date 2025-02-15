using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DemoPhotoBooth.Models
{
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
    }
}
