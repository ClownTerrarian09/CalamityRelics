using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityRelics
{
	public class CalamityRelics : Mod
	{
		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			byte msg = reader.ReadByte();
			switch (msg)
			{
				case 0:
					byte playerId = reader.ReadByte();
					if (Main.netMode == NetmodeID.Server)
					{
						if (playerId >= Main.maxPlayers) return;
						Player player = Main.player[playerId];
						if (!player.active || player.dead) return;
						player.AddBuff(BuffID.Electrified, 180);
						NetworkText deathMessage = NetworkText.FromLiteral(player.name + " got incinerated to dust by the high-voltage barrier.");
						player.Hurt(Terraria.DataStructures.PlayerDeathReason.ByCustomReason(deathMessage), 50, 0);
						NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, playerId);
					}
					break;
				default:
					break;
			}
		}
	}
}
