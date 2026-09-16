using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Items
{
	// This is a basic item template.
	// Please see tModLoader's ExampleMod for every other example:
	// https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
	public class CaudalGeode : ModItem
	{
		// The Display Name and Tooltip of this item can be edited in the 'Localization/en-US_Mods.CalamityRelics.hjson' file.
		public override void SetDefaults()
		{
            Item.width = 32;
            Item.height = 26;
			Item.rare = ItemRarityID.Green;
			Item.maxStack = 9999;
        }
	}
}
