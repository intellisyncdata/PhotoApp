using System.ComponentModel.DataAnnotations;

namespace DemoPhotoBooth.Models.Entities
{
    public class SvgInfor
    {
        [Key]
        public uint Id { get; set; }
        public string Name { get; set; }
        public bool IsLandscape { get; set; } = false;
        public bool IsCutted { get; set; } = false;
        public decimal PrintWidth { get; set; } = 400;
        public double ActualWidth { get; set; } = 400;
        public decimal PrintHeight { get; set; } = 600;
        public double ActualHeight { get; set; } = 600;
        public virtual IEnumerable<SvgRectTag> SvgRectTags { get; set; }
    }
}
