# YAER

YAER (Yet Another Enemy Replacer) is a mod that allows Leshy to use modded cards during battle. It differs from other mods that 
achieve this in that instead of making Leshy play cards completely at random, there is some strategy involved.

## How it works

When a round begins, a turn plan is generated. This turn plan is a list of turns, with each turn being a list of cards. After this plan is generated,
YAER will replace all cards in that plan via the following algorithm:

1. First, create a list of all modded cards
2. Remove all cards from that list that are not for Act 1
3. If the card to replace is rare, remove all non-rare cards from the list.
4. If the card to replace has zero attack, remove all cards with an attack greater than 0.
    a. Alternatively, if the card has an attack greater than 1 remove all cards with an attack of 0.
5. Remove all unobtainable cards
6. Remove all cards with sigils unusable by the opponent.
7. Remove all cards with the Transformer sigil (because they are too OP)
8. If the card to replace has the Evolve sigil, remove all cards that do not have it. (by default this step will be skipped)
9. Remove all spell cards.
10. Replace the card with a random card from the above list.
11. If the replaced card synergizes with gems, play a random gem alongside it.
12. If the replaced card synergizes with conduits, play two null conduits on the next turn.\

## Changelog

### 0.2.0

### 0.1.2
* "Fixed" an error with some cards

### 0.1.1
* Removed dependancy to Spell Card Toolkit
* Leshy will no longer play unobtainable cards

### 0.1.0
* Initial Release