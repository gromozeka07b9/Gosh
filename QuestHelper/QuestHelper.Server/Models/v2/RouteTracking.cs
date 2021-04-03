using System;
using System.ComponentModel.DataAnnotations;

namespace QuestHelper.Server.Models.v2
{
    public class RouteTracking
    {
        public string Id { get; set; }
        public string RouteId { get; set; }
        public DateTime CreateDate { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Version { get; set; }
        public bool IsDeleted { get; set; }
        public int CountTrackPlaces { get; set; }
        public int CountTrackGeopoints { get; set; }
        public RouteTrackingPlace[] Places { get; set; }

        public class RouteTrackingPlace
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Address { get; set; }
            public string Category { get; set; }
            public string Distance { get; set; }
            public DateTime DateTimeBegin { get; set; }        
            public DateTime DateTimeEnd { get; set; }        
            public double? Latitude { get; set; }
            public double? Longitude { get; set; }
            public RouteTrackGeopoint[] Geopoints { get; set; }
            
            public class RouteTrackGeopoint
            {
                public DateTime DateTimePin { get; set; }        
                public double? Latitude { get; set; }
                public double? Longitude { get; set; }
                public double? Elevation { get; set; }
            }
        }
    }
}