using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Implants.Components;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.SS220.PenScrambler;

namespace Content.Server.SS220.PenScrambler;

public sealed class PenScramblerSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PenScramblerComponent, AfterInteractEvent>(OnInteract);
        SubscribeLocalEvent<PenScramblerComponent, CopyDnaToPenEvent>(OnCopyIdentity);
    }

    private void OnInteract(Entity<PenScramblerComponent> ent, ref AfterInteractEvent args)
    {
        if (!args.CanReach || args.Target == null)
            return;

        if (HasComp<HumanoidAppearanceComponent>(args.Target) && !ent.Comp.HaveDna)
        {
            _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager,
                args.User,
                ent.Comp.DelayForExtractDna,
                new CopyDnaToPenEvent(),
                ent.Owner,
                args.Target)
            {
                Hidden = true,
                BreakOnMove = true,
                BreakOnDamage = true,
                BreakOnHandChange = true,
                BreakOnDropItem = true,
                DuplicateCondition = DuplicateConditions.None,
            });
        }

        if (!TryComp<ImplanterComponent>(args.Target, out var implanterComponent))
            return;

        var implantEntity = implanterComponent.ImplanterSlot.ContainerSlot?.ContainedEntity;

        if (HasComp<TransferIdentityComponent>(implantEntity) && ent.Comp.HaveDna)
        {
            _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager,
                args.User,
                ent.Comp.DelayForTransferToImplant,
                new CopyDnaFromPenToImplantEvent(),
                implantEntity,
                ent.Owner)
            {
                BreakOnMove = true,
                BreakOnDamage = true,
                BreakOnHandChange = true,
                BreakOnDropItem = true,
                DuplicateCondition = DuplicateConditions.None,
            });
        }
    }

    private void OnCopyIdentity(Entity<PenScramblerComponent> ent, ref CopyDnaToPenEvent args)
    {
        if (args.Cancelled)
            return;

        if (!TryComp<HumanoidAppearanceComponent>(args.Target, out var humanoidAppearanceComponent))
            return;

        ent.Comp.AppearanceComponent = humanoidAppearanceComponent;
        ent.Comp.Target = args.Target;
        ent.Comp.HaveDna = true;

        _popup.PopupEntity(Loc.GetString("pen-scrambler-success-copy", ("identity", MetaData(args.Target.Value).EntityName)), args.User, args.User);
    }
}
