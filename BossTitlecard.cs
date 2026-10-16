using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using Microsoft.Xna.Framework;

namespace CalamityRelics
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	public class BossTitlecard : ModSystem
	{
        float nameX = -200;
        float titleX = Main.screenWidth + 200;

        public string name;
        public string title;

        public Color fontColorA;
        public Color fontColorB;

        Color finalColor;

        float colorGradient = 0;

        float nameSX;
        float titleSX;

        float DeicideY;
        float DeicideSY;

        public int timere = -1;
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int index = layers.FindIndex(layer => layer.Name == "Vanilla: Inventory");


            if (index != -1)
            {
                layers.Insert(index, new LegacyGameInterfaceLayer(
                    "CalamityRelics: Boss Title",
                    delegate
                    {
                        DrawMyUI(Main.spriteBatch);
                        return true;
                    },
                    InterfaceScaleType.UI
                ));
            }
        }

        private void DrawMyUI(SpriteBatch spriteBatch)
        {
            if (timere != -1)
            {
                timere++;

                colorGradient = (float)(Math.Sin(timere * 25) / 2) + 0.5f;

                finalColor = Color.Lerp(fontColorA, fontColorB, colorGradient);

                Texture2D deicide = ModContent.Request<Texture2D>("CalamityRelics/Assets/Textures/Effects/TitlecardDeicide").Value;

                if (timere == 1)
                {
                    nameX = -200;
                    titleX = Main.screenWidth + 200;
                    DeicideY = -400;

                    nameSX = 0;
                    titleSX = 0;
                    DeicideSY = 0;
                }

                if (timere < 150)
                {
                    nameX += ((Main.screenWidth / 2) - nameX) / 15;
                    titleX += ((Main.screenWidth / 2) - titleX) / 15;
                    DeicideY += ((Main.screenHeight / 2) - DeicideY) / 15;
                }
                else
                {
                    if (timere < 500)
                    {
                        if (titleSX > -25)
                        {
                            titleSX -= 0.2f;
                        }
                        if (nameSX < 25)
                        {
                            nameSX += 0.2f;
                        }
                        if (DeicideSY < 25)
                        {
                            DeicideSY += 0.2f;
                        }

                        nameX += nameSX;
                        titleX += titleSX;
                        DeicideY += DeicideSY;
                    }
                    else
                    {
                        timere = -1;
                    }
                }

                //spriteBatch.Draw(deicide, new Vector2(Main.screenWidth / 2, DeicideY - 60), null, Color.White, 0, deicide.Size() / 2, 1f, SpriteEffects.None, 1);

                Utils.DrawBorderStringBig(spriteBatch, title, new Vector2(titleX, Main.screenHeight / 2 - 60 - 100), finalColor, 0.6f, 0.5f, 0.5f);
                Utils.DrawBorderStringBig(spriteBatch, name, new Vector2(nameX, Main.screenHeight / 2 - 100), finalColor, 1.25f, 0.5f, 0.5f);
            }
        }
    }
}
