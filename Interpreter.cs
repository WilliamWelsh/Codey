using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using OpenAI;

namespace Codey
{
    public class Interpreter
    {
        private readonly OpenAIAPI _openAI;

        private readonly string _prompt;

        private readonly CompletionRequestBuilder requestTemplate;

        private readonly EmbedBuilder LogMessage;

        public Interpreter()
        {
            // Initialize the OpenAI API
            _openAI = new OpenAIAPI(Environment.GetEnvironmentVariable("OpenAIToken"), Engine.Ada);

            // Get our preset promptt
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Codey.TrainingPrompt.txt"))
            using (var reader = new StreamReader(stream))
            {
                _prompt = $"{reader.ReadToEnd()}\nQ:";
            }

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
                .WithPrompt($"{_prompt} {rawInput}\n")
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

            await context.Channel.SendMessageAsync(await DoCommand(new BotCommand
            {
                Name = command,
                FirstInput = firstInput,
                SecondInput = secondInput,
                Context = context
            }));
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
                    return await command.MakeTextChannel();

                case "list-roles":
                    return command.ListAllRoles();

                case "make-role-with-color":
                    return await command.MakeRoleWithColor();

                case "give-user-role":
                    return await command.GiveRole();

                case "remove-user-role":
                case "take-user-role":
                    return await command.RemoveRole();

                default:
                    break;
            }

            return "Sorry, I was unable to complete the task. I need some refining.";
        }
    }
}