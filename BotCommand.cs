using Discord.Commands;

namespace Codey
{
    public class BotCommand
    {
        public string Name { get; set; }
        public string FirstInput { get; set; }
        public string SecondInput { get; set; }
        public SocketCommandContext Context { get; set; }
    }
}