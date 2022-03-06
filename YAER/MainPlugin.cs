using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using System;
using HarmonyLib;
using YAER.Patchers;
using DiskCardGame;
using System.Collections.Generic;
using InscryptionAPI.Card;
using TDLib.GameContent;
using UnityEngine;
using BepInEx.Bootstrap;

namespace YAER {
	[BepInPlugin(GUID, Name, Version)]
	[BepInDependency("cyantist.inscryption.api")]
	[BepInDependency("zorro.inscryption.infiniscryption.spells", BepInDependency.DependencyFlags.SoftDependency)]
	[BepInDependency("io.github.TeamDoodz.TDLib")]
	public class MainPlugin : BaseUnityPlugin {

		internal const string GUID = "io.github.TeamDoodz." + Name;
		internal const string Name = "YAER";
		internal const string Version = "1.0.0";

		internal static ManualLogSource logger;

		internal static int Leeway = 0;
		internal static bool careAboutEvolve = false;
		internal static bool careAboutBlocker = true;
		internal static bool doConduits = true;
		internal static bool doGems = true;

		public static void ModifyTurnPlan(ref List<List<CardInfo>> cardsToPlay) {
			MainPlugin.logger.LogMessage($"----Editing Blueprint----");
			List<CardInfo> playNextTurn = new List<CardInfo>();
			for (int turn = 0; turn < cardsToPlay.Count; turn++) {
				int seed = SaveManager.SaveFile.GetCurrentRandomSeed();
				MainPlugin.logger.LogMessage($"---Editing turn {turn}---");

				// first, we replace all cards in the current turn plan with new ones

				MainPlugin.logger.LogDebug("about to start for loop for this turn");
				for (int card = 0; card < cardsToPlay[turn].Count; card++) {
					if (CardManager.AllCardsCopy.Contains(cardsToPlay[turn][card])) {
						// we dont want to touch modded blueprints
						MainPlugin.logger.LogMessage($"----Canceled editing blueprint; blueprint is modded----");
						return;
					}
					MainPlugin.logger.LogDebug($"about to call {nameof(FindEqualReplacement)}");
					CardInfo replace = FindEqualReplacement(cardsToPlay[turn][card], Leeway, seed);

					MainPlugin.logger.LogInfo($"Replacing {cardsToPlay[turn][card].name} ({cardsToPlay[turn][card].PowerLevel}) with {replace.name} ({replace.PowerLevel})");
					cardsToPlay[turn][card] = replace;
				}

				// then, add new cards to the turn plan

				{
					int i = 0;
					for (i = 0; i < playNextTurn.Count && cardsToPlay[turn].Count < 4; i++) {
						cardsToPlay[turn].Add(playNextTurn[i]);
						MainPlugin.logger.LogInfo($"Added card {playNextTurn[i].name} ({playNextTurn[i].PowerLevel})");
					}
					playNextTurn.RemoveRange(0, i);
				}

				int gems = BoardManager.Instance.GemsOnBoard();
				int conduits = BoardManager.Instance.ConduitsOnBoard();

				for (int i = 0; i < cardsToPlay[turn].Count; i++) {
					// do various things depending on a cards data

					if (doConduits) {
						// play conduits next turn if cards require them
						foreach (var sigil in cardsToPlay[turn][i].Abilities) {
							if (sigil.SynergyWithConduit()) {
								int c = conduits;
								for (int j = 0; j < Mathf.Max(0, 2 - c); j++) {
									if (cardsToPlay[turn].Count < 4) {
										CardInfo nullConduit = CardLoader.GetCardByName("NullConduit");
										playNextTurn.Add(nullConduit);
									}
								}
							}
						}
					}

					if (doGems) {
						// play gems if cards require them
						foreach (var sigil in cardsToPlay[turn][i].Abilities) {
							if (sigil.SynergyWithGems() && gems < 2) {
								if (cardsToPlay[turn].Count < 4) {
									CardInfo gem = CardInfoExtensions.GetRandomGem(seed);
									MainPlugin.logger.LogInfo($"Adding {gem.name} ({gem.PowerLevel}) to opponent queue");
									cardsToPlay[turn].Add(gem);
									gems++;
								}
							}
						}
					}
				}
				// if there are still cards left in playNextTurn, add them here
				if (playNextTurn.Count > 0) {
					MainPlugin.logger.LogMessage($"---Editing turn {cardsToPlay.Count}---");
					var play = new List<CardInfo>();
					foreach (var card in playNextTurn) {
						MainPlugin.logger.LogInfo($"Adding {card.name} ({card.PowerLevel}) to opponent queue");
						play.Add(card);
					}
					cardsToPlay.Add(play);
				}
			}
		}
		public static CardInfo FindEqualReplacement(CardInfo other, int leeway, int seed) {
			MainPlugin.logger.LogDebug("starting to replace card");

			List<CardInfo> replacements = new List<CardInfo>();
			MainPlugin.logger.LogDebug($"{nameof(CardManager.AllCardsCopy)} has {CardManager.AllCardsCopy.Count} cards");
			foreach (var card in CardManager.AllCardsCopy) {
				try {
					MainPlugin.logger.LogDebug($"checkign card {card.name}");
					{
						// Make sure the card can be used by the opponent

						// act 1 cards only
						if (card.temple != CardTemple.Nature) continue;

						// rare cards should only be replaced by rare cards
						if (other.metaCategories.Contains(CardMetaCategory.Rare)) {
							MainPlugin.logger.LogDebug($"checking rare");
							if (!card.metaCategories.Contains(CardMetaCategory.Rare)) continue;
						}

						if (careAboutBlocker) {
							MainPlugin.logger.LogDebug($"checking blocker");
							// blockers should only replace blockers
							if (other.Attack == 0) {
								//MainPlugin.logger.LogDebug($"{card.name} is a blocker");
								if (card.Attack != 0) continue;
							} else {
								//MainPlugin.logger.LogDebug($"{card.name} is not a blocker");
								if (card.Attack == 0) continue;
							}
						}

						if (!(card.metaCategories.Contains(CardMetaCategory.Rare) || card.metaCategories.Contains(CardMetaCategory.ChoiceNode) || card.metaCategories.Contains(CardMetaCategory.TraderOffer))) {
							// card is unobtainable
							continue;
						}

						{
							bool usable = true;
							foreach (var sigil in card.Abilities) {
								// if card has any player-only sigils get rid of it
								if (!AbilitiesUtil.GetInfo(sigil).opponentUsable) usable = false;
								// transformer cards will never be not op, dont use them
								if (sigil == Ability.Transformer) usable = false;
								// if other card has evolve sigil, only use evolving cards
								if (sigil == Ability.Evolve && careAboutEvolve) {
									if (!card.Abilities.Contains(Ability.Evolve)) continue;
								}
							}
							if (!usable) continue;
						}

						// only check if card is spell if spell mod is present
						if (SpellMod) {
							MainPlugin.logger.LogDebug("spell mod present");
							// no spells (spell mod not updated yet)
							//if (IsSpell(card)) continue;
						}
					}

					int min = other.PowerLevel - leeway;
					int max = other.PowerLevel + leeway;
					if (min <= card.PowerLevel && card.PowerLevel <= max) {
						replacements.Add(card);
					}
				} catch (Exception e) {
					MainPlugin.logger.LogWarning($"Failed to check if card {card.name} is a suitable replacement: {e}");
				}
			}
			MainPlugin.logger.LogDebug($"Possible replacements: {replacements.Count}");
			if (replacements.Count > 0) {
				return replacements[SeededRandom.Range(0, replacements.Count, seed)];
			}
			MainPlugin.logger.LogWarning($"Could not find replacement for {other}. Consider going into the config and making replacements broader.");
			return other; // couldnt find one :peeposad:
		}

