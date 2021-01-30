using System;
using System.ComponentModel.DataAnnotations;
using MySql.Data.MySqlClient;

namespace QuestHelper.Server.Models.v2.Public
{
    public class MediaObject
    {
        public MediaObject()
        {
        }

        public string Id { get; set; }
        public string Url { get; set; }
        public MediaObjectTypeEnum MediaType { get; set; }
    }
}
