﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Text;
using Discord;
using Discord.Commands;
using Pootis_Bot.Helper;

namespace Pootis_Bot.Module.Basic
{
	[Group("help")]
	[Name("Help Commands")]
	[Summary("Provides help commands")]
	public class HelpCommands : ModuleBase<SocketCommandContext>
	{
		private readonly CommandService commandService;

		public HelpCommands(CommandService cmdService)
		{
			commandService = cmdService;
		}

		[Command]
		[Summary("Gets help on all commands")]
		public async Task Help()
		{
			await Context.Channel.SendMessageAsync("I will DM you the help info!");

			IDMChannel dm = await Context.User.GetOrCreateDMChannelAsync();

			foreach (string message in BuildHelpMenu())
			{
				await dm.SendMessageAsync(message);
			}
		}

		[Command]
		[Summary("Gets help on a specific command")]
		public async Task Help([Remainder] string query)
		{
			SearchResult searchResult = commandService.Search(Context, query);
			if (!searchResult.IsSuccess)
			{
				await Context.Channel.SendErrorMessageAsync("That command does not exist!");
				return;
			}

			EmbedBuilder embed = new EmbedBuilder();
			embed.WithTitle($"Help for `{query}`");
			foreach (CommandMatch match in searchResult.Commands)
			{
				embed.AddField(match.Command.Name,
					$"**Summary**: {match.Command.Summary}\n**Usage**: {BuildCommandUsage(match.Command)}");
			}

			await Context.Channel.SendEmbedAsync(embed);
		}

		private IEnumerable<string> BuildHelpMenu()
		{
			List<string> groups = new();
			ModuleInfo[] modules = commandService.Modules.ToArray();
			foreach (ModuleInfo module in modules)
			{
				string message = $"```diff\n+ {module.Name}\n  - Summary: {module.Summary}\n";
				message = module.Commands.Aggregate(message, (current, command) => current + $"\n- {BuildCommandFormat(command)}\n  - Summary: {command.Summary}\n  - Usage: {BuildCommandUsage(command)}");
				message += "\n```";

				//If its the first group, ignore
				if (groups.Count != 0)
				{
					//Get the combined message size of the last group and this group
					int lastMessageAndNewMessageLength = groups[^1].Length + message.Length;
					if (lastMessageAndNewMessageLength < 1998)
						groups[^1] += message;
					else //Too big, send as its own
						groups.Add(message);
				}
				else
					groups.Add(message);
			}

			return groups;
		}

		private string BuildCommandUsage(CommandInfo command)
		{
			using Utf16ValueStringBuilder commandUsage = ZString.CreateStringBuilder();
			commandUsage.Append($"`{BuildCommandFormat(command)}");
			foreach (ParameterInfo parameter in command.Parameters)
			{
				commandUsage.Append($" <{parameter.Name.ToLower()}");
				if (parameter.DefaultValue != null)
				{
					commandUsage.Append($" = {parameter.DefaultValue}");
				}

				commandUsage.Append(">");
			}

			commandUsage.Append("`");
			return commandUsage.ToString();
		}

		private string BuildCommandFormat(CommandInfo command)
		{
			string groupName = command.Module.Group;
			string commandName = command.Name.ToLower();

			string commandFormat = commandName;
			if (string.IsNullOrEmpty(groupName)) 
				return commandFormat;
			
			groupName = groupName.ToLower();
			if (groupName != commandName)
				commandFormat = $"{groupName} {commandName}";
			return commandFormat;
		}
	}
}