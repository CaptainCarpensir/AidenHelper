using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using static Celeste.DashSwitch;

namespace AidenHelper.Entities
{
	[CustomEntity("AidenHelper/LinkedDashSwitch")]
	[Tracked]
	class LinkedDashSwitch : Solid 
	{
		public enum Sides
		{
			Up,
			Down,
			Left,
			Right
		}

		private Sides side;

		private Vector2 pressedTarget;
		private bool pressed;
		private Vector2 pressDirection;

		private float speedY;
		private float startY;

		private EntityID id;

		private bool persistent;
		private bool mirrorMode;
		private bool playerWasOn;
		private bool allGates;

		private bool reversed;
		protected string groupFlag;

		private Sprite sprite;

		private string FlagName => GetFlagName(id);

		public LinkedDashSwitch(Vector2 position, Sides side, bool persistent, bool allGates, bool reversed, string groupFlag, EntityID id, string spriteName)
			: base(position, 0f, 0f, safe: true)
		{
			this.reversed = reversed;
			this.groupFlag = groupFlag;
			this.side = side;
			this.persistent = persistent;
			this.allGates = allGates;
			this.id = id;
			mirrorMode = spriteName != "default";
			Add(sprite = GFX.SpriteBank.Create("dashSwitch_" + spriteName));
			sprite.Play("idle");
			if (side == Sides.Up || side == Sides.Down)
			{
				base.Collider.Width = 16f;
				base.Collider.Height = 8f;
			}
			else
			{
				base.Collider.Width = 8f;
				base.Collider.Height = 16f;
			}
			switch (side)
			{
				case Sides.Down:
					sprite.Position = new Vector2(8f, 8f);
					sprite.Rotation = (float)Math.PI / 2f;
					pressedTarget = Position + Vector2.UnitY * 8f;
					pressDirection = Vector2.UnitY;
					startY = base.Y;
					break;
				case Sides.Up:
					sprite.Position = new Vector2(8f, 0f);
					sprite.Rotation = -(float)Math.PI / 2f;
					pressedTarget = Position + Vector2.UnitY * -8f;
					pressDirection = -Vector2.UnitY;
					break;
				case Sides.Right:
					sprite.Position = new Vector2(8f, 8f);
					sprite.Rotation = 0f;
					pressedTarget = Position + Vector2.UnitX * 8f;
					pressDirection = Vector2.UnitX;
					break;
				case Sides.Left:
					sprite.Position = new Vector2(0f, 8f);
					sprite.Rotation = (float)Math.PI;
					pressedTarget = Position + Vector2.UnitX * -8f;
					pressDirection = -Vector2.UnitX;
					break;
			}
			OnDashCollide = OnDashed;
		}

		public static LinkedDashSwitch Create(EntityData data, Vector2 offset, EntityID id)
		{
			Console.WriteLine("Attempts creation: ", id);
			Vector2 position = data.Position + offset;
			bool flag = data.Bool("persistent");
			bool flag2 = data.Bool("allGates");
			bool flag3 = data.Bool("reversed");
			string flag4 = data.Attr("flag");
			string spriteName = data.Attr("sprite", "default");
			if (data.Name.Equals("AidenHelper/LinkedDashSwitchHorizontal"))
			{
				if (data.Bool("leftSide"))
				{
					return new LinkedDashSwitch(position, Sides.Left, flag, flag2, flag3, flag4, id, spriteName);
				}
				return new LinkedDashSwitch(position, Sides.Right, flag, flag2, flag3, flag4, id, spriteName);
			}
			if (data.Bool("ceiling"))
			{
				return new LinkedDashSwitch(position, Sides.Up, flag, flag2, flag3, flag4, id, spriteName);
			}
			return new LinkedDashSwitch(position, Sides.Down, flag, flag2, flag3, flag4, id, spriteName);
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			if (!persistent || !SceneAs<Level>().Session.GetFlag(FlagName))
			{
				return;
			}
			sprite.Play("pushed");
			Position = pressedTarget - pressDirection * 2f;
			pressed = true;
			Collidable = false;
			if (allGates)
			{
				foreach (TempleGate entity in base.Scene.Tracker.GetEntities<TempleGate>())
				{
					if (entity.Type == TempleGate.Types.NearestSwitch && entity.LevelID == id.Level)
					{
						entity.StartOpen();
					}
				}
				return;
			}
			GetGate()?.StartOpen();
		}

