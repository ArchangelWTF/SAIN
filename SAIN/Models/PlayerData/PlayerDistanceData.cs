using SAIN.Components.PlayerComponentSpace;
using UnityEngine;

namespace SAIN.Models.PlayerData;

public sealed class PlayerDistanceData
{
    public PlayerDistanceData(PlayerComponent OtherPlayer)
    {
        Data = new() { MainDirectionData = new() };
    }

    public void SetPlayerDirectionData(PlayerDirectionData data)
    {
        Data = data;
    }

    public PlayerDirectionData Data { get; private set; }

    /// <summary>
    /// The Other Player's Position
    /// </summary>
    public Vector3 Position
    {
        get { return Data.MainDirectionData.Position; }
    }

    /// <summary>
    /// Direction from the owner's position to the other player's position.
    /// </summary>
    public Vector3 Direction
    {
        get { return Data.MainDirectionData.Direction; }
    }

    /// <summary>
    /// Normalized Direction from the owner's position to the other player's position.
    /// </summary>
    public Vector3 DirectionNormal
    {
        get { return Data.MainDirectionData.DirectionNormalized; }
    }

    /// <summary>
    /// Distance, in Meters, from The Owner to the Other Player.
    /// </summary>
    public float Distance
    {
        get { return Data.MainDirectionData.Distance; }
    }
}
