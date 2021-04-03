using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestHelper.Server.Managers;
using QuestHelper.Server.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using QuestHelper.Server.Models.v2;
using QuestHelper.SharedModelsWS;
using Route = QuestHelper.Server.Models.Route;

namespace QuestHelper.Server.Controllers.v2
{
    /// <summary>
    /// CRUD for routes
    /// </summary>
    [Authorize]
    [ServiceFilter(typeof(RequestFilter))]
    [Produces("application/json")]
    [Route("api/v2/[controller]")]
    public class RoutesController : Controller
    {
        private readonly DbContextOptions<ServerDbContext> _dbOptions;
        private readonly ILogger<RoutesController> _logger;
        private MediaManager _mediaManager;

        public RoutesController(IConfiguration configuration, ILogger<RoutesController> logger)
        {
            _dbOptions = ServerDbContext.GetOptionsContextDbServer(configuration);
            _logger = logger;
            _mediaManager = new MediaManager();
        }
        /// <summary>
        /// List all available routes for user
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get([FromQuery] PagingParameters pagingParameters)
        {
            FilterParameters filters = new FilterParameters(pagingParameters.Filter);
            int pageNumber = pagingParameters.IndexesRangeToPageNumber(pagingParameters.Range, pagingParameters.PageSize);
            int totalCountRows = 0;
            List<SharedModelsWS.Route> items = new List<SharedModelsWS.Route>();
            if(!string.IsNullOrEmpty(pagingParameters.Range))
            {
                string userId = IdentityManager.GetUserId(HttpContext);
                using (var db = new ServerDbContext(_dbOptions))
                {
                    var routeaccess = db.RouteAccess.Where(u => u.UserId == userId).Select(u => u.RouteId).ToList();
                    var withoutFilter = db.Route.Where(r => routeaccess.Contains(r.RouteId));
                    withoutFilter = filters.isFilterPresent("createDate") ? withoutFilter.Where(r => r.CreateDate.Equals(filters.GetDateTimeByName("createDate"))) : withoutFilter;
                    withoutFilter = filters.isFilterPresent("isPublished") ? withoutFilter.Where(r => r.IsPublished == filters.GetBooleanByName("isPublished")) : withoutFilter;
                    withoutFilter = filters.isFilterPresent("isDeleted") ? withoutFilter.Where(r => r.IsDeleted == filters.GetBooleanByName("isDeleted")) : withoutFilter;
                    withoutFilter = filters.isFilterPresent("isShared") ? withoutFilter.Where(r => r.IsShared == filters.GetBooleanByName("isShared")) : withoutFilter;
                    withoutFilter = filters.isFilterPresent("creatorId") ? withoutFilter.Where(r => r.CreatorId.Contains(filters.GetStringByName("creatorId"))) : withoutFilter;
                    withoutFilter = filters.isFilterPresent("name") ? withoutFilter.Where(r => r.Name.Contains(filters.GetStringByName("name"))) : withoutFilter;
                    withoutFilter = filters.isFilterPresent("description") ? withoutFilter.Where(r => r.Description.Contains(filters.GetStringByName("description"))) : withoutFilter;
                    if (filters.isFilterPresent("createDate"))
                    {
                        var cd = filters.GetDateTimeByName("createDate");
                        withoutFilter = withoutFilter.Where(r => r.CreateDate.Year.Equals(cd.Year) && r.CreateDate.Month.Equals(cd.Month) && r.CreateDate.Day.Equals(cd.Day));
                    }

                    totalCountRows = withoutFilter.Count();                    
                    var dbRoutes = withoutFilter.OrderBy(r => r.CreateDate).Skip((pageNumber - 1) * pagingParameters.PageSize).Take(pagingParameters.PageSize);
                    
                    items = dbRoutes.Select(route => new SharedModelsWS.Route()
                    {
                        Id = route.RouteId, 
                        PublicReferenceHash = db.RouteShare.Where(r=>r.RouteId.Equals(route.RouteId)).Select(r=>r.ReferenceHash).FirstOrDefault(),
                        CreateDate = route.CreateDate,
                        UpdateDate = db.RoutePoint.Where(rp=>!rp.IsDeleted && rp.RouteId.Equals(route.RouteId)).Select(rp=>rp.UpdateDate).DefaultIfEmpty(route.CreateDate).Max(),
                        CreatorId = route.CreatorId,
                        Description = route.Description,
                        ImgFilename = route.ImgFilename,
                        IsDeleted = route.IsDeleted,
                        IsPublished = route.IsPublished,
                        IsShared = route.IsShared,
                        Name = route.Name,
                        VersionsHash = route.VersionsHash,
                        VersionsList = route.VersionsList,
                        Version = route.Version,
                        CreatorName = (from users in db.User where users.UserId.Equals(route.CreatorId) select users.Name).FirstOrDefault(),
                        LikeCount = db.RouteLike.Count(r=>r.RouteId.Equals(route.RouteId) && r.IsLike == 1),
                        DislikeCount = db.RouteLike.Count(r=>r.RouteId.Equals(route.RouteId) && r.IsLike == 0),
                        ViewCount = db.RouteView.Count(r=>r.RouteId.Equals(route.RouteId)),
                        PointCount = db.RoutePoint.Count(rp=>rp.RouteId.Equals(route.RouteId) && !rp.IsDeleted),
                        DistanceKm = 0,
                        DistanceSt = 0
                    }).ToList();
                }
            }
            HttpContext.Response.Headers.Add("x-total-count", totalCountRows.ToString());
            HttpContext.Response.Headers.Add("Access-Control-Expose-Headers", "x-total-count");
            return new ObjectResult(items);
        }

