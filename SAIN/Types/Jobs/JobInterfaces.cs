using System;
using EFT;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SAIN.Types.Jobs;

public interface IBotRaycastJobSingleOwner
{
    public IPlayer Owner { get; }
}

public interface IBotRaycastJobSingleTarget
{
    public IPlayer Target { get; }
}

public interface IRaycastJob : IDisposable
{
    public JobHandle Schedule(int MaxCommandsPerJob = 8);

    public void Complete();

    public bool IsScheduled { get; }
    public bool IsCompleted { get; }
    public bool IsCreated { get; }
    public JobHandle Handle { get; }
    public NativeArray<RaycastHit> Hits { get; }
    public NativeArray<RaycastCommand> Commands { get; }
    public int TotalRaycasts { get; }
    public LayerMask Mask { get; }
}
