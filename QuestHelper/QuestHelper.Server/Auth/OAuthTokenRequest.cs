using System;
namespace QuestHelper.Server.Auth
{
    public class OAuthTokenRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Locale { get; set; }
        public string ImgUrl { get; set; }
        public string AuthenticatorUserId { get; set; }
    }
}
