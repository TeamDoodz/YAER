using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using APIPlugin;
using BepInEx.Bootstrap;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.Spells.Sigils;
using InscryptionAPI.Card;
using TDLib.GameContent;
using UnityEngine;

namespace YAER.Patchers {
	/// <summary>
	/// When a game has started, change the turn plan of the encounter to include modded cards.
	/// </summary>
	[HarmonyPatch(typeof(TurnManager))]
	[HarmonyPatch("StartGame", typeof(EncounterData))]
	static class ReplaceTurnPlanPatch {
		static void Prefix(EncounterData encounterData) { // if this doesn't work, try adding ref before the arg type
			if (!SaveManager.SaveFile.IsPart1) return; // mod only does things in act 1

			if(SpellMod) {
				MainPlugin.logger.LogMessage("Spell mod is present");
			} else {
				MainPlugin.logger.LogMessage("Spell mod is not present");
			}

			List<List<CardInfo>> cardsToPlay = encounterData.opponentTurnPlan;

			
			encounterData.opponentTurnPlan = cardsToPlay;
		}
		//[MethodImpl(MethodImplOptions.NoInlining)]
		/*
		private static bool IsSpell(CardInfo card) {
			if (card.SpecialAbilities.Contains(GlobalSpellAbility.ID.id)) {
				//MainPlugin.logger.LogDebug($"{card.name} is a spell");
				return true;
			}
			if (card.SpecialAbilities.Contains(TargetedSpellAbility.ID.id)) {
				//MainPlugin.logger.LogDebug($"{card.name} is a spell");
				return true;
			}
			return false;
		}
		*/
	}
}
