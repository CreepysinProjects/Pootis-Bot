﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Helpers;
using Pootis_Bot.Preconditions;
using Pootis_Bot.Services;
using Pootis_Bot.Structs.Server;

namespace Pootis_Bot.Modules.Server
{
	public class ServerPermissions : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Creepysin
		// Description      - Anything permission related
		// Contributors     - Creepysin, 

		private readonly PermissionService _perm;

		public ServerPermissions(CommandService commandService)
		{
			_perm = new PermissionService(commandService);
		}

		[Command("perm")]
		[Summary("Adds or removes command permission")]
		[RequireGuildOwner]
		public async Task Permission(string command, string subCmd, [Remainder] string[] roles)
		{
			switch (subCmd)
			{
				case "add":
					await _perm.AddPerm(command, roles, Context.Channel, Context.Guild);
					break;
				default:
					await _perm.RemovePerm(command, roles, Context.Channel, Context.Guild);
					break;
			}
		}

		[Command("perms")]
		[Alias("permissions", "allperm", "allperms")]
		[RequireGuildOwner]
		public async Task Permissions()
		{
			StringBuilder sb = new StringBuilder();
			ServerList server = ServerListsManager.GetServer(Context.Guild);

			sb.Append("**__Permissions__**\n");

			foreach (ServerList.CommandPermission perm in server.CommandPermissions)
				sb.Append($"__`{perm.Command}`__\nRoles: {FormatRoles(perm.Roles, Context.Guild)}\n\n");

			await Context.Channel.SendMessageAsync(sb.ToString());
		}

		#region Functions

		private static string FormatRoles(IEnumerable<ulong> roles, SocketGuild guild)
		{
			return roles.Aggregate("", (current, role) => current + $"{RoleUtils.GetGuildRole(guild, role).Name}, ");
		}

		#endregion
	}
}