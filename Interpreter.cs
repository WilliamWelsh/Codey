using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using OpenAI;

namespace Codey
{
    public class Interpreter
    {
        public readonly OpenAIAPI _openAI;

        public readonly CompletionRequestBuilder commandRequestTemplate;

        public readonly CompletionRequestBuilder channelToEmojiTemplate;

        public readonly EmbedBuilder LogMessage;

        public readonly Resources _resources;

        private const bool LOG_MESSAGES = true;

        public Interpreter()
        {
            // Initialize the OpenAI API
            _openAI = new OpenAIAPI(Environment.GetEnvironmentVariable("OpenAIToken"), Engine.Ada);

            // Set up our embedded resources
            _resources = new Resources();

            // Set up our CompletionRequestBuilder template for the commands
            commandRequestTemplate = new CompletionRequestBuilder()
                .WithTemperature(0)
                .WithMaxTokens(100)
                .WithTopP(1)
                .WithFrequencyPenalty(0)
                .WithPresencePenalty(0)
                .WithStop(new List<string> { "\n" });

            // Set up our CompletionRequestBuilder template to convert channel names to emojis
            channelToEmojiTemplate = new CompletionRequestBuilder()
                .WithTemperature(0)
                .WithMaxTokens(6)
                .WithTopP(1)
                .WithFrequencyPenalty(0)
                .WithPresencePenalty(0)
                .WithStop(new List<string> { "\n" });

            // Set up our log message EmbedBuilder
            LogMessage = new EmbedBuilder()
                .WithColor(new Color(188, 120, 179));
        }

        public async Task Interpret(SocketCommandContext context)
        {
            // Replace the "@Cody" part of the message
            var rawInput = context.Message.Content.Substring(23, context.Message.Content.Length - 23);

            Console.WriteLine(rawInput);

            // Create our request
            var request = commandRequestTemplate
                .WithPrompt($"{_resources.CommandPrompt} {rawInput}\n")
                .Build();

            // Get the response from OpenAI and clean it up
            _openAI.UsingEngine = Engine.Ada;
            var response = (await _openAI.Completions.CreateCompletionAsync(request)).ToString().Replace("A: ", "");

            // Get the command
            // Example: make-role-with-color
            var command = response;
            if (command.Contains(" "))
                command = response.Substring(0, response.IndexOf(" `"));

            // Get the first input inside the ``
            // Example: Bots
            var firstInput = "";
            if (response.Contains(" "))
            {
                firstInput = response.Substring(response.IndexOf("`") + 1);
                firstInput = firstInput.Substring(0, firstInput.IndexOf("`"));
            }

            // Get the second input after the ``
            // Example: White
            var secondInput = "";
            if (response.Contains("` "))
                secondInput = response.Substring(response.LastIndexOf("` ") + 2);

            if (LOG_MESSAGES)
                await context.Channel.SendMessageAsync(null, false, LogMessage
                    .WithAuthor(new EmbedAuthorBuilder()
                        .WithName(context.User.ToString())
                        .WithIconUrl(context.User.GetAvatarUrl() ?? context.User.GetDefaultAvatarUrl()))
                    .WithDescription($"Command: {command}\n\nFirst input: {firstInput}\n\nSecond input: {(secondInput == "" ? "None" : secondInput)}")
                    .Build());

            var result = await DoCommand(new BotCommand
            {
                Name = command,
                FirstInput = firstInput,
                SecondInput = secondInput,
                Context = context
            });

            if (result.wasSuccessful)
                await context.Message.ReplyAsync($"{_resources.GetRandomSuccess()} {result.Text}");
            else
                await context.Message.ReplyAsync($"{_resources.GetRandomApology()}, {result.Text}");
        }

        public async Task<CommandResult> DoCommand(BotCommand command)
        {
            switch (command.Name)
            {
                case "make-role":
                    return await command.MakeRole();

                case "delete-role":
                    return await command.DeleteRole();

                case "change-role-color":
                    return await command.ChangeRoleColor();

                case "make-text-channel":
                    return await command.MakeTextChannel(_resources);

                case "list-roles":
                    return command.ListAllRoles();

                case "make-role-with-color":
                    return await command.MakeRoleWithColor();

                case "give-user-role":
                    return await command.GiveRole();

                case "remove-user-role":
                case "take-user-role":
                    return await command.RemoveRole();

                case "get-avatar":
                    return command.GetAvatar();

                case "give-every-text-channel-emoji":
                    return await command.GiveEveryTextChannelAnEmoji(this);

                case "give-text-channel-emoji":
                    return await command.GiveTextChannelAnEmoji(this);

                default:
                    break;
            }

            return new CommandResult();
        }
    }
}