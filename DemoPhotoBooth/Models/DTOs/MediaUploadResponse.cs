using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DemoPhotoBooth.Models.DTOs
{
    public class MediaUploadResponse
    {
        [JsonPropertyName("web_medias_url")]
        public string MediaUrl { get; set; }
    }
}
