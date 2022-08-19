﻿using System;
using System.Text.RegularExpressions;
using Damselfly.Core.Constants;
using Damselfly.Core.ScopedServices.Interfaces;
using Damselfly.Shared.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;

namespace Damselfly.Core.ScopedServices;

public class NotificationsService : IAsyncDisposable
{
    private readonly HubConnection hubConnection;

    public NotificationsService( NavigationManager navManager )
    {
        var hubUrl = $"{navManager.BaseUri}{NotificationHub.NotificationRoot}";
        Console.WriteLine($"Setting up notifications listener on {hubUrl}...");

        hubConnection = new HubConnectionBuilder()
                        .WithUrl( hubUrl )
                        .WithAutomaticReconnect()
                        .Build();

        _ = Task.Run(async () =>
        {
            await hubConnection.StartAsync();
        });
    }

    /// <summary>
    /// WASM: TODO: Unsubscribe and decompose
    /// </summary>
    /// <param name="type"></param>
    /// <param name="action"></param>
    public void SubscribeToNotification(NotificationType type, Action action)
    {
        var methodName = type.ToString();

        hubConnection.On<string>(methodName, (payload) =>
        {
            var payloadLog = string.IsNullOrEmpty(payload) ? "(no payload)" : $"(payload: {payload})";
            Console.WriteLine($"Received {methodName.ToString()} - calling action {payloadLog}");
            action.Invoke();
        });

        Console.WriteLine($"Subscribed to {methodName}");
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (hubConnection is not null)
            await hubConnection.DisposeAsync();
    }
}

