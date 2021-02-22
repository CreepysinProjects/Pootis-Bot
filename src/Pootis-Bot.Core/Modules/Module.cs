﻿using System.Diagnostics.CodeAnalysis;
using Discord.WebSocket;

namespace Pootis_Bot.Modules
{
	/// <summary>
	///     A module for Pootis-Bot. Can be used to add command and functions to the bot
	/// </summary>
	public abstract class Module
	{
		private ModuleInfo cachedModuleInfo;

		/// <summary>
		///     Gets info relating to the modules
		/// </summary>
		/// <returns></returns>
		public abstract ModuleInfo GetModuleInfo();

		/// <summary>
		///     Called on initialization
		/// </summary>
		public virtual void Init()
		{
		}

		/// <summary>
		///     Called after all modules are initialized.
		///     <para>
		///         Here is a good spot to check if other modules are loaded
		///         with <see cref="ModuleManager.CheckIfModuleIsLoaded" />, in-case you want to soft-depend on another module.
		///     </para>
		/// </summary>
		public virtual void PostInit()
		{
		}

		/// <summary>
		///		Called when the <see cref="DiscordSocketClient"/> connects
		/// </summary>
		/// <param name="client"></param>
		public virtual void ClientConnected([DisallowNull] DiscordSocketClient client)
		{
		}

		/// <summary>
		///     Called on shutdown
		/// </summary>
		public virtual void Shutdown()
		{
		}

		/// <summary>
		///		Call this if you are accessing <see cref="ModuleInfo"/> from Pootis's core
		///		<para>It uses a cached version of <see cref="ModuleInfo"/>, just in-case the module returns something different each time.</para>
		/// </summary>
		/// <returns></returns>
		internal ModuleInfo GetModuleInfoInternal()
		{
			if (cachedModuleInfo.ModuleName == null)
				cachedModuleInfo = GetModuleInfo();

			return cachedModuleInfo;
		}
	}
}