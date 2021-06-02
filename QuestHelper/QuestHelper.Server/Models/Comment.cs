using System;
using System.ComponentModel.DataAnnotations;

namespace QuestHelper.Server.Models
{
    public class Comment
    {
        [Key]
        [MaxLength(16)]
        public byte[] Id { get; set; }
        [MaxLength(16)]
        public byte[] RelationObjectId { get; set; }
        public int RelationObjectType { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreateDate { get; set; }
        [MaxLength(16)]
        public byte[] ParentId { get; set; }
        public string Text { get; set; }
        [MaxLength(16)]
        public byte[] CreatorId { get; set; }
    }
}