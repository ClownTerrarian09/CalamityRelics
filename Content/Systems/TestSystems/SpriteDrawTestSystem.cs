using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using System.Collections.Generic;
using CalamityRelics.Content.NPCs.DraedonHouseBarrier;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace CalamityRelics.Content.Systems.TestSystems
{

    // Please try to ignore this. this is just here for my (ELITE)'s testing
    public class SpriteDrawTestSystem : ModSystem
    {
        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            //     spriteBatch.Draw((Texture2D)TextureAssets.DukeFishron, new Rectangle(50, 50, 50, 50), Color.White);

            // try
            // {
            //     Effect effect = (Mod as CalamityRelics).rocketRamShader;

            //     if(effect != null)
            //     {
            //         spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, effect);
            //         Main.NewText("yay");
            //         return;
            //     }

            //     // Main.NewText("boo");
                
            // }
            // catch
            // {
            //     Main.NewText("Couldnt execute new shader code");
            // }

                
        }
        
    }
}