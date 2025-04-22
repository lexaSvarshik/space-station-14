// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt

using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.SS220.CultYogg.Pond;

[RegisterComponent, Access(typeof(CultPondSystem))]
public sealed partial class CultPondComponent : Component
{
    [ViewVariables]
    public bool IsFilled = false;

    [DataField("solutionName", required: true), ViewVariables(VVAccess.ReadWrite)]
    public string Solution;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("ammountToAdd")]
    public FixedPoint2 AmmountToAdd = FixedPoint2.New(10);

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("refillCooldown")]
    [AutoNetworkedField]
    public float RefillCooldown = 5f;

    /// <summary>
    /// For the future
    /// </summary>
    [DataField("rechargeSound")]
    [AutoNetworkedField]
    public SoundSpecifier? RechargeSound;

    [ViewVariables(VVAccess.ReadWrite),
    DataField("nextCharge", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan? NextCharge;

    [ViewVariables(VVAccess.ReadWrite), DataField("reagent")]
    public ReagentQuantity? Reagent;
}