        [HttpGet("covers/list")]
        [ProducesResponseType(200)]
        public IActionResult GetRoutesCovers([FromBody]RouteIdArray routes)
        {
            List<RouteCover> covers = new List<RouteCover>();
            string userId = IdentityManager.GetUserId(HttpContext);
            long filesize = 0;
            using (var db = new ServerDbContext(_dbOptions))
            {
                var localRoutes = db.Route.Where(r => routes.IdArray.Contains(r.RouteId) && !r.IsDeleted && !string.IsNullOrEmpty(r.ImgFilename)).Select(r => new {r.RouteId, r.ImgFilename});
                foreach (var route in localRoutes)
                {
                    _logger.LogInformation($"cover getting for routeId:{route.RouteId}");
                    var cover = new RouteCover()
                    {
                        RouteId = route.RouteId, 
                        ImgCoverFilename = string.IsNullOrEmpty(route.ImgFilename) ? String.Empty : route.ImgFilename 
                    };
                    cover.ImgCover = getImageBase64(cover.ImgCoverFilename);
                    covers.Add(cover);
                    filesize += cover.ImgCover.Length;
                }               
            }

            _logger.LogInformation($"cover getting files uncompressed total size:{filesize}");
            return new ObjectResult(covers);
        }
        private string getImageBase64(string filename)
        {
            string base64 = String.Empty;
            _logger.LogInformation($"cover getting file:{filename}");

            if (_mediaManager.MediaFileExist(filename))
            {
                _logger.LogInformation($"cover reading file:{filename}");
                base64 = _mediaManager.ConvertMediafileToBase64(filename);
            }
            else
            {
                _logger.LogError($"cover file not found:{filename}");
            }
            _logger.LogInformation($"cover file size:{base64.Length}");
            return base64;
        }
        
        [HttpGet("/{RouteId}")]
        public IActionResult Get(string RouteId)
        {
            Route resultRoute = new Route();
            string userId = IdentityManager.GetUserId(HttpContext);
            using (var db = new ServerDbContext(_dbOptions))
            {
                RouteManager routeManager = new RouteManager(_dbOptions);
                resultRoute = routeManager.Get(userId, RouteId);
            }
            return new ObjectResult(resultRoute);
        }

