using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using Terraria.Utilities.Terraria.Utilities;

namespace CalamityRelics.ModUtils;
// FROM TERRARIA 1.4.5 DECOMPILE modified to work in 1.4.4.9
public struct StormLightningDrawer
{
	public struct AnimParams
	{
		public float InitialOpacity;

		public float FadeStartProgress;

		public float FadeMidProgress;

		public float FadeEndProgress;

		public float FinalOpacity;

		public float WaveTransitionLength;

		public float WaveTransitionInDuration;

		public float WaveTransitionOutDuration;
	}

	private static VertexStrip _vertexStrip = new VertexStrip();

	private float _width;

	private Color _color;

	private bool _isMainBolt;

	private float _progress;

	private FloatRange _progressRange;

	private bool _taperEnd;

	private AnimParams _anim;

	public static AnimParams DefaultAnim = new AnimParams
	{
		InitialOpacity = 0.5f,
		FadeStartProgress = 0.1f,
		FadeMidProgress = 0.25f,
		FadeEndProgress = 0.75f,
		FinalOpacity = 0f,
		WaveTransitionLength = 0.5f,
		WaveTransitionInDuration = 0.15f,
		WaveTransitionOutDuration = 0.75f
	};

	public static AnimParams ArcSurge = new AnimParams
	{
		InitialOpacity = 0.5f,
		FadeStartProgress = 0.1f,
		FadeMidProgress = 0.4f,
		FadeEndProgress = 1f,
		FinalOpacity = 0.3f,
		WaveTransitionLength = 0.3f,
		WaveTransitionInDuration = 0.3f,
		WaveTransitionOutDuration = 0.5f
	};

	public void Draw(Vector2[] positions, float[] rotations, float width, Color color, float progress, bool isMainBolt, FloatRange progressRange, float intensity, AnimParams animParams, Vector2 offset = default(Vector2))
	{
		_width = width;
		_color = color;
		_isMainBolt = isMainBolt;
		_progress = progress;
		_progressRange = progressRange;
		_anim = animParams;
		_taperEnd = _progressRange.Maximum < 1f;
		MiscShaderData miscShaderData = GameShaders.Misc["CalamityRelics:StormLightning"];
		miscShaderData.UseSaturation(intensity);
		miscShaderData.UseOpacity(Utils.Remap(_progress, _anim.FadeStartProgress, _anim.FadeMidProgress, _anim.InitialOpacity, 1f) * Utils.Remap(_progress, _anim.FadeMidProgress, _anim.FadeEndProgress, 1f, _anim.FinalOpacity));

		Matrix view = Main.GameViewMatrix.TransformationMatrix;
		Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
		miscShaderData.Shader.Parameters["uWorldViewProjection"]?.SetValue(view * projection);

		miscShaderData.Apply();
		_vertexStrip.PrepareStrip(positions, rotations, StripColors, StripWidth, -Main.screenPosition + offset);
		_vertexStrip.DrawTrail();
		Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
		Main.pixelShader.CurrentTechnique.Passes[0].Apply();
	}

	private static float WaveTransition(float progressOnStrip, float waveProgress, float transitionLength, float from, float to)
	{
		return Utils.Remap(progressOnStrip, MathHelper.Lerp(0f - transitionLength, 1f, waveProgress), MathHelper.Lerp(0f, 1f + transitionLength, waveProgress), to, from);
	}

	private Color StripColors(float progressOnStrip)
	{
		progressOnStrip = MathHelper.Lerp(_progressRange.Minimum, _progressRange.Maximum, progressOnStrip);
		float waveProgress = Utils.Remap(_progress, 0f, _anim.WaveTransitionInDuration, 0f, 1f);
		float num = WaveTransition(progressOnStrip, waveProgress, _anim.WaveTransitionLength, 0f, 1f);
		float waveProgress2 = Utils.Remap(_progress, 1f - _anim.WaveTransitionOutDuration, 1f, 0f, 1f);
		float num2 = WaveTransition(progressOnStrip, waveProgress2, _anim.WaveTransitionLength, 1f, 0f);
		float num3 = num * num2;
		return _color * num3;
	}

	private float StripWidth(float progressOnStrip)
	{
		progressOnStrip = MathHelper.Lerp(_progressRange.Minimum, _progressRange.Maximum, progressOnStrip);
		float width = _width;
		width *= Utils.Remap(progressOnStrip, 0.5f, 1f, 1f, 0.5f);
		width *= Utils.Remap(_progress, 0.5f, 1f, 1f, 0.5f);
		if (_taperEnd)
		{
			width *= Utils.Remap(_progressRange.Maximum - progressOnStrip, 0.1f, 0f, 1f, _isMainBolt ? 0.5f : 0f);
		}
		return width;
	}
}
