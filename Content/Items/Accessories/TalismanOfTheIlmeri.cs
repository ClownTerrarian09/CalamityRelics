using System.Collections.Generic;
using System.Linq;
using CalamityRelics.Content.Projectiles.Friendly;
using CalamityRelics.ModUtils;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Items.Accessories
{
    public class TalismanOfTheIlmeri : ModItem
    {
        public override void SetDefaults()
        {
            Item.accessory = true;
            Item.width = 32;
            Item.height = 26;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<TalismanOfTheIlmeriPlayer>();
            modPlayer.hideMistVisual = hideVisual;
            modPlayer.equipped = true;
            modPlayer.sourceItem = Item;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            string key = CalamityMod.CalamityKeybinds.ArmorSetBonusHotKey.GetAssignedKeys()[0];
            var line = new TooltipLine(Mod, "Strike", $"Press {key} to discharge a massive amount of electricity")
            {
                OverrideColor = Color.Cyan
            };
            int index = tooltips.FindLastIndex(t => t.Mod == "Terraria" && t.Name.StartsWith("Tooltip"));
            if (index != -1)
                tooltips.Insert(index + 1, line);
            else
                tooltips.Add(line);
        }
    }

    public class TalismanOfTheIlmeriPlayer : ModPlayer
    {
        public bool equipped;
        public Item sourceItem;
        public List<Vector2> mistPositions = new();
        public int lightningDischargeCooldown;
        public bool hideMistVisual;
        private int mistSpawnTimer;
        private int strikesToSpawn;
        private bool chargeVisuals;
        
        public override void ResetEffects()
        {
            equipped = false;
            if (lightningDischargeCooldown > 0)
                lightningDischargeCooldown--;
            else if (lightningDischargeCooldown == 0)
            {
                lightningDischargeCooldown = -1;
                chargeVisuals = true;
            }
        }

        public override void PostUpdate()
        {
            if (!equipped || Player.whoAmI != Main.myPlayer)
                return;

            if (Player.velocity.Length() > 1f && --mistSpawnTimer <= 0)
            {
                mistSpawnTimer = 10;
                Projectile.NewProjectile(Player.GetSource_Accessory(sourceItem), Player.MountedCenter, Vector2.Zero,
                    ModContent.ProjectileType<IlmeriMist>(), 0, 0, Player.whoAmI, hideMistVisual ? 1 : 0);
            }

            if (strikesToSpawn > 0)
            {
                for(int i = 0; i < strikesToSpawn; i++)
                {
                    
                    
                    RefreshMistPositions();
                    Vector2[] positions = GetLine(Player.Center);
                    if (positions.Length < 2)
                        return;

                    Projectile proj = Projectile.NewProjectileDirect(Player.GetSource_Accessory(sourceItem), positions[0], Vector2.Zero, ModContent.ProjectileType<IlmeriLightning>(), 100, 1f, Player.whoAmI,Main.rand.Next(1 << 24));
                    IlmeriLightning modProj = (IlmeriLightning)proj.ModProjectile;
                    modProj.Waypoints = positions;
                    proj.netUpdate = true;
                }
                strikesToSpawn = 0;
            }

            if (chargeVisuals)
            {
                chargeVisuals = false;
                for (int d = 0; d < Main.rand.Next(8, 15); d++)
                {
                    Dust.NewDust(Player.position, Player.width,Player.height, DustID.Electric, Main.rand.Next(-2, 3), Main.rand.Next(-2, 3));
                }
            }
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (!equipped || lightningDischargeCooldown > 0)
                return;
            if (!CalamityMod.CalamityKeybinds.ArmorSetBonusHotKey.JustPressed)
                return;
            
            SoundEngine.PlaySound(new("CalamityRelics/Assets/Sounds/Item/LightningStrike"), Player.Center);
            
            lightningDischargeCooldown = 240;
            strikesToSpawn = Main.rand.Next(3,5);
            
        }

        private void RefreshMistPositions()
        {
            mistPositions.Clear();
            int mistType = ModContent.ProjectileType<IlmeriMist>();
            foreach (Projectile proj in Main.ActiveProjectiles)
            {
                if (proj.owner == Player.whoAmI && proj.type == mistType)
                    mistPositions.Add(proj.Center);
            }
        }

        public Vector2[] GetLine(Vector2 start)
        {
            float maxStepDist = Main.rand.Next(50, 180);
            List<Vector2> linePos = new() { start };
            List<Vector2> remaining = new(mistPositions);
            float maxDistSq = maxStepDist * maxStepDist;
            
            while (remaining.Count > 0)
            {
                Vector2 lastPos = linePos[^1];
                int closestIndex = -1;
                float closestDistSq = maxDistSq;

                for (int i = 0; i < remaining.Count; i++)
                {
                    float distSq = Vector2.DistanceSquared(lastPos, remaining[i]);
                    if (distSq < closestDistSq)
                    {
                        closestDistSq = distSq;
                        closestIndex = i;
                    }
                }

                if (closestIndex == -1)
                    break;
                linePos.Add(remaining[closestIndex] + Main.rand.NextVector2Circular(50,50));
                remaining.RemoveAt(closestIndex);
            }
            
            if (linePos.Count <= 3)
            {
                linePos.Clear();
                linePos.Add(start);
                Vector2 direction = Main.rand.NextVector2Circular(20, 20);
                int length = Main.rand.Next(5, 7);
                for (int l = 0; l < length; l++)
                {
                    
                    Vector2 lastPos = linePos[^1];
                    linePos.Add(lastPos + direction + Main.rand.NextVector2Circular(30,30));
                }
            }

            return linePos.ToArray();
        }
        
    }
}

