# WildPack
Experimental tools for BOTW unpacking and repacking.

There is currently a bug affecting SARC repacking that crushes BFLIM textures. The SARC files will still work but the textures appear broken. Apart from that, SARC repacking and YAZ0 repacking should be functional for BOTW. YAZ0 currently uses a fake "compression", files will be huge in comparison to the original but also fully functional.

Pay attention to padding. For whatever reason, some SARC files do not use any padding (.pack?), others use 0x80 (.ssarc?) or 0x2000 (sblarc). The tool will tell you which padding to use when repacking the file after YAZ0 decompression.

Based on code from [here](https://github.com/smb123w64gb/Uwizard/tree/master/Uwizard) and [here](https://forum.xentax.com/viewtopic.php?p=128897&sid=ea591bce89cb53612db8df78dbd08a8a#p128897).
