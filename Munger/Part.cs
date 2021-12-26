using System;
using System.Collections.Generic;
using System.Xml;

namespace NetToBOM
{
	class Part : IEquatable<Part>
	{
		public Part(XmlNode nodePart)
		{
			XmlNode nodeLib = nodePart.SelectSingleNode("./libsource");
			Lib = nodeLib.Attributes.GetNamedItem("lib").Value;
			Name = nodeLib.Attributes.GetNamedItem("part").Value;
			Value = nodePart.SelectSingleNode("value").InnerText;
			RealPart = true;
			Description = nodeLib.Attributes.GetNamedItem("description").Value;
			Datasheet = nodePart.SelectSingleNode("datasheet")?.InnerText;
			if (Datasheet == "~")
				Datasheet = null;
			// Other fields
			XmlNode nodeFields = nodePart.SelectSingleNode("./fields");
			if (nodeFields != null) {
				string stDistributor = null;
				string stDistributorPartNum = null;
				string stDistributorPartLink = null;
				foreach (XmlNode nodeField in nodeFields.ChildNodes) {
					string stName = nodeField.Attributes.GetNamedItem("name").Value;
					string stValue = nodeField.InnerText;
					// Commonly-used fields are called out as properties.
					// Some parts have info downloaded from Mouser.
					if (stName == "NoPart") {
						// Not a real part that belongs in the BOM,
						// e.g. mounting holes, logos, etc.
						RealPart = false;
					} else if (stName == "Note") {
						Note = stValue;
					} else if (stName == "Value2") {
						Value2 = stValue;
					} else if (stName == "Manufacturer" || stName == "Manufacturer_Name") {
						Manufacturer = stValue;
					} else if (stName == "ManufacturerPartNum" || stName == "Manufacturer_Part_Number") {
						ManufacturerPartNum = stValue;
					} else if (stName == "Mouser Part Number") {
						stDistributor = "Mouser";
						stDistributorPartNum = stValue;
					} else if (stName == "Mouser Price/Stock") {
						stDistributor = "Mouser";
						stDistributorPartLink = stValue;
					} else {
						fields.Add(stName, stValue);
					}
				}
				// Add a PartSource if "Mouser Part Number" was found.
				if (stDistributor != null)
					sources.Add(new PartSource(stDistributor, stDistributorPartNum, stDistributorPartLink));
				// Find PartSources by looking for "Distributor" fields.
				// Doesn't matter if the first one is "Distributor" or "Distributor1".
				// We'll never have more than 100 sources for a part, will we?
				for (uint i = 0; i < 100; i++) {
					string stFieldName = (i == 0) ? "Distributor" : $"Distributor{i}";
					XmlNode nodeT = nodeFields.SelectSingleNode($"./field[@name='{stFieldName}']");
					if (nodeT != null) {
						stDistributor = nodeT.InnerText;
						stFieldName = (i == 0) ? "DistributorPartNum" : $"DistributorPartNum{i}";
						nodeT = nodeFields.SelectSingleNode($"./field[@name='{stFieldName}']");
						stDistributorPartNum = nodeT?.InnerText;
						stFieldName = (i == 0) ? "DistributorPartLink" : $"DistributorPartLink{i}";
						nodeT = nodeFields.SelectSingleNode($"./field[@name='{stFieldName}']");
						stDistributorPartLink = nodeT?.InnerText;
						Sources.Add(new PartSource(stDistributor, stDistributorPartNum, stDistributorPartLink));
					}
				}
			}
		}

		public string Lib { get; private set; }

		public string Name { get; private set; }

		public string Value { get; private set; }

		public bool RealPart { get; private set; }

		public string Value2 { get; private set; }

		public string Note { get; private set; }

		public string Description { get; private set; }

		public string Datasheet { get; private set; }

		public string Manufacturer { get; private set; }

		public string ManufacturerPartNum { get; private set; }

		private readonly List<PartSource> sources = new List<PartSource>();
		public List<PartSource> Sources { get { return sources; } }

		private readonly SortedList<String, String> refs = new SortedList<String, String>(new PartRefComparer());
		public IList<String> Refs { get { return refs.Values; } }

		/// <summary>
		/// Add another component reference to this part.
		/// Keep the list of refdes's in sorted order.
		/// </summary>
		/// <param name="stRef">Part refdes to add</param>
		public void AddRefDes(string stRef)
		{
			refs.Add(stRef, stRef);
		}

		//public int RefCount { get { return refs.Count; } }

