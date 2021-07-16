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
			try {
				ParseArgs(args, out stInFile, out stOutFile);
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
			munger.ProcessFile();

			if (fOpenedWriter)
				writer.Close();

			return 0;
		}

		static void ParseArgs(string[] args,
								out string stInFile,
								out string stOutFile)
		{
			string st;
			stInFile = null;
			stOutFile = null;
			System.Collections.IEnumerator iter = args.GetEnumerator();
			while (iter.MoveNext() && (st = (string)iter.Current) != null) {
				if (st.Length > 1 && st[0] == '-') {
					if (st == "-o") {
						if (!iter.MoveNext() || (st = (string)iter.Current) == null)
							throw new ApplicationException("Missing argument for -o option");
						stOutFile = st;
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
