// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Shared.Access;
using Content.Shared.Damage;
using Content.Shared.Doors.Components;
using Robust.Shared.Prototypes;
using Content.Shared.Emag.Systems;

namespace Content.Server.SS220.CultYogg.BurglarBug;

[RegisterComponent, Access(typeof(BurglarBugServerSystem))]
public sealed partial class BurglarBugComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float DamageRange = 3f;

    [DataField("timeToOpen", required: true), ViewVariables(VVAccess.ReadWrite)]
    public float TimeToOpen;

    /// <summary>
    /// What type of emag effect this device will do
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public EmagType EmagType = EmagType.Access;

    /// <summary>
    ///     Popup message shown when player stuck entity, but forgot to activate it.
    /// </summary>
    [DataField("notActivatedStickPopupCancellation")]
    [ViewVariables(VVAccess.ReadWrite)]
    public string? NotActivatedStickPopupCancellation;

    /// <summary>
    ///     Popup message shown when player stuck entity tryed on opened door.
    ///     If you want to check on stuck to opened door set this.
    ///     By default this logic is off.
    /// </summary>
    [DataField("openedDoorStickPopupCancellation")]
    [ViewVariables(VVAccess.ReadWrite)]
    public string? OpenedDoorStickPopupCancellation;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool Activated;

    [DataField("ignoreResistances")] public bool IgnoreResistances = false;

    [DataField("damage", required: true)]
    public DamageSpecifier Damage = default!;

    [ViewVariables(VVAccess.ReadWrite)]
    public Entity<DoorComponent>? Door;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan? DoorOpenTime;
}
