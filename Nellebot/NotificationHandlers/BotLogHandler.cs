﻿using DSharpPlus.Entities;
using MediatR;
using Microsoft.Extensions.Options;
using Nellebot.Services.Loggers;
using Nellebot.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Nellebot.NotificationHandlers
{
    public class BotLogHandler : INotificationHandler<GuildBanAddedNotification>,
                                INotificationHandler<GuildBanRemovedNotification>,
                                INotificationHandler<GuildMemberRemovedNotification>,
                                INotificationHandler<MessageDeletedNotification>,
                                INotificationHandler<MessageBulkDeletedNotification>
    {
        private readonly DiscordLogger _discordLogger;
        private readonly DiscordResolver _discordResolver;

        public BotLogHandler(DiscordLogger discordLogger, DiscordResolver discordResolver)
        {
            _discordLogger = discordLogger;
            _discordResolver = discordResolver;
        }

        public async Task Handle(GuildBanAddedNotification notification, CancellationToken cancellationToken)
        {
            var args = notification.EventArgs;

            var memberName = args.Member.GetNicknameOrDisplayName();
            var memberIdentifier = args.Member.GetDetailedMemberIdentifier();

            var auditBanEntry = await args.Guild
                    .GetLastAuditLogEntry<DiscordAuditLogBanEntry>(
                        AuditLogActionType.Ban, (x) => x.Target.Id == args.Member.Id);

            if (auditBanEntry == null) return;

            var memberResponsible = await _discordResolver.ResolveGuildMember(args.Guild, auditBanEntry.UserResponsible.Id);

            if (memberResponsible == null) return;

            var responsibleName = memberResponsible.GetNicknameOrDisplayName();

            await _discordLogger.LogMessage($"**{memberName}** was banned by **{responsibleName}**. Reason: {auditBanEntry.Reason}. [{memberIdentifier}]");
        }

        public async Task Handle(GuildBanRemovedNotification notification, CancellationToken cancellationToken)
        {
            var args = notification.EventArgs;

            var memberName = args.Member.GetNicknameOrDisplayName();
            var memberIdentifier = args.Member.GetDetailedMemberIdentifier();

            var auditUnbanEntry = await args.Guild
                                    .GetLastAuditLogEntry<DiscordAuditLogBanEntry>(
                                        AuditLogActionType.Unban, (x) => x.Target.Id == args.Member.Id);

            if (auditUnbanEntry == null) return;

            var memberResponsible = await _discordResolver.ResolveGuildMember(args.Guild, auditUnbanEntry.UserResponsible.Id);

            if (memberResponsible == null) return;

            var responsibleName = memberResponsible.GetNicknameOrDisplayName();

            await _discordLogger.LogMessage($"**{memberName}** was unbanned by **{responsibleName}**. [{memberIdentifier}]");
        }

        public async Task Handle(GuildMemberRemovedNotification notification, CancellationToken cancellationToken)
        {
            var args = notification.EventArgs;

            var memberName = args.Member.GetNicknameOrDisplayName();

            // It's possible that the audit log entry might not be available right away.
            // If that is the case, consider wrapping this call into some sort of exeponential backoff retry.
            var auditKickEntry = await args.Guild
                                .GetLastAuditLogEntry<DiscordAuditLogKickEntry>(
                                    AuditLogActionType.Kick, (x) => x.Target.Id == args.Member.Id);

            if (auditKickEntry == null) return;

            var memberResponsible = await _discordResolver.ResolveGuildMember(args.Guild, auditKickEntry.UserResponsible.Id);

            if (memberResponsible == null) return;

            var responsibleName = memberResponsible.GetNicknameOrDisplayName();

            await _discordLogger.LogMessage($"**{memberName}** was kicked by **{responsibleName}**. Reason: {auditKickEntry.Reason}.");
        }

        public async Task Handle(MessageDeletedNotification notification, CancellationToken cancellationToken)
        {
            var args = notification.EventArgs;

            var message = args.Message;
            var channel = args.Channel;

            if (channel.IsPrivate) return;

            var auditMessageDeleteEntry = await args.Guild
                                            .GetLastAuditLogEntry<DiscordAuditLogMessageEntry>(
                                                AuditLogActionType.MessageDelete, (x) => x.Target.Id == message.Author.Id);

            if (auditMessageDeleteEntry == null) return;

            if (message.Author.Id == auditMessageDeleteEntry.UserResponsible.Id) return;

            var authorAsMember = await _discordResolver.ResolveGuildMember(args.Guild, message.Author.Id);

            if (authorAsMember == null) return;

            var authorName = authorAsMember.GetNicknameOrDisplayName();

            var memberResponsible = await _discordResolver.ResolveGuildMember(args.Guild, auditMessageDeleteEntry.UserResponsible.Id);

            if (memberResponsible == null) return;

            var responsibleName = memberResponsible.GetNicknameOrDisplayName();

            await _discordLogger.LogMessage($"Message written by **{authorName}** in **{channel.Name}** was removed by **{responsibleName}**.");
        }

        public async Task Handle(MessageBulkDeletedNotification notification, CancellationToken cancellationToken)
        {
            var args = notification.EventArgs;

            var messages = args.Messages;
            var author = args.Messages.FirstOrDefault()?.Author;

            if (author == null) return;

            var auditUnbanEntry = await args.Guild
                        .GetLastAuditLogEntry<DiscordAuditLogBanEntry>(
                            AuditLogActionType.Unban, (x) => x.Target.Id == author.Id);

            if (auditUnbanEntry == null) return;

            var authorName = author.GetUserFullUsername();

            var memberResponsible = await _discordResolver.ResolveGuildMember(args.Guild, auditUnbanEntry.UserResponsible.Id);

            if (memberResponsible == null) return;

            var responsibleName = memberResponsible.GetNicknameOrDisplayName();

            await _discordLogger.LogMessage($"{messages.Count} messages written by **{authorName}** were removed by **{responsibleName}**.");
        }
    }
}