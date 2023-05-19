using System;

namespace DAL.Dto
{
    public class LoginResponse
    {
        public Boolean Success { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime RefreshTokenExpiration { get; set; }
    }
}