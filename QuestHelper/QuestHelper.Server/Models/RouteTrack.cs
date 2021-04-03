using System;
using System.ComponentModel.DataAnnotations;

namespace QuestHelper.Server.Models
{
    public class RouteTrack
    {
        [Key]
        [MaxLength(16)]
        public byte[] Id { get; set; }
        public string RouteId { get; set; }
        public DateTime CreateDate { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Version { get; set; }
        public bool IsDeleted { get; set; }
    }
}