using Terraria;
using Terraria.ModLoader;
using CalamityRelics.Content.GlobalNPCs;

namespace CalamityRelics.Content.Buffs
{
    public class ConductiveBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true; 
            Main.pvpBuff[Type] = false; 
            Main.buffNoSave[Type] = true; 
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<GlobalConductive>().wulfrumConductiveDebuff = true;
        }
    }
}