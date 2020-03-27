namespace Operations.Configuration
{
    public class AppConfig
    {
        public JwtSettings Jwt { get; set; }
    }

    public class JwtSettings
    {
        public string Secret { get; set; }
    }
}