        [HttpPut("{RouteId}")]
        public void Put(string RouteId, [FromBody]Route routeObject)
        {
            string userId = IdentityManager.GetUserId(HttpContext);
            using (var db = new ServerDbContext(_dbOptions))
            {
                var entity = db.Route.Find(routeObject.RouteId);
                if (entity == null)
                {
                    routeObject.CreatorId = userId;
                    routeObject.VersionsHash = string.Empty;
                    routeObject.VersionsList = string.Empty;
                    db.Route.Add(routeObject);
                    RouteAccess accessObject = new RouteAccess();
                    accessObject.UserId = userId;
                    accessObject.RouteId = routeObject.RouteId;
                    accessObject.RouteAccessId = Guid.NewGuid().ToString();
                    accessObject.CreateDate = DateTime.Now;
                    db.RouteAccess.Add(accessObject);
                }
                else
                {
                    if (!string.IsNullOrEmpty(entity.CreatorId))
                    {
                        if (!entity.CreatorId.Equals(routeObject.CreatorId))
                        {
                            //throw new Exception($"Gosh server: Client try to change CreatorId! was {entity.CreatorId} try {routeObject.CreatorId}");
                            //Непонятно, почему меняется CreatorId - он устанавливается только при создании маршрута
                            routeObject.CreatorId = entity.CreatorId;
                        }
                    }
                    routeObject.VersionsHash = string.Empty;
                    routeObject.VersionsList = string.Empty;
                    db.Entry(entity).CurrentValues.SetValues(routeObject);
                }
                db.SaveChanges();
            }
        }
        
        [HttpPost("{RouteId}/updatehash")]
        public void Post(string RouteId)
        {
            string userId = IdentityManager.GetUserId(HttpContext);
            var routeHashUpdater = new RouteHashUpdater(_dbOptions);
            List<RouteVersion> versions = new List<RouteVersion>();
            using (var db = new ServerDbContext(_dbOptions))
            {
                Route entity = db.Route.Find(RouteId);
                if (entity != null)
                {
                    var lstRoute = new List<Route>();
                    lstRoute.Add(entity);
                    versions = routeHashUpdater.Calc(lstRoute);
                }
            }

            Response.StatusCode = 200;
            Response.WriteAsync(versions.Count > 0 ? versions.First().ObjVerHash : String.Empty);
        }

        [HttpGet("{RouteId}/tracks")]
        [ProducesResponseType(200)]
        public IActionResult GetRoutesTracks(string RouteId)
        {
            List<RouteTracking> tracks = new List<RouteTracking>();
            string userId = IdentityManager.GetUserId(HttpContext);
            using (var db = new ServerDbContext(_dbOptions))
            {
                tracks = db.RouteTrack.Where(r => r.RouteId.Equals(RouteId) && !r.IsDeleted).Select(r => new RouteTracking()
                {
                    Id = "0x" + BitConverter.ToString(r.Id).Replace("-",""),
                    Name = r.Name,
                    Description = r.Description,
                    Version = r.Version,
                    CreateDate = r.CreateDate,
                    IsDeleted = r.IsDeleted,
                    RouteId = r.RouteId,
                    Places = db.RouteTrackPlace.Where(p=>p.RouteTrackId.Equals(r.Id)).Select(p=>new RouteTracking.RouteTrackingPlace()
                    {
                        Name = p.Name,
                        Description = p.Description,
                        Address = p.Address,
                        Category = p.Category,
                        Distance = p.Distance,
                        Latitude = p.Latitude,
                        Longitude = p.Longitude,
                        DateTimeBegin = p.DateTimeBegin,
                        DateTimeEnd = p.DateTimeEnd
                    }).OrderBy(p=>p.DateTimeBegin).ToArray()
                }).ToList();
            }

            _logger.LogInformation($"GetRoutesTracks");
            return new ObjectResult(tracks);
        }
        
    }

    public class RouteIdArray
    {
        public List<string> IdArray = new List<string>();
    }
}
