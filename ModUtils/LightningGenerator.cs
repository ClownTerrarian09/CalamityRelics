using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Utilities;
using Terraria.Utilities.Terraria.Utilities;

// FROM TERRARIA 1.4.5 DECOMPILE modified to work in 1.4.4.9
namespace CalamityRelics.ModUtils;

public class LightningGenerator
{
	public class Bolt
	{
		public Vector2[] positions;

		public float[] rotations;

		public FloatRange progressRange;

		public int forkDepth;

		public bool collidedWithTile;

		public bool IsMainBolt => forkDepth == 0;
	}

	public bool SolidTileCollision;

	public float SourceRotationLimit = MathF.PI / 9f;

	public float Length = 1000f;

	public float RotationStrength;

	public int StepSize;

	public int Layers;

	public float LayerStrengthFactor;

	public float PerpendicularDeviationFactor;

	public float ReduceRandomnessAfter;

	public float ForkGenerationThresholdAngleFraction;

	public float ForkReflectAngleMultiplier;

	public float ForkRotationStrengthMultiplier;

	public float ForkStepSizeMultiplier;

	public float ForkLengthMultiplier;

	public int MaxForksPerBolt;

	public int MaxForkDepth;

	public FloatRange ForkProgressRange;
	
	public struct LCG32Random
	{
		public uint state;
		public LCG32Random(uint seed) { state = seed; }
        
		private uint Advance() => state = state * 1664525u + 1013904223u;
		public double NextDouble() => Advance() * (1.0 / 4294967296.0);
		public float NextFloat() => (float)NextDouble();
		public int Next(int maxValue) => (int)((ulong)Advance() * (ulong)maxValue >> 32);
	}
	
	public static LightningGenerator StormLightning = new LightningGenerator
	{
		SourceRotationLimit = MathF.PI / 9f,
		Length = 1000f,
		RotationStrength = 0.9f,
		StepSize = 8,
		Layers = 4,
		LayerStrengthFactor = 1.5f,
		PerpendicularDeviationFactor = 5f,
		ReduceRandomnessAfter = 0.8f,
		ForkGenerationThresholdAngleFraction = 0.65f,
		ForkReflectAngleMultiplier = 0.4f,
		ForkRotationStrengthMultiplier = 0.9f,
		ForkStepSizeMultiplier = 0.8f,
		ForkLengthMultiplier = 0.8f,
		MaxForksPerBolt = 2,
		MaxForkDepth = 2,
		ForkProgressRange = new FloatRange(0.3f, 0.8f),
		SolidTileCollision = true
	};

	public static LightningGenerator LightningStrikeWeapon = new LightningGenerator
	{
		SourceRotationLimit = MathF.PI / 36f,
		Length = 750f,
		RotationStrength = 0.9f,
		StepSize = 8,
		Layers = 4,
		LayerStrengthFactor = 1.5f,
		PerpendicularDeviationFactor = 5f,
		ReduceRandomnessAfter = 0.8f,
		ForkGenerationThresholdAngleFraction = 0.65f,
		ForkReflectAngleMultiplier = 0.4f,
		ForkRotationStrengthMultiplier = 0.9f,
		ForkStepSizeMultiplier = 0.8f,
		ForkLengthMultiplier = 0.8f,
		MaxForksPerBolt = 2,
		MaxForkDepth = 2,
		ForkProgressRange = new FloatRange(0.3f, 0.8f),
		SolidTileCollision = false
	};

	public Bolt Generate(List<Bolt> bolts, uint seed, Vector2 targetPosition, Vector2 direction, bool calcPositions, bool calcRotations)
	{
		direction = direction.RotatedBy((new LCG32Random(seed).NextDouble() * 2.0 - 1.0) * (double)SourceRotationLimit) * Length;
		Bolt result = GenerateBolt(bolts, seed, 0, calcPositions, targetPosition - direction, targetPosition, RotationStrength, StepSize, new FloatRange(0f, 1f));
		if (calcRotations)
		{
			foreach (Bolt bolt in bolts)
			{
				bolt.rotations = CalcRotations(bolt.positions);
			}
		}
		return result;
	}

