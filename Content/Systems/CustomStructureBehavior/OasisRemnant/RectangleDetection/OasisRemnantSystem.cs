using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.DataStructures;
using CalamityRelics.Content.NPCs.Bosses.Cnidrion;

namespace CalamityRelics.Content.Systems.CustomStructureBehavior.OasisRemnant.RectangleDetection
{
    public class OasisRemnantSystem : ModSystem
    {
        public static Rectangle OasisRemnantRect = Rectangle.Empty;
        public static bool DisablePersistentCnidrionSpawnForDebug = false;
        public static int PondSpawnOffsetX = 20;
        public static int PondSpawnOffsetY = 15;
        public static int WorldDay = 0;
        private static bool lastDayFlag = false;
        public static int LastCnidrionKillDay = int.MinValue / 2;
        public static int SpawnTimeStartTicks = 12600;
        public static int SpawnTimeEndTicks = 37800;
        public static int MinPlayerDistanceTiles = 100;
        public static bool HasSpawnedCnidrion = false;
        private bool bossWasPresentLastTick = false;

        public override void ClearWorld()
        {
            OasisRemnantRect = Rectangle.Empty;
            WorldDay = 0;
            lastDayFlag = Main.dayTime;
            LastCnidrionKillDay = int.MinValue / 2;
            HasSpawnedCnidrion = false;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            tag["OasisRemnantRectX"] = OasisRemnantRect.X;
            tag["OasisRemnantRectY"] = OasisRemnantRect.Y;
            tag["OasisRemnantRectW"] = OasisRemnantRect.Width;
            tag["OasisRemnantRectH"] = OasisRemnantRect.Height;
            tag["OasisPondOffsetX"] = PondSpawnOffsetX;
            tag["OasisPondOffsetY"] = PondSpawnOffsetY;
            tag["Oasis_WorldDay"] = WorldDay;
            tag["Oasis_LastCnidrionKillDay"] = LastCnidrionKillDay;
            tag["Oasis_SpawnTimeStartTicks"] = SpawnTimeStartTicks;
            tag["Oasis_SpawnTimeEndTicks"] = SpawnTimeEndTicks;
            tag["Oasis_MinPlayerDistanceTiles"] = MinPlayerDistanceTiles;
            tag["Oasis_HasSpawnedCnidrion"] = HasSpawnedCnidrion;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            if (tag.ContainsKey("OasisRemnantRectX"))
            {
                OasisRemnantRect = new Rectangle(
                    tag.GetInt("OasisRemnantRectX"),
                    tag.GetInt("OasisRemnantRectY"),
                    tag.GetInt("OasisRemnantRectW"),
                    tag.GetInt("OasisRemnantRectH")
                );
            }
            else
            {
                OasisRemnantRect = Rectangle.Empty;
            }

            if (OasisRemnantRect != Rectangle.Empty)
            {
                Mod.Logger.Info($"Calamity Relics: Loaded OasisRemnantRect X:{OasisRemnantRect.X} Y:{OasisRemnantRect.Y} W:{OasisRemnantRect.Width} H:{OasisRemnantRect.Height}");
            }

            if (tag.ContainsKey("OasisPondOffsetX")) PondSpawnOffsetX = tag.GetInt("OasisPondOffsetX");
            if (tag.ContainsKey("OasisPondOffsetY")) PondSpawnOffsetY = tag.GetInt("OasisPondOffsetY");

            if (tag.ContainsKey("Oasis_WorldDay")) WorldDay = tag.GetInt("Oasis_WorldDay");
            lastDayFlag = Main.dayTime;
            if (tag.ContainsKey("Oasis_LastCnidrionKillDay")) LastCnidrionKillDay = tag.GetInt("Oasis_LastCnidrionKillDay");
            if (tag.ContainsKey("Oasis_SpawnTimeStartTicks")) SpawnTimeStartTicks = tag.GetInt("Oasis_SpawnTimeStartTicks");
            if (tag.ContainsKey("Oasis_SpawnTimeEndTicks")) SpawnTimeEndTicks = tag.GetInt("Oasis_SpawnTimeEndTicks");
            if (tag.ContainsKey("Oasis_MinPlayerDistanceTiles")) MinPlayerDistanceTiles = tag.GetInt("Oasis_MinPlayerDistanceTiles");
            if (tag.ContainsKey("Oasis_HasSpawnedCnidrion")) HasSpawnedCnidrion = tag.GetBool("Oasis_HasSpawnedCnidrion");
        }

