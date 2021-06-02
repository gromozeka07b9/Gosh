using System;

namespace QuestHelper.SharedModelsWS
{
    public class Comment
    {
        public string Id { get; set; }
        public string RelationObjectId { get; set; }
        public int RelationObjectType { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreateDate { get; set; }
        public string ParentId { get; set; }
        public string Text { get; set; }
        public string CreatorId { get; set; }
    }
}