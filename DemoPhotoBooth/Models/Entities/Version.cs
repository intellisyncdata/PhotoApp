using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoPhotoBooth.Models.Entities
{
    public class Version
    {
        [Key]
        public long Id { get; set; }
        public string VersionNo { get; set; }
        public string PackageUrl { get; set; }
    }
}
