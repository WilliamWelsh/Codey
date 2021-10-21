using System.Linq;
using System.Threading.Tasks;

namespace Codey
{
    public static class InterpreterActions
    {
        public static async Task<string> MakeRole(this BotCommand command)
        {
            await command.Context.Guild.CreateRoleAsync(command.FirstInput, null, null, false, null);
            return $"I made a new role called `{command.FirstInput}`";
        }

        public static async Task<string> DeleteRole(this BotCommand command)
        {
            var roleToDelete = command.GetRoleFromString();
            if (roleToDelete == null)
                return $"I couldn't find a role named `{command.FirstInput}`.";
            await roleToDelete.DeleteAsync();
            return $"I delete the {roleToDelete.Name} role";
        }

        public static async Task<string> ChangeRoleColor(this BotCommand command)
        {
            var role = command.GetRoleFromString();
            if (role == null)
                return $"I couldn't find a role named `{command.FirstInput}`.";
            await role.ModifyAsync(r => r.Color = Colors.GetColorFromString(command.SecondInput));
            return $"I changed the {role.Name}'s role color to {command.SecondInput}";
        }

        public static string ListAllRoles(this BotCommand command)
        => $"Here are all the roles we have:\n{command.Context.Guild.Roles.Aggregate("", (a, b) => a + b.Mention + ", ")}".Replace("@everyone", "everyone");

        public static async Task<string> MakeTextChannel(this BotCommand command, Resources resources)
        {
            var channel = await command.Context.Guild.CreateTextChannelAsync(command.FirstInput);
            await channel.SendMessageAsync(resources.GetRandomNewChannelMessage());
            return $"I made a new text channel: {channel.Mention}";
        }

        public static async Task<string> MakeRoleWithColor(this BotCommand command)
        {
            await command.Context.Guild.CreateRoleAsync(command.FirstInput, null, Colors.GetColorFromString(command.SecondInput), false, null);
            return $"I made a new role called {command.FirstInput} with a {command.SecondInput} color.";
        }

        public static async Task<string> GiveRole(this BotCommand command)
        {
            var role = command.GetRoleFromSecondString();
            if (role == null)
                return $"I couldn't find a role named `{command.SecondInput}`.";

            var target = command.GetUserFromString();

            if (target == null)
                return $"I couldn't find anyone named {command.FirstInput}.";

            // Give the role to the user
            await target.AddRoleAsync(role);
            return $"I granted {target.Mention} the {role.Name} role.";
        }

        public static async Task<string> RemoveRole(this BotCommand command)
        {
            var role = command.GetRoleFromSecondString();
            if (role == null)
                return $"I couldn't find a role named `{command.SecondInput}`.";

            var target = command.GetUserFromString();

            if (target == null)
                return $"I couldn't find anyone named {command.FirstInput}.";

            // Give the role to the user
            await target.RemoveRoleAsync(role);
            return $"I removed the {role.Name} role from {target.Mention}.";
        }

        public static string GetAvatar(this BotCommand command)
        {
            var target = command.GetUserFromString();
            if (target == null)
                return $"I couldn't find anyone named {command.FirstInput}.";
            return $"Here's {target.Mention}'s avatar: {target.GetAvatarUrl() ?? target.GetDefaultAvatarUrl()}";
        }
    }
}