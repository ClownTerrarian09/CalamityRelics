using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CalamityRelics.Content.Systems.CustomStructureBehavior.DraedonGarage
{
    public class DraedonGarageSystem : ModSystem
    {
        public static Rectangle DraedonGarageRect = Rectangle.Empty;

        public override void ClearWorld()
        {
            DraedonGarageRect = Rectangle.Empty;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            tag["DraedonGarageX"] = DraedonGarageRect.X;
            tag["DraedonGarageY"] = DraedonGarageRect.Y;
            tag["DraedonGarageW"] = DraedonGarageRect.Width;
            tag["DraedonGarageH"] = DraedonGarageRect.Height;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            if (tag.ContainsKey("DraedonGarageX"))
            {
                DraedonGarageRect = new Rectangle(
                    tag.GetInt("DraedonGarageX"),
                    tag.GetInt("DraedonGarageY"),
                    tag.GetInt("DraedonGarageW"),
                    tag.GetInt("DraedonGarageH")
                );
            }
        }
    }
}
