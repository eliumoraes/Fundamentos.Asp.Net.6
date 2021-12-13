namespace Suitex
{
    public static class Configuration
    {
        // Token - JWT - Json Web Token
        public static string JwtKey { get; set; } = "7jDmJQFhn0GzkjjZMd8bpg==";
        public static string ApiKeyName = "api_key";
        public static string ApiKey = "robo_0b1ef8ee-5b2f-4601-bc88-1472e154ac03";
        public static SmtpConfiguration Smtp = new();

        public class SmtpConfiguration
        {
            public string Host { get; set; }
            public int Port { get; set; } = 25;
            public string UserName { get; set; }
            public string Password { get; set; }
        }
    }
}
