using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class Airport
    {
        [JsonPropertyName("code")]
        public string IataCode { get; set; }

        public string Name { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        // Include the type field for filtering
        public string Type { get; set; }
    }
}
