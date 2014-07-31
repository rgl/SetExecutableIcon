// developed by Rui Lopes (ruilopes.com). licensed under GPLv3.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vestris.ResourceLib;

namespace SetExecutableIcon
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                Help();
                return 1;
            }

            var iconPath = args[0];
            var executablePath = args[1];

            if (!File.Exists(iconPath))
            {
                Console.Error.WriteLine("Icon file {0} not found.", iconPath);
                return 1;
            }

            if (!File.Exists(executablePath))
            {
                Console.Error.WriteLine("Executable file {0} not found.", executablePath);
                return 1;
            }

            var iconFile = new IconFile(iconPath);

            if (iconFile.Type != IconFile.GroupType.Icon || iconFile.Icons.Count == 0)
            {
                Console.Error.WriteLine("The file {0} does not contain any icons", iconPath);
                return 1;
            }

            /*using (var info = new ResourceInfo())
            {
                info.Load(executablePath);

                Console.Error.WriteLine("{0} executable resources:", executablePath);

                foreach (var type in info.ResourceTypes)
                {
                    foreach (var resource in info.Resources[type])
                    {
                        Console.Error.WriteLine("{0} - {1} ({2}) - {3} byte(s)", resource.TypeName, resource.Name, resource.Language, resource.Size);
                    }
                }
            }*/

            //
            // Erase existing icon and icon group resources.

            var resourcesToDelete = new List<Resource>();

            using (var vi = new ResourceInfo())
            {
                vi.Load(executablePath);

                foreach (var type in vi.ResourceTypes.Where(r => r.ResourceType == Kernel32.ResourceTypes.RT_GROUP_ICON || r.ResourceType == Kernel32.ResourceTypes.RT_ICON))
                {
                    resourcesToDelete.AddRange(vi.Resources[type]);
                }
            }

            foreach (var resource in resourcesToDelete)
            {
                resource.DeleteFrom(executablePath);
            }

            //
            // Set the icon.
            //
            // Windows NT+ chooses the first RT_GROUP_ICON, which is normally the one with the lowest id as the Application icon.
            // See the "DLL and EXE Files" and "Choosing an Icon" sections at http://msdn.microsoft.com/en-us/library/ms997538.aspx

            var iconDirectory = new IconDirectoryResource();
            iconDirectory.Name = new ResourceId(1);
            iconDirectory.Language = 1033; // US-English

            foreach (var p in iconFile.Icons.Select((icon, i) => new {icon, i}))
            {
                iconDirectory.Icons.Add(
                    new IconResource(
                        p.icon,
                        new ResourceId((uint)p.i + 1),
                        iconDirectory.Language
                    )
                );
            }

            iconDirectory.SaveTo(executablePath);

            return 0;
        }

        private static void Help()
        {
            Console.Error.WriteLine("Usage: SetExecutableIcon <icon path> <executable path>");
            Console.Error.WriteLine();
            Console.Error.WriteLine("WARN This will erase all existing icons.");
        }
    }
}
