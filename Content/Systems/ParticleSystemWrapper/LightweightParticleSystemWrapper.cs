using CalamityRelics.HelperScripts;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Systems.ParticleSystemWrapper
{

    // For now just to handle initialization and calling methods on certain events for the ParticleSystemManager class. 
    // External scripts should still be able to use ParticleSystemManager when creating particles
    public class LightweightParticleSystemWrapper : ModSystem
    {
        public override void Load()
		{
			ParticleSystemManager.InitializeParticleSystem();
		}

        public override void OnWorldUnload()
        {
			ParticleSystemManager.Instance.ResetParicleSystem();
        }

        public override void PreUpdateEntities()
        {
            ParticleSystemManager.Instance.UpdateParticles();
        }

        public override void Unload()
		{
			// idk if this is actually nessecary but im putting it here anyways
			ParticleSystemManager.Instance.ResetParicleSystem();
		}
    }
}