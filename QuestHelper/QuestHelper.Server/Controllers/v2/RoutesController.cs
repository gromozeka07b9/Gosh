using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestHelper.Server.Managers;
using QuestHelper.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace QuestHelper.Server.Controllers.v2
{
    /// <summary>
    /// CRUD for routes
    /// </summary>
    [Authorize]
    [ServiceFilter(typeof(RequestFilter))]
    [Route("api/v2/[controller]")]
    public class RoutesController : Controller
    {
        private readonly DbContextOptions<ServerDbContext> _dbOptions;
        private readonly ILogger<RoutesController> _logger;

        public RoutesController(IConfiguration configuration, ILogger<RoutesController> logger)
        {
            _dbOptions = ServerDbContext.GetOptionsContextDbServer(configuration);
            _logger = logger;
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
                        CreatorId = route.CreatorId,
                        Description = route.Description,
                        ImgFilename = route.ImgFilename,
                        FirstImageName = getFirstImageFilename(db.RoutePointMediaObject
                            .FirstOrDefault(m => !m.IsDeleted && m.MediaType == MediaObjectTypeEnum.Image && m.ImageLoadedToServer
                                                 && m.RoutePointId.Equals(db.RoutePoint
                                                     .Where(rp=>rp.RouteId.Equals(route.RouteId) && !rp.IsDeleted)
                                                     .OrderBy(rp=>rp.CreateDate).FirstOrDefault().RoutePointId)).RoutePointMediaObjectId),
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
                        PointCount = db.RoutePoint.Count(rp=>rp.RouteId.Equals(route.RouteId))
                    }).ToList();
                }
            }
            HttpContext.Response.Headers.Add("x-total-count", totalCountRows.ToString());
            HttpContext.Response.Headers.Add("Access-Control-Expose-Headers", "x-total-count");
            return new ObjectResult(items);
        }

        private string getFirstImageFilename(string id)
        {
            return string.IsNullOrEmpty(id) ? string.Empty : string.Concat("img_", id, ".jpg");
        }

        [HttpGet("{RouteId}")]
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
                    /*if (!string.IsNullOrEmpty(routeObject.CoverImgBase64))
                    {
                        Base64Manager.SaveBase64ToFile(routeObject.CoverImgBase64, Path.Combine(_pathToMediaCatalog, routeObject.ImgFilename));
                    }*/
                }
                db.SaveChanges();
            }
        }

    }

    }
