===================================================================================
MONSTER ARENA 
A multiplayer arena style FM For Thief 2 v1.18
===================================================================================

Author                  : sNeaksieGarrett
Contact Info            : sneaksiegarrett@sbcglobal.net
Homepage                : http://sgfm.webs.com
Date of Release         : July 30th, 2013; Version 1.1

Description             : 
Monster Arena is a battle arena style co-op mission, much like Haunt Stadium. You can use stealth somewhat (at least in the tiled room), but it won’t be easy. For example, you can knock out the guard (with purse) near the giant door in the first room, but doing so will be a bit of a challenge. This mission is rather short and it’s recommended to play with at least one other person. 

Monster Arena is intended for more than one player. You can try playing it alone, but it may or may not be frustrating depending on your skill level. Of course, you can always run away from AIs, so it’s really not impossible to play. There’s only one AI in the entire mission that you are forced to kill, and it is at the end of the mission. 

Important Note: If you play expert you can’t KO anyone! Make sure to read your objectives! Despite what I just said above, this means you will have to kill the gate guard in Expert. This is contrary to what you’d expect from a mission, since typically you can’t kill anyone in Expert. I did this on purpose because blackjacking makes it feel too easy, especially because of a “bug” in multiplayer related to blackjacking AIs. See KNOWN BUGS. 

For added challenge, you can try playing it on Hard or Expert. Keep in mind that if you’re playing this in multiplayer, you can respawn which takes away some of the challenge. After you respawn, you will have less health than you originally had. You can turn off respawning in the multiplayer settings, but I don’t recommend that. You will probably die at least once if not a few times. On hard and expert the difficulty ramps up. If you’re playing alone, you can set up a server and just launch the game without anyone else if you want the respawning ability. 

I’ve only tested this with up to three players, so your mileage may vary with the amount of players and the skill level. I’ve added four spawn points, so you can play with up to four players with their own spawn spots. (Meaning there is no spawning inside each other like a regular multiplayer session!)

Also, I’ve added some custom textures for the light gem. If you’re using the custom light gem from Mission X then you probably won’t see my change unless darkloader overwrites what you have. (Though to be honest, the Mission X light gem is really better, the only thing you’re getting with mine is a color change from yellow to a kind of teal color.)

Briefing                : 
No, except what I tell you here. There’s not really a story to this. Just try to survive and make it through the arena areas! Oh, there IS loot in this to pick up as well. 

A word of warning(spoiler): If you don’t get the mechanist gear in the second room and have already gone in the cave, the only way back up is if you use a vine arrow. I’ve already given you vine arrows. (Note: you may have to lean to get the arrow into the wood, because you can’t shoot a vine arrow from water.) That said, if you die you will start back where the mechanists are if you are playing multiplayer.

**Important Note: Should you die, unfortunately you will have to travel all the way back to wherever you were before. If you’re player1 however, you get a speed potion if you have opened the gate to the final room. Sorry, I would have loved to give player’s 2 through 4 a speed potion, but I couldn’t figure out a way to make it work for other players in the same fashion. I originally toyed with an idea of teleporting each player, but it only worked for player 1. Having said that, there are some speed potions you can find in the level. Use them wisely. Also make sure to check your inventory since it may come in handy, especially on Expert!**

===================================================================================

* Playing Information *

Game                    : Thief 2: The Metal Age, v1.18 English; Thief 2 Multiplayer
Mission Title           : Monster Arena
File Name               : miss25.mis
Difficulty Settings     : Yes
Equipment Store         : No
Map                     : No
Auto Map                : No
New Graphics            : Yes
New Sounds              : Yes – taken from Thief Gold
Multi-Language Support  : No

Briefing                : No
     Length             : n/a
     Size               : n/a

Difficulty Level Info   : Normal, Hard, Expert

* Construction *

Base                    : N/A
Build Time              : Months

===================================================================================

* Loading Information *

Darkloader compatible. Do not unzip. Put this mission into your fan missions folder in your thief2 folder. Remember to uninstall from Darkloader if you want to play normal thief 2. Note that you need Thief 2 1.18 in order to play the multiplayer as of this writing.

First you need 1.18, and then you need to patch Thief 2 with Tos’ Thief 2 Multiplayer Beta version 218. There are a few versions of his patch, but the latest is the one I tested with and I recommend using. Not tested in NewDark, nor intended for NewDark. To re-iterate, NewDark is not compatible with Tos’ multiplayer patch, that’s why you need T2 1.18. I’ve mirrored Tos’ multiplayer patch on my dropbox account here: https://www.dropbox.com/s/dl7ar4bvap5d1wy/T2MPSetup_218.exe

