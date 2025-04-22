// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Client.Light.Components;
using Content.Client.Light.EntitySystems;
using Content.Shared.SS220.Bible;
using Robust.Client.GameObjects;

namespace Content.Client.SS220.Bible;

public sealed class ExorcismPerformerSystem : SharedExorcismPerformerSystem
{

    [Dependency] private readonly LightBehaviorSystem _lightBehavior = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<ExorcismPerformerComponent, AppearanceChangeEvent>(OnAppearanceChanged);
    }

    private void OnAppearanceChanged(Entity<ExorcismPerformerComponent> entity, ref AppearanceChangeEvent args)
    {
        if (!args.AppearanceData.TryGetValue(ExorcismPerformerVisualState.State, out var value) || value is not ExorcismPerformerVisualState state)
        {
            return;
        }
        //ToDo its broken ... Stalen?
        /*
        if (TryComp(entity, out LightBehaviourComponent? lightBehaviour))
        {
            // Reset any running behaviour to reset the animated properties back to the original value, to avoid conflicts between resets
            _lightBehavior.StopLightBehaviour((entity, lightBehaviour));

            if (state == ExorcismPerformerVisualState.Performing)
            {
                _lightBehavior.StartLightBehaviour(entity.Comp.LightBehaviourId);
            }
        }
        */
    }
}
