﻿# YAER

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
5. Remove all cards with sigils unusable by the opponent.
6. Remove all cards with the Transformer sigil (because they are too OP)
7. If the card to replace has the Evolve sigil, remove all cards that do not have it. (by default this step will be skipped)
8. Remove all spell cards.
9. Replace the card with a random card from the above list.
10. If the replaced card synergizes with gems, play a random gem alongside it.
11. If the replaced card synergizes with conduits, play two null conduits on the next turn.

## Changelog

### 0.1.0
* Initial Release