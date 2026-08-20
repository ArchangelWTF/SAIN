using SAIN.Components;
using SAIN.Preset.Shared.Enums;

namespace SAIN.SAINComponent.Classes.Decision;

public class SAINDecisionClass : BotComponentClassBase
{
    public bool HasDecision
    {
        get { return DecisionManager.HasDecision; }
    }

    public ECombatDecision CurrentCombatDecision
    {
        get { return DecisionManager.CurrentCombatDecision; }
    }

    public ECombatDecision PreviousCombatDecision
    {
        get { return DecisionManager.PreviousCombatDecision; }
    }

    public ESquadDecision CurrentSquadDecision
    {
        get { return DecisionManager.CurrentSquadDecision; }
    }

    public ESquadDecision PreviousSquadDecision
    {
        get { return DecisionManager.PreviousSquadDecision; }
    }

    public ESelfActionType CurrentSelfDecision
    {
        get { return DecisionManager.CurrentSelfDecision; }
    }

    public ESelfActionType PreviousSelfDecision
    {
        get { return DecisionManager.PreviousSelfDecision; }
    }

    public float ChangeDecisionTime
    {
        get { return DecisionManager.ChangeDecisionTime; }
    }

    public float TimeSinceChangeDecision
    {
        get { return DecisionManager.TimeSinceChangeDecision; }
    }

    public bool RunningToCover
    {
        get
        {
            switch (CurrentCombatDecision)
            {
                case ECombatDecision.Retreat:
                case ECombatDecision.RunAway:
                    return true;
                case ECombatDecision.SeekCover:
                    return Bot.Cover.CoverInUse == null && Bot.Cover.SprintingToCover;

                default:
                    return false;
            }
        }
    }

    public bool IsSearching
    {
        get
        {
            return CurrentCombatDecision == ECombatDecision.Search
                || CurrentSquadDecision == ESquadDecision.Search
                || CurrentSquadDecision == ESquadDecision.GroupSearch;
        }
    }

    public BotDecisionManager DecisionManager { get; }
    public DogFightDecisionClass DogFightDecision { get; }
    public SelfActionDecisionClass SelfActionDecisions { get; }
    public EnemyDecisionClass EnemyDecisions { get; }
    public SquadDecisionClass SquadDecisions { get; }

    public SAINDecisionClass(BotComponent bot)
        : base(bot)
    {
        TickRequirement = ESAINTickState.OnlyBotActive;
        DecisionManager = new BotDecisionManager(this);
        SelfActionDecisions = new SelfActionDecisionClass(bot);
        EnemyDecisions = new EnemyDecisionClass(bot);
        SquadDecisions = new SquadDecisionClass(bot);
        DogFightDecision = new DogFightDecisionClass(bot);
    }

    public override void Init()
    {
        DecisionManager.Init();
        SelfActionDecisions.Init();
        EnemyDecisions.Init();
        SquadDecisions.Init();
        DogFightDecision.Init();
        base.Init();
    }

    public override void ManualUpdate()
    {
        DecisionManager.ManualUpdate();
        SelfActionDecisions.ManualUpdate();
        EnemyDecisions.ManualUpdate();
        SquadDecisions.ManualUpdate();
        DogFightDecision.ManualUpdate();
        base.ManualUpdate();
    }

    public override void Dispose()
    {
        DecisionManager.Dispose();
        SelfActionDecisions.Dispose();
        EnemyDecisions.Dispose();
        SquadDecisions.Dispose();
        DogFightDecision?.Dispose();
        base.Dispose();
    }

    public void ResetDecisions(bool active)
    {
        DecisionManager.ResetDecisions(active);
    }
}
