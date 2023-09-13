using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Mono.Cecil.Cil;
using System;
using System.Reflection;

namespace Celeste.Mod.AidenHelper.Triggers
{
    [CustomEntity("AidenHelper/RunAndGunTrigger")]
    public class RunAndGunTrigger : Trigger
    {
        private static Hook hookDuckingGetter;
        private static ILHook hookDashCoroutine;

        public static void Load()
        {
            IL.Celeste.Player.NormalUpdate += modPlayerNormalUpdate;
            IL.Celeste.Player.DashUpdate += modPlayerDashUpdate;
            IL.Celeste.Player.Update += modPlayerUpdate;
            On.Celeste.Player.Die += modPlayerDie;

            hookDuckingGetter = new Hook(
                typeof(Player).GetMethod("get_Ducking"),
                typeof(RunAndGunTrigger).GetMethod("modDuckingGetter", BindingFlags.NonPublic | BindingFlags.Static));
            hookDashCoroutine = new ILHook(
                typeof(Player).GetMethod("DashCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(),
                ilHookDashCoroutine);
        }

        public static void Unload()
        {
            IL.Celeste.Player.NormalUpdate -= modPlayerNormalUpdate;
            IL.Celeste.Player.DashUpdate -= modPlayerDashUpdate;
            IL.Celeste.Player.Update -= modPlayerUpdate;
            On.Celeste.Player.Die -= modPlayerDie;

            hookDuckingGetter?.Dispose();
            hookDashCoroutine?.Dispose();
            hookDuckingGetter = null;
            hookDashCoroutine = null;
        }

        public static void modPlayerNormalUpdate(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            Logger.Log("AidenHelper/RunAndGunTrigger", $"Modding player normal update at {cursor.Index} in IL for {il.Method.FullName}");
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<Player>>(player => {
                if (Module.AidenHelper.Instance.Session.RunAndGunEnabled)
                {
                    player.moveX = Module.AidenHelper.Instance.Session.RunAndGunMoveX;
                    player.Facing = (Facings)player.moveX;
                }
            });
            while (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdcR4(1.4f) || instr.MatchLdcR4(0.6f)))
            {
                Logger.Log("AidenHelper/RunAndGunTrigger", $"Unsquishing sprite at {cursor.Index} in CIL code for {cursor.Method.FullName}");
                cursor.Remove();
                cursor.Emit(OpCodes.Ldc_R4, 1f);
            }
        }

        public static void modPlayerDashUpdate(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchStfld<Player>("Dashdir")))
            {
                Logger.Log("AidenHelper/RunAndGunTrigger", $"Altering player DashDir at {cursor.Index} in IL for {cursor.Method.FullName}");
                cursor.EmitDelegate<Action<Player>>(player =>
                {
                    if (Module.AidenHelper.Instance.Session.RunAndGunEnabled)
                    {
                        player.DashDir = new Vector2((Module.AidenHelper.Instance.Session.RunAndGunMoveX == 1) ? 1 : -1 * Math.Abs(player.DashDir.X), player.DashDir.Y);
                    }
                });
            }
        }

        public static void modPlayerUpdate(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdfld<Player>("moveX")))
            {
                Logger.Log("AidenHelper/RunAndGunTrigger", $"Altering player moveX at {cursor.Index} in IL for {cursor.Method.FullName}");
                cursor.Remove();
                cursor.EmitDelegate<Func<Player, int>>(player =>
                {
                    if (Module.AidenHelper.Instance.Session.RunAndGunEnabled)
                    {
                        return Module.AidenHelper.Instance.Session.RunAndGunMoveX * player.moveX;
                    }
                    return player.moveX;
                });
            }
        }

        public static PlayerDeadBody modPlayerDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 v1, bool v2, bool v3)
        {
            Module.AidenHelper.Instance.Session.RunAndGunEnabled = false;
            return orig(self, v1, v2, v3);
        }

        private static bool modDuckingGetter(Func<Player, bool> orig, Player self)
        {
            if (Module.AidenHelper.Instance.Session.RunAndGunEnabled && self.StateMachine == Player.StNormal)
            {
                return false;
            }
            return orig(self);
        }

        private static void ilHookDashCoroutine(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            Logger.Log("AidenHelper/RunAndGunTrigger", $"Modding player dash coroutine at {cursor.Index} in IL for {il.Method.FullName}");
            cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdfld<Player>("lastAim"));
            cursor.Remove();
            cursor.EmitDelegate<Func<Player, Vector2>>(player => {
                if (Module.AidenHelper.Instance.Session.RunAndGunEnabled)
                {
                    if (player.moveX == -Module.AidenHelper.Instance.Session.RunAndGunMoveX)
                    {
                        // Changes lastAim to act as if the opposite direction is not being held
                        return new Vector2((player.lastAim.Y == 0) ? Module.AidenHelper.Instance.Session.RunAndGunMoveX : 0, player.lastAim.Y);
                    }
                }
                return player.lastAim;
            });
        }

        private bool enableRunAndGun;
        private int moveDirection;

        public RunAndGunTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            enableRunAndGun = data.Bool("enable", true);
            moveDirection = data.Bool("facingRight", true) ? 1: -1;
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);

            Module.AidenHelper.Instance.Session.RunAndGunEnabled = enableRunAndGun;
            if (enableRunAndGun)
            {
                Module.AidenHelper.Instance.Session.RunAndGunMoveX = moveDirection;
            }
        }
    }
}