using CalamityRelics.Content.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Items.Utilities.StructureSpawner
{
    public class StructureSpawner : ModItem
    {
        private static readonly string[] SchematicNames =
        [
            "Draedon's House",
            "Cnidrion's Pond",
            "Draedon Garage"
        ];

        private int selectedSchematicIndex;

        public override string Texture => "Terraria/Images/Item_" + ItemID.Ruler;
        protected override bool CloneNewInstances => true;

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.rare = ItemRarityID.Cyan;
            Item.autoReuse = false;
            Item.consumable = false;
        }

        public override bool CanRightClick() => true;

        public override void RightClick(Player player)
        {
            selectedSchematicIndex = (selectedSchematicIndex + 1) % SchematicNames.Length;
            Main.NewText($"Structure selected: {SchematicNames[selectedSchematicIndex]}", Color.LimeGreen);
            Item.stack++;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return true;

            Point origin = new(Player.tileTargetX, Player.tileTargetY);
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                var packet = Mod.GetPacket();
                packet.Write((byte)1);
                packet.Write((byte)selectedSchematicIndex);
                packet.Write(origin.X);
                packet.Write(origin.Y);
                packet.Send();
                Main.NewText($"Requesting {SchematicNames[selectedSchematicIndex]} at {origin.X}, {origin.Y}.", Color.Cyan);
            }
            else if (StructureSpawnerSystem.TryPlaceSchematic(selectedSchematicIndex, origin))
            {
                Main.NewText($"Placed {SchematicNames[selectedSchematicIndex]} at {origin.X}, {origin.Y}.", Color.Cyan);
            }
            else
            {
                Main.NewText("Could not place the selected schematic. Check the mod log for details.", Color.OrangeRed);
            }

            return true;
        }
    }
}
