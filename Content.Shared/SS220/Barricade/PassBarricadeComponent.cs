// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Robust.Shared.GameStates;

namespace Content.Shared.SS220.Barricade;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedBarricadeSystem))]
public sealed partial class PassBarricadeComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public Dictionary<EntityUid, bool> CollideBarricades = new();
}
