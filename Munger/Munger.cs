using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace NetToBOM
{
	public class Munger
	{
		public TextReader Input { get; set; }

		public TextWriter Output { get; set; }

		private XmlNode nodeLibParts;

		public void ProcessFile()
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(Input);
			XmlNode nodeRoot = doc.DocumentElement; // "export"
			if (nodeRoot == null)
				throw new ApplicationException("No root node");
			nodeLibParts = nodeRoot.SelectSingleNode("./libparts");
			if (nodeLibParts == null)
				throw new ApplicationException("No libparts node");

			// Process the list of components
			List<Part> parts = new List<Part>();
			XmlNode nodeParts = nodeRoot.SelectSingleNode("./components");
			foreach (XmlNode nodePart in nodeParts.ChildNodes) {
				string stRef = GetPartRef(nodePart);
				XmlNode nodeLibPart = FindLibPart(nodePart);
				// BUG: For parts with aliases, KiCad doesn't output the libpart in the netlist!
				// So it's referrencing a libpart that doesn't exist. :-(
				// DEBUG Output.WriteLine(stRef);
				// Find this part in the list, or add it if it's not there.
				Part part = new Part(nodePart, nodeLibPart);
				int i = parts.IndexOf(part);
				if (i < 0)
					parts.Add(part);
				else
					part = parts[i];
				part.AddRef(stRef);
			}

			// Sort the BOM list
			parts.Sort(new PartComparer());
			
			// Output the BOM list
			Output.WriteLine("Ref,Qty,Name,Value,Value2,Description,Note");
			foreach (Part part in parts) {
				string st = String.Format("{0},\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\"",
					part.GetRefListString(), part.Refs.Count, part.Name, part.Value, part.Value2, part.Description, part.Note);
				Output.WriteLine(st);
			}
		}

		private XmlNode FindLibPart(XmlNode nodePart)
		{
			XmlNode nodeLib = nodePart.SelectSingleNode("./libsource");
			string stLib = nodeLib.Attributes.GetNamedItem("lib").Value;
			string stPart = nodeLib.Attributes.GetNamedItem("part").Value;
			return FindLibPart(stLib, stPart);
		}

		private XmlNode FindLibPart(string stLib, string stPart)
		{
			string stQuery = String.Format("./libpart[@lib='{0}' and @part='{1}']", stLib, stPart);
			XmlNode nodeLibpart = nodeLibParts.SelectSingleNode(stQuery);
			return nodeLibpart;
		}

		private string GetPartRef(XmlNode nodePart)
		{
			return nodePart.Attributes.GetNamedItem("ref").Value;
		}
	}
}
