using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NetToBOM
{
	class Program
	{
		static int Main(string[] args)
		{
			string stInFile;
			string stOutFile;
			bool fInfoHeader;
			try {
				ParseArgs(args, out stInFile, out stOutFile, out fInfoHeader);
			} catch (ApplicationException) {
				Console.WriteLine("Usage: nettobom [-o outfile] [infile]");
				return -1;
			}

			TextReader reader;
			if (String.IsNullOrEmpty(stInFile))
				reader = Console.In;
			else
				reader = new StreamReader(stInFile);

			TextWriter writer;
			bool fOpenedWriter;
			if (String.IsNullOrEmpty(stOutFile)) {
				writer = Console.Out;
				fOpenedWriter = false;
			} else {
				writer = new StreamWriter(stOutFile);
				fOpenedWriter = true;
			}

			// Read and process the input file and write the output file.
			Munger munger = new NetToBOM.Munger();
			munger.Input = reader;
			munger.Output = writer;
			munger.ProcessFile(fInfoHeader);

			if (fOpenedWriter)
				writer.Close();

			return 0;
		}

		static void ParseArgs(string[] args,
								out string stInFile,
								out string stOutFile,
								out bool fInfoHeader)
		{
			string st;
			stInFile = null;
			stOutFile = null;
			fInfoHeader = false;
			System.Collections.IEnumerator iter = args.GetEnumerator();
			while (iter.MoveNext() && (st = (string)iter.Current) != null) {
				if (st.Length > 1 && st[0] == '-') {
					if (st == "-o") {
						if (!iter.MoveNext() || (st = (string)iter.Current) == null)
							throw new ApplicationException("Missing argument for -o option");
						stOutFile = st;
					} else if (st == "-h") {
						fInfoHeader = true;
					} else {
						throw new ApplicationException("Unknown option: " + st);
					}
				} else {
					if (stInFile != null)
						throw new ApplicationException("Too many arguments");
					stInFile = st;
				}
			}
		}
	}
}
