using System.Collections.Generic;
using SAIN.SAINComponent;

namespace SAIN.Components.PlayerComponentSpace;

public class OtherPlayersData : PlayerComponentBase
{
    public Dictionary<string, OtherPlayerData> DataDictionary { get; } = [];
    public HashSet<OtherPlayerData> DataHashSet { get; } = [];
    public List<OtherPlayerData> DataList { get; } = [];

    public OtherPlayersData(PlayerComponent playerComponent)
        : base(playerComponent)
    {
        var playerTracker = GameWorldComponent.Instance?.PlayerTracker;
        if (playerTracker == null)
        {
#if DEBUG
            Logger.LogError("player tracker null");
#endif
            return;
        }
        // Subscribe to player added or removed events
        playerTracker.OnPlayerAdded += PlayerAdded;
        playerTracker.OnPlayerRemoved += PlayerRemoved;
        // Add any already existing player.
        foreach (PlayerComponent player in playerTracker.AlivePlayerArray)
        {
            PlayerAdded(player);
        }
    }

    public override void Dispose()
    {
        var playerTracker = GameWorldComponent.Instance?.PlayerTracker;
        if (playerTracker != null)
        {
            playerTracker.OnPlayerAdded -= PlayerAdded;
            playerTracker.OnPlayerRemoved -= PlayerRemoved;
        }

        foreach (OtherPlayerData data in DataHashSet)
        {
            data.Dispose();
        }
        DataDictionary.Clear();
        DataHashSet.Clear();
        DataList.Clear();
    }

    private void PlayerAdded(PlayerComponent playerComp)
    {
        GetOrAddData(playerComp);
    }

    /// <summary>
    /// Fetches this player's data for <paramref name="playerComp"/>, creating it if it is missing.
    /// </summary>
    public OtherPlayerData GetOrAddData(PlayerComponent playerComp)
    {
        if (playerComp == null || playerComp == PlayerComponent)
        {
            return null;
        }

        string profileId = playerComp.ProfileId;

        if (string.IsNullOrEmpty(profileId) || profileId == PlayerComponent.ProfileId)
        {
            return null;
        }

        if (DataDictionary.TryGetValue(profileId, out OtherPlayerData data))
        {
            if (data.OtherPlayerComponent == playerComp)
            {
                return data;
            }

            RemoveData(profileId, data);
        }

        data = new OtherPlayerData(profileId, playerComp);
        DataHashSet.Add(data);
        DataList.Add(data);
        DataDictionary.Add(profileId, data);
        return data;
    }

    private void RemoveData(string profileId, OtherPlayerData data)
    {
        data.Dispose();
        DataHashSet.Remove(data);
        DataList.Remove(data);
        DataDictionary.Remove(profileId);
    }

    private void PlayerRemoved(string profileId, PlayerComponent playerComp)
    {
        if (DataDictionary.TryGetValue(profileId, out OtherPlayerData data))
        {
            RemoveData(profileId, data);
        }
    }
}
