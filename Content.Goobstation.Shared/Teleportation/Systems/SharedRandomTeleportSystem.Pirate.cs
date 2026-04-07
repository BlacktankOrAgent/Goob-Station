using System.Linq;
using System.Numerics;
using Content.Goobstation.Common.MartialArts;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Physics;
using Content.Shared.Station;
using Content.Shared.Station.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;

namespace Content.Goobstation.Shared.Teleportation.Systems;

public partial class SharedRandomTeleportSystem
{
    [Dependency] private readonly SharedStationSystem _stationSystem = default!;

    /// <summary>
    /// Teleports an entity to a random location on the station.
    /// </summary>
    public Vector2? RandomTeleportToStation(EntityUid uid, int triesBase = 50, bool teleportPulledEntities = false)
    {
        if (!CanTeleport(uid))
            return null;

        if (!EntityManager.EntityExists(uid))
        {
            return null;
        }

        if (!TryComp<TransformComponent>(uid, out var xform))
        {
            return null;
        }

        var entityCoords = _xform.ToMapCoordinates(xform.Coordinates);
        var mapId = entityCoords.MapId;

        // Get the station on this map
        var stationUid = _stationSystem.GetStationInMap(mapId);
        if (stationUid == null)
        {
            return null;
        }

        // Get all grids owned by the station
        if (!TryComp<StationDataComponent>(stationUid.Value, out var stationData))
        {
            return null;
        }

        var stationGrids = stationData.Grids
            .Where(gridUid => TryComp<MapGridComponent>(gridUid, out _))
            .Select(gridUid => new Entity<MapGridComponent>(gridUid, CompOrNull<MapGridComponent>(gridUid)!))
            .ToList();

        if (stationGrids.Count == 0)
        {
            return null;
        }


        // Pick a random station grid
        var stationGrid = stationGrids[_random.Next(stationGrids.Count)];

        var targetCoords = new MapCoordinates();
        var foundValid = false;
        EntityUid? pullableEntity = null;
        var stage = GrabStage.No;

        if (TryComp<PullerComponent>(uid, out var puller))
        {
            stage = puller.GrabStage;
            pullableEntity = puller.Pulling;
        }

        for (var i = 0; i < triesBase; i++)
        {
            var bounds = stationGrid.Comp.LocalAABB;
            var x = _random.NextFloat(bounds.Left, bounds.Right);
            var y = _random.NextFloat(bounds.Bottom, bounds.Top);
            var worldPos = _map.LocalToWorld(stationGrid.Owner, stationGrid.Comp, new Vector2(x, y));
            targetCoords = new MapCoordinates(worldPos, mapId);

            // Check if we picked a position inside a solid object
            var valid = true;
            foreach (var entity in _map.GetAnchoredEntities(stationGrid, targetCoords))
            {
                if (!_physicsQuery.TryGetComponent(entity, out var body))
                    continue;

                if (body.BodyType != BodyType.Static || !body.Hard ||
                    (body.CollisionLayer & (int) CollisionGroup.Impassable) == 0)
                    continue;

                valid = false;
                break;
            }

            if (valid)
            {
                foundValid = true;
                break;
            }
        }

        if (!foundValid)
        {
            var bounds = stationGrid.Comp.LocalAABB;
            var x = _random.NextFloat(bounds.Left, bounds.Right);
            var y = _random.NextFloat(bounds.Bottom, bounds.Top);
            var worldPos = _map.LocalToWorld(stationGrid.Owner, stationGrid.Comp, new Vector2(x, y));
            targetCoords = new MapCoordinates(worldPos, mapId);
        }

        _pullingSystem.StopAllPulls(uid);

        var newPos = targetCoords.Position;
        _xform.SetWorldPosition(uid, newPos);

        if (pullableEntity != null && teleportPulledEntities)
        {
            _xform.SetWorldPosition(pullableEntity.Value, newPos);
            _pullingSystem.TryStartPull(uid, pullableEntity.Value, grabStageOverride: stage, force: true);
        }

        return newPos;
    }
}
