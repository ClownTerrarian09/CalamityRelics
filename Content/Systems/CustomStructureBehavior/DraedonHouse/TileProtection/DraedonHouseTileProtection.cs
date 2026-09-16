using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityRelics.Content.Systems.CustomStructureBehavior.DraedonHouse.RectangleDetection;
using CalamityRelics.Content.Systems.CustomStructureBehavior.DraedonHouse.PlayerTrap;

namespace CalamityRelics.Content.Systems.CustomStructureBehavior.DraedonHouse.TileProtection
{
    public class DraedonHouseTileProtection : GlobalTile
    {
        public override bool CanKillTile(int i, int j, int type, ref bool blockDamaged)
        {
            if (Terraria.Wiring.running)
            {
                return base.CanKillTile(i, j, type, ref blockDamaged);
            }

            if (!DraedonHouseSystem.IsHouseUnlocked &&
                DraedonHouseSystem.IsTileInHouse(i, j) &&
                DraedonHouseSystem.ProtectedLabTiles.Contains(type))
            {
                return false;
            }

            return base.CanKillTile(i, j, type, ref blockDamaged);
        }

        public override bool CanExplode(int i, int j, int type)
        {
            if (!DraedonHouseSystem.IsHouseUnlocked &&
                DraedonHouseSystem.IsTileInHouse(i, j) &&
                DraedonHouseSystem.ProtectedLabTiles.Contains(type))
            {
                return false;
            }
            return base.CanExplode(i, j, type);
        }

        public override void RightClick(int i, int j, int type)
        {
            if (!DraedonHouseSystem.IsHouseUnlocked &&
                DraedonHouseSystem.IsTileInHouse(i, j) &&
                DraedonHouseSystem.ProtectedLabTiles.Contains(type))
            {
                if (type == TileID.ClosedDoor || type == TileID.OpenDoor || type == TileID.Switches || type == TileID.Lever)
                {
                    return;
                }

                Main.LocalPlayer.chest = -1;
                Main.LocalPlayer.GetModPlayer<DraedonHousePlayer>().ApplySecurityShock(i, j);
            }
        }
    }
}