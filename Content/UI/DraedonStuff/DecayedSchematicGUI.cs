using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace CalamityRelics.Content.UI.DraedonStuff
{
    public class DecayedSchematicGUI : OldDraedonLogGUI
    {
        public override int TotalPages => 1;
        public override string GetTextByPage()
        {
            return "Amogus";
        }
        public override Texture2D GetTextureByPage()
        {
            return ModContent.Request<Texture2D>("CalamityRelics/Content/UI/DraedonStuff/Amongus").Value;
        }
    }
}