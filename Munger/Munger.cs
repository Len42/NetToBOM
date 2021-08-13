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
				Output.Write(String.Format("\"{0}\",", st));
				st = nodeSheet.SelectSingleNode("rev").InnerText;
				Output.Write(String.Format("\"rev {0}\",",st));
				st = nodeSheet.SelectSingleNode("date").InnerText;
				Output.Write(String.Format("\"{0}\",", st));
				st = nodeSheet.SelectSingleNode("company").InnerText;
				Output.Write(String.Format("\"{0}\",", st));
				st = nodeSheet.SelectSingleNode("source").InnerText;
				Output.WriteLine(String.Format("\"{0}\"", st));
			}

			// Process the list of components
			// Note that we do not use the <libparts> data because it omits some properties,
			// e.g. attributes of parts that are defined as aliases of other parts.
			List<Part> parts = new List<Part>();
			XmlNode nodeParts = nodeRoot.SelectSingleNode("./components");
			foreach (XmlNode nodePart in nodeParts.ChildNodes) {
				string stRef = GetPartRef(nodePart);
				// Find this part in the list, or add it if it's not there.
				Part part = new Part(nodePart);
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
			Output.WriteLine("Ref,Qty,Name,Value,Value2,Note,Description,Datasheet,Manufacturer,ManuPartNum,Distributor,DistribPartNum,DistribPartLink");
			foreach (Part part in parts) {
				string st = String.Format("{0},\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\"",
					part.GetRefListString(), part.Refs.Count, part.Name, part.Value, part.Value2, part.Note, part.Description, part.Datasheet, part.Manufacturer, part.ManufacturerPartNum, part.Distributor, part.DistributorPartNum, part.DistributorPartLink);
				Output.WriteLine(st);
			}
		}

		private string GetPartRef(XmlNode nodePart)
		{
			return nodePart.Attributes.GetNamedItem("ref").Value;
		}
	}
}