As of this writing, his website and the original download links from the Thief 2 Multiplayer beta TTLG thread over at Thief General Discussion are broken. I’ve posted some mirror links here:

http://www.ttlg.com/forums/showthread.php?t=124169&p=2202662&viewfull=1#post2202662
===================================================================================

* Credits and Thanks *

Rob Hicks for his DedX01 package, which is where the Wraith AI came from.
R Soul and LarryG (and anyone else I’ve forgotten) who helped me with dromed problems. R Soul laid down a tutorial for respawning AIs that didn’t use NVScript. (NVScript isn’t compatible with multiplayer.)
Tos for creating the multiplayer patch which inspired me to make this mission in the first place.
Gundown and another friend I won’t name here for testing this mission with me in multiplayer.
Targa for his custom bow sights.
Thank you to NV for the fixed vine arrow model from EP.crf. I think Winter Cat actually made the fix though? Not sure.
Thank you to kdau for helping me solve an issue with my custom dark.cfg file.
Finally, thank you to MysterMan for helping me with objectives related to KO’ing.

===================================================================================

* KNOWN BUGS *

Disclaimer: Because this is intended to be a multiplayer mission, any bugs you may find are most likely a result of playing the mission in multiplayer. I recommend checking the multiplayer documentation to find out what bugs it comes with. The patch is not bug free. All bugs listed below were found while playing a multiplayer session.

First, there aren’t any known bugs that I know of, except while playing multiplayer. Occasionally, there seems to be a problem in which a floating hammer appears out of nowhere when fighting AIs.

A rather annoying bug with potions where a client can drop infinite potions from his inventory. (Haven’t been able to replicate as a Host, but I watched as the client player dropped infinite speed potions at one point.) I’d completely recommend that you do not exploit this bug, at least not if you want a fair playing experience. There’s also the problem with rendering that occurs if you make too many objects in thief. I’ve seen objects disappear in a room and re-appear after a player has dropped tons of items on the floor. This bug does not always appear. My fellow tester said it happened after spiraling blood splatter appeared. I don’t know what triggers this or how to fix it. It is a known problem with multiplayer.

There’s a problem with vine arrows in multiplayer. Once you’ve fired an arrow in multiplayer, you cannot retrieve that arrow. If you try, you’ll lose the rope and will not get the arrow back. In addition, if multiple players shoot vine arrows there’s the potential to where the arrows will bug out and not work. I’ve seen where a friend has shot vine arrows and then the one I shot disappears. There’s also a bug between client and host where the other player can’t see the other person’s vine rope. I’ve watched as a client was climbing an invisible rope and appeared to be floating in air.

There’s a giant door that opens and closes based on the player entering the room. Apparently, with more than one player you can run into the problem where the door won’t open unless Player 1 (the host) is in the room. There’s a timer on the door, so that’s why it doesn’t open as soon as you walk in. In recent testing with only two players it seems to work without issue. I first discovered the bug while playing with three players. You may never encounter this bug. Maybe you will. You might want to save when all players are in a good spot, or restart the mission if this happens. (Note that the door is meant to close behind players once they enter the next room after some seconds have passed.)

I discovered a weird bug while playing with two other players where I could get into the armory but they couldn’t. Has to do with the boards. Didn’t always happen, may depend on who hits them first, or it’s just completely random. Tried replicating it with just two players, hasn’t happened again.

Apparently the client (person who is not hosting) can pick up one of the respawning zombies. This is a multiplayer bug. The host doesn’t have this issue. I’d recommend avoiding picking up the zombie, because when you drop him again he’ll be half way into the floor. Unless you want to cheat, then go right ahead.

Torches sometimes look like they go out on the client but not on the host’s machine. Also, the mechanist cameras appear to still be on from the client’s point of view. They won’t rotate, so they’re definitely off, but for some reason they still have the green light on. Unfortunately, I can’t seem to fix any of these bugs so you’ll just have to contend with them.
===================================================================================

* Copyright Information *

Distribution of this level is allowed as long as it is free and the package is kept intact. You may not include this level to any map pack without my permission. No one may edit and re-distribute this mission without my express permission. 

This level was not made and is not supported by Looking Glass Studios or Eidos Interactive.


* Changelog*

Version 1.1
Fixed metal gear. This is the only change from the initial release.
Version 1.0 – Initial release. July 29th, 2013.