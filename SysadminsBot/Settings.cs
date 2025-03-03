﻿namespace SysadminsBot
{
    public sealed class Settings
    {
        public required string User { get; set; }
        public required string Password { get; set; }
        public required Topic[] Topics { get; set; }
        public required string[] SkipUsers { get; set; }
        public required string ApiKey { get; set; }
        public required string BaseUrl { get; set; }
        public required int PollingInterval { get; set; }
        public required string Module { get; set; }
        public required string ScriptPath { get; set; }
    }
}
