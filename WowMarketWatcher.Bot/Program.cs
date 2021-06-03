using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

[assembly: CLSCompliant(false)]
namespace WowMarketWatcher.Bot
{
    public sealed class Program : IDisposable
    {
        private readonly DiscordSocketClient client;

        public Program()
        {
            // It is recommended to Dispose of a client when you are finished
            // using it, at the end of your app's lifetime.
            this.client = new DiscordSocketClient();

            this.client.Log += this.LogAsync;
            this.client.Ready += this.ReadyAsync;
            this.client.MessageReceived += this.MessageReceivedAsync;
        }

        // Discord.Net heavily utilizes TAP for async, so we create
        // an asynchronous context from the beginning.
        public static void Main(string[] _)
        {
            using var program = new Program();
            program.MainAsync().GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            this.client.Dispose();
        }

        public async Task MainAsync()
        {
            // Tokens should be considered secret data, and never hard-coded.
            await this.client.LoginAsync(TokenType.Bot, "");
            await this.client.StartAsync();

            // Block the program until it is closed.
            await Task.Delay(Timeout.Infinite);
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        // The Ready event indicates that the client has opened a
        // connection and it is now safe to access the cache.
        private Task ReadyAsync()
        {
            Console.WriteLine($"{this.client.CurrentUser} is connected!");

            return Task.CompletedTask;
        }

        // This is not the recommended way to write a bot - consider
        // reading over the Commands Framework sample.
        private async Task MessageReceivedAsync(SocketMessage message)
        {
            // The bot should never respond to itself.
            if (message.Author.Id == this.client.CurrentUser.Id)
                return;

            if (message.Content == "!ping")
                await message.Channel.SendMessageAsync("pong!");
        }
    }
}