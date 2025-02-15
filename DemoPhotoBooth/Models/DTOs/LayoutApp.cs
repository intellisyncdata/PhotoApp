using DemoPhotoBooth.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoPhotoBooth.Models.DTOs
{
    public class LayoutApp
    {
        public LayoutApp()
        {

        }

        public LayoutApp(Entities.LayoutApp itm)
        {
            Id = itm.Id;
            LayoutImage = itm.LayoutImage;
            BackgroudImage = itm.BackgroudImage;
            Color = itm.Color;
            Width = itm.Width;
            Height = itm.Height;
            Quantity = itm.Quantity;
            FrameType = itm.FrameType;
            PaymentId = itm.PaymentId;
        }

        public int Id { get; set; }
        public string LayoutImage { get; set; }
        public string? BackgroudImage { get; set; }
        public string? Color { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public int? Quantity { get; set; }
        public int? PaymentId { get; set; }
        public string FrameType { get; set; }
        public string? QRLink { get; set; }
        public string? ImageFolderPath { get; set; }
        public string? SVGMappingName { get; set; }
        public string? PrintName { get; set; } = "DS-RX1";
        public int PrintQuantity { get; set; } = 2;
        public bool IsSelected { get; set; } = false;
        public bool IsBackgroundColor { get; set; } = false;
    }
}
