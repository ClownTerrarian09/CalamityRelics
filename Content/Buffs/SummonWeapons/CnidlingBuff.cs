using Terraria;
using Terraria.ModLoader;
using CalamityRelics.Content.Projectiles.Summon;

namespace CalamityRelics.Content.Buffs.SummonWeapons
{
    public class CnidlingBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Cnidling>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
                return;
            }
            player.DelBuff(buffIndex);
            buffIndex--;
        }
    }
}