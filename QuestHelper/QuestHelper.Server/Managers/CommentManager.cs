using Microsoft.EntityFrameworkCore;
using QuestHelper.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuestHelper.Server.Managers
{
    public class CommentManager
    {
        DbContextOptions<ServerDbContext> _db;

        public CommentManager(DbContextOptions<ServerDbContext> db)
        {
            _db = db;
        }

        public async Task<bool> Add(string userId, string routeId, string parentCommentId, int objectType, string text)
        {
            using (var db = new ServerDbContext(_db))
            {
                db.Comment.Add(new Comment()
                {
                    Id = Guid.NewGuid().ToByteArray(),
                    CreatorId = Guid.Parse(userId).ToByteArray(),
                    ParentId = !string.IsNullOrEmpty(parentCommentId) ? Guid.Parse(parentCommentId).ToByteArray() : null,
                    RelationObjectId = Guid.Parse(routeId).ToByteArray(),
                    RelationObjectType = objectType,
                    CreateDate = DateTime.Now,
                    Text = text,
                    IsDeleted = false
                });
                await db.SaveChangesAsync();
                return true;
            }
            return false;
        }

    }
}