		/// <summary>
		/// Whether or not the Spell Card Toolkit mod is present.
		/// </summary>
		private static bool SpellMod {
			get {
				return Chainloader.PluginInfos.ContainsKey("zorro.inscryption.infiniscryption.spells");
			}
		}

		private void Awake() {
			logger = Logger;
			logger.LogMessage($"{Name} v{Version} Loaded!");

			GetConfig();

			//logger.LogDebug($"peepee is {CardLoader.GetCardByName("Act3Cards_XFormerGrizzlyBot").PowerLevel}");

			new Harmony(GUID).PatchAll();
		}

		private void GetConfig() {
			Leeway = Config.Bind("general", "leeway", 1, new ConfigDescription("Power level leeway values (inclusive). The higher the value, the more varied the cards will be but the more unfair battles may be.")).Value;
			careAboutEvolve = Config.Bind("general", "careAboutEvolve", false, new ConfigDescription("If a card has the Evolve sigil, only replace it with other cards with that sigil.")).Value;
			careAboutBlocker = Config.Bind("general", "careAboutBlocker", true, new ConfigDescription("If a card has an attack of 0, only replace it with cards of the same attack.")).Value;
			doConduits = Config.Bind("general", "doConduits", true, new ConfigDescription("If a card has an attack of 0, only replace it with cards of the same attack.")).Value;
			doGems = Config.Bind("general", "doGems", true, new ConfigDescription("If a card has an attack of 0, only replace it with cards of the same attack.")).Value;
		}

	}
}
