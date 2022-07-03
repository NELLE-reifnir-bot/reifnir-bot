﻿using DSharpPlus.EventArgs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nellebot.NotificationHandlers
{
    public abstract record EventNotification() : INotification;

    public record GuildMemberUpdatedNotification(GuildMemberUpdateEventArgs EventArgs) : EventNotification;
    public record GuildMemberAddedNotification(GuildMemberAddEventArgs EventArgs) : EventNotification;
    public record GuildMemberRemovedNotification(GuildMemberRemoveEventArgs EventArgs) : EventNotification;
    public record GuildBanAddedNotification(GuildBanAddEventArgs EventArgs) : EventNotification;
    public record GuildBanRemovedNotification(GuildBanRemoveEventArgs EventArgs) : EventNotification;

    public record MessageCreatedNotification(MessageCreateEventArgs EventArgs) : EventNotification;
    public record MessageDeletedNotification(MessageDeleteEventArgs EventArgs) : EventNotification;
    public record MessageBulkDeletedNotification(MessageBulkDeleteEventArgs EventArgs) : EventNotification;
}