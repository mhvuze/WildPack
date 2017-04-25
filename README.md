# WildPack
Experimental tools for BOTW unpacking and repacking.

There is currently a bug affecting SARC repacking which contain BFLIM textures. The SARC files should still work but the textures appear broken. YAZ0 currently uses a fake "compression", files will be huge in comparison to the original but are fully functional.

I only toyed around with Common.pack and some stuff in there so far, don't expect anything to work.

Pay attention to padding. For whatever reason, some SARC files do not use any padding (.pack?), others use 0x80 (.ssarc?) or 0x2000 (.sblarc?). The tool will tell you which padding possibly is to use when repacking the file after YAZ0 decompression.

## Modes
SARC:

* x: Extract file provided in arg2.
* p: Pack folder provided in arg2 to SARC. Respect hex padding provided in arg4.
* l: Create a csv about all files in the provided SARC in arg2.
* s: Swap data file offsets in the file provided in arg2. You need to specify the desired swaps in arg4 and arg5.

YAZ0:
* d: Decode file provided in arg2.
* e: Encode file provided in arg2. Respect hex padding provided in arg4.

YAML:
* c: Convert file provided in arg2.

Based on code from [here](https://github.com/Chadderz121/yamlconv), [here](https://github.com/smb123w64gb/Uwizard/tree/master/Uwizard) and [here](https://forum.xentax.com/viewtopic.php?p=128897&sid=ea591bce89cb53612db8df78dbd08a8a#p128897).

## Personal todo
* Port everything to BE binary reader to clean up source
* Fix repacking of bflim (and potentially other file types that use customized padding)
