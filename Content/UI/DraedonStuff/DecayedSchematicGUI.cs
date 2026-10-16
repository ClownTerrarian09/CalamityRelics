using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace CalamityRelics.Content.UI.DraedonStuff
{
    public class DecayedSchematicGUI : OldDraedonLogGUI
    {
        public override int TotalPages => 1;
        public override string GetTextByPage()
        {
            return ("This wulfrum material is quite flimsy. This will not do. Technological advancement has still got a ways to go. In the meantime, I will improve these materials, just as how I will improve the world.");
        }
        public override Texture2D GetTextureByPage()
        {
            return ModContent.Request<Texture2D>("CalamityRelics/Content/UI/DraedonStuff/DecayedSchematic").Value;
        }
    }
}