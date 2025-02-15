using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoPhotoBooth.Models.DTOs
{
    public class PreviewGrid
    {
        public int No { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public decimal X { get; set; }
        public decimal Y { get; set; }
        public string Uri { get; set; }
    }
}
