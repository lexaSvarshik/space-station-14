// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Client.SS220.CultYogg.MiGo.UI;
using Content.Shared.SS220.CultYogg.Buildings;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Placement;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Client.SS220.CultYogg.MiGo
{
    public sealed class MiGoErectPlacementHijack : PlacementHijack
    {
        private readonly IEntityManager _entityManager;
        private readonly IPrototypeManager _prototypeManager;
        private readonly IComponentFactory _componentFactory;

        private readonly MiGoErectBoundUserInterface _presenter;
        private readonly CultYoggBuildingPrototype? _prototype;

        public override bool CanRotate { get; }

        public MiGoErectPlacementHijack(MiGoErectBoundUserInterface presenter, CultYoggBuildingPrototype? prototype)
        {
            _entityManager = IoCManager.Resolve<IEntityManager>();
            _prototypeManager = IoCManager.Resolve<IPrototypeManager>();
            _componentFactory = IoCManager.Resolve<IComponentFactory>();

            _presenter = presenter;
            _prototype = prototype;
            CanRotate = true;
        }

        /// <inheritdoc />
        public override bool HijackPlacementRequest(EntityCoordinates coordinates)
        {
            if (_prototype != null)
            {
                var dir = Manager.Direction;
                _presenter.SendBuildMessage(_prototype, coordinates, dir);
            }
            return true;
        }

        /// <inheritdoc />
        public override bool HijackDeletion(EntityUid entity)
        {
            _presenter.SendEraseMessage(entity);
            return true;
        }

        /// <inheritdoc />
        public override void StartHijack(PlacementManager manager)
        {
            base.StartHijack(manager);
            if (_prototype is null)
                return;
            var entityProto = _prototypeManager.Index(_prototype.ResultProtoId);
            if (!entityProto.TryGetComponent<SpriteComponent>(out var sprite, _componentFactory))
                return;
            if (sprite?.BaseRSI is null)
                return;
            var textures = new List<IDirectionalTextureProvider>();
            foreach (var layer in sprite.AllLayers)
            {
                if (layer?.ActualRsi is null || !layer.ActualRsi.TryGetState(layer.RsiState, out var state))
                    continue;
                textures.Add(state);
            }
            manager.CurrentTextures = textures;
        }
    }
}
