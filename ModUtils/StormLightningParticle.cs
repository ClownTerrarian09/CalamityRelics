using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.Utilities;
using CalamityRelics.ModUtils;
using Terraria;
using Terraria.Graphics.Renderers;

namespace CalamityRelics.ModUtils;
// FROM TERRARIA 1.4.5 DECOMPILE modified to work in 1.4.4.9
public class StormLightningParticle : IPooledParticle, IParticle
{
	public Color Color;

	public float Width;

	private List<LightningGenerator.Bolt> bolts = new List<LightningGenerator.Bolt>();

	public int AnchorToPlayerHand = -1;

	private float Intensity = 1f;

	private bool SteadyLight;

	private int _lifeTimeCounted;

	private int _lifeTimeTotal;

	private StormLightningDrawer.AnimParams _animParams;

	public bool ShouldBeRemovedFromRenderer { get; private set; }

	private LightningGenerator.Bolt MainBolt => bolts.Last();

	public Vector2 EndPosition => MainBolt.positions.Last();

	public bool IsRestingInPool { get; private set; }
	

	public void RestInPool()
	{
		IsRestingInPool = true;
	}

	public virtual void FetchFromPool()
	{
		AnchorToPlayerHand = -1;
		_lifeTimeCounted = 0;
		_lifeTimeTotal = 0;
		IsRestingInPool = false;
		ShouldBeRemovedFromRenderer = false;
		bolts.Clear();
	}

	public void Prepare(LightningGenerator generator, uint seed, Vector2 sourcePosition, Vector2 targetPosition, int lifeTimeTotal, Color color, float width, float intensity, StormLightningDrawer.AnimParams animParams, int anchorToPlayer, bool steadyLight)
	{
		_Prepare(generator, seed, targetPosition, targetPosition - sourcePosition, lifeTimeTotal, color, width, intensity, animParams, anchorToPlayer, steadyLight);
	}

	public void Prepare(LightningGenerator generator, uint seed, Vector2 targetPosition, int lifeTimeTotal, Color color, float width, StormLightningDrawer.AnimParams animParams)
	{
		_Prepare(generator, seed, targetPosition, null, lifeTimeTotal, color, width, 1f, animParams, null, steadyLight: false);
	}

	private void _Prepare(LightningGenerator generator, uint seed, Vector2 targetPosition, Vector2? fromDirection, int lifeTimeTotal, Color color, float width, float intensity, StormLightningDrawer.AnimParams animParams, int? anchorToPlayerHand, bool steadyLight)
	{
		AnchorToPlayerHand = anchorToPlayerHand ?? (-1);
		Color = color;
		Width = width;
		Intensity = intensity;
		SteadyLight = steadyLight;
		_animParams = animParams;
		_lifeTimeTotal = lifeTimeTotal;
		LightningGenerator.Bolt mainBolt = generator.Generate(bolts, seed, targetPosition, fromDirection);
		EmitSpawnDust(seed, color, mainBolt);
	}

	private void EmitSpawnDust(uint seed, Color color, LightningGenerator.Bolt mainBolt)
	{
		LightningGenerator.LCG32Random lCG32Random = new LightningGenerator.LCG32Random(seed);
		short type = 226;
		float num = 1f;
		float num2 = 0.4f;
		int num3 = 6;
		float num4 = 10f;
		bool flag = AnchorToPlayerHand >= 0;
		if (flag)
		{
			num4 = Utils.Remap(Vector2.Distance(mainBolt.positions.Last(), mainBolt.positions.First()), 100f, 500f, 2f, 5f);
			type = 278;
			num2 = 0.7f;
			color *= 1.2f;
			num = 1.2f;
			num3 = 2;
		}
		int maxValue = (int)Math.Ceiling((float)mainBolt.positions.Length / num4);
		for (int i = 5; i < mainBolt.positions.Length - 5; i++)
		{
			if (lCG32Random.Next(maxValue) == 0)
			{
				Vector2 position = mainBolt.positions[i];
				Vector2 velocity = Vector2.UnitY;
				if (mainBolt.rotations != null)
				{
					velocity = -mainBolt.rotations[i].ToRotationVector2();
				}
				Dust dust = Dust.NewDustPerfect(position, type);
				dust.color = color;
				dust.velocity = velocity;
				dust.velocity *= (3f + lCG32Random.NextFloat() * 6.5f) * num;
				dust.fadeIn = 0f;
				dust.scale = num2 + lCG32Random.NextFloat() * 0.5f;
				dust.noGravity = true;
				if (SteadyLight)
				{
					dust.noLight = true;
				}
				dust.position -= dust.velocity * num3;
				if (AnchorToPlayerHand >= 0 && lCG32Random.Next(mainBolt.positions.Length) >= i)
				{
					dust.customData = Main.player[AnchorToPlayerHand];
				}
				Dust dust2 = Dust.CloneDust(dust);
				dust2.velocity *= 0.5f;
				dust2.scale -= 0.3f;
				if (flag)
				{
					dust2.velocity = dust.velocity;
					dust2.scale = dust.scale * 0.66f;
					dust2.color = new Color(255, 255, 255, 0);
				}
			}
		}
	}

