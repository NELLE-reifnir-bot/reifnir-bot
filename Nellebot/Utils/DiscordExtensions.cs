﻿using DSharpPlus.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nellebot.Utils;

public static class DiscordExtensions
{
    public static string GetNicknameOrDisplayName(this DiscordMember member)
    {
        var username = !string.IsNullOrWhiteSpace(member.Nickname)
            ? member.Nickname
            : member.DisplayName;

        return username;
    }

    public static string GetDetailedMemberIdentifier(this DiscordMember member)
    {
        return !string.IsNullOrWhiteSpace(member.Username)
            ? $"{member.Username}#{member.Discriminator}, User Id: {member.Id}"
            : string.Empty;
    }

    public static string GetUserFullUsername(this DiscordUser user)
    {
        return !string.IsNullOrWhiteSpace(user.Username)
            ? $"{user.Username}#{user.Discriminator}"
            : string.Empty;
    }

    public static async Task<T?> GetLastAuditLogEntry<T>(this DiscordGuild guild, AuditLogActionType logType, Func<T, bool> predicate) where T : DiscordAuditLogEntry
    {
        var entry = (await guild.GetAuditLogsAsync(limit: 10, by_member: null, action_type: logType))
                .Where(x => x.CreationTimestamp > DateTimeOffset.UtcNow.AddMinutes(-5))
                .Cast<T>()
                .FirstOrDefault(predicate);

        return entry;
    }
}