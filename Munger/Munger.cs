﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace NetToBOM
{
	public class Munger
	{
		public Munger(TextReader input, TextWriter output)
		{
			Input = input;
			Output = output;
		}

		public TextReader Input { get; set; }

		public TextWriter Output { get; set; }

		public void ProcessFile(bool fInfoHeader)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(Input);
			XmlNode nodeRoot = doc.DocumentElement; // "export"
			if (nodeRoot == null)
				throw new ApplicationException("No root node");

			if (fInfoHeader) {
				// Write a header section with info about the schematic.
				// Note: If there are multiple hierarchical sheets, take the info from the
				// first (root) sheet.
				string st;
				XmlNode nodeSheet = nodeRoot.SelectSingleNode("./design/sheet/title_block");
				Output.WriteLine("Title,Rev,Date,By,File");
				st = nodeSheet.SelectSingleNode("title").InnerText;
				Output.Write($"\"{st}\",");
				st = nodeSheet.SelectSingleNode("rev").InnerText;
				Output.Write($"\"{st}\",");
				st = nodeSheet.SelectSingleNode("date").InnerText;
				Output.Write($"\"{st}\",");
				st = nodeSheet.SelectSingleNode("company").InnerText;
				Output.Write($"\"{st}\",");
				st = nodeSheet.SelectSingleNode("source").InnerText;
				Output.WriteLine($"\"{st}\",");
			}

			// Process the list of components. Create a new Part for each *different* component.
			// NOTE: We do not use the <libparts> data because it omits some properties,
			// e.g. attributes of parts that are defined as aliases of other parts.
			// NOTE: If a Part already exists for a component, some of that component's fields
			// will be ignored (e.g. Manufacturer, Distributor, etc.).
			List<Part> parts = new List<Part>();
			XmlNode nodeParts = nodeRoot.SelectSingleNode("./components");
			int numDistributors = 0;
			foreach (XmlNode nodePart in nodeParts.ChildNodes) {
				string stRef = GetPartRef(nodePart);
				// Find this part in the list, or add it if it's not there.
				Part part = new Part(nodePart);
				// Do not add the part if it's not an actual part that belongs in the BOM.
				if (part.RealPart) {
					int i = parts.IndexOf(part);
					if (i < 0)
						parts.Add(part);
					else
						part = parts[i];
					// Add the current component's refdes to the list of refs for this part.
					part.AddRefDes(stRef);
					// Keep track of how many "Distributor" columns are needed.
					numDistributors = Math.Max(numDistributors, part.Sources.Count);
				}
			}

			// Sort the BOM list
			parts.Sort(new PartComparer());
			
			// Output the BOM list
			Output.Write("Ref,Qty,Value,Value2,Note,Description,Datasheet,Manufacturer,ManuPartNum");
			for(int i = 1; i <= numDistributors; i++) {
				Output.Write($",Distributor{i},DistributorPartNum{i},DistributorPartLink{i}");
			}
			Output.WriteLine();
			foreach (Part part in parts) {
				Output.WriteLine(part.InfoLine(numDistributors));
			}
		}

		private static string GetPartRef(XmlNode nodePart)
		{
			return nodePart.Attributes.GetNamedItem("ref").Value;
		}
	}
}
