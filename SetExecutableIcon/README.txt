The included Vestris.ResourceLib.dll was build from https://github.com/dblock/resourcelib/tree/f015f9c75caa64291dc7bd78f645be73925665c3
and placed into the Lib subdirectory.

To merge everything into a single executable use LibZ (https://libz.codeplex.com/):

	libz inject-dll --assembly SetExecutableIcon.exe --include *.dll --move
