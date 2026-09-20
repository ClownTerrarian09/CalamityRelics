using System;
using System.Collections.Generic;
using System.Linq;
using CalamityMod;
using CalamityRelics.Content.Buffs;
using CalamityRelics.Content.Projectiles.Friendly;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Mounts
{
	public class Mount_EnchantedBloom : ModMount
	{
		public override void SetStaticDefaults()
		{

			MountData.blockExtraJumps = true;
			MountData.constantJump = true;
			MountData.fallDamage = 0f;

			MountData.runSpeed = 20f;
			MountData.dashSpeed = 20f;
			MountData.swimSpeed = 10f;
			MountData.acceleration = 0.4f;
			MountData.jumpHeight = 10;
			MountData.jumpSpeed = 5f;
			MountData.spawnDust = ModContent.DustType<Dusts.EnchantedPetal>();
			MountData.buff = ModContent.BuffType<Buffs.EnchantedBloomBuff>();
			MountData.usesHover = true;
			MountData.flightTimeMax = int.MaxValue;
			MountData.fatigueMax = 320;
			MountData.totalFrames = 1;
			MountData.bodyFrame = 3;
			MountData.heightBoost = 50;
			MountData.playerYOffsets = new[] { 50 };
			MountData.playerHeadOffset = 50;
			MountData.standingFrameCount = 1;
			MountData.runningFrameCount = 1;
			MountData.flyingFrameCount = 1;
			MountData.inAirFrameCount = 1;
			MountData.idleFrameCount = 1;
			MountData.swimFrameCount = 1;
			
			MountData.emitsLight = true;


			if (!Main.dedServ)
			{
				MountData.textureWidth = MountData.backTexture.Width();
				MountData.textureHeight = MountData.backTexture.Height();
			}
		}
		

		public override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture,
			ref Texture2D glowTexture,
			ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation,
			ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow)
		{
			if(drawType >= 2)
				return false; 
			
			spriteEffects &= ~SpriteEffects.FlipHorizontally;
			rotation = 0.12f * MathF.Tanh(drawPlayer.velocity.X / 9f);
			return true;
		}
		
	}

	public class EnchantedBloomPlayer : ModPlayer
    {
	    private bool inEnchantedBloom;
	    private int vineTimer = 0;
	    private int leafTimer = 0;
	    public override void UpdateEquips()
	    {
		    inEnchantedBloom = Player.mount.Active && Player.mount.Type == ModContent.MountType<Mount_EnchantedBloom>() && Player.HasBuff(ModContent.BuffType<EnchantedBloomBuff>());
		    
		    if (inEnchantedBloom)
		    {
			    Player.mount.ResetFlightTime(Player.velocity.X);

			    DoWhipStuff();
			    DoAttackStuff();
			    if (!Main.dedServ)
			    {
				    if (Main.rand.Next((int)MathF.Ceiling(MathF.Max(2, 20f - Player.velocity.Length()))) == 0)
				    {
					    Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(40, 20), ModContent.DustType<Dusts.EnchantedPetal>());
				    }
			    }
			    
		    }
	    }

	    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
	    {
		    if(!inEnchantedBloom)
			    return;
		    drawInfo.isSitting = true;
	    }

	    public override void PostUpdate()
	    {
		    
		    if(!inEnchantedBloom || !Main.CurrentFrameFlags.AnyActiveBossNPC)
			    return;
		    
		    Player.eyeHelper.CurrentEyeFrame = PlayerEyeHelper.EyeFrame.EyeClosed;
		    
		    
	    }

	    public override void PreUpdate()
	    {
		    if(!inEnchantedBloom || !Main.CurrentFrameFlags.AnyActiveBossNPC)
			    return;
		    Player.controlMount = false;
		    Player.releaseMount = false;
		    Player.controlHook = false;
		    Player.releaseHook = false;
	    }


	    public override void PreUpdateMovement() {
		    if (!inEnchantedBloom)
			    return;

		    const float upSpeed = 20f;
		    const float downSpeed = 20f;
		    const float horizontalSpeed = 20f;
		    const float accel = 0.4f;

		    if (Player.controlUp || Player.controlJump) {
			    Player.velocity.Y = MathHelper.Max(Player.velocity.Y - accel, -upSpeed);
		    }
		    else if (Player.controlDown) {
			    Player.velocity.Y = MathHelper.Min(Player.velocity.Y + accel, downSpeed);
		    }
		    else {
			    Player.velocity.Y *= 0.85f;
			    if (System.Math.Abs(Player.velocity.Y) < 0.1f)
				    Player.velocity.Y = 0f;
		    }
		    
		    if (Player.controlLeft) {
			    Player.velocity.X = MathHelper.Max(Player.velocity.X - accel, -horizontalSpeed);
		    }
		    else if (Player.controlRight) {
			    Player.velocity.X = MathHelper.Min(Player.velocity.X + accel, horizontalSpeed);
		    }
		    else {
			    Player.velocity.X *= 0.85f;
			    if (System.Math.Abs(Player.velocity.X) < 0.1f)
				    Player.velocity.X = 0f;
		    }
		    
	    }
	    private void DoWhipStuff()
	    {
		    if (vineTimer <= 0 && Player.whoAmI == Main.myPlayer) {
			    Projectile best = null;
			    Vector2 bestDir = Vector2.Zero;
			    float bestTime = float.MaxValue;

			    foreach (Projectile proj in Main.projectile) {
				    if (!proj.active || !proj.hostile || proj.damage <= 0)
					    continue;
				    if (TryGetIntercept(Player, proj, out Vector2 dir, out float time) && time < bestTime) {
					    best = proj;
					    bestDir = dir;
					    bestTime = time;
				    }
			    }

			    if (best != null) {
				    vineTimer = Main.CurrentFrameFlags.AnyActiveBossNPC ? 15 : 40;
				    Projectile.NewProjectile(Player.GetSource_Accessory(Player.miscEquips[3]), Player.MountedCenter,
					    bestDir * 4f, ModContent.ProjectileType<EnchantedBloomVine>(), 0, 0, Player.whoAmI);
			    }
		    }
		    else if (vineTimer > 0) {
			    vineTimer--;
		    }
	    }

	    private void DoAttackStuff()
	    {
		    if (leafTimer <= 0)
		    {
			    float maxDist = Main.CurrentFrameFlags.AnyActiveBossNPC ? 1200f : 600f;
			    float dist = maxDist;
			    foreach (NPC npc in Main.npc)
			    {
				    if (!npc.active || !npc.CanBeChasedBy())
					    continue;
				    if (Vector2.Distance(npc.Center, Player.MountedCenter) < dist)
				    {
					    dist = Vector2.Distance(npc.Center, Player.MountedCenter);
				    }
				    
			    }
			    
			    if (dist < maxDist)
			    {
				    for (int i = 0; i < 5; i++)
				    {
					    SoundEngine.PlaySound(SoundID.Grass, Player.Center);
						int damage = Main.CurrentFrameFlags.AnyActiveBossNPC ? 1100 : 500;
				    
					    Vector2 offset = new Vector2(Main.rand.NextFloat(-30f, 15f), Main.rand.NextFloat(-5f, 5f));
					    Projectile.NewProjectile(Player.GetSource_Accessory(Player.miscEquips[3]), Player.MountedCenter + offset,
						    new Vector2(Main.rand.NextFloat(-4, 4), -12f) * Main.rand.NextFloat(0.9f, 1.1f), ModContent.ProjectileType<EnchantedLeaf>(),damage,2f,Player.whoAmI);
				    }
				    leafTimer = Main.CurrentFrameFlags.AnyActiveBossNPC ? 45 : 70;
			    }
		    }
		    else
		    {
			    leafTimer--;
		    }
		    
		    
	    }

	    public override bool CanUseItem(Item item)
	    {
		    return !(Main.CurrentFrameFlags.AnyActiveBossNPC && inEnchantedBloom);
	    }

	    private static bool TryGetIntercept(Player player, Projectile target, out Vector2 aimDir, out float hitTime) {
		    Vector2 relPos = target.Center - player.MountedCenter;
		    Vector2 relVel = target.velocity - player.velocity;

		    float t = 0f;
		    for (int i = 0; i < 4; i++) {
			    float d = (relPos + relVel * t).Length();
			    t = EnchantedBloomVine.FlyOutTime / MathHelper.Pi * MathF.Asin(MathF.Min(d / EnchantedBloomVine.MaxLength, 1f));
		    }

		    Vector2 predicted = relPos + relVel * t;
		    aimDir = predicted.SafeNormalize(Vector2.UnitX);
		    hitTime = t;
		    return predicted.Length() <= EnchantedBloomVine.MaxLength;
	    }
	    public override void ResetEffects()
	    {
		    base.ResetEffects();
		    inEnchantedBloom = false;
	    }
	    
	    
	    public override void HideDrawLayers(PlayerDrawSet drawInfo) {
		    if (Player.mount.Active && Player.mount.Type == ModContent.MountType<Mount_EnchantedBloom>()) {
			    // PlayerDrawLayers.ArmOverItem.Hide();
			    // PlayerDrawLayers.HandOnAcc.Hide();
			    // PlayerDrawLayers.Wings.Hide();
			    
		    }
	    }
    }
}

