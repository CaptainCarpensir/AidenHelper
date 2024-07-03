using System;
using Monocle;
using Celeste.Mod.AidenHelper.Effects;
using Celeste.Mod.AidenHelper.Triggers;

namespace Celeste.Mod.AidenHelper.Module
{
    public class AidenHelper : EverestModule
    {
        public override Type SessionType => typeof(AidenHelperSession);
        public AidenHelperSession Session => (AidenHelperSession)_Session;

        public static AidenHelper Instance;

        public AidenHelper()
        {
            Instance = this;
        }

        public override void Load()
        {
            Everest.Events.Level.OnLoadBackdrop += onLoadBackdrop;

            InvertStaminaOnDashTrigger.Load();
            RunAndGunTrigger.Load();
        }

        public override void Initialize()
        {
        }

        public override void LoadContent(bool firstLoad)
        {
        }

        public override void Unload()
        {
            Everest.Events.Level.OnLoadBackdrop -= onLoadBackdrop;

            InvertStaminaOnDashTrigger.Unload();
            RunAndGunTrigger.Unload();
        }

        private Backdrop onLoadBackdrop(MapData map, BinaryPacker.Element child, BinaryPacker.Element above)
        {
            if (child.Name.Equals("AidenHelper/MeteorShower", StringComparison.OrdinalIgnoreCase))
            {
                return new MeteorShower(child.AttrFloat("frequency"), Calc.HexToColor(child.Attr("color", "FFFFFF")));
            }
            return null;
        }

    }
}
