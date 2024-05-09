SHTUP: The System Shock 2 Texture Upgrade Project

BETA 5
Dec 24, 2004

CONTRIBUTORS
Clay "ZylonBane" Halliwell (clay.h@att.net) - project lead, texturing, modeling
Kai "Hires" Kloss - texturing, modeling
Eshaktaar - lots of modeling
Jan "Rattkin" Muller - texturing
Darksharp - texturing
Shock Unlimited - current forum hosting
Wuggles Unlimited - old forum hosting
Ryan Lesser - original high-resolution SHODAN render
SNAFU - hi-res SS1 V-Mail
Garcie - texturing
Ichu - texturing
Nameless Voice - texturing
Pride Assassin - texturing
Thunderpeel - texturing
Shadowspawn - mesh conversion tools and high-poly basketball mesh
Telliamed - font conversion tool

DESCRIPTION
The System Shock 2 Texture Upgrade Project (SHTUP) is primarily an effort to increase the quality of SS2's textures by redrawing them as accurately as possible at a higher resolution. Most of the textures that SS2 shipped with were only 128x128 (or worse), even though the Dark Engine (on which SS2 is based) supports textures up to 256x256. Due to the way the Dark Engine works, it's only practical to increase the resolution of object textures (signs, decals, furniture, etc). Wall/floor texture resolution cannot be increased without editing the levels thenselves.

Secondary objectives of SHTUP include correcting typos and other graphical glitches, enhancing animations, and basically doing whatever can be done to pretty up the graphics.

As you can see, this project is far from complete. If you're a talented artist or 3D modeler, we'd be glad to accept any help you can offer.

INSTALLATION INSTRUCTIONS
System Shock 2 install must be mod-ready (see below).
Unzip into root of System Shock 2 install folder, with "Use Folder Names" enabled.

UNINSTALLATION INSTRUCTIONS
Delete the following folders from the SS2 directory:
   BITMAP
   FAM
   FONTS
   INTRFACE
   OBJ
If you have any other mods installed which placed files in these
folders, you'll have to reinstall them.

HOW TO MAKE SS2 MOD-READY
1.	Create a new folder called RES inside the SS2 install directory.
2.	Move all the files ending in .CRF from the root of the SS2 directory to the newly created folder (there should be 16 of them).
	 
3.	Make a backup copy of INSTALL.CFG (located in the root of the SS2 directory). Just highlight it, then press CTRL-C, CTRL-V.
	 
4.	Open up INSTALL.CFG using Notepad.
5.	Find the line that starts with "resname_base". Add "\res" to the first path in this line. For example:
		resname_base d:\games\shock2+f:\shock2
	becomes...
		resname_base d:\games\shock2\res+f:\shock2
	(where "d:" is the install drive, and "f:" is the CD-ROM drive)

	 
	 
6.	Save the file.

NOTES
Due to the increased texture sizes in this mod, level load times will increase slightly, and a video card with at least 32MB of onboard memory is recommended.

ONLINE RESOURCES
Latest version: http://shtup.home.att.net/
Technical support: http://www.ttlg.com/forums/forumdisplay.php?f=78
System Shock 2 info: http://www.sshock2.com/
System Shock 2 mods: http://shock.musicexplosion.net/
System Shock Rebirth mod: http://perso.wanadoo.fr/etienne.aubert/sshock/sshock_rebirth.htm
System Shock 2 Information Hub: http://www.timmymagic.com/sshock2/
