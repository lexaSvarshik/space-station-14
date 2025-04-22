// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Shared.Mobs.Systems;
using Robust.Server.GameObjects;
using Content.Shared.SS220.CultYogg.MiGo;
using Content.Shared.Humanoid;

namespace Content.Server.SS220.CultYogg.Nyarlathotep;

/// <summary>
/// Searches for entities within a given radius to further pursue them
/// </summary>
public sealed class NyarlathotepTargetSearcherSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookupSystem = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NyarlathotepSearchTargetsComponent, MapInitEvent>(OnSearchMapInit);
    }

    /// <summary>
    /// Adds a component to pursue targets
    /// Performs a duplicate component check, on the MiGi component to not harass cult members
    /// and cuts off entities that are not alive
    /// </summary>
    private void SearchNearNyarlathotep(EntityUid user, float range)
    {
        foreach (var target in _entityLookupSystem.GetEntitiesInRange<HumanoidAppearanceComponent>(_transform.GetMapCoordinates(user), range))
        {
            if (HasComp<MiGoComponent>(target.Owner))
                continue;

            if (HasComp<NyarlathotepTargetComponent>(target.Owner))
                continue;

            if (_mobStateSystem.IsAlive(target.Owner))
                AddComp(target.Owner, new NyarlathotepTargetComponent());
        }
    }
    private void OnSearchMapInit(Entity<NyarlathotepSearchTargetsComponent> component, ref MapInitEvent args)
    {
        component.Comp.NextSearchTime = _gameTiming.CurTime + TimeSpan.FromSeconds(component.Comp.SearchMaxInterval);
    }

    /// <summary>
    /// Updates the target seeker's cooldowns.
    /// Periodically checks for new targets in the radius.
    /// </summary>
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<NyarlathotepSearchTargetsComponent>();
        while (query.MoveNext(out var uid, out var targetSearcher))
        {
            if (targetSearcher.NextSearchTime > _gameTiming.CurTime)
                continue;

            SearchNearNyarlathotep(uid, targetSearcher.SearchRange);
            var delay = TimeSpan.FromSeconds(_random.NextFloat(targetSearcher.SearchMinInterval, targetSearcher.SearchMaxInterval));
            targetSearcher.NextSearchTime += delay;
        }
    }
}

/// <summary>
/// Component for entities to be attacked by Nyarlathotep.
/// </summary>
[RegisterComponent]
public sealed partial class NyarlathotepTargetComponent : Component;
