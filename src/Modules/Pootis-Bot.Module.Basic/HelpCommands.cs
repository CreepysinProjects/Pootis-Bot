﻿using System.Collections.Generic;
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

			foreach (Utf16ValueStringBuilder stringBuilder in BuildHelpMenu())
			{
				await dm.SendMessageAsync(stringBuilder.ToString());
				stringBuilder.Dispose();
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

		private Utf16ValueStringBuilder[] BuildHelpMenu()
		{
			List<Utf16ValueStringBuilder> groups = new List<Utf16ValueStringBuilder>();
			foreach (ModuleInfo module in commandService.Modules)
			{
				Utf16ValueStringBuilder sb = ZString.CreateStringBuilder();
				sb.Append("```diff\n");
				sb.Append($"+ {module.Name}\n");
				sb.Append($"  - Summary: {module.Summary}\n");

				foreach (CommandInfo command in module.Commands)
				{
					sb.Append(
						$"\n- {BuildCommandFormat(command)}\n  - Summary: {command.Summary}\n  - Usage: {BuildCommandUsage(command)}");
				}

				sb.Append("\n```");
				groups.Add(sb);
			}

			return groups.ToArray();
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