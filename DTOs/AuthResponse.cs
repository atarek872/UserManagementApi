namespace UserManagementApi.DTOs
{
    public class AuthResponse
    {
        public string Username { get; set; }
        public string JwtToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
