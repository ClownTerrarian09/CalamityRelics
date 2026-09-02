using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityRelics.Content.Tiles{
    internal class RustedCodebreakerFurnitureOff : ModTile{
        public override void SetStaticDefaults(){
            Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			DustType = DustID.Silt;
			AddMapEntry(new Color(70, 60, 50)); 

            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.Width = 5;
            TileObjectData.newTile.Height = 4;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 18];
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.Table | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.Origin = new Point16(2, 3);
            TileObjectData.addTile(Type);
        }
        public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}

    }
}