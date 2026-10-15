
using CalamityRelics.HelperScripts;

namespace CalamityRelics.HelperScripts
{   
    // Example implementation of IParticleComponent used in the SineMissiles for the wulfrum boss
    public class SineMissileParticleComponent : IParticleComponent
    {
        // Modifies each particle referencing an instance of this component to scale them down proportionally
        public BaseParticle UpdateParticle(BaseParticle particle)
        {
            particle.scale *= 0.9f;
            return particle;
        }
    }
}