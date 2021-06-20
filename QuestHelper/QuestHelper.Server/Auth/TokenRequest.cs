using System;
namespace QuestHelper.Server.Auth
{
    public class TokenRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
