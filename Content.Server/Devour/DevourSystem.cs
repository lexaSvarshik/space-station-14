using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Devour;
using Content.Shared.Devour.Components;
using Content.Shared.Humanoid;
using Content.Server.Body.Components;
using Robust.Server.Containers;
using Content.Shared.Mobs.Components; // SS220 dragon devour fix

namespace Content.Server.Devour;

public sealed class DevourSystem : SharedDevourSystem
{
    [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DevourerComponent, DevourDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<DevourerComponent, BeingGibbedEvent>(OnGibContents);
    }

    private void OnDoAfter(EntityUid uid, DevourerComponent component, DevourDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        var ichorInjection = new Solution(component.Chemical, component.HealRate);

        if (component.FoodPreference == FoodPreference.All ||
            (component.FoodPreference == FoodPreference.Humanoid && HasComp<HumanoidAppearanceComponent>(args.Args.Target)))
        {
            if (component.ShouldStoreDevoured && args.Args.Target is not null)
            {
                ContainerSystem.Insert(args.Args.Target.Value, component.Stomach);
            }
            _bloodstreamSystem.TryAddToChemicals(uid, ichorInjection);
        }

        //TODO: Figure out a better way of removing structures via devour that still entails standing still and waiting for a DoAfter. Somehow.
        //If it's not human, it must be a structure
        else if (args.Args.Target != null)
        {
            QueueDel(args.Args.Target.Value);
        }

        // SS220 dragon devour fix begin
        //_audioSystem.PlayPvs(component.SoundDevour, uid);
        if (HasComp<MobStateComponent>(args.Args.Target))
            _audioSystem.PlayPvs(component.SoundDevourMob, uid);
        else
            _audioSystem.PlayPvs(component.SoundDevour, uid);
        // SS220 dragon devour fix end
    }

    private void OnGibContents(EntityUid uid, DevourerComponent component, ref BeingGibbedEvent args)
    {
        if (!component.ShouldStoreDevoured)
            return;

        // For some reason we have two different systems that should handle gibbing,
        // and for some another reason GibbingSystem, which should empty all containers, doesn't get involved in this process
        ContainerSystem.EmptyContainer(component.Stomach);
    }
}

