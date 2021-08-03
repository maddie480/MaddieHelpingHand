# max480's Helping Hand

This helper provides a few entities and triggers that were requested, and that could be useful for other people as well.  

Download it [here!](https://0x0a.de/twoclick?https://gamebanana.com/mmdl/629485)

Here is what is in the pack:  

*   **New entities:**

    *   **Upside-Down and Sideways Jumpthroughs**: platforms you can only cross in one direction (left to right, right to left, or up to down).
        *   The helper also comes with **Floaty Space Blocks** that support Sideways Jumpthroughs attached to them. For example, those will sink when the player grabs a jumpthrough attached to it, and they will move if the player dashes into the jumpthrough.  
        
    *   **Flag Touch Switches and Flag Switch Gates**: touch switches that activate a flag, and switch gates that listen to a flag. [Check further in this readme for more details on their usage.](https://github.com/max4805/MaxHelpingHand/blob/master/README.md#more-details-about-the-flag-touch-switch-and-flag-switch-gates) You can also customize the delays for the flag switch gate, customize sounds, or make the gates come back to their initial position. _Note that those have a lot of options,_ but the default values match vanilla, so you just have to adjust "Flag" as needed if you don't need much customization.
    *   **Moving Flag Touch Switches** _(requires [Outback Helper](https://gamebanana.com/gamefiles/7555))_: a mix of the above with moving touch switches from Outback Helper!
    *   **Shatter Flag Switch Gates** _(requires [Vortex Helper](https://gamebanana.com/mods/53759))_: a flag switch gate that explodes into debris when triggered, instead of moving to another position. Check Vortex Helper if you need a version that works with vanilla touch switches!  
    
    *   **Flag Exit Blocks**: exit blocks that only appear when a flag is enabled. You can also make them appear or disappear while Madeline is in the room.
    *   **Grouped Trigger Spikes**: trigger spikes that all come out at the same time once the player leaves the group.
    *   **Multi-Node Moving Platforms**: moving platforms as found in chapters 3 and 4, except they can have more than 2 positions, and have more settings (speed, if they should loop or go back and forth, etc.) They can also be flag-activated: they won't move if a session flag is not active.  
    
    *   **Multi-Node Bumpers**: bumpers that can move between more than 2 nodes, with the same options as multi-node moving platforms.
    *   **Rotating Bumpers**: bumpers that rotate around a point. They can also attach to the object they rotate around, and move along with it.
    *   **Multi-Room Strawberry Seeds**: strawberry seeds that don't pop when you touch the ground (you keep them even if you die, like keys), and can be carried across multiple rooms. They will form a strawberry if you collect them all and reach the room where the strawberry is.
    *   **Strawberry Seeds not popping on ground**: work exactly like strawberry seeds, except you don't lose them when standing on ground. You will still lose them if you die or change rooms: if you don't want this, use multi-room strawberry seeds instead (see above).  
    
    *   **Kevin Barrier**: a barrier blocking Kevins, but letting everything else go through.
    *   **Respawning Jellyfish**: jellyfish that respawns at its initial position after a small delay once it is destroyed (squished, destroyed by a seeker barrier, or fallen off-screen).
    *   **Static Puffer**: just like puffer fish, except it doesn't wave around randomly.
    *   **Temple Eye tracking Madeline**: a temple eye that follows Madeline instead of Theo.
    *   **One-Way Invisible Barriers**: barriers that you can only cross left to right, or right to left. As opposed to sideways jumpthrus, you cannot climb on them, or walljump off them.
    *   **Core Mode Spikes**: spikes that change textures depending on core mode. By default those spikes look like "cliffside" ones that turn red when the core mode is hot, but those are reskinnable like vanilla ones.
    *   **Lit Blue Torch**: a blue torch (as found in Mirror Temple), except it starts out already lit... and that's it.
    *   **Sideways Lava**: lava that comes from the sides of the screen (either from the left, from the right, or both, like sandwich lava).
    *   **Sideways Moving Platforms**: like multi-node moving platforms, except facing left or right! The player can go through one side, and grab / climb / wall jump on the other side.
    *   **No Dash Refill Springs**: springs that don't give your dash back when you use them.
    *   **Custom Tutorials with No Bird**: like the Custom Bird Tutorial from Everest... but without the bird, just with the bubble. You can use it with a decal to make something else "talk" for example. You can also remove the pointer and turn the bubble into a rectangle! Can be used with Custom Tutorial Bird Triggers to show or hide the bubble.
    *   **Hint Decals**: decals that only show up when a given button is held (by default H on keyboard, Right Shoulder on controller). Intended for optional hints for a puzzle. The helper also comes with a Custom Tutorial Bird preset to tell the player about the button.
    *   **Glass Exit Blocks**: a glass block that fades in when you get out of its hitbox, like vanilla exit blocks. _Only works in combination with customizable glass blocks from this helper._    
    *   **More Custom NPC**: Custom NPCs from Everest, but you can add 2 nodes to define the zone in which they can be interacted with. You can also pick to only play some frames of the chosen animation: this is mainly useful for vanilla NPCs. This also comes with presets for Granny, Oshiro and Theo, and the Badeline boss you can directly use in Ahorn!
    *   **Custom NPC with XML**: This is useful if you want to make a custom NPC with more complex animations (varying frame rates or randomized animations). If you give it no dialogue, you can use it as an "XML decal", without a speech bubble.
    *   **Badeline Sprite**: a standing or floating Badeline NPC you can place in your map. Couple it with an invisible custom NPC and you'll get an interactable Badeline NPC with hair!  
    
*   **Customizable versions of vanilla entities:**
    *   **Customizable Crumble Blocks**: crumble blocks that don't respawn after they crumbled, or that respawn after a custom delay. They can also be grouped, so that crumble platforms next to each other crumble together. You can also reskin the outline that appears while they are respawning.   
    *   **Customizable Refill**: refill with customizable sprite, particle colors and respawn time.
    *   **Reskinnable Star Track Spinner / Star Rotate Spinners**: star track / rotate spinners from chapter 9, except you can change their textures and particle colors.
    *   **Reskinnable Swap Block** and **Reskinnable Kevin**: like regular Swap Blocks and Kevins, except you can change all their textures. You can also change the sounds of the swap block!
    *   **Customizable Glass Blocks**: Glass Blocks with customizable background color and star colors. Default values try to replicate the colors seen on early Farewell screenshots. When using those, you should place a **Customizable Glass Block Controller** in the room, as this is the one allowing you to set the colors.
    *   **Custom Summit Checkpoints**: summit checkpoints that can display more than just numbers. You can have minuses, question marks, or make your own digits! You can also completely reskin the checkpoint, and change the confetti colors.
    *   **Golden Strawberry with Custom Conditions**: a golden berry that can appear even if the player didn't finish the map, didn't unlock C-sides yet and/or died if you want it to.
    *   **Custom Sandwich Lava**: sandwich lava with customizable direction, speed, gap and colors. You can also change these settings mid-room by placing a Custom Sandwich Lava Settings Trigger!
    *   **Reskinnable Floating Debris**: reskinnable version of the decorative floating debris found in Farewell.
    *   **Custom Memorial with Dreaming option**: this is the Custom Memorial from Everest, but with an extra "dreaming" option allowing you to pick between the "not dreaming" (end of chapter 1) and the "dreaming" (beginning of chapter 2) memorial. The Everest memorial uses the "Dreaming" checkbox in _map metadata_ for this; if using this checkbox doesn't have side-effects, you don't need this mod.
    *   **Reskinnable Playback Billboards**: playback billboards as seen in the wavedash tutorial in Farewell, except you can reskin their borders and disable their extra bloom.
    *   **Secret Berry**: allows you to make your own "secret" berry with custom sprites, sounds and particle colors! The berry won't appear in chapter select and pause menu, and you can pick if it counts in the total number of berries in the chapter or not.
    *   **Custom Seeker Barriers**: a seeker barrier which allows you to change the background color and/or particle color and direction. If you want to change all seeker barriers on the screen to the same settings, you can use a _Seeker Barrier Color Controller_ instead.  
    
*   **Customizable versions of vanilla styleground effects:**
    *   **Custom Planets**: much like the Planets styleground, but allowing to make planets scroll and be animated. Multiple examples of custom planets directories can be found in the mod zip, in `Graphics/Atlases/Gameplay/MaxHelpingHand/customplanets`.
    *   **Custom Stars effect**: much like the vanilla Stars styleground, except you can use your own sprites for the stars.
    *   **Snow with Custom Colors**: this is the "snowBg" and "snowFg" effect, except you can customize the snow colors!
    *   **Blackhole with Custom Colors**: just like the vanilla blackhole effect, except you can change all the colors, and set its opacity! You can also make it fade between different colors over time.  
        *   If you have multiple blackhole effects and want to change the strength of all of them, you can use the "Black Hole Strength (All Black Holes)" trigger provided with this helper.
    *   **Northern Lights with Custom Colors**: northern lights, but with customizable colors for the lights and background. You can also remove the gradient background and only keep the lights!
    *   **Heat Wave not affecting color grade**: same as the vanilla Heat Wave effect, except it does not change the color grade, allowing you to set your own color grade instead.
	*   **All Side Tentacles**: a version of the Tentacles effect that reacts to the player position when placed on the left or on the top of the screen (as opposed to only right and bottom like vanilla).

*   **New controller entities** (invisible entities having effect on a whole room/map):
    *   **Horizontal Room Wrap Controller**: drop this controller in a room, and Madeline will reappear on the left if she goes off-screen on the right! For this to work, be sure to lock the camera horizontally by using a Camera Target Trigger with lerp = 1 and X Only enabled. Pulled from Celsius by 0x0ade.
    *   **Rainbow Spinner Color Controller**: a controller that changes the color gradient of all rainbow spinners in the room. If you want to only change the color of _some_ spinners in the room, you can also do that by putting a **Rainbow Spinner Color Area Controller** over the spinners you want to recolor! Those have **"flag"** versions, so you can have different sets of colors depending on whether a session flag is active or not.
    *   **Seeker Barrier Color Controller**: a controller that changes the background color and/or particle color and direction of all seeker barriers in the room.
    *   **Parallax Fade Out Controller**: if this controller is present in a room, the parallax stylegrounds that are set to "fade in" will fade out as well in this room.
    *   **Flag Logic Gate**: this is a controller (invisible on the map) that allows you to calculate a flag based on other flags (for example flag1 = flag2 AND flag3). Useful for advanced setups.
    *   **Set Flag On Spawn Controller**: this controller can set or unset a flag (or multiple of them) when the player enters the room it is in, or respawns in it. This is useful in (rare) cases where resetting a flag on respawn is necessary, and a flag trigger doesn't set the flag early enough after respawn.
*   **New triggers:**  
    *   **Strawberry Collection Field**: a zone where strawberries are collected, even if Madeline is not on the ground. The opposite of Strawberry Blockfield, if you want.
    *   **Gradient Dust Trigger**: gives the dust sprites in your rooms a color gradient, similarly to rainbow spinners.
    *   **Madeline Silhouette Trigger**: a trigger turning Madeline into a ghost playback. Madeline's color will match her hair color.
    *   **Madeline Ponytail Trigger**: a trigger turning on another skin for Madeline, changing her hair color and appearance to look more like a ponytail.  
        
    *   **Color Grade Fade Trigger**: a trigger allowing to fade between two color grades in a similar way as Light Fade Trigger (color grade A on the left, color grade B on the right).
    *   **Camera Catchup Speed Trigger**: allows to set how fast the camera reaches its target. This is useful for _really_ fast sections, where growing offsets would be necessary otherwise.
    *   **Flag-Toggled Camera Triggers**: camera offset, camera target and smooth camera offset triggers you can turn on or off with a session flag.
    *   **Pause Badeline Bosses Trigger**: Badeline bosses will stop attacking Madeline while she sits inside this trigger.
    *   **Speed-Based Music Param Trigger**: this allows modulating a music param depending on the player's speed, for example to enable a layer or make it louder when the player is fast enough. The change is persistent (the music param will continue to be adjusted even when the player leaves the trigger and goes to other screens), but you can override/disable this with another trigger.
    *   **Ambience Volume Trigger**: allows you to lower the ambience volume, much like music fade triggers.
    *   **Pop Strawberry Seeds Trigger**: Madeline will lose all the strawberry seeds she is carrying when entering this trigger.
    *   **Activate Timed Touch Switches Timer Trigger** _(requires [Outback Helper](https://gamebanana.com/gamefiles/7555))_: this will freeze the timer of timed touch switches (from Outback Helper) until you hit the trigger, which can be useful if you want to give the player a break before the timer starts.
    *   **Camera Offset Border**: this trigger will act as a screen border for the camera. This can be used to stick the camera in a part of the room that is bigger than one screen. The border can be toggled by the player position (for example, only be active if the player is on the top-left of it), and/or with a flag.
    *   **One-Way Camera Trigger**: prevents the camera from going in some directions while the player is in it. For example, it can make the camera go to the right only in an area of your map. Also has an option to block the player if they try to go in a blocked direction, and can be toggled with a flag.  
        
    *   **Disable Ice Physics Trigger**: makes the ground not slippery even in ice mode.
    *   **Rainbow Spinner Color Fade Trigger**: allows to switch between two rainbow spinner colors smoothly within the same screen. _When using this, place a **Rainbow Spinner Color Trigger** on each spawn point in the room to ensure the spinner color is correct if the player dies in the room._  
	*   **Persistent Music Fade Trigger**: similarly to a music fade trigger, this allows you to make any music parameter fade, but the new value of the parameter will persist even if the player dies or saves & quits.
        
This helper also provides support for **Animated Parallax stylegrounds**. In order to make an animated parallax:  

*   add all your frames in `Graphics/Atlases/Gameplay/bgs/MaxHelpingHand/animatedParallax/YourModName/bgName00.png`, `bgName01.png`, etc...
*   if you want a specific frame rate, specify it in the animation name by adding `XXfps` at the end of it: `mySlowBackground3fps00.png`, `mySlowBackground3fps01.png`... If you don't, the default frame rate will be 12fps (same as for decals). The frame rate does not have to be an integer: `myVerySlowBackground0.5fps00.png` will display at 0.5 frames per second (1 frame every 2 seconds).
    
*   then, set up one of the frames as a parallax styleground in Ahorn as you would for a regular parallax.  

Installing this helper also allows you to **reskin the strawberry in the game's menus**. If your map is in `Maps/yourmapfoldername/map.bin`, put the strawberry image to use in `Graphics/Atlases/Gui/MaxHelpingHand/yourmapfoldername/strawberry.png`. To reskin the golden berry icon, do the same, but name the file `goldberry.png` instead. (You can reskin the strawberry in the level itself by using Sprites.xml, [check the wiki for that](https://github.com/EverestAPI/Resources/wiki/Reskinning-Entities#reskinning-entities-through-spritesxml).)

If you have a series of 1920x1080 PNG files in black and white (or black and transparent), you can turn it into a **custom wipe** with this mod. [Visit this website for instructions!](https://max480-random-stuff.herokuapp.com/wipe-converter)

#### More details about the flag touch switch and flag switch gates:
  - When used alone, flag touch switches can be used to activate a flag when you collect all the switches in a group. _This allows, for example, to open a gate from [Pandora's Box](https://gamebanana.com/gamefiles/9518) with touch switches._
  - When used alone, flag switch gates can be opened by activating a flag. _This allows, for example, to open a switch gate with a lever from [Pandora's Box](https://gamebanana.com/gamefiles/9518)._
  - When used together, flag touch switches and flag switch gates can be used to:
    - have multiple groups on a single screen: give them different flags (w/ different colors, icons coming soon)
    - have a group scattered on multiple screens: give the same flag to a switch in another room and it will belong to the same group. _If you do this, remember that non persistent switches get disabled when you leave the room they're in._
    - have persistent touch switches (touch switches that stay collected even if you die / change screens and didn't collect the whole group)
    - have a mix of persistent and non-persistent switch gates
    - _if all touch switches or all switch gates are persistent_, the flag associated to them will activate once you complete the group, so you can use it
