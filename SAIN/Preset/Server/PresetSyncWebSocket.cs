using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SAIN.Preset.Shared.Models.WS;
using SPT.Common.Http;
using WebSocketSharp;

namespace SAIN.Preset.Server;

public sealed class PresetSyncWebSocket
{
    private const int RECONNECT_DELAY_MS = 10_000;

    private static readonly JsonSerializerSettings _settings = new() { Converters = { new StringEnumConverter() } };
    private static readonly ConcurrentQueue<SAINPresetSyncMessage> _pending = new();

    public static PresetSyncWebSocket Instance { get; private set; }

    private static bool _startAttempted;
    private static volatile bool _resyncRequested;

    public static void Start()
    {
        if (_startAttempted)
        {
            return;
        }
        _startAttempted = true;
        try
        {
            _ = new PresetSyncWebSocket();
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"[SAIN] Preset sync unavailable: {ex.Message}");
        }
    }

    private readonly WebSocket _webSocket;
    private bool _reconnecting;
    private bool _closing;

    public bool Connected
    {
        get { return _webSocket != null && _webSocket.ReadyState == WebSocketState.Open; }
    }

    public PresetSyncWebSocket()
    {
        Instance = this;

        string host = RequestHandler.Host.Replace("http", "ws");
        string url = $"{host}/sain/websocket/";

        _webSocket = new WebSocket(url) { WaitTime = TimeSpan.FromSeconds(15), EmitOnPing = true };

        _webSocket.OnMessage += onMessage;
        _webSocket.OnError += (sender, e) => Logger.LogDebug($"[SAIN] Preset sync socket error: {e.Message}");
        _webSocket.OnOpen += (sender, e) =>
        {
            _resyncRequested = true;
            Logger.LogInfo("[SAIN] Preset sync socket connected; resyncing custom presets.");
        };
        _webSocket.OnClose += (sender, e) =>
        {
            if (_closing || _reconnecting)
            {
                return;
            }
            Task.Run(reconnect);
        };

        try
        {
            _webSocket.ConnectAsync();
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"[SAIN] Could not open the preset sync socket, live updates disabled: {ex.Message}");
        }
    }

    public void Close()
    {
        _closing = true;
        try
        {
            _webSocket?.Close();
        }
        catch (Exception ex)
        {
            Logger.LogDebug($"[SAIN] Error closing preset sync socket: {ex.Message}");
        }
    }

    private void onMessage(object sender, MessageEventArgs e)
    {
        if (e == null || e.IsPing || string.IsNullOrEmpty(e.Data))
        {
            return;
        }

        try
        {
            var message = JsonConvert.DeserializeObject<SAINPresetSyncMessage>(e.Data, _settings);
            if (message == null)
            {
                return;
            }

            if (message.Change != EPresetSyncChange.ConfigChanged && string.IsNullOrEmpty(message.PresetName))
            {
                return;
            }
            _pending.Enqueue(message);
        }
        catch (Exception ex)
        {
            Logger.LogError($"[SAIN] Malformed preset sync message: {ex.Message}");
        }
    }

    public static void Resync()
    {
        if (_resyncRequested)
        {
            _resyncRequested = false;
            try
            {
                PresetSync.ResyncCustomPresets();
            }
            catch (Exception ex)
            {
                Logger.LogError($"[SAIN] Preset resync after connect failed: {ex.Message}");
            }
        }

        // A change that happened in raid is applied once the raid ends.
        PresetSync.ProcessDeferred();

        while (_pending.TryDequeue(out var message))
        {
            try
            {
                if (message.Change == EPresetSyncChange.ConfigChanged)
                {
                    PresetSync.ApplyRemoteConfigChange();
                }
                else
                {
                    PresetSync.ApplyRemoteChange(message);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"[SAIN] Failed to apply pushed change '{message.PresetName}': {ex}");
            }
        }
    }

    private async Task reconnect()
    {
        _reconnecting = true;
        while (_reconnecting && !_closing)
        {
            if (_webSocket.ReadyState == WebSocketState.Open)
            {
                break;
            }
            if (_webSocket.ReadyState != WebSocketState.Connecting)
            {
                try
                {
                    _webSocket.Connect();
                }
                catch (Exception ex)
                {
                    Logger.LogDebug($"[SAIN] Preset sync reconnect failed: {ex.Message}");
                }
            }

            await Task.Delay(RECONNECT_DELAY_MS);

            if (_webSocket.ReadyState == WebSocketState.Open)
            {
                break;
            }
        }
        _reconnecting = false;
    }
}
