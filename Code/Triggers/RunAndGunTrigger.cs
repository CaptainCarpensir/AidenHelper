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
        private static ILHook hookPlayerUpdate;

        public static void Load()
        {
            IL.Celeste.Player.NormalUpdate += modPlayerNormalUpdate;
            IL.Celeste.Player.DashUpdate += modPlayerDashUpdate;
            On.Celeste.Player.Die += modPlayerDie;

            hookPlayerUpdate = new ILHook(
                typeof(Player).GetMethod("orig_Update", BindingFlags.Public | BindingFlags.Instance),
                modPlayerUpdate);
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
            On.Celeste.Player.Die -= modPlayerDie;

            hookPlayerUpdate?.Dispose();
            hookDuckingGetter?.Dispose();
            hookDashCoroutine?.Dispose();
            hookPlayerUpdate = null;
            hookDuckingGetter = null;
            hookDashCoroutine = null;
        }

        public static void modPlayerNormalUpdate(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            Logger.Log("AidenHelper/RunAndGunTrigger", $"Modding player normal update at {cursor.Index} in IL for {il.Method.FullName}");
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<Player>>(OverrideMoveXAndFacing);

            while (cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdfld<Player>("Sprite"),
                instr => instr.MatchLdcR4(0.6f),
                instr => instr.MatchLdcR4(1.4f),
                instr => instr.MatchNewobj<Vector2>()))
            {
                Logger.Log("AidenHelper/RunAndGunTrigger", $"Unsquishing sprite at {cursor.Index} in CIL code for {cursor.Method.FullName}");
                cursor.EmitDelegate<Func<Vector2, Vector2>>(OverrideSquishFactor);
            }
        }

        public static void modPlayerDashUpdate(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.Before, 
                instr => instr.MatchStfld<Player>("DashDir")))
            {
                Logger.Log("AidenHelper/RunAndGunTrigger", $"Altering player DashDir at {cursor.Index} in IL for {cursor.Method.FullName}");
                cursor.EmitDelegate<Func<Vector2, Vector2>>(OverrideDashDir);
                cursor.Index++;
            }
        }

        public static void modPlayerUpdate(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, 
                instr => instr.MatchLdfld<Player>("moveX")))
            {
                Logger.Log("AidenHelper/RunAndGunTrigger", $"Altering player moveX at {cursor.Index} in IL for {cursor.Method.FullName}");
                cursor.EmitDelegate<Func<int, int>>(OverrideLoadedMoveX);
            }
        }

        public static PlayerDeadBody modPlayerDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
        {
            Module.AidenHelper.Instance.Session.RunAndGunEnabled = false;
            return orig(self, direction, evenIfInvincible, registerDeathInStats);
        }

        private static bool modDuckingGetter(Func<Player, bool> orig, Player self) =>
            (Module.AidenHelper.Instance.Session.RunAndGunEnabled && self.StateMachine == Player.StNormal)
                ? false 
                : orig(self);

        private static void ilHookDashCoroutine(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            if (!cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<Player>("lastAim")))
            {
                Logger.Log(LogLevel.Error, "AidenHelper/RunAndGunTrigger", $"Could not mod player dash coroutine in IL for {il.Method.FullName}");
                return;
            }
            Logger.Log("AidenHelper/RunAndGunTrigger", $"Modding player dash coroutine at {cursor.Index} in IL for {il.Method.FullName}");
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.EmitDelegate<Func<Vector2, Player, Vector2>>(OverrideLastAim);
        }

        private bool enableRunAndGun;
        private int moveDirection;

        public RunAndGunTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            enableRunAndGun = data.Bool("enable", true);
            moveDirection = data.Bool("facingRight", true) ? 1 : -1;
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);

            Module.AidenHelper.Instance.Session.RunAndGunEnabled = enableRunAndGun;

            if (enableRunAndGun)
                Module.AidenHelper.Instance.Session.RunAndGunMoveX = moveDirection;
        }

        private static Vector2 OverrideSquishFactor(Vector2 @default)
            => Module.AidenHelper.Instance.Session.RunAndGunEnabled 
                ? Vector2.One
                : @default;

        private static void OverrideMoveXAndFacing(Player player)
        {
            if (!Module.AidenHelper.Instance.Session.RunAndGunEnabled)
                return;

            player.moveX = Module.AidenHelper.Instance.Session.RunAndGunMoveX;
            player.Facing = (Facings)player.moveX;
        }

        private static Vector2 OverrideDashDir(Vector2 @default) =>
            Module.AidenHelper.Instance.Session.RunAndGunEnabled
                ? new Vector2(((Module.AidenHelper.Instance.Session.RunAndGunMoveX == 1) ? 1 : -1) * Math.Abs(@default.X), @default.Y)
                : @default;

        private static int OverrideLoadedMoveX(int @default) =>
            Module.AidenHelper.Instance.Session.RunAndGunEnabled
                ? Module.AidenHelper.Instance.Session.RunAndGunMoveX * @default
                : @default;

        private static Vector2 OverrideLastAim(Vector2 @default, Player player) =>
            (Module.AidenHelper.Instance.Session.RunAndGunEnabled && player.moveX == -Module.AidenHelper.Instance.Session.RunAndGunMoveX)
                // Changes lastAim to act as if the opposite direction is not being held
                ? new Vector2((@default.Y == 0) ? Module.AidenHelper.Instance.Session.RunAndGunMoveX : 0, @default.Y)
                : @default;
    }
}