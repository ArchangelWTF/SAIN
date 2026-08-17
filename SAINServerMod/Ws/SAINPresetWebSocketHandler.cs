using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using SAIN.Preset.Shared.Models.WS;
using SAINServerMod.Utils;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Servers.Ws;

namespace SAINServerMod.Ws;

[Injectable(InjectionType.Singleton)]
public sealed class SAINPresetWebSocketHandler(ISptLogger<SAINPresetWebSocketHandler> logger) : IWebSocketConnectionHandler
{
    private readonly ConcurrentDictionary<WebSocket, string> _sockets = new();

    public string GetHookUrl()
    {
        return "/sain/websocket/";
    }

    public string GetSocketId()
    {
        return "SAIN Preset Sync";
    }

    public Task OnConnectionAsync(WebSocket ws, HttpContext context, string sessionIdContext)
    {
        string sessionId = SessionIdFrom(context);
        _sockets.AddOrUpdate(ws, sessionId, (_, _) => sessionId);
        logger.Info($"[SAIN] Preset sync socket connected for session {sessionId}.");
        return Task.CompletedTask;
    }

    public Task OnMessageAsync(byte[] rawData, WebSocketMessageType messageType, WebSocket ws, HttpContext context)
    {
        return Task.CompletedTask;
    }

    public Task OnCloseAsync(WebSocket ws, HttpContext context, string sessionIdContext)
    {
        _sockets.TryRemove(ws, out _);
        return Task.CompletedTask;
    }

    public async Task BroadcastPresetChanged(string presetName, EPresetSyncChange change, string? exceptSessionId = null)
    {
        var message = new SAINPresetSyncMessage { PresetName = presetName, Change = change };
        byte[] payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message, SAINJsonUtil.Indented));

        int sent = 0;
        foreach (var (socket, session) in _sockets)
        {
            if (socket.State != WebSocketState.Open)
            {
                _sockets.TryRemove(socket, out _);
                continue;
            }

            if (exceptSessionId != null && string.Equals(session, exceptSessionId, StringComparison.Ordinal))
            {
                continue;
            }

            try
            {
                await socket.SendAsync(new ArraySegment<byte>(payload), WebSocketMessageType.Text, true, CancellationToken.None);
                sent++;
            }
            catch (Exception ex)
            {
                logger.Error($"[SAIN] Failed to push preset change to session {session}: {ex.Message}");
            }
        }
    }

    private static string SessionIdFrom(HttpContext context)
    {
        string path = context.Request.Path.Value ?? string.Empty;
        string last = path.TrimEnd('/').Split('/').LastOrDefault() ?? string.Empty;
        return last;
    }
}
