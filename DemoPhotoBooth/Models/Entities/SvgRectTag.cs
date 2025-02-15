using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoPhotoBooth.Models.Entities
{
    public class SvgRectTag
    {
        [Key]
        public uint Id { get; set; }
        public uint SvgInforId { get; set; }
        public uint No { get; set; }
        public string Name { get; set; }
        public decimal X { get; set; }
        public decimal Y { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public bool IsQRRect { get; set; }
        public virtual SvgInfor SvgInfor { get; set; }
    }
}
