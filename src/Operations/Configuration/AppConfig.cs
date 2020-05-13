namespace Operations.Configuration
{
    public class AppConfig
    {
        public MatchingEngineSettings MatchingEngine { get; set; }
        public ExchangeFeesSettings ExchangeFeesService { get; set; }
        public JwtSettings Jwt { get; set; }
    }
}
