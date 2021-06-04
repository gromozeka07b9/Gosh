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

        [HttpGet("routes/{RelationObjectId}")]
        public IActionResult GetRouteComments(string relationObjectId)
        {
            string userId = IdentityManager.GetUserId(HttpContext);
            return getCommentByIdAndType(relationObjectId, 0);
        }

        [HttpGet("points/{RelationObjectId}")]
        public IActionResult GetRoutePointComments(string relationObjectId)
        {
            string userId = IdentityManager.GetUserId(HttpContext);
            return getCommentByIdAndType(relationObjectId, 1);
        } 
        private IActionResult getCommentByIdAndType(string relationObjectId, int objectType)
        {
            var objectIdAsBytesArray = relationObjectId.Contains('-')
                ? Guid.Parse(relationObjectId).ToByteArray()
                : System.Text.Encoding.ASCII.GetBytes(relationObjectId);
            using (var db = new ServerDbContext(_dbOptions))
            {
                var publishRoutes = db.Route.Where(r => r.IsPublished && r.IsDeleted == false).Select(r => r.RouteId).ToList();
                //objectType: 0 - route, 1 - routePoint
                var comments = db.Comment.Where(c => c.RelationObjectId.Equals(objectIdAsBytesArray) && !c.IsDeleted && c.RelationObjectType.Equals(objectType))
                    .OrderBy(c=>c.CreateDate)
                    .Select(c => new
                    {
                        Id = new Guid(c.Id),
                        c.CreateDate,
                        CreatorId = new Guid(c.CreatorId),
                        ParentId = c.ParentId != null ? new Guid(c.ParentId).ToString() : String.Empty,
                        RelationObjectId = new Guid(c.RelationObjectId),
                        c.Text
                    }).ToList();
                return new ObjectResult(comments);
            }
        }

        [HttpPost]
        public async Task<StatusCodeResult> Post([FromBody] SharedModelsWS.Comment comment)
        {
            string userId = IdentityManager.GetUserId(HttpContext);
            if (await _commentManager.Add(userId, comment.RelationObjectId, comment.ParentId, comment.RelationObjectType, comment.Text))
            {
                return Ok();
            }

            Response.StatusCode = 500;
            return NotFound();
        }
    }
}
