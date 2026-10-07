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
using CalamityRelics.Content.Items.Utilities.StructureSpawner;
using CalamityRelics.Content.Systems;
using Microsoft.Xna.Framework;

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
				case 1:
					byte schematicIndex = reader.ReadByte();
					Point origin = new(reader.ReadInt32(), reader.ReadInt32());
					if (Main.netMode != NetmodeID.Server || whoAmI < 0 || whoAmI >= Main.maxPlayers)
						break;

					Player requestingPlayer = Main.player[whoAmI];
					if (!requestingPlayer.active || requestingPlayer.dead ||
						requestingPlayer.HeldItem.type != ModContent.ItemType<StructureSpawner>() ||
						schematicIndex >= StructureSpawnerSystem.SchematicCount ||
						origin.X < 0 || origin.Y < 0 || origin.X >= Main.maxTilesX - 200 || origin.Y >= Main.maxTilesY - 200)
						break;

					if (StructureSpawnerSystem.TryPlaceSchematic(schematicIndex, origin))
					{
						for (int xOffset = 50; xOffset <= 150; xOffset += 100)
						{
							for (int yOffset = 50; yOffset <= 150; yOffset += 100)
							{
								NetMessage.SendTileSquare(-1, origin.X + xOffset, origin.Y + yOffset, 100);
							}
						}
					}
					break;
				default:
					break;
			}
		}
	}
}
