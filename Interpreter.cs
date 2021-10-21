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
        private readonly OpenAIAPI _openAI;

        private readonly CompletionRequestBuilder requestTemplate;

        private readonly EmbedBuilder LogMessage;

        private readonly Resources _resources;

        public Interpreter()
        {
            // Initialize the OpenAI API
            _openAI = new OpenAIAPI(Environment.GetEnvironmentVariable("OpenAIToken"), Engine.Ada);

            // Set up our embedded resources
            _resources = new Resources();

            // Set up our CompletionRequestBuilder template
            requestTemplate = new CompletionRequestBuilder()
                .WithTemperature(0)
                .WithMaxTokens(100)
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
            var request = requestTemplate
                .WithPrompt($"{_resources.TrainingPrompt} {rawInput}\n")
                .Build();

            // Get the response from OpenAI and clean it up
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

            await context.Channel.SendMessageAsync(result.Contains("I was unable to complete the task. I need some refining") ? result : $"{_resources.GetRandomSuccess()} {result}");
        }

        public async Task<string> DoCommand(BotCommand command)
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

                default:
                    break;
            }

            return $"{_resources.GetRandomApology()}, I was unable to complete the task. I need some refining.";
        }
    }
}