using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuestHelper.SharedModelsWS
{
    public class RouteRoot
    {
        public Route Route { get; set; }

        public RouteRoot()
        {
            Route = new Route();
        }

    }
}
