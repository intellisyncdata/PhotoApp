using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DemoPhotoBooth.Models.DTOs
{
    public record VersionDTO
    {
        [JsonPropertyName("action")]
        public string Action { get; init; }
        [JsonPropertyName("package_url")]
        public string PackageUrl { get; init; }
        [JsonPropertyName("version")]
        public string Version { get; init; }
        [JsonPropertyName("message")]
        public string Message { get; init; }
    }
}
