using System.Text.Json.Serialization;

namespace DemoPhotoBooth.Models.DTOs
{
    public class CalculatePaymentPayload
    {
        [JsonPropertyName("layout_id")]
        public int LayoutId { get; set; }
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
        [JsonPropertyName("payment_provider")]
        public string PaymentProvider { get; set; }

        [JsonPropertyName("payment_id")]
        public int PaymentId { get; set; }

        [JsonPropertyName("real_price")]
        public decimal TotalPrice { get; set; }
    }
}
