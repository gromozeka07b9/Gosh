using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuestHelper.Server.Auth;
using QuestHelper.Server.Managers;
using QuestHelper.Server.Models;

namespace QuestHelper.Server.Controllers.v2.Public
{
    /// <summary>
    /// Контроллер задуман для доступа без авторизации к публичным маршрутам - они должны иметь внешнюю ссылку и быть опубликованы в ленте
    /// </summary>
    [Route("api/v2/public/[controller]")]
    public class RoutesController : Controller
    {

        private readonly DbContextOptions<ServerDbContext> _dbOptions;
        private readonly MediaManager _mediaManager;
        private readonly string _pathToMediaCatalog;
        private readonly ILogger<v2.RoutesController> _logger;

        public RoutesController(IConfiguration configuration, ILogger<v2.RoutesController> logger)
        {
            _dbOptions = ServerDbContext.GetOptionsContextDbServer(configuration);
            _mediaManager = new MediaManager();
            _pathToMediaCatalog = _mediaManager.PathToMediaCatalog;
            _logger = logger;
        }

        /// <summary>
        /// Список всех маршрутов доступных без авторизации.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get([FromQuery] PagingParameters pagingParameters)
        {
            FilterParameters filters = new FilterParameters(pagingParameters.Filter);
            int pageNumber = pagingParameters.IndexesRangeToPageNumber(pagingParameters.Range, pagingParameters.PageSize);
            int totalCountRows = 0;
            List<SharedModelsWS.Route> items = new List<SharedModelsWS.Route>();
            var userIdClaim = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            string userId = userIdClaim != null ? userIdClaim.Value : String.Empty;
            if (!string.IsNullOrEmpty(pagingParameters.Range))
            {
                using (var db = new ServerDbContext(_dbOptions))
                {
                    var withoutFilter = from route in db.Route where !route.IsDeleted && route.IsPublished
                        join share in db.RouteShare on new {route.RouteId, user = route.CreatorId} equals new {share.RouteId, user = share.UserId} into shares
                        from share in shares
                        select new SharedModelsWS.Route { Id = route.RouteId, 
                            PublicReferenceHash = share.ReferenceHash,
                            CreateDate = route.CreateDate,
                            CreatorId = route.CreatorId,
                            Description = string.IsNullOrEmpty(route.Description) ? String.Empty : route.Description,
                            ImgFilename = string.IsNullOrEmpty(route.ImgFilename) ? String.Empty : route.ImgFilename,
                            FirstImageName = (db.RoutePointMediaObject
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
                            LikedByCurrentUser = !string.IsNullOrEmpty(userId) ? db.RouteLike.Any(r=>r.RouteId.Equals(route.RouteId) && r.IsLike == 1 && r.UserId.Equals(userId)) : false,
                            DislikeCount = db.RouteLike.Count(r=>r.RouteId.Equals(route.RouteId) && r.IsLike == 0),
                            ViewCount = db.RouteView.Count(r=>r.RouteId.Equals(route.RouteId)),
                            PointCount = db.RoutePoint.Count(rp=>rp.RouteId.Equals(route.RouteId))
                        };
                    withoutFilter = filters.isFilterPresent("createDate") ? withoutFilter.Where(r => r.CreateDate.Equals(filters.GetDateTimeByName("createDate"))) : withoutFilter;
                    withoutFilter = filters.isFilterPresent("creatorId") ? withoutFilter.Where(r => r.CreatorId.Contains(filters.GetStringByName("creatorId"))) : withoutFilter;
                    withoutFilter = filters.isFilterPresent("id") ? withoutFilter.Where(r => r.Id.Equals(filters.GetStringByName("id"))) : withoutFilter;
                    withoutFilter = filters.isFilterPresent("name") ? withoutFilter.Where(r => r.Name.Contains(filters.GetStringByName("name"))) : withoutFilter;
                    withoutFilter = filters.isFilterPresent("description") ? withoutFilter.Where(r => r.Description.Contains(filters.GetStringByName("description"))) : withoutFilter;
                    if (filters.isFilterPresent("createDate"))
                    {
                        var cd = filters.GetDateTimeByName("createDate");
                        withoutFilter = withoutFilter.Where(r => r.CreateDate.Year.Equals(cd.Year) && r.CreateDate.Month.Equals(cd.Month) && r.CreateDate.Day.Equals(cd.Day));
                    }

                    totalCountRows = withoutFilter.Count();
                    var onePage = withoutFilter.OrderByDescending(r => r.CreateDate)
                        .Skip((pageNumber - 1) * pagingParameters.PageSize).Take(pagingParameters.PageSize).ToList();
                    foreach (var route in onePage)
                    {
                        route.FirstImageName = getFirstImageFilename(route.FirstImageName);
                    }
                    items = onePage;
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
    }
}
