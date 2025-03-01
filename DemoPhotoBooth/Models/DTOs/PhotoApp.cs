using DemoPhotoBooth.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DemoPhotoBooth.Models.DTOs
{
    public class ConfigPhotoApp
    {
        [JsonPropertyName("bg_main")]
        public string BgMain { get; set; }
        [JsonPropertyName("bg_layout")]
        public string BgLayout { get; set; }
        [JsonPropertyName("bg_theme")]
        public string BgTheme { get; set; }
        [JsonPropertyName("bg_payment")]
        public string BgPayment { get; set; }
        [JsonPropertyName("bg_camera_mode")]
        public string BgCameraMode { get; set; }
        [JsonPropertyName("bg_frame_horizontal")]
        public string BgFrameHorizontal { get; set; }
        [JsonPropertyName("bg_frame_vertical")]
        public string BgFrameVertical { get; set; }
        [JsonPropertyName("bg_preview_horizontal")]
        public string BgPreviewHorizontal { get; set; }
        [JsonPropertyName("bg_preview_vertical")]
        public string BgPreviewVertical { get; set; }
        [JsonPropertyName("bg_print")]
        public string BgPrint { get; set; }
    }
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
        [JsonPropertyName("config_photo_app")]
        public ConfigPhotoApp configPhotoApp { get; set; } = new ConfigPhotoApp();
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
