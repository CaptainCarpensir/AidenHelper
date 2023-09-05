using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.AidenHelper.Effects
{
    public class MeteorShower : Backdrop
    {
        private struct Meteor
        {
            public Vector2 Position;
            public int TextureSet;
            public float Timer;
            public float RateFPS;
            public float Rotation;
        }

        private List<Meteor> meteors;
        public float MeteorFrequency;

        private List<List<MTexture>> textures;

        public MeteorShower(float meteorFrequency)
        {
            MeteorFrequency = meteorFrequency;
            textures = new List<List<MTexture>> {
                GFX.Game.GetAtlasSubtextures("bgs/AidenHelper/meteors/arcComet"),
                GFX.Game.GetAtlasSubtextures("bgs/AidenHelper/meteors/slantComet")
            };
            meteors = new List<Meteor>();
        }

        private Meteor newMeteor()
        {
            return new Meteor
            {
                Position = new Vector2(Calc.Random.NextFloat(320f), Calc.Random.NextFloat(180f) - 20f),
                Timer = 0f,
                RateFPS = 12.5f + Calc.Random.NextFloat(7.5f),
                TextureSet = Calc.Random.Next(textures.Count),
                Rotation = Calc.Random.NextFloat((float)Math.PI / 3),
            };
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);

            if (Visible)
            {
                if (Engine.DeltaTime > 0 && MeteorFrequency > 0)
                {
                    if (Calc.Random.NextDouble() / (Engine.DeltaTime * MeteorFrequency) < 1f)
                    {
                        meteors.Add(newMeteor());
                    }
                }

                for (int i = 0; i < meteors.Count; i++)
                {
                    meteors[i] = new Meteor {
                        Position = meteors[i].Position,
                        Timer = meteors[i].Timer + Engine.DeltaTime,
                        RateFPS = meteors[i].RateFPS,
                        TextureSet = meteors[i].TextureSet,
                        Rotation = meteors[i].Rotation,
                    };
                }
            }
        }

        public override void Render(Scene scene)
        {
            base.Render(scene);

            for (int i = meteors.Count - 1; i >= 0; i--)
            {
                int textAnimId = (int)(meteors[i].Timer * meteors[i].RateFPS);

                if (textAnimId < textures[meteors[i].TextureSet].Count)
                {
                    MTexture texture = textures[meteors[i].TextureSet][textAnimId];
                    texture.DrawCentered(meteors[i].Position, Color.White, 1f, meteors[i].Rotation);
                } 
                else
                {
                    meteors.RemoveAt(i);
                }
            }
        }
    }
}