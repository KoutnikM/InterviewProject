using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;
using VeeamInterviewProject.Abstractions;
using VeeamInterviewProject.Models;

namespace VeeamInterviewProject.Infrastructure
{
    /// <summary>
    /// Configurates command line, adds --source, --target, --interval, --arguments and parses them into Config, stops the program if any of arguments is missing or if interval is greater then 0 or not int
    /// </summary>
    internal class CommandLineParser : ICommandLineParser
    {
        private readonly Option<string> sourceOption = new("--source")
        {
            Required = true,
            Description = "Source folder path"
        };

        private readonly Option<string> targetOption = new("--target")
        {
            Required = true,
            Description = "Target folder path"
        };

        private readonly Option<int> intervalOption = new("--interval")
        {
            Required = true,
            Description = "Synchronization interval in seconds",
            CustomParser = result =>
            {
                if (int.TryParse(result.Tokens.Single().Value, out var interval))
                {
                    if (interval < 1)
                    {
                        result.AddError("--interval must be greater than 0");
                    }
                    return interval;
                }
                else
                {
                    result.AddError("Not an int.");
                    return 0;
                }
            }
        };

        private readonly Option<string> logOption = new("--log")
        {
            Required = true,
            Description = "Log file path",
        };

        public async Task<Config> ParseAsync(string[] args)
        {
            Config config = new();

            try
            {
                RootCommand rootCommand = new RootCommand("Folder synchronizer");
                rootCommand.Options.Add(sourceOption);
                rootCommand.Options.Add(targetOption);
                rootCommand.Options.Add(intervalOption);
                rootCommand.Options.Add(logOption);

                ParseResult parseResult = rootCommand.Parse(args);

                config.Source = parseResult.GetRequiredValue<string>(sourceOption);
                config.Target = parseResult.GetRequiredValue<string>(targetOption);
                config.Interval = parseResult.GetRequiredValue<int>(intervalOption);
                config.LogPath = parseResult.GetRequiredValue<string>(logOption);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing arguments: {ex.Message}");
                Environment.Exit(1);
            }

            return config;
        }
    }
}
