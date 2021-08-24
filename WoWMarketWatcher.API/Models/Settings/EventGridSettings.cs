using System;

namespace WoWMarketWatcher.API.Models.Settings
{
    public record EventGridSettings
    {
        public Uri TopicUrl { get; init; } = default!;

        public string AccessKey { get; init; } = default!;

        public string HandlerAccessKey { get; init; } = default!;

        public bool SendingEnabled { get; init; }
    }
}