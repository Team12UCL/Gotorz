using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shared.Models
{
    public class TravelGuide
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Destination { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        public string Language { get; set; } = "English";

        public List<TravelGuideSection> Sections { get; set; } = new List<TravelGuideSection>();

        public bool HasMap { get; set; }

        // Navigation properties
        public List<string> Tags { get; set; } = new List<string>();

        public string ThumbnailUrl { get; set; }
    }

    public class TravelGuideSection
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public int Order { get; set; }
    }
}