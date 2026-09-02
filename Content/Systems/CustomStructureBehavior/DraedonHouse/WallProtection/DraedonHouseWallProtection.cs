using Terraria;
using Terraria.ModLoader;
using CalamityRelics.Content.Systems.CustomStructureBehavior.DraedonHouse.RectangleDetection;

namespace CalamityRelics.Content.Systems.CustomStructureBehavior.DraedonHouse.WallProtection
{
    public class DraedonHouseWallProtection : GlobalWall
    {
        public override void KillWall(int i, int j, int type, ref bool fail)
        {
            if (!DraedonHouseSystem.IsHouseUnlocked &&
                DraedonHouseSystem.DraedonHouseRect.Contains(i, j) &&
                DraedonHouseSystem.ProtectedLabWalls.Contains(type))
            {
                fail = true;    
            }
        }

        public override bool CanExplode(int i, int j, int type)
        {
            if (!DraedonHouseSystem.IsHouseUnlocked &&
                DraedonHouseSystem.DraedonHouseRect.Contains(i, j) &&
                DraedonHouseSystem.ProtectedLabWalls.Contains(type))
            {
                return false;
            }
            return base.CanExplode(i, j, type);
        }
    }
}