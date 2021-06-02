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

namespace QuestHelper.Server.Controllers.v2
{
    /// <summary>
    /// CRUD for route points
    /// </summary>
    [Authorize]
    [ServiceFilter(typeof(RequestFilter))]
    [Route("api/v2/[controller]")]
    public class CommentsController : Controller
    {
        private DbContextOptions<ServerDbContext> _dbOptions;
        private readonly CommentManager _commentManager;

        public CommentsController(IConfiguration configuration)
        {
            _dbOptions = ServerDbContext.GetOptionsContextDbServer(configuration);
            _commentManager = new CommentManager(_dbOptions);
        }

        /*[HttpGet("{RoutePointId}")]
        public IActionResult Get(string RoutePointId)
        {
            string userId = IdentityManager.GetUserId(HttpContext);
            RoutePoint point = new RoutePoint();
            using (var db = new ServerDbContext(_dbOptions))
            {
                var publishRoutes = db.Route.Where(r => r.IsPublished && r.IsDeleted == false).Select(r => r.RouteId).ToList();
                var routeaccess = db.RouteAccess.Where(u => u.UserId == userId).Select(u => u.RouteId).ToList();
                point = db.RoutePoint.SingleOrDefault(x => x.RoutePointId == RoutePointId && (routeaccess.Contains(x.RouteId) || (publishRoutes.Contains(x.RouteId))));
            }
            return new ObjectResult(point);
        }*/

        [HttpPost]
        public async Task<StatusCodeResult> Post([FromBody] SharedModelsWS.Comment comment)
        {
            string userId = IdentityManager.GetUserId(HttpContext);
            if (await _commentManager.Add(userId, comment.RelationObjectId, comment.ParentId, true, comment.Text))
            {
                return Ok();
            }

            Response.StatusCode = 500;
            return NotFound();
        }
    }
}
