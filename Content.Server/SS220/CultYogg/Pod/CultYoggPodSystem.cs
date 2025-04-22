// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt

using Content.Shared.DragDrop;
using Content.Shared.SS220.CultYogg.Pod;
using Content.Shared.SS220.CultYogg.MiGo;
using Robust.Shared.Containers;
using Robust.Server.GameObjects;

namespace Content.Server.SS220.CultYogg.Pod;

public sealed partial class CultYoggPodSystem : SharedCultYoggPodSystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CultYoggPodComponent, EntInsertedIntoContainerMessage>(GotInserted);
        SubscribeLocalEvent<CultYoggPodComponent, EntRemovedFromContainerMessage>(GotRemoved);
        SubscribeLocalEvent<CultYoggPodComponent, EntityTerminatingEvent>(GotTerminated);
        SubscribeLocalEvent<CultYoggPodComponent, DragDropTargetEvent>(OnCanDropHandle);
    }

    private void OnCanDropHandle(Entity<CultYoggPodComponent> ent, ref DragDropTargetEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = TryInsert(args.User, args.Dragged, ent);
    }

    private void GotTerminated(Entity<CultYoggPodComponent> ent, ref EntityTerminatingEvent args)
    {
        if (ent.Comp.MobContainer.ContainedEntity is null)
            return;

        RemComp<CultYoggHealComponent>(ent.Comp.MobContainer.ContainedEntity.Value);
    }

    private void GotRemoved(Entity<CultYoggPodComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        RemComp<CultYoggHealComponent>(args.Entity);
        _appearance.SetData(ent, CultYoggPodComponent.CultPodVisuals.Inserted, false);
    }

    private void GotInserted(Entity<CultYoggPodComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        var healComp = EnsureComp<CultYoggHealComponent>(args.Entity); //applying heal from MiGo
        healComp.Heal = ent.Comp.Heal;
        healComp.TimeBetweenIncidents = ent.Comp.HealingFreq;
        healComp.BloodlossModifier = ent.Comp.BloodlossModifier;
        healComp.ModifyBloodLevel = ent.Comp.ModifyBloodLevel;
        Dirty(args.Entity, healComp);

        _appearance.SetData(ent, CultYoggPodComponent.CultPodVisuals.Inserted, true);
    }
}
