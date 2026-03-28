using System;
using System.Collections.Generic;
using System.Text;
using VeeamInterviewProject.Models;
using VeeamInterviewProject.Infrastructure;
using VeeamInterviewProject.Services;

namespace VeeamInterviewProject.Startup
{
    internal class Startup
    {
        /// <summary>
        /// Initialize all classes, receive and parses arguments into Config and prepares sync
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task<(SyncFolder sync, CancellationTokenSource token, Log log)> Initialize(string[] args)
        {
            CommandLineParser command = new CommandLineParser();
            Config config = await command.ParseAsync(args);

            Log log = new Log(config.LogPath!);
            CompareFiles compare = new CompareFiles(log);
            SyncFolder sync = new SyncFolder(config.Source!, config.Target!, log, config.Interval, compare);

            var token = new CancellationTokenSource();

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true; // prevent immediate exit
                log.InfoMessage("Cancellation requested");
                token.Cancel();            // signal the token
            };
            return (sync, token, log);
        }
    }
}
