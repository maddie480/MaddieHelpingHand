# max480's Helping Hand

Just a grab bag of Celeste code mods that were requested to me, and that could be useful for others as well.

Provides the following things:
- **Temple Eye tracking Madeline**: a temple eye that follows Madeline instead of Theo.
- **Camera Catchup Speed Trigger**: allows to set how fast the camera reaches its target. This is useful for _really_ fast sections, where growing offsets would be necessary otherwise.
- **Flag Touch Switches and Flag Switch Gates**: touch switches that activate a flag, and flag switch gates that listen to a flag.
  -  When used alone, flag touch switches can be used to activate a flag when you collect all the switches in a group. _This allows, for example, to open a gate from [Pandora's Box](https://gamebanana.com/gamefiles/9518) with touch switches._
  - When used alone, flag switch gates can be opened by activating a flag. _This allows, for example, to open a switch gate with a lever from [Pandora's Box](https://gamebanana.com/gamefiles/9518)._
  - When used together, flag touch switches and flag switch gates can be used to:
    - have multiple groups on a single screen: give them different flags (w/ different colors, icons coming soon)
    - have a group scattered on multiple screens: give the same flag to a switch in another room and it will belong to the same group. _If you do this, remember that non persistent switches get disabled when you leave the room they're in._
    - have persistent touch switches (touch switches that stay collected even if you die / change screens and didn't collect the whole group)
    - have a mix of persistent and non-persistent switch gates
    - _if all touch switches or all switch gates are persistent_, the flag associated to them will activate once you complete the group, so you can use it to activate other entities.
- **Customizable Crumble Blocks**: crumble blocks that don't respawn after they crumbled, or that respawn after a custom delay.
- **Upside-Down and Sideways Jumpthroughs**: platforms you can only cross in one direction (left to right, right to left, or up to down).
- **One-Way Invisible Barriers**: barriers that you can only cross left to right, or right to left. As opposed to sideways jumpthrus, you cannot climb on them, or walljump off them.
- **Custom Summit Checkpoints**: summit checkpoints that can display more than just numbers. You can have minuses, question marks, or make your own digits!
- **Respawning Jellyfish**: jellyfish that respawns at its initial position after a small delay once it is destroyed (squished, destroyed by a seeker barrier, or fallen off-screen).
- **Grouped Trigger Spikes**: trigger spikes that all come out at the same time once the player leaves the group.
- **Lit Blue Torch**: a blue torch (as found in Mirror Temple), except it starts out already lit... and that's it.
- **Rainbow Spinner Color Controller**: a controller that changes the color gradient of all rainbow spinners in the room.
- **Strawberry Collection Field**: a zone where strawberries are collected, even if Madeline is not on the ground. The opposite of Strawberry Blockfield, if you want.
- **Customizable Refill**: refill with customizable sprite, particle colors and respawn time.
- **Heat Wave not affecting color grade**: same as the vanilla Heat Wave effect, except it does not change the color grade, allowing you to set your own color grade instead.
- **Static Puffer**: just like puffer fish, except it doesn't wave around randomly.
- **Blackhole with Custom Colors**: just like the vanilla blackhole effect, except you can change all the colors!
- **Color Grade Fade Trigger**: a trigger allowing to fade between two color grades in a similar way as Light Fade Trigger (color grade A on the left, color grade B on the right).
- **Reskinnable Swap Block**: like a regular Swap Block, except you can change all its textures.
- **Custom Planets**: much like the Planets effect, but allowing to make planets scroll and be animated. Multiple examples of custom planets directories can be found in the mod zip, in `Graphics/Atlases/Gameplay/MaxHelpingHand/customplanets`.