	private Bolt GenerateBolt(List<Bolt> bolts, uint seed, int depth, bool calcPositions, Vector2 startPos, Vector2 targetPos, float rotationStrength, float stepSize, FloatRange progressRange)
	{
		LCG32Random lCG32Random = new LCG32Random(seed);
		float num = 0f;
		float[] array = new float[Layers];
		Point point = targetPos.ToTileCoordinates();
		Vector2 vector = startPos;
		Vector2 value = targetPos - startPos;
		float num2 = value.Length();
		value /= num2;
		Vector2 value2 = new Vector2(value.Y, 0f - value.X);
		int num3 = (int)Math.Max(num2 * 2f / stepSize, 1f);
		int num4 = 0;
		Vector2[] array2 = (calcPositions ? new Vector2[num3] : null);
		Bolt bolt = new Bolt
		{
			positions = array2,
			forkDepth = depth,
			progressRange = progressRange
		};
		int i;
		for (i = 0; i < num3; i++)
		{
			if (calcPositions)
			{
				array2[i] = vector;
			}
			Vector2 vector2 = targetPos - vector;
			float num5 = Vector2.Dot(vector2, value);
			if (num5 < stepSize)
			{
				break;
			}
			float num6 = MathHelper.Clamp(1f - num5 / num2, 0f, 1f);
			if (SolidTileCollision && vector.ToTileCoordinates() != point && TileCollision(vector))
			{
				bolt.progressRange = new FloatRange(progressRange.Minimum, MathHelper.Lerp(progressRange.Minimum, progressRange.Maximum, num6));
				bolt.collidedWithTile = true;
				break;
			}
			vector2 /= vector2.Length();
			float num7 = 0f - Vector2.Dot(vector2, value2);
			float num8 = Math.Max(0.01f, Math.Min(num6, 1f - num6) * PerpendicularDeviationFactor * 2f);
			float num9 = MathHelper.Clamp(num7 / num8, -1f, 1f);
			if (PickLayerToReroll(lCG32Random.NextDouble(), 0.5f, out var layer))
			{
				float num10 = rotationStrength;
				for (int num11 = Layers - 1; num11 > layer; num11--)
				{
					num10 /= LayerStrengthFactor;
				}
				float num12 = (float)lCG32Random.NextDouble() * 2f - 1f;
				num12 += (num9 - num12 * Math.Abs(num9)) / 2f;
				float num13 = num12 * num10;
				float num14 = array[layer];
				float num15 = num13 - num14;
				num += num15;
				array[layer] = num13;
				if (layer == Layers - 1)
				{
					float num16 = lCG32Random.NextFloat();
					float num17 = Utils.Remap(num4, 0f, MaxForksPerBolt, 1f, 0f);
					float num18 = num - num15 * (1f + ForkReflectAngleMultiplier);
					if (bolts != null && Math.Abs(num15) >= rotationStrength * ForkGenerationThresholdAngleFraction && (num6 >= ForkProgressRange.Minimum && num6 <= ForkProgressRange.Maximum)  && depth < MaxForkDepth && num16 < num17 && Math.Abs(num18) < MathF.PI * 4f / 9f)
					{
						num4++;
						float num19 = (1f - num6) * ForkLengthMultiplier;
						Vector2 targetPos2 = vector + vector2.RotatedBy(num18) * num2 * num19;
						GenerateBolt(bolts, lCG32Random.state + 1u, depth + 1, calcPositions, vector, targetPos2, rotationStrength * ForkRotationStrengthMultiplier, stepSize * ForkStepSizeMultiplier, new FloatRange(MathHelper.Lerp(progressRange.Minimum, progressRange.Maximum, num6), MathHelper.Lerp(progressRange.Minimum, progressRange.Maximum, num6 + num19)));
					}
				}
			}
			float num20 = Utils.Remap(num6, ReduceRandomnessAfter, 1f, 0f, 1f);
			num20 += Utils.Remap(Math.Abs(num9), 0.5f, 1f, 0f, 1f);
			if (PickHighLayerToReroll(lCG32Random.NextDouble(), num20, out layer))
			{
				num -= array[layer];
				array[layer] = 0f;
			}
			vector += vector2.RotatedBy(num) * stepSize;
		}
		if (calcPositions && i < num3)
		{
			Array.Resize(ref array2, i + 1);
			bolt.positions = array2;
		}
		if (bolts != null && (bolt.IsMainBolt || i > 2))
		{
			bolts.Add(bolt);
		}
		return bolt;
	}

