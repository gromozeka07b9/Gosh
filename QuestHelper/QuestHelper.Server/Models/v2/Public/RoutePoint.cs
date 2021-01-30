using System;
using System.ComponentModel.DataAnnotations;

namespace QuestHelper.Server.Models.v2.Public
{
    public class RoutePoint
    {
        public RoutePoint()
        {
        }
        public string RoutePointId { get; set; }
        public string RouteId { get; set; }
        public string Name { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdatedUserId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public int Version { get; set; }
        public bool IsDeleted { get; set; }
        public MediaObject[] Medias { get; set; }
    }
}
