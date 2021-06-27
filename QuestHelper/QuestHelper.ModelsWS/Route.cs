using System;
using System.Collections.Generic;

namespace QuestHelper.SharedModelsWS
{
    public class Route : ModelBase
    {
        public ICollection<RoutePoint> Points { get; set; }
        public Route()
        {
            Points = new List<RoutePoint>();
        }
        
        public string Name { get; set; }
        public DateTimeOffset? UpdateDate { get; set; }
        public string CreatorId { get; set; }
        public bool IsShared { get; set; }
        public bool IsPublished { get; set; }
        public string ImgFilename { get; set; }
        public string Description { get; set; }
        public string VersionsHash { get; set; }
        public string VersionsList { get; set; }
        public string PublicReferenceHash { get; set; }
        public string CreatorName { get; set; }
        public int PointCount { get; set; }
        public int LikeCount { get; set; }
        public bool LikedByCurrentUser { get; set; }
        public string LastComment { get; set; }
        public int DislikeCount { get; set; }
        public int ViewCount { get; set; }
        public string FirstImageName { get; set; } //убрать использование, только imgFilename! Но сейчас сайт использует.
        public int DistanceKm { get; set; }
        public int DistanceSt { get; set; }
    }
}