        public override void PostUpdateWorld()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            if (lastDayFlag != Main.dayTime)
            {
                if (Main.dayTime)
                {
                    WorldDay++;
                }
                lastDayFlag = Main.dayTime;
            }

            if (OasisRemnantRect != Rectangle.Empty)
            {
                Mod.Logger.Debug($"Calamity Relics: OasisRemnantRect present X:{OasisRemnantRect.X} Y:{OasisRemnantRect.Y} W:{OasisRemnantRect.Width} H:{OasisRemnantRect.Height}");
                int cnidrionType = ModContent.NPCType<Cnidrion>();
                bool cnidrionPresent = NPC.AnyNPCs(cnidrionType);

                if (bossWasPresentLastTick && !cnidrionPresent)
                {
                    LastCnidrionKillDay = WorldDay;
                    HasSpawnedCnidrion = false;
                }

                bossWasPresentLastTick = cnidrionPresent;

                if (!cnidrionPresent)
                {
                    int cnidrionTypeCheck = ModContent.NPCType<Cnidrion>();
                    if (HasSpawnedCnidrion && !NPC.AnyNPCs(cnidrionTypeCheck))
                    {
                        Mod.Logger.Warn("Calamity Relics: HasSpawnedCnidrion was true but no active Cnidrion NPC found. Clearing flag to allow runtime spawn.");
                        HasSpawnedCnidrion = false;
                    }

                    if (HasSpawnedCnidrion) return;
                    int spawnX = (OasisRemnantRect.X + PondSpawnOffsetX) * 16 + 8;
                    int spawnY = (OasisRemnantRect.Y + PondSpawnOffsetY) * 16 + 8;

                    if (WorldDay <= LastCnidrionKillDay)
                    {
                        // Cnidrion hasn't defeated yet, skip auto-respawn.
                        return;
                    }
                    else
                    {
                        if (Main.dayTime && Main.time >= SpawnTimeStartTicks && Main.time <= SpawnTimeEndTicks)
                        {
                            Vector2 spawnPos = new Vector2(spawnX, spawnY);
                            float minDistSq = (MinPlayerDistanceTiles * 16f) * (MinPlayerDistanceTiles * 16f);
                            bool anyPlayerNear = false;
                            foreach (Player p in Main.player)
                            {
                                if (p == null || !p.active || p.dead) continue;
                                if (Vector2.DistanceSquared(p.Center, spawnPos) <= minDistSq)
                                {
                                    anyPlayerNear = true;
                                    break;
                                }
                            }

                            if (!anyPlayerNear && !DisablePersistentCnidrionSpawnForDebug)
                            {
                                bool playerNearby = false;
                                int checkRadius = 200;
                                int spawnTileX = (OasisRemnantRect.X + PondSpawnOffsetX);
                                int spawnTileY = (OasisRemnantRect.Y + PondSpawnOffsetY);
                                for (int pi = 0; pi < Main.maxPlayers; pi++)
                                {
                                    Player pl = Main.player[pi];
                                    if (pl == null || !pl.active || pl.dead) continue;
                                    var pt = pl.Center.ToTileCoordinates();
                                    int dx = pt.X - spawnTileX;
                                    int dy = pt.Y - spawnTileY;
                                    if (dx * dx + dy * dy <= checkRadius * checkRadius)
                                    {
                                        playerNearby = true;
                                        break;
                                    }
                                }

                                if (playerNearby)
                                {
                                    Mod.Logger.Info($"Calamity Relics: Spawning Cnidrion at X:{spawnX} Y:{spawnY}");
                                    int spawnedIndex = NPC.NewNPC(
                                        new EntitySource_Misc("CalamityRelics: Cnidrion Persistence"),
                                        spawnX,
                                        spawnY,
                                        cnidrionType
                                    );
                                    if (spawnedIndex >= 0 && spawnedIndex < Main.maxNPCs && Main.npc[spawnedIndex].active)
                                    {
                                        HasSpawnedCnidrion = true;
                                        Mod.Logger.Info($"Calamity Relics: Cnidrion spawned index:{spawnedIndex} at X:{Main.npc[spawnedIndex].Center.X} Y:{Main.npc[spawnedIndex].Center.Y}");
                                    }
                                    else
                                    {
                                        Mod.Logger.Warn($"Calamity Relics: Cnidrion spawn attempt returned index:{spawnedIndex}. Active check failed.");
                                        Mod.Logger.Warn($"Calamity Relics: NPC.AnyNPCs(cnidrionType)={NPC.AnyNPCs(cnidrionType)}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
