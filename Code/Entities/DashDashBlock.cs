using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.AidenHelper.Entities
{
    [CustomEntity("AidenHelper/DashDashBlock")]
    public class DashDashBlock : DashBlock
    {
        private bool horizontal;
        private bool vertical;
        private bool requiresLineOfSight;

        public DashDashBlock(Vector2 position, char tileType, float width, float height, bool blendIn, bool permanent, EntityID id)
            : base(position, tileType, width, height, blendIn, permanent, false, id)
        {
        }

        public DashDashBlock(EntityData data, Vector2 offset, EntityID id) 
            : this(data.Position + offset, data.Char("tiletype", '3'), data.Width, data.Height, data.Bool("blendin"), data.Bool("permanent", true), id)
        {
            horizontal = data.Bool("horizontal");
            vertical = data.Bool("vertical");
            requiresLineOfSight = data.Bool("requiresLineOfSight");
        }

        // Some code for this function is taken from CommunalHelper, written by Catapillie
        // https://github.com/CommunalHelper/CommunalHelper/blob/dev/src/Entities/Misc/Melvin.cs
        private bool IsPlayerSeen(Player player, Vector2 dir)
        {
            Rectangle rect = new Rectangle();
            int y1 = (int)Math.Max(player.Top, Top);
            int y2 = (int)Math.Min(player.Bottom, Bottom);
            int x1 = (int)Math.Max(player.Left, Left);
            int x2 = (int)Math.Min(player.Right, Right);
            if (dir.X == 1 && dir.Y == 0)
            {
                // right
                rect = new Rectangle((int)(X + Width), y1, (int)(player.Left - X - Width), y2 - y1);
            }
            else if (dir.X == -1 && dir.Y == 0)
            {
                // left
                rect = new Rectangle((int)player.Right, y1, (int)(X - player.Right), y2 - y1);
            }
            else if (dir.X == 0 && dir.Y == 1)
            {
                // down
                rect = new Rectangle(x1, (int)(Y + Height), x2 - x1, (int)(player.Top - Y - Height));
            }
            else if (dir.X == 0 && dir.Y == -1)
            {
                // up
                rect = new Rectangle(x1, (int)player.Bottom, x2 - x1, (int)(Y - player.Bottom));
            }

            if (dir.Y != 0)
            {
                for (int i = 0; i < rect.Width; i++)
                {
                    Rectangle lineRect = new(rect.X + i, rect.Y, 1, rect.Height);
                    if (!Scene.CollideCheck<Solid>(lineRect))
                        return true;
                }
                return false;
            }
            else
            {
                for (int i = 0; i < rect.Height; i++)
                {
                    Rectangle lineRect = new(rect.X, rect.Y + i, rect.Width, 1);
                    if (!Scene.CollideCheck<Solid>(lineRect))
                        return true;
                }
                return false;
            }
        }

        public override void Update()
        {
            base.Update();

            /*
             *  Check for player pressing dash button and call Break() if player is directly horizontally or vertically alligned
             */

            if (!Input.Dash.Pressed && !Input.CrouchDash.Pressed) return;

            foreach (Player entity in base.Scene.Tracker.GetEntities<Player>())
            {
                Vector2 dir = entity.Position - (Position + new Vector2(Width / 2, Height / 2));
                if (Math.Abs(dir.X) <= Width / 2 && Math.Abs(dir.Y) >= Height / 2)
                {
                    dir.X = 0;
                    dir.Y = dir.Y / Math.Abs(dir.Y);
                }
                else if (Math.Abs(dir.Y) <= Height / 2 && Math.Abs(dir.X) >= Width / 2)
                {
                    dir.X = dir.X / Math.Abs(dir.X);
                    dir.Y = 0;
                }

                if ((dir.Y == 0 && horizontal) || (dir.X == 0 && vertical))
                {
                    if (requiresLineOfSight && !IsPlayerSeen(entity, dir))
                    {
                        return;
                    }
                    Break(entity.Position, dir, true);
                }
            }
        }
    }
}
