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

        public DashDashBlock(Vector2 position, char tileType, float width, float height, bool blendIn, bool permanent, EntityID id)
            : base(position, tileType, width, height, blendIn, permanent, false, id)
        {
        }

        public DashDashBlock(EntityData data, Vector2 offset, EntityID id) 
            : this(data.Position + offset, data.Char("tiletype", '3'), data.Width, data.Height, data.Bool("blendin"), data.Bool("permanent", true), id)
        {
            horizontal = data.Bool("horizontal");
            vertical = data.Bool("vertical");
        }

        public override void Update()
        {
            base.Update();

            /*
             *  Check for player pressing dash button and call Break() if player is directly horizontally or vertically alligned
             */

            if (!Input.Dash.Pressed) return;

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

                if ((dir.X == 1 && dir.Y == 0 && horizontal) || (dir.X == 0 && dir.Y == 1 && vertical))
                {
                    Break(entity.Position, dir, true);
                }
            }
        }
    }
}
