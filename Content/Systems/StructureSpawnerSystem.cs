using System;
using System.IO;
using System.Reflection;
using CalamityMod.Schematics;
using CalamityRelics.Content.Systems.CustomStructureBehavior.DraedonGarage;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityRelics.Content.Systems
{
    public class StructureSpawnerSystem : ModSystem
    {
        public const int DraedonHouseIndex = 0;
        public const int CnidrionPondIndex = 1;
        public const int DraedonGarageIndex = 2;
        public const int SchematicCount = 3;

        private const string DraedonHouseKey = "CalamityRelics:DraedonHouse";
        private const string CnidrionPondKey = "CalamityRelics:CnidrionComfyPond";
        private const string DraedonGarageKey = "CalamityRelics:DraedonGarage";
        private const string DraedonGaragePath = "Content/Structures/draedongarage.csch";
        private const string CalamityModName = "CalamityMod";
        private const string SchematicIOType = "CalamityMod.Schematics.CalamitySchematicIO";
        private const string SchematicManagerType = "CalamityMod.Schematics.SchematicManager";

        public override void PostSetupContent()
        {
            if (!ModLoader.TryGetMod(CalamityModName, out Mod calamity))
            {
                Mod.Logger.Error("Calamity Relics: Cannot register Draedon Garage schematic because Calamity Mod is not loaded.");
                return;
            }

            try
            {
                using Stream stream = Mod.GetFileStream(DraedonGaragePath);
                Type ioType = calamity.Code.GetType(SchematicIOType);
                MethodInfo importMethod = ioType?.GetMethod("ImportSchematic", BindingFlags.NonPublic | BindingFlags.Static);
                object parsedSchematic = importMethod?.Invoke(null, [stream]);

                Type managerType = calamity.Code.GetType(SchematicManagerType);
                FieldInfo tileMapsField = managerType?.GetField("TileMaps", BindingFlags.NonPublic | BindingFlags.Static);
                var tileMaps = (System.Collections.IDictionary)tileMapsField?.GetValue(null);

                if (parsedSchematic == null || tileMaps == null)
                    throw new InvalidOperationException("Calamity's schematic importer or tile map registry could not be found.");

                tileMaps[DraedonGarageKey] = parsedSchematic;
            }
            catch (Exception ex)
            {
                Mod.Logger.Error($"Calamity Relics: Failed to register Draedon Garage schematic. {ex}");
            }
        }

        public static bool TryPlaceSchematic(int index, Point origin)
        {
            string key = index switch
            {
                DraedonHouseIndex => DraedonHouseKey,
                CnidrionPondIndex => CnidrionPondKey,
                DraedonGarageIndex => DraedonGarageKey,
                _ => null
            };

            Point schematicSize = index switch
            {
                DraedonHouseIndex => new Point(172, 129),
                CnidrionPondIndex => new Point(42, 27),
                DraedonGarageIndex => new Point(66, 38),
                _ => Point.Zero
            };

            if (key == null || origin.X < 0 || origin.Y < 0 ||
                origin.X + schematicSize.X > Main.maxTilesX || origin.Y + schematicSize.Y > Main.maxTilesY)
                return false;

            try
            {
                bool specialCondition = false;
                SchematicManager.PlaceSchematic<Action<Chest>>(key, origin, SchematicAnchor.TopLeft, ref specialCondition, null);
                Terraria.WorldGen.RangeFrame(origin.X, origin.Y, schematicSize.X, schematicSize.Y);

                if (index == DraedonGarageIndex)
                    DraedonGarageSystem.DraedonGarageRect = new Rectangle(origin.X, origin.Y, 66, 38);
                return true;
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<StructureSpawnerSystem>().Mod.Logger.Error($"Calamity Relics: Failed to place schematic '{key}' at {origin}. {ex}");
                return false;
            }
        }
    }
}
