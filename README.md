# max480's Helping Hand

Just a grab bag of Celeste code mods that were requested to me, and that could be useful for others as well.

You can find the list of entities and triggers it provides and download it [on GameBanana](https://gamebanana.com/gamefiles/11423).

Report any issues on the GameBanana page, as a GitHub issue or [in #modding_feedback on the Celeste Discord](https://discord.gg/6qjaePQ) pinging max480#4596.

**More details about the flag touch switch and flag switch gates:**
  - When used alone, flag touch switches can be used to activate a flag when you collect all the switches in a group. _This allows, for example, to open a gate from [Pandora's Box](https://gamebanana.com/gamefiles/9518) with touch switches._
  - When used alone, flag switch gates can be opened by activating a flag. _This allows, for example, to open a switch gate with a lever from [Pandora's Box](https://gamebanana.com/gamefiles/9518)._
  - When used together, flag touch switches and flag switch gates can be used to:
    - have multiple groups on a single screen: give them different flags (w/ different colors, icons coming soon)
    - have a group scattered on multiple screens: give the same flag to a switch in another room and it will belong to the same group. _If you do this, remember that non persistent switches get disabled when you leave the room they're in._
    - have persistent touch switches (touch switches that stay collected even if you die / change screens and didn't collect the whole group)
    - have a mix of persistent and non-persistent switch gates
    - _if all touch switches or all switch gates are persistent_, the flag associated to them will activate once you complete the group, so you can use it