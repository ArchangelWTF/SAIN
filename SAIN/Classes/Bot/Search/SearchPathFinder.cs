using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Classes.Bot.Search;

public class SearchPathFinder : BotSubClass<SearchClass>
{
    public EnemyPlace TargetPlace { get; private set; }
    public bool SearchedTargetPosition
    {
        get { return TargetPlace == null || TargetPlace.HasArrivedPersonal || TargetPlace.HasArrivedSquad; }
    }

    public bool FinishedPeeking { get; set; }

    public SearchPathFinder(SearchClass searchClass)
        : base(searchClass) { }

    public bool HasPathToSearchTarget(Enemy enemy, out string failReason)
    {
        return CheckEnemyPath(enemy, out failReason);
    }

    public void UpdateSearchDestination(Enemy enemy)
    {
        if (!SearchedTargetPosition)
        {
            checkFinishedSearch(enemy);
        }

        if (_nextCheckPosTime < Time.time || SearchedTargetPosition || TargetPlace == null)
        {
            _nextCheckPosTime = Time.time + 4f;
        }
    }

    private void checkFinishedSearch(Enemy enemy)
    {
        if (SearchedTargetPosition)
        {
            return;
        }
        var lastKnown = enemy.KnownPlaces.LastKnownPlace;
        if (lastKnown == null)
        {
            Reset();
            return;
        }
        if (lastKnown.HasArrivedPersonal || lastKnown.HasArrivedSquad)
        {
            Reset();
            return;
        }
        EnemyPlace targetPlace = TargetPlace;
        if (targetPlace == null || targetPlace != lastKnown)
        {
            Reset();
            return;
        }

        if (targetPlace.DistanceToBot <= ARRIVE_DISTANCE && enemy.Path.PathCornerCount <= 2)
        {
            enemy.KnownPlaces.SetPlaceAsSearched(targetPlace);
            Reset();
            return;
        }

        if (ReachedEndOfPartialPath(enemy))
        {
            enemy.KnownPlaces.SetPlaceAsSearched(targetPlace);
            Reset();
        }
    }

    private bool ReachedEndOfPartialPath(Enemy enemy)
    {
        if (enemy.Path.PathToEnemyStatus != NavMeshPathStatus.PathPartial)
        {
            return false;
        }
        int cornerCount = enemy.Path.PathCornerCount;
        if (cornerCount < 1)
        {
            return false;
        }
        Vector3 pathEnd = enemy.Path.PathCorners[cornerCount - 1];
        return (pathEnd - Bot.Position).sqrMagnitude <= ARRIVE_DISTANCE * ARRIVE_DISTANCE;
    }

    private const float ARRIVE_DISTANCE = 2f;

    public void Reset()
    {
        TargetPlace = null;
        FinishedPeeking = false;
    }

    public bool CheckEnemyPath(Enemy enemy, out string failReason)
    {
        EnemyPlace lastKnownPlace = enemy.KnownPlaces.LastKnownPlace;
        if (lastKnownPlace == null)
        {
            failReason = "lastKnown null";
            return false;
        }
        NavMeshPath path = enemy.Path.PathToEnemy;
        if (path == null || path.status == NavMeshPathStatus.PathInvalid)
        {
            failReason = "path Invalid";
            return false;
        }
        Vector3[] corners = enemy.Path.PathCorners;
        int length = enemy.Path.PathCornerCount;
        if (length < 2)
        {
            failReason = "path Invalid corner length";
            return false;
        }
        Vector3 destination = corners[length - 1];
        if ((destination - Bot.Position).sqrMagnitude <= 0.33f)
        {
            failReason = "tooClose";
            return false;
        }

        if (
            (destination - lastKnownPlace.Position).sqrMagnitude > 0.5f
            && Physics.SphereCast(destination + Vector3.up, 0.1f, lastKnownPlace.Position - destination, out _, 1f)
        )
        {
            failReason = "path not complete";
            return false;
        }

        BaseClass.Reset();
        TargetPlace = lastKnownPlace;
        failReason = string.Empty;
        return true;
    }

    private float _nextCheckPosTime;
}
