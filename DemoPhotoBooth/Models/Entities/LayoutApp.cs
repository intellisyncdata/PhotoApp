using System.ComponentModel.DataAnnotations;

namespace DemoPhotoBooth.Models.Entities
{
    public class LayoutApp
    {
        public LayoutApp()
        {

        }

        public LayoutApp(DTOs.LayoutApp itm)
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

        [Key]
        public int Id { get; set; }
        public string LayoutImage { get; set; }
        public string? BackgroudImage { get; set; }
        public string? Color { get; set; }
        public string? QRLink { get; set; }
        public string? ImageFolderPath { get; set; }
        public string? SVGMappingName { get; set; }
        public string? PrintName { get; set; } = "DS-RX1";
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public int? Quantity { get; set; }
        public int PrintQuantity { get; set; } = 2;
        public int? PaymentId { get; set; }
        public bool IsSelected { get; set; } = false;
        public bool IsBackgroundColor { get; set; } = false;
        public string FrameType { get; set; }
    }
}
