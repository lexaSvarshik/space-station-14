// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using System.Linq;
using System.Numerics;
using Content.Client.SS220.SuperMatter.Observer;
using Content.Client.SS220.UserInterface.PlotFigure;
using Content.Shared.SS220.SuperMatter.Functions;
using Content.Shared.SS220.SuperMatter.Ui;
using Robust.Client.AutoGenerated;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Utility;

namespace Content.Client.SS220.Cartridges;

[GenerateTypedNameReferences]
public sealed partial class SupermatterObserverUiFragment : BoxContainer
{
    [Dependency] private readonly ILocalizationManager _localization = default!;

    public event Action<BaseButton.ButtonEventArgs, SuperMatterObserverComponent>? OnServerButtonPressed;
    public event Action<BaseButton.ButtonEventArgs, int>? OnCrystalButtonPressed;
    public event Action<BaseButton.ButtonEventArgs>? OnRefreshButton;

    public SuperMatterObserverComponent? Observer;
    public int? CrystalKey;

    public const int MAX_DATA_LENGTH = 120;

    public SupermatterObserverUiFragment()
    {
        IoCManager.InjectDependencies(this);
        RobustXamlLoader.Load(this);

        RefreshButton.IconTexture = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/refresh.svg.192dpi.png"));
        RefreshButton.IconScale = new Vector2(0.3f); //convert 192px to 32px... but weird
        RefreshButton.OnPressed += args =>
            OnRefreshButton?.Invoke(args);

        PlotValueOverTime.SetLabels(_localization.GetString("smObserver-plotXLabel-integrity"), _localization.GetString("smObserver-plotYLabel-integrity"), _localization.GetString("smObserver-plotTitle-integrity"));

        ColorState.EvalFunctionOnMeshgrid(GetIntegrityDamageMap);
        ColorState.SetLabels(_localization.GetString("smObserver-plotXLabel-colorState"), _localization.GetString("smObserver-plotYLabel-colorState"), _localization.GetString("smObserver-plotTitle-colorState"));
    }

    private float GetIntegrityDamageMap(float matter, float internalEnergy)
    {
        return SuperMatterFunctions.EnergyToMatterDamageFactorFunction(internalEnergy
                - SuperMatterFunctions.SafeInternalEnergyToMatterFunction(matter / SuperMatterFunctions.MatterNondimensionalization),
            matter / SuperMatterFunctions.MatterNondimensionalization);
    }
    public void LoadCrystal()
    {
        CrystalNavigationBar.RemoveAllChildren();
        if (Observer == null)
            return;
        foreach (var (crystalKey, name) in Observer.Names)
        {
            var crystalButton = new CrystalButton
            {
                Text = name,
                StyleBoxOverride = new StyleBoxFlat(Color.DarkGray),
                CrystalKey = crystalKey,
                ToggleMode = true,
                Margin = new Thickness(2, 0, 2, 0),
                StyleClasses = { "OpenBoth" }
            };

            crystalButton.OnPressed += args =>
            {
                OnCrystalButtonPressed?.Invoke(args, crystalButton.CrystalKey);
            };
            CrystalNavigationBar.AddChild(crystalButton);
        }
    }
    public void LoadCachedData()
    {
        if (Observer == null
            || CrystalKey == null)
            return;
        PlotValueOverTime.LoadPlot2DTimePoints(new PlotPoints2D(MAX_DATA_LENGTH, Observer.Integrities[CrystalKey.Value],
                                                        -1f, Observer.Integrities[CrystalKey.Value].Count));
        ColorState.LoadMovingPoint(new Vector2(Observer.Matters[CrystalKey.Value].Last().Value, Observer.InternalEnergy[CrystalKey.Value].Last().Value),
                                     new Vector2(Observer.Matters[CrystalKey.Value].Last().Derv, Observer.InternalEnergy[CrystalKey.Value].Last().Derv));
    }
    public void UpdateState(SuperMatterObserverUpdateState msg)
    {
        if (Disposed)
            return;

        if (Observer == null
            || CrystalKey == null)
            return;
        if (msg.Id != CrystalKey)
            return;

        PlotValueOverTime.AddPointToPlot(new Vector2(PlotValueOverTime.GetLastAddedPointX() + 1f, msg.Integrity));
        ColorState.LoadMovingPoint(new Vector2(msg.Matter.Value, msg.InternalEnergy.Value), new Vector2(msg.Matter.Derivative, msg.InternalEnergy.Derivative));
    }
    public void LoadState(List<Entity<SuperMatterObserverComponent>> observerEntities)
    {
        if (Disposed)
            return;

        ServerNavigationBar.RemoveAllChildren();
        foreach (var (observerUid, observerComp) in observerEntities)
        {
            var serverButton = new ServerButton
            {
                Text = observerUid.ToString(),
                StyleBoxOverride = new StyleBoxFlat(Color.DarkGray),
                ObserverComponent = observerComp,
                Margin = new Thickness(2, 0, 2, 0),
                ToggleMode = true,
                StyleClasses = { "OpenBoth" }
            };

            serverButton.OnPressed += args =>
            {
                OnServerButtonPressed?.Invoke(args, serverButton.ObserverComponent);
            };
            ServerNavigationBar.AddChild(serverButton);
        }
    }
    private sealed class ServerButton : Button
    {
        public SuperMatterObserverComponent? ObserverComponent;
    }
    private sealed class CrystalButton : Button
    {
        public int CrystalKey;
    }
}