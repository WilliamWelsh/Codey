using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Codey
{
    public class Resources
    {
        public readonly string TrainingPrompt;

        public readonly List<string> Apologies;

        public readonly List<string> Successes;

        public readonly List<string> NewChannelMessages;

        private readonly Random random;

        public Resources()
        {
            // Get our preset prompt
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Codey.Resources.TrainingPrompt.txt"))
            using (var reader = new StreamReader(stream))
            {
                TrainingPrompt = $"{reader.ReadToEnd()}\nQ:";
            }

            Apologies = ReadResourceIntoList("Apologies");

            Successes = ReadResourceIntoList("Successes");

            NewChannelMessages = ReadResourceIntoList("NewChannelMessages");

            random = new Random();
        }

        public string GetRandomApology() => Apologies[random.Next(Apologies.Count)];

        public string GetRandomSuccess() => Successes[random.Next(Successes.Count)];

        public string GetRandomNewChannelMessage() => NewChannelMessages[random.Next(NewChannelMessages.Count)];

        private List<string> ReadResourceIntoList(string fileName)
        {
            var list = new List<string>();
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"Codey.Resources.{fileName}.txt"))
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    list.Add(line);
                }
            }
            return list;
        }
    }
}