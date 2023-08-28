
using Monocle;
using Celeste;
using System;
using Celeste.Mod;
using AidenHelper.Effects;

namespace AidenHelper.Module
{
    public class AidenHelper : EverestModule
    {

        // Only one alive module instance can exist at any given time.
        public static AidenHelper Instance;

        public AidenHelper()
        {
            Instance = this;
        }

        // Set up any hooks, event handlers and your mod in general here.
        // Load runs before Celeste itself has initialized properly.
        public override void Load()
        {
            Everest.Events.Level.OnLoadBackdrop += onLoadBackdrop;
        }

        // Optional, initialize anything after Celeste has initialized itself properly.
        public override void Initialize()
        {
        }

        // Optional, do anything requiring either the Celeste or mod content here.
        public override void LoadContent(bool firstLoad)
        {
        }

        // Unload the entirety of your mod's content. Free up any native resources.
        public override void Unload()
        {
            Everest.Events.Level.OnLoadBackdrop -= onLoadBackdrop;
        }

        private Backdrop onLoadBackdrop(MapData map, BinaryPacker.Element child, BinaryPacker.Element above)
        {
            if (child.Name.Equals("AidenHelper/MeteorShower", StringComparison.OrdinalIgnoreCase))
            {
                return new MeteorShower(child.AttrInt("numberOfMeteors"));
            }
            return null;
        }

    }
}