		/// <summary>
		/// A string listing of all schematic components (refdes) that use this part
		/// </summary>
		/// <returns>String - list of component refdes</returns>
		public string RefListString
		{
			get
			{
				string stList = null;
				foreach (string st in Refs) {
					if (stList == null)
						stList = st;
					else
						stList = stList + ' ' + st;
				}
				return stList;
			}
		}

		/// <summary>
		/// A string summarizing this part
		/// </summary>
		/// <param name="numDistributors">Number of Distributor, DistributorPartNum, and
		/// DistributorPartLink fields that are needed</param>
		/// <returns>String - list of fields in CSV format</returns>
		public string InfoLine(int numDistributors)
		{
			string stInfo = String.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\"",
				RefListString, Refs.Count, Name, Value, Value2,
				Note, Description, Datasheet, Manufacturer, ManufacturerPartNum);
			for (int i = 0; i < numDistributors; i++) {
				// Make sure the desired number of Distributor fields are included,
				// even if this Part doesn't have that many.
				if (i < Sources.Count) {
					PartSource source = Sources[i];
					stInfo += $",\"{source.Distributor}\",\"{source.PartNum}\",\"{source.PartLink}\"";
				} else {
					stInfo += $",\"\",\"\",\"\"";
				}
			}
			return stInfo;
		}

		readonly SortedDictionary<String, String> fields = new SortedDictionary<String, String>();
		public IDictionary<String, String> ExtraFields { get { return fields; } }

		public override bool Equals(object obj)
		{
			if (null == obj) {
				return false;
			} else if (Object.ReferenceEquals(this, obj)) {
				return true;
			} else if (!(obj is Part part)) {
				return false;
			} else {
				return EqualsPart(part);
			}
		}

		public bool Equals(Part other)
		{
			if (null == other) {
				return false;
			} else if (Object.ReferenceEquals(this, other)) {
				return true;
			} else {
				return EqualsPart(other);
			}
		}

		private bool EqualsPart(Part other)
		{
			// Don't bother comparing Description because it's determined by Lib & Part
			// Don't compare fields that don't change the component per se (Label, Distributor, etc.).
			return Lib == other.Lib && Name == other.Name && Value == other.Value && Value2 == other.Value2 && Note == other.Note;
		}

		private string HashString
		{
			get
			{
				// Don't bother hashing Description because it's determined by Lib & Part.
				// Don't hash fields that don't change the component per se (Label, Distributor, etc.).
				return $"{Name} {Lib} {Value} {Value2} {Note}";
			}
		}

		public override int GetHashCode()
		{
			return HashString.GetHashCode();
		}

		public override string ToString()
		{
			return $"{base.ToString()} {HashString}";
		}
	}

	/// <summary>
	/// Implmentation of IComparer to compare refdes strings.
	/// Refdes strings have to be sorted by prefix first then trailing number.
	/// </summary>
	class PartRefComparer : Comparer<String>
	{
		public override int Compare(string stRef1, string stRef2)
		{
			int nCompare;
			DivideRef(stRef1, out string stPrefix1, out int n1);
			DivideRef(stRef2, out string stPrefix2, out int n2);
			nCompare = String.Compare(stPrefix1, stPrefix2);
			if (nCompare == 0) {
				if (n1 == n2)
					nCompare = 0;
				else
					nCompare = (n1 < n2) ? -1 : 1;
			}
			return nCompare;
		}

		private static readonly char[] digitList = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
		private static void DivideRef(string stRef, out string stPrefix, out int n)
		{
			int ich = stRef.IndexOfAny(digitList);
			if (ich < 0) {
				stPrefix = stRef;
				n = 0;
			} else {
				stPrefix = stRef.Substring(0, ich);
				n = Int32.Parse(stRef.Substring(ich));
			}
		}
	}

	/// <summary>
	/// Compare two Parts so they are sorted in a reasonable order.
	/// We'll compare by the first refdes, so C1 before C10 before R1.
	/// </summary>
	/// <remarks>
	/// Other comparisons could be used, e.g. compare by name then value then refdes.
	/// </remarks>
	class PartComparer : Comparer<Part>
	{
		private readonly PartRefComparer refComparer = new PartRefComparer();

		public override int Compare(Part part1, Part part2)
		{
			int nCompare;
			if (part1.Refs.Count == 0) {
				// just in case
				if (part2.Refs.Count == 0)
					nCompare = String.Compare(part1.Name, part2.Name);
				else
					nCompare = 1;
			} else {
				nCompare = refComparer.Compare(part1.Refs[0], part2.Refs[0]);
			}
			return nCompare;
		}
	}
}
