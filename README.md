# NetToBOM

Creates a bill-of-materials file from a KiCad EDA schematic.

Input: Netlist file in XML format (produced by KiCad automatically by the "Generate Bill of Materials" command)

Output: Bill of materials file in CSV format (comma-separated values)

Usage: nettobom [-h] [-o outfile] [infile]  
-h          Include a header section with info about the schematic.  
-o outfile  Write output to the given file; otherwise, write to the console.  
infile      Read input from the given file; otherwise, read from standard input.  
