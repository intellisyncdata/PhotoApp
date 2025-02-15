using DemoPhotoBooth.Models.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DemoPhotoBooth.Models.Entities
{
    public class PhotoApp
    {
        public PhotoApp()
        {
            
        }

        public PhotoApp(DTOs.PhotoApp itm)
        {
            Id = itm.Id;
            Code = itm.Code;
            Name = itm.Name;
            FontColor = itm.FontColor;
            Background = itm.Background;
            PaymentMethods = JsonSerializer.Serialize(itm.PaymentMethods);
            DownloadMediaTypes = JsonSerializer.Serialize(itm.DownloadMediaTypes);
            MaxPrints = itm.MaxPrints;
            Token = itm.Token;
        }

        [Key]
        public uint Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Background { get; set; }
        public string FontColor { get; set; }
        public string PaymentMethods { get; set; }
        public string DownloadMediaTypes { get; set; }
        public int MaxPrints { get; set; }
        public string Token { get; set; }
    }
}
