# NetToBOM

This is my [KiCad](https://www.kicad.org/) BOM plug-in. There are many like it but this one is mine. It creates a bill-of-materials file from a KiCad EDA schematic.

**Input:** Netlist file in XML format (produced by KiCad automatically by the "Generate Bill of Materials" command)

**Output:** Bill of materials file in CSV format (comma-separated values)

**Usage:** `nettobom [-h] [-o outfile] [infile]`  
`-h`          Include a header section with info about the schematic.  
`-o outfile`  Write output to the given file; otherwise, write to the console.  
`infile`      Read input from the given file; otherwise, read from standard input.  

**Notes**  
This plug-in has been tested with KiCad versions 5.1 and 6.0.

Releases up to version 1.2.5 use the "old" .NET Framework and were built with Microsoft Visual Studio 2019.

Releases from version 1.3.0 use the "new" .NET Core and were built with Microsoft Visual Studio 2022.

The Visual Studio solution requires the [MakeVersionInfo](https://github.com/Len42/MakeVersionInfo) project located in an adjacent directory.