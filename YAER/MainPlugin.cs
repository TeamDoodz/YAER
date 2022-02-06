using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using System;
using HarmonyLib;
using YAER.Patchers;
using DiskCardGame;

namespace YAER {
	[BepInPlugin(GUID, Name, Version)]
	[BepInDependency("cyantist.inscryption.api")]
	[BepInDependency("zorro.inscryption.infiniscryption.spells")]
	[BepInDependency("io.github.TeamDoodz.TDLib")]
	public class MainPlugin : BaseUnityPlugin {

		internal const string GUID = "io.github.TeamDoodz." + Name;
		internal const string Name = "YAER";
		internal const string Version = "1.0.0";

		internal static ManualLogSource logger;

		private void Awake() {
			logger = Logger;
			logger.LogMessage($"{Name} v{Version} Loaded!");

			GetConfig();

			//logger.LogDebug($"peepee is {CardLoader.GetCardByName("Act3Cards_XFormerGrizzlyBot").PowerLevel}");

			new Harmony(GUID).PatchAll();
		}

		private void GetConfig() {
			ReplaceTurnPlanPatch.Leeway = Config.Bind("general", "leeway", 1, new ConfigDescription("Power level leeway values (inclusive). The higher the value, the more varied the cards will be but the more unfair battles may be.")).Value;
			ReplaceTurnPlanPatch.careAboutEvolve = Config.Bind("general", "careAboutEvolve", false, new ConfigDescription("If a card has the Evolve sigil, only replace it with other cards with that sigil.")).Value;
			ReplaceTurnPlanPatch.careAboutBlocker = Config.Bind("general", "careAboutBlocker", true, new ConfigDescription("If a card has an attack of 0, only replace it with cards of the same attack.")).Value;
			ReplaceTurnPlanPatch.doConduits = Config.Bind("general", "doConduits", true, new ConfigDescription("If a card has an attack of 0, only replace it with cards of the same attack.")).Value;
			ReplaceTurnPlanPatch.doGems = Config.Bind("general", "doGems", true, new ConfigDescription("If a card has an attack of 0, only replace it with cards of the same attack.")).Value;
		}

	}
}
