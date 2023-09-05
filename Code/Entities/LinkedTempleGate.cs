using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.AidenHelper.Entities
{
	[CustomEntity("AidenHelper/LinkedTempleGate")]
	[Tracked]
	public class LinkedTempleGate : Solid
	{

		public EntityID id;

		private int closedHeight;

		private Sprite sprite;
		private Shaker shaker;

		private float drawHeight;
		private float drawHeightMoveSpeed;

		public bool open;

		private bool reversed;
		private bool persistent;
		public string groupFlag;

		private string OpenFlag => GetOpenFlag(id);
		private string ConstructedFlag => GetConstructedFlag(id);

		public LinkedTempleGate(Vector2 position, int height, bool reversed, bool persistent, string groupFlag, string spriteName, EntityID id)
			: base(position, 8f, height, safe: true)
		{
			closedHeight = height;
			Add(sprite = GFX.SpriteBank.Create("templegate_" + spriteName));
			sprite.X = base.Collider.Width / 2f;
			sprite.Play("idle");
			this.id = id;
			this.groupFlag = groupFlag;
			this.reversed = reversed;
			this.persistent = persistent;
			Add(shaker = new Shaker(on: false));
			base.Depth = -9000;
		}


		public LinkedTempleGate(EntityData data, Vector2 offset) : this(
			data.Position + offset, 
			data.Height, 
			data.Bool("reversed"), 
			data.Bool("persistent"), 
			data.Attr("flag"), 
			data.Attr("sprite", "default"), 
			new EntityID(data.Level.Name, data.ID)
		){}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			if (!SceneAs<Level>().Session.GetFlag(ConstructedFlag) || !persistent)
			{
				SceneAs<Level>().Session.SetFlag(ConstructedFlag);
				SceneAs<Level>().Session.SetFlag(OpenFlag, reversed);
			}
			if (SceneAs<Level>().Session.GetFlag(OpenFlag)) 
			{
				SetHeight(0);
				open = true;
			}
			else
            {
				SetHeight(closedHeight);
				open = false;
			}
			drawHeight = Math.Max(4f, base.Height);
		}

		public void SwitchOpen()
		{
			sprite.Play("open");
			Alarm.Set(this, 0.2f, delegate
			{
				shaker.ShakeFor(0.2f, removeOnFinish: false);
				Alarm.Set(this, 0.2f, Open);
			});
		}

		public void SwitchClose()
        {
			Alarm.Set(this, 0.01f, delegate
			{
				Alarm.Set(this, 0.01f, Close);
			});
		}

		public void Open()
		{
			Audio.Play("event:/game/05_mirror_temple/gate_main_open", Position);
			drawHeightMoveSpeed = 200f;
			drawHeight = base.Height;
			shaker.ShakeFor(0.2f, removeOnFinish: false);
			SetHeight(0);
			sprite.Play("open");
			open = true;
			SceneAs<Level>().Session.SetFlag(OpenFlag, true);
		}

		public void Close()
		{
			Audio.Play("event:/game/05_mirror_temple/gate_main_close", Position);
			drawHeightMoveSpeed = 300f;
			drawHeight = Math.Max(4f, base.Height);
			shaker.ShakeFor(0.2f, removeOnFinish: false);
			SetHeight(closedHeight);
			sprite.Play("hit");
			open = false;
			SceneAs<Level>().Session.SetFlag(OpenFlag, false);
		}

		private void SetHeight(int height)
		{
			if ((float)height < base.Collider.Height)
			{
				base.Collider.Height = height;
				return;
			}
			float y = base.Y;
			int num = (int)base.Collider.Height;
			if (base.Collider.Height < 64f)
			{
				base.Y -= 64f - base.Collider.Height;
				base.Collider.Height = 64f;
			}
			MoveVExact(height - num);
			base.Y = y;
			base.Collider.Height = height;
		}

		public override void Update()
		{
			base.Update();
			float num = Math.Max(4f, base.Height);
			if (drawHeight != num)
			{
				drawHeight = Calc.Approach(drawHeight, num, drawHeightMoveSpeed * Engine.DeltaTime);
			}
		}

		public override void Render()
		{
			Vector2 vector = new Vector2(Math.Sign(shaker.Value.X), 0f);
			Draw.Rect(base.X - 2f, base.Y - 8f, 14f, 10f, Color.Black);
			sprite.DrawSubrect(Vector2.Zero + vector, new Rectangle(0, (int)(sprite.Height - drawHeight), (int)sprite.Width, (int)drawHeight));
		}

		public static string GetOpenFlag(EntityID id)
		{
			return "linkedTempleGate_" + id.Key;
		}

		public static string GetConstructedFlag(EntityID id)
		{
			return "linkedTempleGateConstructed_" + id.Key;
		}
	}
}