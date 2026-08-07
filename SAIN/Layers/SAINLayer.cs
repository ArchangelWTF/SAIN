using System.Text;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using SAIN.Components.BotController;
using SAIN.Models.Enums;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;

namespace SAIN.Layers;

public abstract class SAINLayer : CustomLayer
{
    public static string BuildLayerName(string name)
    {
        return $"SAIN : {name}";
    }

    protected virtual bool GetBotComponent()
    {
        if (Bot == null)
        {
            Bot = BotSpawnController.Instance.GetSAIN(BotOwner);
            if (Bot != null)
            {
                Bot.Decision.DecisionManager.OnDecisionMade += BotDecisionMade;
            }
        }
        return Bot != null;
    }

    public override bool IsCurrentActionEnding()
    {
        if (_actionReset)
        {
            _actionReset = false;
            return true;
        }
        return false;
    }

    protected void CheckActiveChanged(bool isActiveNow)
    {
        if (isActiveNow)
        {
            BotOwner.PatrollingData.Pause();
            SetLayer(true);
        }
        else
        {
            SetLayer(false);
        }
        _wasActive = isActiveNow;
    }

    private bool _wasActive = false;

    private void BotDecisionMade(
        ECombatDecision combatDecision,
        ESquadDecision squadDecision,
        ESelfActionType selfDecision,
        Enemy targetEnemy,
        BotComponent bot
    )
    {
        if (_wasActive)
        {
            _actionReset = true;
        }
    }

    private bool _actionReset = false;

    private readonly string LayerName;
    private readonly ESAINLayer ELayer;

    private string _currentLayerName;

    protected SAINLayer(BotOwner botOwner, int priority, string layerName, ESAINLayer eSainLayer) : base(botOwner, priority)
    {
        LayerName = layerName;
        ELayer = eSainLayer;

        botOwner.Brain.BaseBrain.OnLayerChangedTo += OnLayerChanged;
    }

    private void OnLayerChanged(AICoreLayer<BotLogicDecision> layer)
    {
        var newLayerName = layer.Name();

        var mover = BotOwner.Mover;

        if (newLayerName == LayerName)
        {
            // If we activated this (SAIN) layer, wipe the builtin bot mover
            mover.Stop();
        }
        else if (_currentLayerName == LayerName)
        {
            // If we switched away from this layer to a different one, set the player to the navmesh to ensure it has a consistent state
            var playerPosition = BotOwner.GetPlayer.Position;
            mover._lastGoodCastPoint = mover._prevSuccessLinkedFrom = mover._prevLinkPos = mover.PositionOnWayInner = playerPosition;
            mover._lastGoodCastPointTime = Time.time;
            // Prevents the mover from re-issuing a move command to it's last target in SetPlayerToNavMesh
            mover._prevPosLinkedTime = 0f;
            // Final insurance that the bot is set to the navmesh before we hand over the brain
            mover.SetPlayerToNavMesh(playerPosition);
        }

        _currentLayerName = newLayerName;
    }

    private void SetLayer(bool active)
    {
        if (Bot != null)
        {
            if (active)
            {
                Bot.ActiveLayer = ELayer;
            }
            else if (Bot.ActiveLayer == ELayer)
            {
                Bot.ActiveLayer = ESAINLayer.None;
            }
        }
    }

    public override string GetName()
    {
        return LayerName;
    }

    public static BotManagerComponent BotController
    {
        get { return BotManagerComponent.Instance; }
    }

    public BotComponent Bot { get; private set; }

    public override void BuildDebugText(StringBuilder stringBuilder)
    {
        if (Bot != null)
        {
            DebugOverlay.AddBaseInfo(Bot, BotOwner, stringBuilder);
        }
    }
}
