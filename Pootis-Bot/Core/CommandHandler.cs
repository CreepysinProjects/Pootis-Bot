﻿using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Pootis_Bot.Core
{
    public class CommandHandler
    {
        //https://docs.stillu.cc/guides/commands/intro.html
        //For helping me updated the old commandhandler to discord.net 2.0

        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;

        private readonly string _prefix;

        public CommandHandler(DiscordSocketClient client, CommandService commands, string prefix)
        {
            _commands = commands;
            _client = client;
            _prefix = prefix;
        }

        public async Task InstallCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;

            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                                            services: null);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            if (!(messageParam is SocketUserMessage msg)) return;
            var context = new SocketCommandContext(_client, msg);
            int argPos = 0;
            if (msg.Author.IsBot) //Check to see if user is bot, if is bot return.
                return;
            if (msg.HasStringPrefix(_prefix, ref argPos)
                || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, services: null);
                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                {
                    Global.ColorMessage($"[{ Global.TimeNow()}] " + result.ErrorReason, ConsoleColor.Red);
                }
            }
        }

    }
}
