using CalamityRelics.Content.NPCs.DraedonHouseBarrier;
using CalamityRelics.Content.Projectiles.Environment.ClickEffectProj;
using CalamityRelics.Content.Projectiles.Environment.SecurityControllerProj;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Items.Utilities.SecurityController
{
    public class SecurityController : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 120;
            Item.useAnimation = 120;
            Item.autoReuse = false;
            Item.consumable = false;
            Item.UseSound = SoundID.Item92;

            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<SecurityControllerProj>();
            Item.shootSpeed = 1f;
        }

        public override void HoldItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                if (IsHoveringOverBarrier(out _))
                {
                    player.cursorItemIconEnabled = true;
                    player.cursorItemIconID = Type;
                }
            }
        }

        public override bool CanUseItem(Player player)
        {
            if (IsHoveringOverBarrier(out int npcIndex))
            {
                Item.useTime = 300;
                Item.useAnimation = 300;
                Item.UseSound = SoundID.Item93;

                if (player.whoAmI == Main.myPlayer)
                {
                    NPC barrier = Main.npc[npcIndex];
                    if (barrier.ModNPC is DraedonBarrierNPC barrierNPC)
                    {
                        barrierNPC.StartUnlockSequence();

                        Projectile.NewProjectile(
                            player.GetSource_ItemUse(Item),
                            player.Top,
                            Vector2.Zero,
                            ModContent.ProjectileType<ClickEffectProj>(),
                            0,
                            0,
                            player.whoAmI
                        );
                    }
                }
            }
            else
            {
                Item.useTime = 120;
                Item.useAnimation = 120;
                Item.UseSound = SoundID.Item92;
            }

            return true;
        }

        private bool IsHoveringOverBarrier(out int npcIndex)
        {
            npcIndex = -1;
            Vector2 mouseWorld = Main.MouseWorld;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];

                if (npc.active && npc.type == ModContent.NPCType<DraedonBarrierNPC>() && npc.Hitbox.Contains(mouseWorld.ToPoint()))
                {
                    if (Vector2.Distance(Main.LocalPlayer.Center, npc.Center) < 200f)
                    {
                        npcIndex = i;
                        return true;
                    }
                }
            }
            return false;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Player player = Main.LocalPlayer;

            string texturePath = Texture;

            if (player.HeldItem.type == Type && player.itemTime > 0)
            {
                texturePath += "Press";
            }

            Texture2D drawTexture = ModContent.Request<Texture2D>(texturePath).Value;
            spriteBatch.Draw(drawTexture, position, null, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);

            return false;
        }
    }
}