	private bool TileCollision(Vector2 pos)
	{
		Point p = pos.ToTileCoordinates();
		if (!WorldGen.InWorld(p.X, p.Y) || Main.tile[p.X, p.Y] == null)
		{
			return false;
		}
		if (WorldGen.SolidOrSlopedTile(p.X, p.Y))
		{
			return true;
		}
		int liquid = Main.tile[p.X, p.Y].LiquidAmount;
		if (liquid > 0 && (int)pos.Y % 16 > 16 * (255 - liquid) / 255)
		{
			return true;
		}
		return false;
	}

	private bool PickLayerToReroll(double r, float chance, out int layer)
	{
		for (layer = 0; layer < Layers; layer++)
		{
			if (r >= (double)(1f - chance))
			{
				return true;
			}
			r /= (double)chance;
		}
		return false;
	}

	private bool PickHighLayerToReroll(double r, float chance, out int layer)
	{
		if (!PickLayerToReroll(r, chance, out layer))
		{
			return false;
		}
		layer = Layers - 1 - layer;
		return true;
	}

	private static float[] CalcRotations(Vector2[] positions)
	{
		float[] array = new float[positions.Length];
		CalcRotations(positions, array);
		return array;
	}

	public static void CalcRotations(Vector2[] positions, float[] rotations)
	{
		if (rotations.Length >= 2)
		{
			int num = 0;
			float num2 = (positions[0] - positions[1]).ToRotation();
			rotations[num++] = num2;
			while (num < rotations.Length - 1)
			{
				float num3 = (positions[num] - positions[num + 1]).ToRotation();
				rotations[num++] = num2 + MathHelper.WrapAngle(num3 - num2) / 2f;
				num2 = num3;
			}
			rotations[num] = num2;
			SmoothRotations(rotations);
		}
	}

	private static void SmoothRotations(float[] rotations)
	{
		float num = rotations[0];
		for (int i = 1; i < rotations.Length - 1; i++)
		{
			float num2 = rotations[i];
			float num3 = rotations[i + 1];
			rotations[i] = num2 + (MathHelper.WrapAngle(num - num2) + MathHelper.WrapAngle(num3 - num2)) / 2f;
			num = num2;
		}
	}

	public bool CanHitTarget(uint seed, Vector2 targetPosition, Vector2? direction = null)
	{
		return !Generate(null, seed, targetPosition, direction, calcPositions: false, calcRotations: false).collidedWithTile;
	}

	public Bolt GenerateMainBoltPath(uint seed, Vector2 targetPosition, Vector2? direction = null)
	{
		return Generate(null, seed, targetPosition, direction, calcPositions: true, calcRotations: false);
	}

	public Bolt Generate(List<Bolt> bolts, uint seed, Vector2 targetPosition, Vector2? direction = null, bool calcPositions = true, bool calcRotations = true)
	{
		return Generate(bolts, seed, targetPosition, direction ?? Vector2.UnitY, calcPositions, calcRotations);
	}

	public static LightningGenerator GetArcSurgeWeaponGenerator(Vector2 start, Vector2 end)
	{
		float fromValue = Vector2.Distance(start, end);
		return new LightningGenerator
		{
			SourceRotationLimit = 0f,
			Length = 1f,
			RotationStrength = 0.7f,
			StepSize = 6,
			Layers = 5,
			LayerStrengthFactor = 1.2f,
			PerpendicularDeviationFactor = Utils.Remap(fromValue, 0f, 1000f, 5f, 1f),
			ReduceRandomnessAfter = 0.7f,
			ForkGenerationThresholdAngleFraction = Utils.Remap(fromValue, 0f, 1000f, 0.3f, 0.5f),
			ForkReflectAngleMultiplier = Utils.Remap(fromValue, 0f, 1000f, 0.6f, 0.2f),
			ForkRotationStrengthMultiplier = 0.9f,
			ForkStepSizeMultiplier = 0.8f,
			ForkLengthMultiplier = 0.7f,
			MaxForksPerBolt = 3,
			MaxForkDepth = 3,
			ForkProgressRange = new FloatRange(Utils.Remap(fromValue, 0f, 1000f, 0.1f, 0.5f), 0.8f),
			SolidTileCollision = true
		};
	}
}
