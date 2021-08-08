using CommandLine;

namespace WoWMarketWatcher.API.Core
{
    public sealed class CommandLineOptions
    {
        public const string seedArgument = "seeder";

        public const string dropArgument = "drop";

        public const string seedDataArgument = "seed";

        public const string migrateArgument = "migrate";

        public const string clearDataArgument = "clear";

        [Option("password", Required = false, HelpText = "Input password.")]
        public string? Password { get; set; }
    }
}