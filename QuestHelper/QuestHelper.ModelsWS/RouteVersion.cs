using System;
using System.Collections.Generic;

namespace QuestHelper.SharedModelsWS
{
    public class RouteVersion : ModelVersionBase
    {
        public ICollection<PointVersion> Points { get; set; }
        public string ObjVerHash { get; set; }
        public string ObjVer { get; set; }
        public RouteVersion()
        {
            Points = new List<PointVersion>();
        }
    }
}
