using DemoPhotoBooth.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DemoPhotoBooth.Models.DTOs
{
    public class PhotoApp
    {
        public PhotoApp()
        {
            
        }

        public PhotoApp(Entities.PhotoApp itm)
        {
            Id = itm.Id;
            Code = itm.Code;
            Name = itm.Name;
            //Background = itm.Background;
            FontColor = itm.FontColor;
            PaymentMethods = JsonSerializer.Deserialize<List<string>>(itm.PaymentMethods);
            DownloadMediaTypes = JsonSerializer.Deserialize<List<string>>(itm.DownloadMediaTypes);
            MaxPrints = itm.MaxPrints;
            Token = itm.Token;
        }

        [JsonPropertyName("id")]
        public uint Id { get; set; }
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        //[JsonPropertyName("background")]
        //public string Background { get; set; }
        [JsonPropertyName("font_color")]
        public string FontColor { get; set; }
        [JsonPropertyName("payment_method")]
        public List<string> PaymentMethods { get; set; }
        [JsonPropertyName("download_media_type")]
        public List<string> DownloadMediaTypes { get; set; }
        [JsonPropertyName("max_prints")]
        public int MaxPrints { get; set; }
        [JsonPropertyName("access_token")]
        public string Token { get; set; }
    }
}
