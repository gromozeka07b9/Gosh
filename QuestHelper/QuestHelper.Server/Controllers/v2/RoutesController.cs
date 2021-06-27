using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestHelper.Server.Managers;
using QuestHelper.Server.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Pomelo.EntityFrameworkCore.MySql.Query.ExpressionTranslators.Internal;
using QuestHelper.Server.Models.v2;
using QuestHelper.SharedModelsWS;
using Route = QuestHelper.Server.Models.Route;
using QuestHelper.Server.Models.Tracks;
using QuestHelper.Server.Models.Tracks.KML22;
using Placemark = QuestHelper.Server.Models.Tracks.KML22.Placemark;

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
                    
                    DateTime test = DateTime.MinValue;
                    
                    items = dbRoutes.Select(route => new SharedModelsWS.Route()
                    {
                        Id = route.RouteId, 
                        PublicReferenceHash = db.RouteShare.Where(r=>r.RouteId.Equals(route.RouteId)).Select(r=>r.ReferenceHash).FirstOrDefault(),
                        CreateDate = route.CreateDate,
                        UpdateDate = db.RoutePoint.Any(rp => !rp.IsDeleted && rp.RouteId.Equals(route.RouteId)) ?
                            db.RoutePoint.Where(rp=>!rp.IsDeleted && rp.RouteId.Equals(route.RouteId)).Select(rp=>rp.UpdateDate).Max()
                        :new DateTimeOffset?(route.CreateDate),
                        //UpdateDate = route.UpdateDate,
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
                        LikedByCurrentUser = db.RouteLike.Any(r=>r.RouteId.Equals(route.RouteId) && r.IsLike == 1 && r.UserId.Equals(userId)),
                        LastComment = String.Empty,
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
        public JsonResult GetRoutesTracks(string RouteId)
        {
            RouteTracking track = new RouteTracking();
            string userId = IdentityManager.GetUserId(HttpContext);
            using (var db = new ServerDbContext(_dbOptions))
            {
                track = db.RouteTrack.Where(r => r.RouteId.Equals(RouteId) && !r.IsDeleted).Select(r => new RouteTracking()
                {
                    Id = BitConverter.ToString(r.Id).Replace("-",""),
                    Name = r.Name,
                    Description = r.Description,
                    Version = r.Version,
                    CreateDate = r.CreateDate,
                    IsDeleted = r.IsDeleted,
                    RouteId = r.RouteId,
                    Places = db.RouteTrackPlace.Where(p=>p.RouteTrackId.Equals(r.Id)).Select(p=>new RouteTracking.RouteTrackingPlace()
                    {
                        Id = BitConverter.ToString(p.Id).Replace("-",""),
                        Name = p.Name,
                        Description = p.Description,
                        Address = p.Address,
                        Category = p.Category,
                        Distance = p.Distance,
                        Latitude = p.Latitude,
                        Longitude = p.Longitude,
                        DateTimeBegin = p.DateTimeBegin,
                        DateTimeEnd = p.DateTimeEnd,
                    }).OrderBy(p=>p.DateTimeBegin).ToArray()
                }).FirstOrDefault();
            }
            _logger.LogInformation($"GetRoutesTracks");
            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
            {
                IgnoreNullValues = true
            };
            return Json(track, jsonSerializerOptions);
        }
        
        [HttpPost("{RouteId}/tracks")]
        public async Task PostNewTrack(string routeId, IFormFile file)
        {
            string userId = IdentityManager.GetUserId(HttpContext);
            if ((file.Length > 0) && (!string.IsNullOrEmpty(file.FileName)))
            {
                string tempFileName = Path.GetTempFileName();
                using (var stream = new FileStream(tempFileName, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                if (file.FileName.EndsWith(".kml", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (!saveKml21ToDbAsTrack(tempFileName, routeId))
                    {
                        saveKml22ToDbAsTrack(tempFileName, routeId);
                    }
                } else if (file.FileName.EndsWith(".gpx", StringComparison.CurrentCultureIgnoreCase))
                {
                    saveGpxToDbAsTrack(tempFileName, routeId);
                }
                else
                {
                    Response.StatusCode = StatusCodes.Status400BadRequest;
                }
            }
            else
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
            }
            Response.StatusCode = 200;
        }

        private bool saveKml21ToDbAsTrack(string pathToFile, string routeId)
        {
            //создание модели на основе xml
            //https://xmltocsharp.azurewebsites.net/
            using (var fileStream = System.IO.File.Open(pathToFile, FileMode.Open))
            {
                var serializer = new XmlSerializer(typeof(Kml21CustomScheme));
                Kml21CustomScheme kmlObject = new Kml21CustomScheme();
                try
                {
                    kmlObject = (Kml21CustomScheme) serializer.Deserialize(fileStream);
                }
                catch (Exception e)
                {
                    return false;
                }
                if (kmlObject.Folder != null)
                {
                    using (var db = new ServerDbContext(_dbOptions))
                    {
                        RouteTrack dbRouteTrack = new RouteTrack();
                        dbRouteTrack.Id = Guid.NewGuid().ToByteArray();
                        dbRouteTrack.RouteId = routeId;
                        dbRouteTrack.CreateDate = DateTime.Now;
                        dbRouteTrack.Version = 1;
                        dbRouteTrack.Name = kmlObject.Folder.Name;
                        dbRouteTrack.Description = !string.IsNullOrEmpty(kmlObject.Folder.Description)?kmlObject.Folder.Description.Substring(0, kmlObject.Folder.Description.Length):string.Empty;
                        foreach (var folderInside in kmlObject.Folder.FolderInside)
                        {
                            foreach (var placemark in folderInside.Placemark)
                            {
                                RouteTrackPlace dbRoutePlace = new RouteTrackPlace();
                                dbRoutePlace.Id = Guid.NewGuid().ToByteArray();
                                dbRoutePlace.RouteTrackId = dbRouteTrack.Id;
                                dbRoutePlace.Name = placemark.Name;
                                dbRoutePlace.Description = !string.IsNullOrEmpty(placemark.Description)?placemark.Description.Substring(0, placemark.Description.Length):string.Empty;
                                if (placemark.TimeSpanKml != null)
                                {
                                    dbRoutePlace.DateTimeBegin = placemark.TimeSpanKml != null ? DateTime.Parse(placemark.TimeSpanKml.Begin) : DateTime.MinValue;
                                    dbRoutePlace.DateTimeEnd = placemark.TimeSpanKml != null ? DateTime.Parse(placemark.TimeSpanKml.End) : DateTime.MinValue;
                                    var pointCoords = placemark.Point?.Coordinates?.Split(",");
                                    //37.40418431349098682403564453125,55.81698308698832988739013671875, 277
                                    //37.397304866462946, 55.81914712674916
                                    if (pointCoords.Length >= 2)
                                    {
                                        dbRoutePlace.Latitude = Convert.ToDouble(pointCoords[1]);
                                        dbRoutePlace.Longitude = Convert.ToDouble(pointCoords[0]);
                                        dbRoutePlace.Elevation = pointCoords.Length > 2 ? Convert.ToDouble(pointCoords[2]) : 0;
                                    }
                                    db.RouteTrackPlace.Add(dbRoutePlace);
                                }
                            }
                        }
                        db.RouteTrack.Add(dbRouteTrack);
                        db.SaveChanges();
                        return true;
                    }
                }
            }

            return false;
        }
        private bool saveKml22ToDbAsTrack(string pathToFile, string routeId)
        {
            //создание модели на основе xml
            //https://xmltocsharp.azurewebsites.net/
            using (var fileStream = System.IO.File.Open(pathToFile, FileMode.Open))
            {
                var serializer = new XmlSerializer(typeof(Kml22CustomScheme));
                Kml22CustomScheme kmlObject = new Kml22CustomScheme();
                try
                {
                    kmlObject = (Kml22CustomScheme) serializer.Deserialize(fileStream);
                }
                catch (Exception e)
                {
                    return false;
                }
                if (kmlObject.Document != null)
                {
                    using (var db = new ServerDbContext(_dbOptions))
                    {
                        RouteTrack dbRouteTrack = new RouteTrack();
                        dbRouteTrack.Id = Guid.NewGuid().ToByteArray();
                        dbRouteTrack.RouteId = routeId;
                        dbRouteTrack.CreateDate = DateTime.Now;
                        dbRouteTrack.Version = 1;
                        dbRouteTrack.Name = kmlObject.Document.Name;
                        dbRouteTrack.Description = !string.IsNullOrEmpty(kmlObject.Document.Description)?kmlObject.Document.Description.Substring(0, kmlObject.Document.Description.Length):string.Empty;
                        foreach (var folderInside in kmlObject.Document.Folder)
                        {
                            foreach (var placemark in folderInside.Placemark)
                            {
                                if (placemark.LookAt != null)
                                {
                                    var dbRoutePlace = makeNewTrackPlace(dbRouteTrack.Id,
                                        placemark.Name,
                                        !string.IsNullOrEmpty(placemark.Description)
                                            ? placemark.Description.Substring(0, placemark.Description.Length)
                                            : string.Empty,
                                        DateTime.Parse(placemark.TimeSpanKml.Begin),
                                        DateTime.Parse(placemark.TimeSpanKml.End),
                                        ConvertStringToDoubleGeo(placemark.LookAt.Latitude),
                                        ConvertStringToDoubleGeo(placemark.LookAt.Longitude),
                                        ConvertStringToDoubleGeo(placemark.LookAt.Altitude)
                                    );
                                    db.RouteTrackPlace.Add(dbRoutePlace);
                                }
                            }
                        }
                        foreach (var placemark in kmlObject.Document.Placemark)
                        {
                            if (placemark.LookAt != null)
                            {
                                var dbRoutePlace = makeNewTrackPlace(dbRouteTrack.Id,
                                    placemark.Name,
                                    !string.IsNullOrEmpty(placemark.Description)
                                        ? placemark.Description.Substring(0, placemark.Description.Length)
                                        : string.Empty,
                                    DateTime.Parse(placemark.TimeSpanKml.Begin),
                                    DateTime.Parse(placemark.TimeSpanKml.End),
                                    ConvertStringToDoubleGeo(placemark.LookAt.Latitude),
                                    ConvertStringToDoubleGeo(placemark.LookAt.Longitude),
                                    ConvertStringToDoubleGeo(placemark.LookAt.Altitude)
                                );
                                db.RouteTrackPlace.Add(dbRoutePlace);
                            } else if (placemark.Point?.Coordinates != null)
                            {
                                var coordsArray = parseCoordinatesToList(placemark.Point.Coordinates);
                                if (coordsArray.Count > 0)
                                {
                                    var dbRoutePlace = makeNewTrackPlace(dbRouteTrack.Id,
                                        placemark.Name,
                                        !string.IsNullOrEmpty(placemark.Description)
                                            ? placemark.Description.Substring(0, placemark.Description.Length)
                                            : string.Empty,
                                        DateTime.Parse(placemark.TimeSpanKml.Begin),
                                        DateTime.Parse(placemark.TimeSpanKml.End),
                                        ConvertStringToDoubleGeo(coordsArray[0][0]),
                                        ConvertStringToDoubleGeo(coordsArray[0][1]),
                                        (coordsArray[0].Length > 2) ? ConvertStringToDoubleGeo(coordsArray[0][2]) : 0.0
                                    );
                                    db.RouteTrackPlace.Add(dbRoutePlace);
                                }
                            } else if (placemark.LineString?.Coordinates != null)
                            {
                                var coordsArray = parseCoordinatesToList(placemark.LineString.Coordinates);
                                if (coordsArray.Count > 0)
                                {
                                    foreach (var geopoint in coordsArray.Where(s=>s.Length > 1))
                                    {
                                        var dbRoutePlace = makeNewTrackPlace(dbRouteTrack.Id,
                                            placemark.Name,
                                            !string.IsNullOrEmpty(placemark.Description)
                                                ? placemark.Description.Substring(0, placemark.Description.Length)
                                                : string.Empty,
                                            DateTime.Parse(placemark.TimeSpanKml.Begin),
                                            DateTime.Parse(placemark.TimeSpanKml.End),
                                            ConvertStringToDoubleGeo(geopoint[0]),
                                            ConvertStringToDoubleGeo(geopoint[1]),
                                            (geopoint.Length > 2) ? ConvertStringToDoubleGeo(geopoint[2]) : 0.0
                                        );
                                        db.RouteTrackPlace.Add(dbRoutePlace);
                                    }
                                }
                            }
                        }
                        db.RouteTrack.Add(dbRouteTrack);
                        db.SaveChanges();
                        return true;
                    }
                }
            }

            return false;
        }

        private RouteTrackPlace makeNewTrackPlace(byte[] routeTrackid, string placemarkName, string description, DateTime dateTimeBegin, DateTime dateTimeEnd, double latitude, double longitude, double altitude)
        {
            RouteTrackPlace dbRoutePlace = new RouteTrackPlace();
            dbRoutePlace.Id = Guid.NewGuid().ToByteArray();
            dbRoutePlace.RouteTrackId = routeTrackid;
            dbRoutePlace.Name = placemarkName;
            dbRoutePlace.Description = description;
            dbRoutePlace.DateTimeBegin = dateTimeBegin;
            dbRoutePlace.DateTimeEnd = dateTimeEnd;
            dbRoutePlace.Latitude = latitude;
            dbRoutePlace.Longitude = longitude;
            dbRoutePlace.Elevation = altitude;
            return dbRoutePlace;
        }

        private static double ConvertStringToDoubleGeo(string geoCoordinateValue)
        {
            double result = 0.0;
            if (!string.IsNullOrEmpty(geoCoordinateValue))
            {
                try
                {
                    result = Convert.ToDouble(geoCoordinateValue.Replace("−","-"));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return result;
        }

        private List<string[]> parseCoordinatesToList(string pointCoordinates)
        {
            List<string[]> geopoints = new List<string[]>();
            if (!string.IsNullOrEmpty(pointCoordinates))
            {
                var geopointsCoordinates = pointCoordinates.Split(" ");
                foreach (var geopoint in geopointsCoordinates)
                {
                    geopoints.Add(geopoint.Split(","));
                }
            }

            return geopoints;
        }

        private void saveGpxToDbAsTrack(string pathToFile, string routeId)
        {
            //создание модели на основе xml
            //https://xmltocsharp.azurewebsites.net/
            using (var fileStream = System.IO.File.Open(pathToFile, FileMode.Open))
            {
                var serializer = new XmlSerializer(typeof(GPXCustomScheme));
                var trackObject = (GPXCustomScheme) serializer.Deserialize(fileStream);
                if (trackObject.Trk != null)
                {
                    using (var db = new ServerDbContext(_dbOptions))
                    {
                        RouteTrack dbRouteTrack = new RouteTrack
                        {
                            Id = Guid.NewGuid().ToByteArray(),
                            RouteId = routeId,
                            CreateDate = DateTime.Now,
                            Version = 1,
                            Name = trackObject.Trk.Name,
                            Description = !string.IsNullOrEmpty(trackObject.Trk.Type)
                                ? trackObject.Trk.Type.Substring(0, trackObject.Trk.Type.Length)
                                : string.Empty
                        };
                        foreach (var segments in trackObject.Trk.Trkseg)
                        {
                            foreach (var placemark in segments.Trkpt)
                            {
                                RouteTrackPlace dbRoutePlace = new RouteTrackPlace();
                                dbRoutePlace.Id = Guid.NewGuid().ToByteArray();
                                dbRoutePlace.RouteTrackId = dbRouteTrack.Id;
                                if (placemark.Time != null)
                                {
                                    dbRoutePlace.DateTimeBegin = DateTime.Parse(placemark.Time);
                                    dbRoutePlace.Latitude = Convert.ToDouble(placemark.Lat);
                                    dbRoutePlace.Longitude = Convert.ToDouble(placemark.Lon);
                                    dbRoutePlace.Elevation = Convert.ToDouble(placemark.Ele);
                                    db.RouteTrackPlace.Add(dbRoutePlace);
                                }
                            }
                        }
                        db.RouteTrack.Add(dbRouteTrack);
                        db.SaveChanges();
                    }
                }
            }
        }
    }

    public class RouteIdArray
    {
        public List<string> IdArray = new List<string>();
    }
}
