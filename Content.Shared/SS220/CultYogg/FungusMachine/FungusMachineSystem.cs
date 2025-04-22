// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Shared.Popups;
using Content.Shared.UserInterface;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.SS220.CultYogg.FungusMachine;

public abstract class SharedFungusMachineSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FungusMachineComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<FungusMachineComponent, ActivatableUIOpenAttemptEvent>(OnAttemptOpenUI);
    }

    protected virtual void OnComponentInit(EntityUid uid, FungusMachineComponent component, ComponentInit args)
    {
        RestockInventoryFromPrototype(uid, component);
    }

    private void OnAttemptOpenUI(Entity<FungusMachineComponent> entity, ref ActivatableUIOpenAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        var (uid, comp) = entity;
        var user = args.User;

        if (_entityWhitelist.IsWhitelistFail(comp.UsersWhitelist, user))
        {
            _popupSystem.PopupPredicted(Loc.GetString("cult-yogg-fungus-denied-to-use"), uid, user);
            args.Cancel();
            return;
        }
    }

    public void RestockInventoryFromPrototype(EntityUid uid, FungusMachineComponent? component = null)
    {
        if (!Resolve(uid, ref component))
        {
            return;
        }

        if (!_prototypeManager.TryIndex(component.PackPrototypeId, out FungusMachineInventoryPrototype? packPrototype))
            return;

        AddInventoryFromPrototype(uid, packPrototype.StartingInventory, component);
    }

    public List<FungusMachineInventoryEntry> GetInventory(EntityUid uid, FungusMachineComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return new();
        var inventory = new List<FungusMachineInventoryEntry>(component.Inventory.Values);
        return inventory;
    }

    private void AddInventoryFromPrototype(EntityUid uid, Dictionary<string, uint>? entries, FungusMachineComponent? component = null)
    {
        if (!Resolve(uid, ref component) || entries == null)
        {
            return;
        }

        var inventory = component.Inventory;

        foreach (var (id, amount) in entries)
        {
            if (!_prototypeManager.HasIndex<EntityPrototype>(id))
                continue;

            var restock = amount;
            inventory.Add(id, new FungusMachineInventoryEntry(id, restock));
        }
    }
}


[NetSerializable, Serializable]
public sealed class FungusMachineInterfaceState(List<FungusMachineInventoryEntry> inventory) : BoundUserInterfaceState
{
    public List<FungusMachineInventoryEntry> Inventory = inventory;
}

[Serializable, NetSerializable]
public sealed class FungusSelectedId(string id) : BoundUserInterfaceMessage
{
    public readonly string Id = id;
}

[Serializable, NetSerializable]
public enum FungusMachineUiKey
{
    Key,
}
