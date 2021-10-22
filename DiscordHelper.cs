using System;
using System.Linq;
using Discord.WebSocket;

namespace Codey
{
    public static class DiscordHelper
    {
        // Convert a string to a SocketRole
        public static SocketRole GetRoleFromString(this BotCommand command)
        {
            var possibleRoles = command.Context.Guild.Roles.Where(x => x.Name.ToLower() == command.FirstInput.ToLower());
            return possibleRoles.Count() > 0 ? possibleRoles.First() : null;
        }

        // Convert a string to a SocketRole (when the role is the second parameter)
        public static SocketRole GetRoleFromSecondString(this BotCommand command)
        {
            var possibleRoles = command.Context.Guild.Roles.Where(x => x.Name.ToLower() == command.SecondInput.ToLower());
            return possibleRoles.Count() > 0 ? possibleRoles.First() : null;
        }

        // Get a SocketGuildUser from a string (first input is the user)
        public static SocketGuildUser GetUserFromString(this BotCommand command)
        {
            SocketGuildUser user = null;

            // Target might be the user that did the command
            if (command.FirstInput == "me")
                user = (SocketGuildUser)command.Context.User;

            // Target might be Codey
            if (command.FirstInput == "self")
                user = command.Context.Guild.Users.Where(x => x.Id == 900493627623604234).First();

            // Target might be a string (username)
            if (user == null)
            {
                var possibleUsers = command.Context.Guild.Users.Where(x => x.Username.ToLower() == command.FirstInput.ToLower());
                user = possibleUsers.Count() > 0 ? possibleUsers.First() : null;
            }

            // Target might be an ID
            if (user == null)
            {
                var id = Convert.ToUInt64(command.FirstInput.Replace("!", ""));
                var possibleUsers = command.Context.Guild.Users.Where(x => x.Id == id);
                user = possibleUsers.Count() > 0 ? possibleUsers.First() : null;
            }

            return user;
        }
    }
}