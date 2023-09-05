using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using static Celeste.DashSwitch;

namespace Celeste.Mod.AidenHelper.Entities
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

		private Vector2 unpressedTarget;
		private Vector2 pressedTarget;
		private bool pressed;
		private Vector2 pressDirection;

		private float speedY;
		private float startY;

		private EntityID id;

		private bool persistent;
		private bool mirrorMode;
		private bool playerWasOn;

		private bool reversed;
		protected string groupFlag;

		private Sprite sprite;

		private string StateFlag => GetStateFlag(id);
		private string ConstructedFlag => GetConstructedFlag(id);

		public LinkedDashSwitch(Vector2 position, Sides side, bool persistent, bool reversed, string groupFlag, EntityID id, string spriteName)
			: base(position, 0f, 0f, safe: true)
		{
			this.reversed = reversed;
			this.groupFlag = groupFlag;
			this.side = side;
			this.persistent = persistent;
			this.id = id;
			mirrorMode = spriteName != "default";
			Add(sprite = GFX.SpriteBank.Create("dashSwitch_" + spriteName));
			sprite.Play("idle");
			OnDashCollide = OnDashed;
		}

		public LinkedDashSwitch(EntityData data, Vector2 offset) : this(
			data.Position + offset,
			data.Enum("side", Sides.Up),
			data.Bool("persistent"),
			data.Bool("reversed"),
			data.Attr("flag"),
			new EntityID(data.Level.Name, data.ID),
			data.Attr("sprite", "default")) 
		{
			String sideAttr = data.Attr("direction");
			switch (sideAttr)
            {
				case "Up":
					this.side = Sides.Up;
					sprite.Position = new Vector2(8f, 8f);
					sprite.Rotation = (float)Math.PI / 2f;
					pressedTarget = Position + Vector2.UnitY * 8f;
					pressDirection = Vector2.UnitY;
					base.Collider.Width = 16f;
					base.Collider.Height = 8f;
					startY = base.Y;
					break;
				case "Down":
					this.side = Sides.Down;
					sprite.Position = new Vector2(8f, 0f);
					sprite.Rotation = -(float)Math.PI / 2f;
					pressedTarget = Position + Vector2.UnitY * -8f;
					pressDirection = -Vector2.UnitY;
					base.Collider.Width = 16f;
					base.Collider.Height = 8f;
					startY = base.Y;
					break;
				case "Left":
					this.side = Sides.Left;
					sprite.Position = new Vector2(8f, 8f);
					sprite.Rotation = 0f;
					pressedTarget = Position + Vector2.UnitX * 8f;
					pressDirection = Vector2.UnitX;
					base.Collider.Width = 8f;
					base.Collider.Height = 16f;
					break;
				case "Right":
					this.side = Sides.Right;
					sprite.Position = new Vector2(0f, 8f);
					sprite.Rotation = (float)Math.PI;
					pressedTarget = Position + Vector2.UnitX * -8f;
					pressDirection = -Vector2.UnitX;
					base.Collider.Width = 8f;
					base.Collider.Height = 16f;
					break;
            }
			unpressedTarget = Position;
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			if (!SceneAs<Level>().Session.GetFlag(ConstructedFlag))
			{
				SceneAs<Level>().Session.SetFlag(ConstructedFlag);
				SceneAs<Level>().Session.SetFlag(StateFlag, reversed);
			}
			if (!SceneAs<Level>().Session.GetFlag(StateFlag))
			{
				return;
			}
			sprite.Play("pushed");
			Position = pressedTarget - pressDirection * 2f;
			pressed = true;
			Collidable = false;
		}

		public override void Update()
		{
			base.Update();
			if (pressed || side != Sides.Up)
			{
				return;
			}
			Player playerOnTop = GetPlayerOnTop();
			if (playerOnTop != null)
			{
				if (playerOnTop.Holding != null)
				{
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
					if (entity.id.Equals(id)) continue;

					if (entity.groupFlag == groupFlag && groupFlag != "")
					{
						entity.OnGroupDashed(direction);
					}
				}
				foreach (LinkedTempleGate entity in base.Scene.Tracker.GetEntities<LinkedTempleGate>())
                {
					if (entity.groupFlag == groupFlag && groupFlag != "")
                    {
						Console.WriteLine("gate " + entity.id.Key + "switched");
						if (entity.open) 
						{
							entity.SwitchClose();
                        } 
						else
                        {
							entity.SwitchOpen();
                        }
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
				sprite.Play("idle");
				pressed = false;
				Collidable = true;
				MoveTo(unpressedTarget);
				SceneAs<Level>().ParticlesFG.Emit(mirrorMode ? P_PressAMirror : P_PressA, 10, Position + sprite.Position, direction.Perpendicular() * 6f, sprite.Rotation - (float)Math.PI);
				SceneAs<Level>().ParticlesFG.Emit(mirrorMode ? P_PressBMirror : P_PressB, 4, Position + sprite.Position, direction.Perpendicular() * 6f, sprite.Rotation - (float)Math.PI);
				if (persistent) SceneAs<Level>().Session.SetFlag(StateFlag, false);
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

			base.Scene.Entities.FindFirst<TempleMirrorPortal>()?.OnSwitchHit(Math.Sign(base.X - (float)(base.Scene as Level).Bounds.Center.X));
			if (persistent) SceneAs<Level>().Session.SetFlag(StateFlag);
		}

		public static string GetStateFlag(EntityID id)
		{
			return "linkedDashSwitch_" + id.Key;
		}

		public static string GetConstructedFlag(EntityID id)
		{
			return "linkedDashSwitchConstructed_" + id.Key;
		}
	}
}
