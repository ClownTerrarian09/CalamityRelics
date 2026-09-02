using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using System.Collections.Generic;
using CalamityRelics.Content.NPCs.DraedonHouseBarrier;

namespace CalamityRelics.Content.Systems.CustomStructureBehavior.DraedonHouse.RectangleDetection
{
    public class DraedonHouseSystem : ModSystem
    {
        public static Rectangle DraedonHouseRect = Rectangle.Empty;
        public static bool IsHouseUnlocked = false;

        public static int DoorOffsetX = 32;
        public static int DoorOffsetY = 73;

        public static HashSet<int> ProtectedLabTiles = new HashSet<int>();
        public static HashSet<int> ProtectedLabWalls = new HashSet<int>();

        public override void PostSetupContent()
        {
            ProtectedLabTiles.Clear();
            ProtectedLabWalls.Clear();

            int[] vanillaTiles = new int[]
            {
                TileID.IronBrick, TileID.Glass, TileID.TopazGemspark,
                TileID.MarbleBlock, TileID.Chain, TileID.MinecartTrack,
                TileID.MetalBars, TileID.Switches, TileID.Furnaces,
                TileID.Bottles, TileID.BouncyBoulder, TileID.ItemFrame
            };

            foreach (int id in vanillaTiles) ProtectedLabTiles.Add(id);

            int[] vanillaWalls = new int[]
            {
                WallID.IronBrick, WallID.Glass, WallID.MarbleBlock
            };

            foreach (int id in vanillaWalls) ProtectedLabWalls.Add(id);

            string[] calamityTiles = new string[]
            {
                "RustedPlating", "WulfrumPanels", "RustedPipes", "RustedShelf",
                "MiniAgedFrostlight", "MiniCagedFrostlight", "WulfrumPlating",
                "AnodizedWulfrumPlatform", "RoundedAnodizedWulfrumPanels", "WulfrumSiding",
                "LaboratoryPipePlating", "PowerCellFactory", "ChargingStation",
                "AgedLaboratoryContainmentBox", "AgedSecurityChest", "AnodizedWulfrumSink",
                "AnodizedWulfrumChest", "WulfrumSink", "WulfrumToilet", "WulfrumLabStation",
                "WulfrumTable", "WulfrumBed", "LaboratoryConsole", "AgedLaboratoryDesign",
                "AgedLaboratoryConsole", "PlaguedBed", "CodebreakerTile", "ChargedWulfrumEnergyBarrier"
            };
            foreach (string name in calamityTiles)
            {
                if (ModContent.TryFind("CalamityMod", name, out ModTile tile)) ProtectedLabTiles.Add(tile.Type);
            }

            string[] calamityWalls = new string[]
            {
                "WulfrumSidingWall", "HazardChevronWall", "WulfrumSheetWall",
                "RoundedAnodizedWulfrumPanelWall", "RustedPlatingWall", "RustedPlatePillar",
                "RustedPlateBeam"
            };
            foreach (string name in calamityWalls)
            {
                if (ModContent.TryFind("CalamityMod", name, out ModWall wall)) ProtectedLabWalls.Add(wall.Type);
            }
        }

        public override void ClearWorld()
        {
            DraedonHouseRect = Rectangle.Empty;
            IsHouseUnlocked = false;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            tag["DraHouseX"] = DraedonHouseRect.X;
            tag["DraHouseY"] = DraedonHouseRect.Y;
            tag["DraHouseW"] = DraedonHouseRect.Width;
            tag["DraHouseH"] = DraedonHouseRect.Height;
            tag["DraHouseUnlocked"] = IsHouseUnlocked;
            tag["DraDoorOffsetX"] = DoorOffsetX;
            tag["DraDoorOffsetY"] = DoorOffsetY;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            if (tag.ContainsKey("DraHouseX"))
            {
                DraedonHouseRect = new Rectangle(
                    tag.GetInt("DraHouseX"),
                    tag.GetInt("DraHouseY"),
                    tag.GetInt("DraHouseW"),
                    tag.GetInt("DraHouseH")
                );
            }
            IsHouseUnlocked = tag.GetBool("DraHouseUnlocked");
            DoorOffsetX = tag.GetInt("DraDoorOffsetX");
            DoorOffsetY = tag.GetInt("DraDoorOffsetY");
        }

        public override void PostUpdateWorld()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            if (!IsHouseUnlocked && DraedonHouseRect != Rectangle.Empty)
            {
                int barrierType = ModContent.NPCType<DraedonBarrierNPC>();
                if (!NPC.AnyNPCs(barrierType))
                {
                    int spawnX = (DraedonHouseRect.X + DoorOffsetX) * 16 + 8;
                    int spawnY = (DraedonHouseRect.Y + DoorOffsetY) * 16 + 8;

                    NPC.NewNPC(
                        new Terraria.DataStructures.EntitySource_Misc("CalamityRelics: Draedon Barrier Persistence"),
                        spawnX,
                        spawnY,
                        barrierType
                    );
                }
            }
        }
    }
}