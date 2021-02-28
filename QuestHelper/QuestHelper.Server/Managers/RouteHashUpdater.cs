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
        
        public List<RouteVersion> Calc(List<Models.Route> routes)
        {
            List<RouteVersion> routeVersions;

            using (var db = new ServerDbContext(_dbOptions))
            {
                routeVersions = routes.Select(r => new RouteVersion() {Id = r.RouteId, Version = r.Version, ObjVerHash = r.VersionsHash, ObjVer = r.VersionsList}).ToList();
                var pointIds = db.RoutePoint.Where(p => routeVersions.Any(r => r.Id == p.RouteId))
                    .Select(p => new {p.RouteId, p.RoutePointId, p.Version}).ToList();
                var mediaIds = db.RoutePointMediaObject
                    .Where(m => pointIds.Any(p => p.RoutePointId == m.RoutePointId))
                    .Select(m => new {m.RoutePointMediaObjectId, m.Version, m.RoutePointId})
                    .ToList();
                foreach (var route in routeVersions)
                {
                    if (string.IsNullOrEmpty(route.ObjVerHash))
                    {
                        StringBuilder versions = new StringBuilder();
                        versions.Append(route.Version.ToString());
                        var routePoints = pointIds.Where(p => p.RouteId == route.Id).Select(p => new { p.Version, p.RoutePointId })
                            .OrderBy(p => p.RoutePointId);
                        foreach (var item in routePoints)
                        {
                            versions.Append(item.Version.ToString());
                        }

                        var mediaVersions = mediaIds.Where(m => routePoints.Any(p => p.RoutePointId == m.RoutePointId))
                            .OrderBy(m => m.RoutePointMediaObjectId).Select(m => m.Version);
                        foreach (int version in mediaVersions)
                        {
                            versions.Append(version.ToString());
                        }

                        route.ObjVer = versions.ToString();
                        route.ObjVerHash = HashGenerator.Generate(route.ObjVer);

                        _routeManager.SetHash(route.Id, route.ObjVerHash, route.ObjVer);
                    }
                }
            }

            return routeVersions;
        }

    }
}