using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DemoPhotoBooth.Models.DTOs
{
    public class PingPayload
    {
        [JsonPropertyName("printer_paper_count")]
        public int PrinterCount { get; set; }
        [JsonPropertyName("printer")]
        public bool Printer { get; set; } = true;
        [JsonPropertyName("camera")]
        public bool Camera { get; set; } = true;
        [JsonPropertyName("bill_acceptor")]
        public bool BillAcceptor { get; set; } = true;
    }
}
