using CalamityRelics.Content.Projectiles.Friendly;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Items.Reworks.Weapons
{
	public class Grenade : GlobalItem
	{
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.type == ItemID.Grenade;
        }

        public override void SetDefaults(Item item)
        {
            if (item.type != ItemID.Grenade) return;

            item.width = 24;
            item.height = 24;
            item.useTime = 40;
            item.useAnimation = 40;
            item.channel = true;
            item.autoReuse = false;

            item.shoot = ModContent.ProjectileType<GrenadeProj>();
            item.shootSpeed = 0;
        }

        public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D texture = ModContent.Request<Texture2D>("CalamityRelics/Content/Items/Reworks/Weapons/Grenade").Value;

            spriteBatch.Draw(texture, position, null, Color.White, 0, texture.Size() / 2, scale, SpriteEffects.None, 1);

            return false;
        }

        public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = ModContent.Request<Texture2D>("CalamityRelics/Content/Items/Reworks/Weapons/Grenade").Value;

            spriteBatch.Draw(texture, item.Center - Main.screenPosition, null, lightColor, rotation, texture.Size() / 2, scale, SpriteEffects.None, 1);

            return false;
        }
	}

    public class StickyGrenade : GlobalItem
    {
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.type == ItemID.StickyGrenade;
        }

        public override void SetDefaults(Item item)
        {
            if (item.type != ItemID.StickyGrenade) return;

            item.width = 24;
            item.height = 24;
            item.useTime = 40;
            item.useAnimation = 40;
            item.channel = true;
            item.autoReuse = false;

            item.shoot = ModContent.ProjectileType<StickyGrenadeProj>();
            item.shootSpeed = 0;
        }

        public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D texture = ModContent.Request<Texture2D>("CalamityRelics/Content/Items/Reworks/Weapons/StickyGrenade").Value;

            spriteBatch.Draw(texture, position, null, Color.White, 0, texture.Size() / 2, scale, SpriteEffects.None, 1);

            return false;
        }

        public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = ModContent.Request<Texture2D>("CalamityRelics/Content/Items/Reworks/Weapons/StickyGrenade").Value;

            spriteBatch.Draw(texture, item.Center - Main.screenPosition, null, lightColor, rotation, texture.Size() / 2, scale, SpriteEffects.None, 1);

            return false;
        }
    }
}
