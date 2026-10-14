using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;

namespace CalamityRelics.HelperScripts
{

    public enum IntendedDrawHook // Will be added to when need be
    {
        ANY,
        PRE_DRAW,
        POST_DRAW
    }

    // The core manager class for particle handling. Should be initialized on mod load if used, and reset on mod unload
    public class ParticleSystemManager
    {
        public static ParticleSystemManager Instance {get; private set;}
        private int maxParticles;
        private int ticksElapsed;
        private BaseParticle[] particlePool;
        
        public static bool InitializeParticleSystem(int maxParticles = 2000)
        {
            if(Instance == null)
            {
                Instance = new ParticleSystemManager(maxParticles);
                return true;
            }

            return false;
        }

        private ParticleSystemManager(int maxParticles)
        {
            this.maxParticles = maxParticles;
            particlePool = new BaseParticle[maxParticles];
            ticksElapsed = 0;
        }

        public void CreateParticle(BaseParticle baseParticle)
        {
            for (int i = 0; i < particlePool.Length; i++)
            {
                if (!particlePool[i].IsAlive)
                {
                    particlePool[i] = baseParticle; 
                }
            }
        }

        /// <summary>
        /// Removes all particle data being acted upon.
        /// </summary>
        public void ResetParicleSystem()
        {
            particlePool = new BaseParticle[maxParticles];
        }

        /// <summary>
        /// ONLY to be called in a draw hook. Will draw every alive particle through their settings and components
        /// No shader application or support right now, will do that when the time comes.
        /// </summary>
        /// <param name="spriteBatch">The sprite batcher given through the hook method</param>
        /// <param name="drawHook">The draw hook representing when the method was called. please refrain from using the ANY option here, and assign this to the corresponding hook</param>
        public void DrawParticles(SpriteBatch spriteBatch, IntendedDrawHook drawHook)
        {
            for (int i = 0; i < particlePool.Length; i++)
            {
                if(
                    !particlePool[i].IsAlive ||
                    (drawHook != particlePool[i].drawLocation && particlePool[i].drawLocation != IntendedDrawHook.ANY)
                ) continue;

                spriteBatch.Draw(
                    particlePool[i].mainTexAsset.Value,
                    RectangleHelper.CreateAnchored(RectangleHelper.Anchor.CENTER, particlePool[i].position, particlePool[i].scale),
                    null,
                    particlePool[i].mainColor,
                    particlePool[i].rotation,
                    particlePool[i].rotationPivotInfo,
                    SpriteEffects.None,
                    0
                );
            }
        }


        public void UpdateParticles()
        {
            for (int i = 0; i < particlePool.Length; i++)
            {
                if( !particlePool[i].IsAlive ) continue;
                particlePool[i].currentLife++; // I put this before component updates, since draw code should come after this logic update, therefore putting it in front might actually desync things by a tick worse than if it were executed last

                for (int j = 0; j < particlePool[i].components.Length; j++)
                {
                    particlePool[i] = particlePool[i].components[j].UpdateParticle(particlePool[i]); // im kinda dumb rn and cant remember 100% if structs update automatically, so im treating it as if it doesnt since its not an object like a class.
                }
            }
            
            ticksElapsed++;
        }
    }

    /// <summary>
    /// The core struct for particle data. This will be the main data structure acted upon when dealing with particles.
    /// 
    /// </summary>
    public struct BaseParticle
    {
        public Vector2 position;
        public float rotation;
        public int lifetime, currentLife;
        public Vector2 scale;
        public Asset<Texture2D> mainTexAsset;
        public Asset<Effect> effectAsset; // unused for now, here for custom shader support once that gets developed
        public Color mainColor;
        public IntendedDrawHook drawLocation;
        public IParticleComponent[] components;
        public Vector2 rotationPivotInfo; // Used for the sprite batchers rotation origin system. Could be useful if you want the image to rotate around the position point instead of reside on the position itself

        public readonly bool IsAlive => currentLife < lifetime;

        public BaseParticle(
            Asset<Texture2D> texture, 
            Vector2 position, 
            int lifetime = 4, 
            float rotation = 0, 
            Vector2? rotationPivot = null, 
            Color? color = null, 
            Vector2? scale = null, 
            IntendedDrawHook drawLocation = IntendedDrawHook.ANY, 
            Asset<Effect> shaderAsset = null, 
            params IParticleComponent[] components
        )
        {
            mainTexAsset = texture;
            this.position = position;
            this.lifetime = lifetime;
            this.rotation = rotation;
            mainColor = color ?? Color.White;
            this.scale = scale ?? texture.Value.Size();
            rotationPivotInfo = rotationPivot ?? new Vector2(this.scale.X, this.scale.Y) * 0.5f;
            this.drawLocation = drawLocation;
            effectAsset = shaderAsset;
            this.components = components;
            currentLife = 0;
        }
    }


    /// <summary>
    /// The core interface for any particle component. 
    /// Particle components will take particle data, apply delta modifications to it, then return new updated particle data. 
    /// </summary>
    public interface IParticleComponent
    {
        public abstract BaseParticle UpdateParticle(BaseParticle particle);
    }
}