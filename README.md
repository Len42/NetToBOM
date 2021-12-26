# NetToBOM

This is my KiCad BOM plug-in. There are many like it but this one is mine. It creates a bill-of-materials file from a KiCad EDA schematic.

**Input:** Netlist file in XML format (produced by KiCad automatically by the "Generate Bill of Materials" command)

**Output:** Bill of materials file in CSV format (comma-separated values)

**Usage:** `nettobom [-h] [-o outfile] [infile]`  
`-h`          Include a header section with info about the schematic.  
`-o outfile`  Write output to the given file; otherwise, write to the console.  
`infile`      Read input from the given file; otherwise, read from standard input.  

**Build Notes**  
Requires the [MakeVersionInfo](../MakeVersionInfo) project to be located in an adjacent directory.