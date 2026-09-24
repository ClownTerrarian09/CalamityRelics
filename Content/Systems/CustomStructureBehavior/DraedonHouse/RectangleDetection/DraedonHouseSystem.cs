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
        public static bool DisablePersistentBarrierSpawnForDebug = false;
        public static bool HasSpawnedBarrier = false;
        public static Rectangle DraedonHouseRect = Rectangle.Empty;
        public static Rectangle DraedonHouseLegsRect = Rectangle.Empty;
        public static bool IsHouseUnlocked = false;
        public static int DoorOffsetX = 22;
        public static int DoorOffsetY = 64;
        public static HashSet<int> ProtectedLabTiles = new HashSet<int>();
        public static HashSet<int> ProtectedLabWalls = new HashSet<int>();

        /// <summary>
        /// Checks if a given tile coordinate (i, j) is inside any part of the Draedon House structure.
        /// </summary>
        public static bool IsTileInHouse(int i, int j)
        {
            if (DraedonHouseRect == Rectangle.Empty)
                return false;

            Point tilePoint = new Point(i, j);

            return DraedonHouseRect.Contains(tilePoint) || DraedonHouseLegsRect.Contains(tilePoint);
        }

        public override void PostSetupContent()
        {
            ProtectedLabTiles.Clear();
            ProtectedLabWalls.Clear();

            int[] vanillaTiles =
            [
                TileID.IronBrick, TileID.Glass, TileID.TopazGemspark,
                TileID.TopazGemsparkOff, TileID.Chain, TileID.ItemFrame,
                TileID.MetalBars, TileID.Switches, TileID.Furnaces,
                TileID.Bottles, TileID.BouncyBoulder, TileID.Grate,
                TileID.GrateClosed, TileID.MarbleBlock, TileID.MinecartTrack
            ];

            foreach (int id in vanillaTiles) ProtectedLabTiles.Add(id);

            int[] vanillaWalls =
            [
                WallID.IronBrick, WallID.Glass, WallID.MarbleBlock
            ];

            foreach (int id in vanillaWalls) ProtectedLabWalls.Add(id);

            string[] calamityTiles =
            [
                "RustedPlating", "WulfrumPanels", "RustedPipes", "RustedShelf",
                "MiniAgedFrostlight", "MiniCagedFrostlight", "WulfrumPlating",
                "AnodizedWulfrumPlatform", "RoundedAnodizedWulfrumPanels", "WulfrumSiding",
                "LaboratoryPipePlating", "PowerCellFactory", "ChargingStation",
                "AgedLaboratoryContainmentBox", "AgedSecurityChestTile", "AnodizedWulfrumSink",
                "AnodizedWulfrumChest", "WulfrumSink", "WulfrumToilet", "WulfrumLabstation",
                "WulfrumTable", "WulfrumBed", "LaboratoryConsole", "AgedLaboratoryScreen",
                "AgedLaboratoryConsole", "PlaguedPlateBed", "ChargedWulfrumEnergyBarrier", "LaboratoryDisplay",
                "AgedLaboratoryDisplay", "AgedLaboratoryDoorClosed", "AgedLaboratoryDoorOpen"
            ];
            foreach (string name in calamityTiles)
            {
                if (ModContent.TryFind("CalamityMod", name, out ModTile tile)) ProtectedLabTiles.Add(tile.Type);
            }

            string[] calamityWalls =
            [
                "WulfrumSidingWall", "HazardChevronWall", "WulfrumSheetWall",
                "RoundedAnodizedWulfrumPanelWall", "RustedPlatingWall", "RustedPlatePillar",
                "RustedPlateBeam"
            ];
            foreach (string name in calamityWalls)
            {
                if (ModContent.TryFind("CalamityMod", name, out ModWall wall)) ProtectedLabWalls.Add(wall.Type);
            }

            string[] relicsTiles = 
            [
                "RustedCodebreakerFurniture"
            ];

            foreach (string name in relicsTiles)
            {
                if (ModContent.TryFind("CalamityRelics", name, out ModTile tile)) ProtectedLabTiles.Add(tile.Type);
            }

            string[] relicsWalls =
            [
                //
            ];

            foreach (string name in relicsWalls)
            {
                if (ModContent.TryFind("CalamityRelics", name, out ModWall wall)) ProtectedLabWalls.Add(wall.Type);
            }
        }

        public override void ClearWorld()
        {
            DraedonHouseRect = Rectangle.Empty;
            DraedonHouseLegsRect = Rectangle.Empty;
            IsHouseUnlocked = false;
            HasSpawnedBarrier = false;
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
            tag["DraHouse_HasSpawnedBarrier"] = HasSpawnedBarrier;
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

                DraedonHouseLegsRect = new Rectangle(
                    DraedonHouseRect.X + 68,
                    DraedonHouseRect.Y + 65,
                    17,
                    47
                );
            }

            IsHouseUnlocked = tag.GetBool("DraHouseUnlocked");
            DoorOffsetX = tag.GetInt("DraDoorOffsetX");
            DoorOffsetY = tag.GetInt("DraDoorOffsetY");
            if (tag.ContainsKey("DraHouse_HasSpawnedBarrier")) HasSpawnedBarrier = tag.GetBool("DraHouse_HasSpawnedBarrier");
        }

        public override void PostUpdateWorld()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            if (DraedonHouseRect != Rectangle.Empty)
            {
                Mod.Logger.Debug($"Calamity Relics: DraedonHouseRect present X:{DraedonHouseRect.X} Y:{DraedonHouseRect.Y} W:{DraedonHouseRect.Width} H:{DraedonHouseRect.Height} Unlocked:{IsHouseUnlocked} HasSpawnedBarrier:{HasSpawnedBarrier}");

                int barrierType = ModContent.NPCType<DraedonBarrierNPC>();
                if (HasSpawnedBarrier && !NPC.AnyNPCs(barrierType))
                {
                    Mod.Logger.Warn($"Calamity Relics: HasSpawnedBarrier was true but no active barrier NPC was found. Clearing flag to allow runtime spawn.");
                    HasSpawnedBarrier = false;
                }

                if (!IsHouseUnlocked && !HasSpawnedBarrier && !DisablePersistentBarrierSpawnForDebug)
                {
                    bool anyPlayerNear = false;
                    int checkRadiusTiles = 200;
                    int spawnTileX = DraedonHouseRect.X + DoorOffsetX;
                    int spawnTileY = DraedonHouseRect.Y + DoorOffsetY;
                    for (int i = 0; i < Main.maxPlayers; i++)
                    {
                        Player pl = Main.player[i];
                        if (pl == null || !pl.active || pl.dead) continue;
                        var pt = pl.Center.ToTileCoordinates();
                        int dx = pt.X - spawnTileX;
                        int dy = pt.Y - spawnTileY;
                        if (dx * dx + dy * dy <= checkRadiusTiles * checkRadiusTiles)
                        {
                            anyPlayerNear = true;
                            break;
                        }
                    }

                    if (!anyPlayerNear)
                    {
                        return;
                    }
                    else
                    {
                        if (!NPC.AnyNPCs(barrierType))
                        {
                            int spawnX = (DraedonHouseRect.X + DoorOffsetX) * 16 + 8;
                            int spawnY = (DraedonHouseRect.Y + DoorOffsetY) * 16 + 8;

                            Mod.Logger.Info($"Calamity Relics: Spawning Draedon Barrier at X:{spawnX} Y:{spawnY}");
                            int spawnedIndex = NPC.NewNPC(
                                new Terraria.DataStructures.EntitySource_Misc("CalamityRelics: Draedon Barrier Persistence"),
                                spawnX,
                                spawnY,
                                barrierType
                            );
                            if (spawnedIndex >= 0 && spawnedIndex < Main.maxNPCs && Main.npc[spawnedIndex].active)
                            {
                                HasSpawnedBarrier = true;
                                Mod.Logger.Info($"Calamity Relics: Draedon Barrier spawned index:{spawnedIndex} at X:{Main.npc[spawnedIndex].Center.X} Y:{Main.npc[spawnedIndex].Center.Y}");
                            }
                            else
                            {
                                Mod.Logger.Warn($"Calamity Relics: Draedon Barrier spawn attempt returned index:{spawnedIndex}. Active check failed.");
                                Mod.Logger.Warn($"Calamity Relics: NPC.AnyNPCs(barrierType)={NPC.AnyNPCs(barrierType)}");
                            }
                        }
                    }
                }
            }
        }
    }
}