namespace WoWMarketWatcher.API.Constants
{
    public static class ConfigurationKeys
    {
        public const string ApplicationInsightsCloudRoleName = "ApplicationInsights:CloudRoleName";
        public const string HangfireDatabaseName = "Hangfire:DatabaseName";
        public const string HangfireDatabaseConnection = "Hangfire:DatabaseConnection";
        public const string HangfireDashboardUsername = "Hangfire:Dashboard:Username";
        public const string HangfireDashboardPassword = "Hangfire:Dashboard:Password";
        public const string SwaggerEndpoint = "Swagger:SwaggerEndpoint";
        public const string SwaggerHangfireEndpoint = "Swagger:HangfireEndpoint";
    }
}