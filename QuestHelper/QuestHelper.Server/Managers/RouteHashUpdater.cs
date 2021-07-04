using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QuestHelper.SharedModelsWS;

namespace QuestHelper.Server.Managers
{
    public class RouteHashUpdater
    {
        private RouteManager _routeManager;
        private DbContextOptions<ServerDbContext> _dbOptions;
        
        public RouteHashUpdater(DbContextOptions<ServerDbContext> dbOptions)
        {
            _dbOptions = dbOptions;
            _routeManager = new RouteManager(_dbOptions);
        }
        
        public List<RouteVersion> CalcAndGetHashForRoutes(List<Models.Route> routesForCalc)
        {
            List<RouteVersion> routeVersions = new List<RouteVersion>();
            var routeIdsCommon = routesForCalc.Select(r => r.RouteId);

            using (var db = new ServerDbContext(_dbOptions))
            {
                var routes = (from route in db.Route where routeIdsCommon.Contains(route.RouteId) select route).ToList();

                var routePointsCommon = (from point in db.RoutePoint
                    join route in db.Route on point.RouteId equals route.RouteId
                    where routeIdsCommon.Contains(point.RouteId)
                    select new
                    {
                        RoutePointId = point.RoutePointId,
                        RouteId = point.RouteId,
                        Version = point.Version,
                    });

                var routePoints = routePointsCommon.ToList();
                
                var medias = (from media in db.RoutePointMediaObject
                    join point in routePointsCommon on media.RoutePointId equals point.RoutePointId
                    select new
                    {
                        media.RoutePointMediaObjectId,
                        media.RoutePointId,
                        media.Version
                    }).ToList();

                foreach (var route in routes)
                {
                    if (string.IsNullOrEmpty(route.VersionsHash))
                    {
                        StringBuilder versions = new StringBuilder();
                        versions.Append(route.Version.ToString());

                        var currentRoutePoints = routePoints.Where(p => p.RouteId == route.RouteId);
                        currentRoutePoints
                            .Select(p => new { p.Version, p.RoutePointId })
                            .OrderBy(p => p.RoutePointId).ToList().ForEach(rp =>
                            {
                                versions.Append(rp.Version.ToString());
                            });
                        
                        medias.Where(m=> currentRoutePoints.Any(p=>p.RoutePointId.Equals(m.RoutePointId)))
                            .OrderBy(m => m.RoutePointMediaObjectId).ToList()
                            .ForEach(mrp =>
                            {
                                versions.Append(mrp.Version.ToString());
                            });
                        string versionsString = versions.ToString();
                        _routeManager.SetHash(route.RouteId, HashGenerator.Generate(versionsString), versionsString);
                    }
                }
                
            }

            using (var db = new ServerDbContext(_dbOptions))
            {
                (from route in db.Route where routeIdsCommon.Contains(route.RouteId) select route).ToList().ForEach(r =>
                {
                    routeVersions.Add(new RouteVersion()
                    {
                        Id = r.RouteId,
                        Version = r.Version,
                        ObjVer = r.VersionsList,
                        ObjVerHash = r.VersionsHash
                    });
                });
            }
            
            return routeVersions.ToList();
        }

    }
}