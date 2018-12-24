﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core.UserAccounts;

namespace Pootis_Bot.Modules
{
    public class ProfileMang : ModuleBase<SocketCommandContext>
    {      
        [Command("MakeNotWarnable")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task NotWarnable(IGuildUser user)
        {
            var userAccount = UserAccounts.GetAccount((SocketUser)user);
            if (userAccount.IsNotWarnable == true)
            {
                await Context.Channel.SendMessageAsync($"The user {user} is already not warnable.");
            }
            else
            {
                userAccount.IsNotWarnable = true;
                userAccount.NumberOfWarnings = 0;
                UserAccounts.SaveAccounts();
                Console.WriteLine($"The user {user} was made not warnable.");
                await Context.Channel.SendMessageAsync($"The user {user} was made not warnable.");
            }
        }
    
        [Command("MakeWarnable")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task MakeWarnable(IGuildUser user)
        {
            var userAccount = UserAccounts.GetAccount((SocketUser)user);
            if (userAccount.IsNotWarnable == false)
            {
                await Context.Channel.SendMessageAsync($"The user {user} is already warnable.");
            }
            else
            {
                userAccount.IsNotWarnable = false;
                UserAccounts.SaveAccounts();
                Console.WriteLine($"The user {user} was made warnable.");
                await Context.Channel.SendMessageAsync($"The user {user} was made warnable.");
            }
        }

        [Command("Warn")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task WarnUser(IGuildUser user)
        {
            if (!UserIsStaff((SocketGuildUser)Context.User)) return;
            var userAccount = UserAccounts.GetAccount((SocketUser)user);

            if (userAccount.IsNotWarnable == true)
            {
                await Context.Channel.SendMessageAsync($"A warning cannot be given to {user}. That person's account is set to not warnable.");
                return;
            }
            else
            {
                userAccount.NumberOfWarnings++;
                UserAccounts.SaveAccounts();
                Console.WriteLine($"A warning was given to {user}");
                await Context.Channel.SendMessageAsync($"A warning was given to {user}");
            }

            if (userAccount.NumberOfWarnings >= 3)
            {
                Console.WriteLine($"{user} was kicked due to having 3 warnings.");
                await user.KickAsync("Was kicked due to having 3 warnings.");
            }

            if (userAccount.NumberOfWarnings >= 4)
            {
                Console.WriteLine($"{user} was baned due to having 4 warnings.");
                await user.Guild.AddBanAsync(user, 5, "Was baned due to having 4 warnings.");
            }
        }

        [Command("profile")]       
        public async Task Profile()
        {
            string userprofilpic = Context.User.GetAvatarUrl();

            var account = UserAccounts.GetAccount(Context.User);
            string WarningText = $"You Currently have {account.NumberOfWarnings} Warnings.";
            var embed = new EmbedBuilder();

            if (account.IsNotWarnable == true)
            {
                WarningText = "Your account is not warnable.";
            }

            embed.WithThumbnailUrl(userprofilpic);
            embed.WithTitle(Context.User.Username + "'s Profile.");
            embed.WithDescription($"You have {account.XP} XP. \nYou have {account.Points} points. \n \n" + WarningText);
            embed.WithColor(new Color(56, 56, 56));

            if (UserIsStaff((SocketGuildUser)Context.User))
                embed.WithColor(new Color(255, 119, 0));
            if (UserIsOverseer((SocketGuildUser)Context.User))
                embed.WithColor(new Color(255, 251, 35));
            await Context.Channel.SendMessageAsync("", false, embed);
        }       

        private bool UserIsStaff(SocketGuildUser user)
        {
            string targetRoleName = "Staff";
            var result = from r in user.Guild.Roles
                         where r.Name == targetRoleName
                         select r.Id;
            ulong roleID = result.FirstOrDefault();
            if (roleID == 0) return false;
            var targetRole = user.Guild.GetRole(roleID);
            return user.Roles.Contains(targetRole);
        }

        private bool UserIsOverseer(SocketGuildUser user)
        {
            string targetRoleName = "Overseer";
            var result = from r in user.Guild.Roles
                         where r.Name == targetRoleName
                         select r.Id;
            ulong roleID = result.FirstOrDefault();
            if (roleID == 0) return false;
            var targetRole = user.Guild.GetRole(roleID);
            return user.Roles.Contains(targetRole);
        }
    }
}