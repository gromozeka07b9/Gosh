using System;
using System.ComponentModel.DataAnnotations;

namespace QuestHelper.Server.Models
{
    public class RouteTrackPlace
    {
        [Key]
        [MaxLength(16)]
        public byte[] Id { get; set; }
        public byte[] RouteTrackId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string Category { get; set; }
        public string Distance { get; set; }
        public DateTime DateTimeBegin { get; set; }        
        public DateTime DateTimeEnd { get; set; }        
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? Elevation { get; set; }
    }
}