		public override void Update()
		{
			base.Update();
			if (pressed || side != Sides.Down)
			{
				return;
			}
			Player playerOnTop = GetPlayerOnTop();
			if (playerOnTop != null)
			{
				if (playerOnTop.Holding != null)
				{
					// TODO: Make sure master switching happens here as to not recursively press switches
					OnDashed(playerOnTop, Vector2.UnitY);
				}
				else
				{
					if (speedY < 0f)
					{
						speedY = 0f;
					}
					speedY = Calc.Approach(speedY, 70f, 200f * Engine.DeltaTime);
					MoveTowardsY(startY + 2f, speedY * Engine.DeltaTime);
					if (!playerWasOn)
					{
						Audio.Play("event:/game/05_mirror_temple/button_depress", Position);
					}
				}
			}
			else
			{
				if (speedY > 0f)
				{
					speedY = 0f;
				}
				speedY = Calc.Approach(speedY, -150f, 200f * Engine.DeltaTime);
				MoveTowardsY(startY, (0f - speedY) * Engine.DeltaTime);
				if (playerWasOn)
				{
					Audio.Play("event:/game/05_mirror_temple/button_return", Position);
				}
			}
			playerWasOn = playerOnTop != null;
		}

		public DashCollisionResults OnDashed(Player player, Vector2 direction)
		{
			if (!pressed && direction == pressDirection)
			{
				PressButton(direction);

				// Linking behavior
				foreach (LinkedDashSwitch entity in base.Scene.Tracker.GetEntities<LinkedDashSwitch>())
				{
					if (!entity.id.Equals(id) && entity.groupFlag == groupFlag)
					{
						entity.OnGroupDashed(direction);
					}
				}
			}
			return DashCollisionResults.NormalCollision;
		}

		public void OnGroupDashed(Vector2 direction)
        {
			if (pressed)
            {
				// Reset the button
				// TODO: Make the button reset visually and mechanically work (needs to interact with temple gates)
			}
			else
            {
				PressButton(direction);
            }
        }

		private void PressButton(Vector2 direction)
        {
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			Audio.Play("event:/game/05_mirror_temple/button_activate", Position);
			sprite.Play("push");
			pressed = true;
			MoveTo(pressedTarget);
			Collidable = false;
			Position -= pressDirection * 2f;
			SceneAs<Level>().ParticlesFG.Emit(mirrorMode ? P_PressAMirror : P_PressA, 10, Position + sprite.Position, direction.Perpendicular() * 6f, sprite.Rotation - (float)Math.PI);
			SceneAs<Level>().ParticlesFG.Emit(mirrorMode ? P_PressBMirror : P_PressB, 4, Position + sprite.Position, direction.Perpendicular() * 6f, sprite.Rotation - (float)Math.PI);
			if (allGates)
			{
				foreach (TempleGate entity in base.Scene.Tracker.GetEntities<TempleGate>())
				{
					if (entity.Type == TempleGate.Types.NearestSwitch && entity.LevelID == id.Level)
					{
						entity.SwitchOpen();
					}
				}
			}
			else
			{
				GetGate()?.SwitchOpen();
			}
			base.Scene.Entities.FindFirst<TempleMirrorPortal>()?.OnSwitchHit(Math.Sign(base.X - (float)(base.Scene as Level).Bounds.Center.X));
			if (persistent)
			{
				SceneAs<Level>().Session.SetFlag(FlagName);
			}
		}

		private TempleGate GetGate()
		{
			List<Entity> entities = base.Scene.Tracker.GetEntities<TempleGate>();
			TempleGate templeGate = null;
			float num = 0f;
			foreach (TempleGate item in entities)
			{
				if (item.Type == TempleGate.Types.NearestSwitch && !item.ClaimedByASwitch && item.LevelID == id.Level)
				{
					float num2 = Vector2.DistanceSquared(Position, item.Position);
					if (templeGate == null || num2 < num)
					{
						templeGate = item;
						num = num2;
					}
				}
			}
			if (templeGate != null)
			{
				templeGate.ClaimedByASwitch = true;
			}
			return templeGate;
		}

		public static string GetFlagName(EntityID id)
		{
			return "dashSwitch_" + id.Key;
		}
	}
}
