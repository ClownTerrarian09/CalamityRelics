
using Microsoft.Xna.Framework;

namespace CalamityRelics.HelperScripts
{
    // Created by ELITEGameDev to help with some annoyances I have with XNA's rectangle initialization. 
    // Having otherwise limited control of rectangle drawing means less capability to align elements better to eachother without making things jank or unreliable. This aims to fix that.
    public static class RectangleHelper 
    {
        public enum Anchor
        {
            TOP_LEFT,
            TOP_MID,
            TOP_RIGHT,
            MID_LEFT,
            CENTER,
            MID_RIGHT,
            BOT_LEFT,
            BOT_MID,
            BOT_RIGHT
        }

        public static Vector2 AnchorToPivot(Anchor anchor)
        {
            var pivot = anchor switch
            {
                Anchor.TOP_LEFT => new Vector2(0, 1),
                Anchor.TOP_MID => new Vector2(0.5f, 1),
                Anchor.TOP_RIGHT => new Vector2(1, 1),
                Anchor.MID_LEFT => new Vector2(0.5f, 0.5f),
                Anchor.CENTER => new Vector2(0.5f, 0.5f),
                Anchor.MID_RIGHT => new Vector2(0.5f, 0.5f),
                Anchor.BOT_MID => new Vector2(0.5f, 0),
                Anchor.BOT_RIGHT => new Vector2(1, 0),
                // Anchor.BOT_LEFT:
                _ => new Vector2(0, 0),
            };
            return pivot;
            
        }

        /// <summary>
        /// Creates a rectangle with the position and size anchored to your choosing, not limited to bottom left alignment unlike the default Rectangle initialization 
        /// </summary>
        /// <param name="anchor">Choose between 9 anchor settings to base your rectangle bounds from. If you would like to use a custom pivot option, use the Vector 2 overload in place of this Anchor type parameter</param>
        /// <param name="position"></param>
        /// <param name="size"></param>
        /// <returns>A new AABB rectangle to be used for collisions or drawing sprites</returns>
        public static Rectangle CreateAnchored(Anchor anchor, Vector2 position, Vector2 size)
        {
            Vector2 pivot = AnchorToPivot(anchor);
            
            float x = position.X - (size.X * pivot.X);
            float y = position.Y - (size.Y * pivot.Y);

            
            return new Rectangle((int)x, (int)y, (int)size.X, (int)size.Y);
        }

        /// <summary>
        /// Creates a rectangle with the position and size anchored to your choosing, not limited to bottom left alignment unlike the default Rectangle initialization 
        /// </summary>
        /// <param name="anchor">Choose between 9 anchor settings to base your rectangle bounds from. If you would like to use a custom pivot option, use the Vector 2 overload in place of this Anchor type parameter</param>
        /// <param name="positionX"></param>
        /// <param name="positionY"></param>
        /// <param name="sizeX"></param>
        /// <param name="sizeY"></param>
        /// <returns>A new AABB rectangle to be used for collisions or drawing sprites</returns>
        public static Rectangle CreateAnchored(Anchor anchor, float positionX, float positionY, float sizeX, float sizeY)
        {
            Vector2 pivot = AnchorToPivot(anchor);
            
            float x = positionX - (sizeX * pivot.X);
            float y = positionY - (sizeY * pivot.Y);
            
            return new Rectangle((int)x, (int)y, (int)sizeX, (int)sizeY);
        }

        /// Creates a rectangle with the position and size anchored to your choosing, not limited to bottom left alignment unlike the default Rectangle initialization 
        /// </summary>
        /// <param name="pivot">The custom pivot for your rectangle bounds to be based from</param>
        /// <param name="position"></param>
        /// <param name="size"></param>
        /// <returns>A new AABB rectangle to be used for collisions or drawing sprites</returns>
        public static Rectangle CreateAnchored(Vector2 pivot, Vector2 position, Vector2 size)
        {
            pivot = Vector2.Clamp(pivot, Vector2.Zero, Vector2.One);
            
            float x = position.X - (size.X * pivot.X);
            float y = position.Y - (size.Y * pivot.Y);

            
            return new Rectangle((int)x, (int)y, (int)size.X, (int)size.Y);
        }
    }
}