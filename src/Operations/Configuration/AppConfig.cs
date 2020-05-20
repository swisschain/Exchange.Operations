namespace Operations.Configuration
{
    public class AppConfig
    {
        public MatchingEngineSettings MatchingEngine { get; set; }
        public FeesSettings FeesService { get; set; }
        public AccountsSettings AccountsService { get; set; }
        public JwtSettings Jwt { get; set; }
    }
}
