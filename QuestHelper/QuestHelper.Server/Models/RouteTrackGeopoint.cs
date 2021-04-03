using System;
using System.ComponentModel.DataAnnotations;

namespace QuestHelper.Server.Models
{
    public class RouteTrackGeopoint
    {
        [Key]
        [MaxLength(16)]
        public byte[] Id { get; set; }
        public byte[] RouteTrackPlaceId { get; set; }

        public DateTime DateTimePin { get; set; }        
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? Elevation { get; set; }
    }
}