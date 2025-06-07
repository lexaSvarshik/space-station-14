// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt

using Robust.Shared.Containers;
using Robust.Shared.Serialization;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Content.Shared.SS220.CultYogg.MiGo;

namespace Content.Shared.SS220.CultYogg.Pod;

[RegisterComponent, NetworkedComponent]
public sealed partial class CultYoggPodComponent : Component
{
    /// <summary>
    /// Time between each healing incident
    /// </summary>
    [DataField(required: true)]
    public TimeSpan HealingFreq = TimeSpan.FromSeconds(1);

    [DataField]
    public TimeSpan InsertDelay = TimeSpan.FromSeconds(6);

    /// <summary>
    /// Whitelist of entities that are cultists
    /// </summary>
    [DataField]
    public EntityWhitelist? CultistsWhitelist = new()
    {
        Components =
        [
            "CultYogg",
            "MiGo"
        ]
    };

    [DataField(required: true)]
    public DamageSpecifier Heal = new();

    [DataField(required: true)]
    public float BloodlossModifier = -4;

    /// <summary>
    /// Restore missing blood.
    /// </summary>
    [DataField]
    public float ModifyBloodLevel = 2;

    [DataField]
    public float ModifyStamina = -5;

    public ContainerSlot MobContainer = default!;

    [Serializable, NetSerializable]
    public enum CultPodVisuals : byte
    {
        Inserted,
    }
}
