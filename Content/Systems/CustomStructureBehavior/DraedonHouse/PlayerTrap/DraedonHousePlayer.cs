using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using CalamityRelics.Content.Systems.CustomStructureBehavior.DraedonHouse.RectangleDetection;

namespace CalamityRelics.Content.Systems.CustomStructureBehavior.DraedonHouse.PlayerTrap
{
    public class DraedonHousePlayer : ModPlayer
    {
        public override void PostUpdate()
        {
            if (DraedonHouseSystem.IsHouseUnlocked) return;

            // Check if the player is actively swinging a tool
            if (Player.whoAmI == Main.myPlayer && Player.ItemAnimationActive)
            {
                bool swingingPickaxe = Player.HeldItem.pick > 0;
                bool swingingHammer = Player.HeldItem.hammer > 0;

                if (swingingPickaxe || swingingHammer)
                {
                    int targetX = Player.tileTargetX;
                    int targetY = Player.tileTargetY;

                    if (DraedonHouseSystem.DraedonHouseRect.Contains(targetX, targetY))
                    {
                        Tile targetTile = Main.tile[targetX, targetY];
                        bool triggerShock = false;

                        if (swingingPickaxe && targetTile.HasTile && DraedonHouseSystem.ProtectedLabTiles.Contains(targetTile.TileType))
                        {
                            triggerShock = true;
                        }
                        else if (swingingHammer && targetTile.WallType > 0 && DraedonHouseSystem.ProtectedLabWalls.Contains(targetTile.WallType))
                        {
                            triggerShock = true;
                        }

                        if (triggerShock && Player.IsInTileInteractionRange(targetX, targetY, Terraria.DataStructures.TileReachCheckSettings.Simple))
                        {
                            ApplySecurityShock(targetX, targetY);
                        }
                    }
                }
            }
        }

        public void ApplySecurityShock(int tileX, int tileY)
        {
            Player.AddBuff(BuffID.Electrified, 180);

            Vector2 tileCenter = new Vector2(tileX * 16 + 8, tileY * 16 + 8);
            Vector2 knockbackDir = Player.Center - tileCenter;
            knockbackDir.Normalize();

            Player.velocity = knockbackDir * 12f;

            NetworkText deathMessage = NetworkText.FromLiteral(Player.name + " triggered Draedon's security countermeasures.");
            Player.Hurt(Terraria.DataStructures.PlayerDeathReason.ByCustomReason(deathMessage), 50, 0);

            Player.itemAnimation = 0;
            Player.itemTime = 0;
        }
    }
}