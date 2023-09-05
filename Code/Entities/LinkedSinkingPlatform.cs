using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.AidenHelper.Entities
{
	[CustomEntity("AidenHelper/LinkedSinkingPlatform")]
	[TrackedAs(typeof(JumpThru))]
	[Tracked]
	public class LinkedSinkingPlatform : JumpThru
	{
		private const int NUM_SUBTEXTURES = 4;
		private const int SUBTEXTURE_WIDTH = 8;

		private float speed;
		private float startY;
		private float endY;
		private float riseTimer;
		private float downTimer;
		private bool reversed;
		private bool enabled;

		// Used for boolean case to prevent lag caused by time crystals
		private bool downSFXEnabled;
		private bool upSFXEnabled;

		private string innerColor;
		private string outerColor;
		private string levelTag;

		private MTexture[] textures;
		private Shaker shaker;
		private SoundSource downSfx;
		private SoundSource upSfx;

		public string overrideTexture;

		protected string groupFlag;

		public LinkedSinkingPlatform master;
		public List<LinkedSinkingPlatform> Group;
		public bool HasGroup { get; set; }
		public bool MasterOfGroup { get; set; }

		public LinkedSinkingPlatform(Vector2 position, int width, string groupFlag)
			: base(position, width, safe: false)
		{
			startY = base.Y;
			base.Depth = 1;
			SurfaceSoundIndex = 15;
			Add(shaker = new Shaker(on: false));
			Add(new LightOcclude(0.2f));
			Add(downSfx = new SoundSource());
			Add(upSfx = new SoundSource());
			Group = new List<LinkedSinkingPlatform>();
		}

		public LinkedSinkingPlatform(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, "")
		{
			levelTag = data.Level.Name;
			overrideTexture = data.Attr("texture", "");
			SurfaceSoundIndex = data.Int("surfaceIndex", -1);
			groupFlag = data.Attr("flag", "") + levelTag;
			endY = data.NodesWithPosition(new Vector2(0, 0))[1].Y + offset.Y;
			reversed = data.Bool("reversed");
			outerColor = data.Attr("outerColor", "2a1923");
			innerColor = data.Attr("innerColor", "160b12");
			if (reversed) this.Position = new Vector2(X, endY);
			enabled = false;
			downSFXEnabled = false;
			upSFXEnabled = false;
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			if (!HasGroup)
			{
				master = this;
				foreach (LinkedSinkingPlatform item in scene.Tracker.GetEntities<LinkedSinkingPlatform>())
				{
					if (item.groupFlag == groupFlag && item.groupFlag != levelTag)
					{
						Group.Add(item);
					}
				}
				HasGroup = true;
				MasterOfGroup = false;
			}
			if (!reversed) enabled = true;
		}

		public override void Added(Scene scene)
		{
			AreaData areaData = AreaData.Get(scene);
			string woodPlatform = areaData.WoodPlatform;
			if (overrideTexture != null)
			{
				areaData.WoodPlatform = overrideTexture;
			}
			orig_Added(scene);
			areaData.WoodPlatform = woodPlatform;
		}

		public override void Render()
		{
			Vector2 value = shaker.Value;
			textures[0].Draw(Position + value);
			for (int i = 8; (float)i < base.Width - 8f; i += 8)
			{
				textures[1].Draw(Position + value + new Vector2(i, 0f));
			}
			textures[3].Draw(Position + value + new Vector2(base.Width - 8f, 0f));
			textures[2].Draw(Position + value + new Vector2(base.Width / 2f - 4f, 0f));
		}

		public override void Update()
		{
			base.Update();
			Player playerRider = GetPlayerRider();
			if (playerRider != null) // If player is riding
			{
				if (!MasterOfGroup && !reversed) // Set new master of group
				{
					foreach (LinkedSinkingPlatform item in Group)
					{
						// Call move on each item so that you can't prevent movement by buffering inputs
						item.Move(playerRider, endY - startY);

						item.MasterOfGroup = false;
						item.master = this;
						if(item.reversed)
						{
							// Enabling prevents reverse platforms from ever acting as master
							item.enabled = true;
						}
					}
					MasterOfGroup = true;
				}
			}

			if(enabled)
            {
				if (!MasterOfGroup)
				{
					// Code to follow master
					Player masterRider = master.GetPlayerRider();
					Move(masterRider, master.endY - master.startY);
				}
				else
				{
					Move(playerRider, endY - startY);
				}
			}
		}

		public void orig_Added(Scene scene)
		{
			base.Added(scene);
			MTexture mTexture = GFX.Game["objects/woodPlatform/" + AreaData.Get(scene).WoodPlatform];
			textures = new MTexture[NUM_SUBTEXTURES];
			for (int i = 0; i < NUM_SUBTEXTURES; i++)
			{
				textures[i] = mTexture.GetSubtexture((i * SUBTEXTURE_WIDTH)%mTexture.Width, 0, SUBTEXTURE_WIDTH, 8);
			}
			scene.Add(new AidenHelperSinkingPlatformLine(
				new Vector2(X, startY) + new Vector2(base.Width / 2f, base.Height / 2f),
				new Vector2(X, endY) + new Vector2(base.Width / 2f, base.Height / 2f),
				innerColor, outerColor));
		}

		private void Move(Player playerRider, float masterDiff)
		{
			float mult = (endY - startY) / masterDiff;

			// Set speed values and start movement audio
			if (playerRider != null)
			{
				if (riseTimer <= 0f)
				{
					if (base.ExactPosition.Y <= startY)
					{
						Audio.Play("event:/game/03_resort/platform_vert_start", Position);
					}
					shaker.ShakeFor(0.15f, removeOnFinish: false);
				}
				riseTimer = 0.1f;
				speed = Calc.Approach(speed, (reversed ? -1 : 1) * (playerRider.Ducking ? 60f : 30f) * mult, 400f * mult * Engine.DeltaTime);
			}
			else if (riseTimer > 0f)
			{
				riseTimer -= Engine.DeltaTime;
				speed = Calc.Approach(speed, (reversed ? -1 : 1) * 45f * mult, 600f * mult * Engine.DeltaTime);
			}
			else
			{
				downTimer -= Engine.DeltaTime;
				speed = Calc.Approach(speed, (reversed ? -1 : 1) * -50f, 400f * mult * Engine.DeltaTime);
			}


			if (speed > 0f && base.ExactPosition.Y < endY)
			{
				// DownSFX looks redundant here, but prevents from start/stop cycle from happening with time stop
				if (!downSFXEnabled)
				{
					downSFXEnabled = true;
					if(!reversed)
					{
						downSfx.Play("event:/game/03_resort/platform_vert_down_loop");
					}
					else
					{
						downSfx.Play("event:/game/03_resort/platform_vert_up_loop");
					}
				}
				if (upSFXEnabled)
				{
					upSFXEnabled = false;
					upSfx.Stop();
				}
				downSfx.Param("ducking", (playerRider != null && playerRider.Ducking) ? 1 : 0);
				MoveTowardsY(endY, speed * Engine.DeltaTime);
			}
			else if (speed < 0f && base.ExactPosition.Y > startY)
			{
				// UpSFX looks redundant here, but prevents from start/stop cycle from happening with time stop
				if (!upSFXEnabled)
				{
					upSFXEnabled = true;
					if (!reversed)
					{
						upSfx.Play("event:/game/03_resort/platform_vert_up_loop");
					}
					else
					{
						upSfx.Play("event:/game/03_resort/platform_vert_down_loop");
					}
				}
				if (downSFXEnabled)
				{
					downSFXEnabled = false;
					downSfx.Stop();
				}
				MoveTowardsY(startY, (0f - speed) * Engine.DeltaTime);
				if (base.ExactPosition.Y <= startY)
				{
					upSfx.Stop();
					Audio.Play("event:/game/03_resort/platform_vert_end", Position);
					shaker.ShakeFor(0.1f, removeOnFinish: false);
				}
			}
			else if (speed > 0f)
			{
				
				if (downTimer <= 0f)
				{
					downSfx.Stop();
					Audio.Play("event:/game/03_resort/platform_vert_end", Position);
					shaker.ShakeFor(0.1f, removeOnFinish: false);
				}
				downTimer = 0.1f;
			}

			// Case to fix platforms not stopping audio caused by reversed linked movement
			if (reversed && downSFXEnabled && base.ExactPosition.Y >= endY)
			{
				downSFXEnabled = false;
				downSfx.Stop();
			}
		}
	}
}
