using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;

namespace Celeste.Mod.AidenHelper.Triggers
{
    [CustomEntity("AidenHelper/InvertStaminaOnDashTrigger")]
    public class InvertStaminaOnDashTrigger : Trigger
    {

        public static void Load()
        {
            IL.Celeste.Player.StartDash += modPlayerStartDash;
        }

        public static void Unload()
        {
            IL.Celeste.Player.StartDash -= modPlayerStartDash;
        }

        public static void modPlayerStartDash(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            Logger.Log("AidenHelper/InvertStaminaOnDashTrigger", $"Modding player dash at {cursor.Index} in IL for {il.Method.FullName}");
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<Player>>(player => {
                if (Module.AidenHelper.Instance.Session.InvertStaminaOnDashEnabled)
                {
                    player.Stamina = Player.ClimbMaxStamina - player.Stamina;
                }
            });
        }
        
        private bool enableStaminaInversion;

        public InvertStaminaOnDashTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            enableStaminaInversion = data.Bool("enable", false);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);

            Module.AidenHelper.Instance.Session.InvertStaminaOnDashEnabled = enableStaminaInversion;
        }
    }
}