using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using OpenAI;

namespace Codey
{
    public static class InterpreterActions
    {
        public static async Task<CommandResult> MakeRole(this BotCommand command)
        {
            var role = await command.Context.Guild.CreateRoleAsync(command.FirstInput, null, null, false, null);
            return new CommandResult($"I made a new role called {role.Mention}.");
        }

        public static async Task<CommandResult> DeleteRole(this BotCommand command)
        {
            var roleToDelete = command.GetRoleFromString();
            if (roleToDelete == null)
                return new CommandResult($"I couldn't find a role named {command.FirstInput}.", false);
            await roleToDelete.DeleteAsync();
            return new CommandResult($"I deleted the {roleToDelete.Name} role");
        }

        public static async Task<CommandResult> ChangeRoleColor(this BotCommand command)
        {
            var role = command.GetRoleFromString();
            if (role == null)
                return new CommandResult($"I couldn't find a role named {command.FirstInput}.", false);
            await role.ModifyAsync(r => r.Color = Colors.GetColorFromString(command.SecondInput));
            return new CommandResult($"I changed the {role.Name}'s role color to {command.SecondInput}");
        }

        public static CommandResult ListAllRoles(this BotCommand command)
        => new CommandResult($"Here are all the roles we have:\n{command.Context.Guild.Roles.Aggregate("", (a, b) => a + b.Mention + ", ")}".Replace("@everyone", "everyone"));

        public static async Task<CommandResult> MakeTextChannel(this BotCommand command, Resources resources)
        {
            var channel = await command.Context.Guild.CreateTextChannelAsync(command.FirstInput);

            // If it's a rule channel, put some template rules for them
            if (channel.Name.Contains("rules"))
            {
                await channel.SendMessageAsync(resources.RulesTemplate.Replace("{DISCORD_NAME}", command.Context.Guild.Name));
                await channel.SendMessageAsync($"There's some template rules to get you started 😊 {command.Context.User.Mention}");
            }
            else
                await channel.SendMessageAsync(resources.GetRandomNewChannelMessage());
            return new CommandResult($"I made a new text channel: {channel.Mention}");
        }

        public static async Task<CommandResult> MakeRoleWithColor(this BotCommand command)
        {
            var role = await command.Context.Guild.CreateRoleAsync(command.FirstInput, null, Colors.GetColorFromString(command.SecondInput), false, null);
            return new CommandResult($"I made a new role called {role.Mention} with a {command.SecondInput} color.");
        }

        public static async Task<CommandResult> GiveRole(this BotCommand command)
        {
            var role = command.GetRoleFromSecondString();
            if (role == null)
                return new CommandResult($"I couldn't find a role named {command.SecondInput}.", false);

            var target = command.GetUserFromString();

            if (target == null)
                return new CommandResult($"I couldn't find anyone named {command.FirstInput}.", false);

            // Give the role to the user
            await target.AddRoleAsync(role);
            return new CommandResult($"I granted {target.Mention} the {role.Name} role.");
        }

        public static async Task<CommandResult> RemoveRole(this BotCommand command)
        {
            var role = command.GetRoleFromSecondString();
            if (role == null)
                return new CommandResult($"I couldn't find a role named {command.SecondInput}.", false);

            var target = command.GetUserFromString();

            if (target == null)
                return new CommandResult($"I couldn't find anyone named {command.FirstInput}.", false);

            // Give the role to the user
            await target.RemoveRoleAsync(role);
            return new CommandResult($"I removed the {role.Name} role from {target.Mention}.");
        }

        public static CommandResult GetAvatar(this BotCommand command)
        {
            var target = command.GetUserFromString();
            if (target == null)
                return new CommandResult($"I couldn't find anyone named {command.FirstInput}.", false);
            return new CommandResult($"Here's {target.Mention}'s avatar: {target.GetAvatarUrl() ?? target.GetDefaultAvatarUrl()}");
        }

        public static async Task<CommandResult> GiveTextChannelAnEmoji(this BotCommand command, Interpreter interpreter)
        {
            await command.Context.Channel.SendMessageAsync($"Sure. Please give me a moment...");

            var r = new Regex(interpreter._resources.EmojiPattern);

            // Change the engine to babbage
            interpreter._openAI.UsingEngine = Engine.Babbage;

            // Try to find the channel
            var channel = command.Context.Guild.TextChannels.Where(c => c.Name.Contains(command.FirstInput.ToLower().Replace(" ", "-"))).First();
            if (channel == null)
                return new CommandResult($"I couldn't find a text channel named {command.FirstInput}.", false);

            // Check if it has an emoji already
            if (r.IsMatch(channel.Name))
                return new CommandResult($"That channel already has an emoji.", false);

            // Give the channel an emoji
            var emoji = (await interpreter._openAI.Completions.CreateCompletionAsync(interpreter.channelToEmojiTemplate
                .WithPrompt($"{interpreter._resources.ChannelEmojiPrompt}\n{channel.Name}:")
                .Build()));
            var channelName = channel.Name;
            await channel.ModifyAsync(c => c.Name = $"{emoji}┃{channelName}");

            return new CommandResult($"I gave {channel.Mention} an emoji.");
        }

        public static async Task<CommandResult> GiveEveryTextChannelAnEmoji(this BotCommand command, Interpreter interpreter)
        {
            await command.Context.Channel.SendMessageAsync($"Sure. Please give me a moment...");

            var r = new Regex(interpreter._resources.EmojiPattern);

            // Change the engine to babbage
            interpreter._openAI.UsingEngine = Engine.Babbage;

            foreach (var channel in command.Context.Guild.TextChannels)
            {
                // Skip if it already has an emoji
                if (r.IsMatch(channel.Name))
                    continue;

                var emoji = (await interpreter._openAI.Completions.CreateCompletionAsync(interpreter.channelToEmojiTemplate
                    .WithPrompt($"{interpreter._resources.ChannelEmojiPrompt}\n{channel.Name}:")
                    .Build()));
                var channelName = channel.Name;
                await channel.ModifyAsync(c => c.Name = $"{emoji}┃{channelName}");
                Thread.Sleep(2000);
            }

            return new CommandResult("I gave all channels that don't have an emoji, an emoji.");
        }
    }
}