using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using Terraria;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Generation;
using Terraria.DataStructures;

namespace CalamityRelics.Content.WorldGen
{
    public class OasisRemnants : ModSystem
    {
        //public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        //{
            //int relicsIndex = tasks.FindIndex(genpass => genpass.Name.Contains("Oasis Remnants"));

            //if (relicsIndex != -1)
            //{
                //tasks.Insert(relicsIndex + 1, new PassLegacy("Oasis Remnants", (progress, configuration) =>
                //{
                    //progress.Message = "Salvaging Ilmeris' Outskirts";
                    //GenOasisRemnants();
                //}));
            //}
            //else
            //{
                //Mod.Logger.Warn("Calamity Relics: Could not find 'Oasis Remnants' generation pass. The Relics Structure will not be generated.");
            //}
        //}

        private void GenOasisRemnants()
        {

            int startX = GenVars.UndergroundDesertLocation.X;
            int endX = GenVars.UndergroundDesertLocation.X + GenVars.UndergroundDesertLocation.Width;

            for (int x = startX; x < endX; x++)
            {
                for (int y = 100; y < Main.worldSurface; y++)
                {
                    Tile tile = Framing.GetTileSafely(x, y);

                    Point16 structurePosition = new Point16(x - 20, y - 8);

                    
                }
            }
        }
    }
}