	public static Vector2 GetPlayerAnchorPos(Player player)
	{
		return player.RotatedRelativePoint(player.HandPosition ?? player.MountedCenter);
	}

	public void Update(ref ParticleRendererSettings settings)
	{
		bool flag = true;
		Vector2 vector = MainBolt.positions.First();
		Vector2 movementVector = Vector2.Zero;
		if (AnchorToPlayerHand >= 0)
		{
			Player obj = Main.player[AnchorToPlayerHand];
			vector = GetPlayerAnchorPos(obj);
			movementVector = obj.velocity;
			flag = false;
		}
		Color color = Color;
		float num = (float)_lifeTimeCounted / (float)_lifeTimeTotal;
		float num2 = Utils.Remap(num, 0f, 0.4f, 1f, 0f);
		if (SteadyLight)
		{
			Vector2[] positions = MainBolt.positions;
			Vector3 rgb = Color.ToVector3() * Utils.Remap(num, 0f, 0.3f, 0f, 1f) * Utils.Remap(num, 0.7f, 1f, 1f, 0f);
			for (int i = 0; i < positions.Length; i += 20)
			{
				Lighting.AddLight(positions[i], rgb);
			}
		}
		if (flag)
		{
			if (num < 0.5f)
			{
				for (int j = 0; j < 3; j++)
				{
					if (Main.rand.Next(4) == 0 && !(Main.rand.NextFloat() > num2 * 0.13f))
					{
						Dust dust = Dust.NewDustDirect(vector, 16, 16, 306, 0f, 0f, 0, new Color(color.R, color.G, color.B, 0));
						dust.velocity = new Vector2(0f, -4f).RotatedByRandom(1.5707963705062866) * (0.5f + 0.2f * Main.rand.NextFloatDirection());
						dust.scale = 1.8f;
						dust.fadeIn = 0f;
						dust.noGravity = Main.rand.Next(3) != 0;
						dust.noLight = true;
						Dust dust2 = Dust.CloneDust(dust);
						dust2.color = new Color(255, 255, 255, 0);
						dust2.scale = 1.3f;
					}
				}
				for (int k = -1; k <= 1; k += 2)
				{
					if (Main.rand.Next(4) == 0 && !(Main.rand.NextFloat() > num2 * 0.2f))
					{
						Dust dust3 = Dust.NewDustPerfect(vector, 306, new Vector2(0f, -4f).RotatedBy(MathF.PI / 4f * (float)k * 1f));
						dust3.color = new Color(color.R, color.G, color.B, 0);
						dust3.scale = 1.8f;
						dust3.fadeIn = 0f;
						dust3.noGravity = Main.rand.Next(3) != 0;
						dust3.noLight = true;
						Dust dust4 = Dust.CloneDust(dust3);
						dust4.color = new Color(255, 255, 255, 0);
						dust4.scale = 1.3f;
					}
				}
				for (int l = 0; l < 2; l++)
				{
					if (Main.rand.Next(4) == 0 && !(Main.rand.NextFloat() > 0.2f))
					{
						Dust dust5 = Dust.NewDustPerfect(vector, 226);
						dust5.color = color;
						dust5.customData = dust5.color;
						dust5.velocity *= 1f + Main.rand.NextFloat() * 2.5f;
						dust5.velocity += new Vector2(0f, -2f);
						dust5.fadeIn = 0f;
						dust5.scale = 0.4f + Main.rand.NextFloat() * 0.5f;
						dust5.velocity.X *= 2f;
						dust5.velocity = Main.rand.NextVector2Circular(3f, 2f) + new Vector2(0f, -2f);
						dust5.noLight = true;
						dust5.position -= dust5.velocity * 3f;
					}
				}
			}
		}
		if (++_lifeTimeCounted >= _lifeTimeTotal)
		{
			ShouldBeRemovedFromRenderer = true;
		}
	}

	public void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
	{
		_ = MainBolt;
		if (AnchorToPlayerHand >= 0)
		{
			Vector2 vector = MainBolt.positions.First();
			Vector2 value = MainBolt.positions.Last();
			float num = Vector2.DistanceSquared(vector, value);
			Vector2 vector2 = GetPlayerAnchorPos(Main.player[AnchorToPlayerHand]) - vector;
			foreach (LightningGenerator.Bolt bolt in bolts)
			{
				for (int i = 0; i < bolt.positions.Length; i++)
				{
					float num2 = MathHelper.Clamp(1f - Vector2.DistanceSquared(bolt.positions[i], vector) / num, 0f, 1f);
					num2 *= num2;
					bolt.positions[i] += num2 * vector2;
				}
				LightningGenerator.CalcRotations(bolt.positions, bolt.rotations);
			}
		}
		StormLightningDrawer stormLightningDrawer = default;
		float progress = (float)_lifeTimeCounted / (float)_lifeTimeTotal;
		foreach (LightningGenerator.Bolt bolt2 in bolts)
		{
			float num3 = (bolt2.IsMainBolt ? 1f : (0.5f * (float)Math.Pow(0.8, bolt2.forkDepth - 1)));
			num3 *= Intensity;
			stormLightningDrawer.Draw(bolt2.positions, bolt2.rotations, Width, Color, progress, bolt2.IsMainBolt, bolt2.progressRange, num3, _animParams);
		}
	}
}
