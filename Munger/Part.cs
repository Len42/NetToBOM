using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using NetToBOM.Properties;

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
					//} else if (stName == "Distributor") {
					//	stDistributor = stValue;
					//} else if (stName == "DistributorPartNum") {
					//	stDistributorPartNum = stValue;
					} else if (stName == "Mouser Part Number") {
						stDistributor = "Mouser";
						stDistributorPartNum = stValue;
					//} else if (stName == "DistributorPartLink") {
					//	stDistributorPartLink = stValue;
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
				// Find any additional PartSources by looking for "Distributor" fields.
				// Doesn't matter if the first one is "Distributor" or "Distributor1".
				// We'll never have more than 10 sources for a part, will we?
				for (uint i = 0; i < 10; i++) {
					string stFieldName = (i == 0) ? "Distributor" : $"Distributor{i}";
					XmlNode nodeT = nodeFields.SelectSingleNode($"./field[@name='{stFieldName}']");
					if (nodeT != null) {
						stDistributor = nodeT.InnerText;
						stFieldName = (i == 0) ? "DistributorPartNum" : $"DistributorPartNum{i}";
						nodeT = nodeFields.SelectSingleNode($"./field[@name='{stFieldName}']");
						stDistributorPartNum = (nodeT == null) ? null : nodeT.InnerText;
						stFieldName = (i == 0) ? "DistributorPartLink" : $"DistributorPartLink{i}";
						nodeT = nodeFields.SelectSingleNode($"./field[@name='{stFieldName}']");
						stDistributorPartLink = (nodeT == null) ? null : nodeT.InnerText;
						Sources.Add(new PartSource(stDistributor, stDistributorPartNum, stDistributorPartLink));
					}
				}
			}
			// TODO: Make a list!
			//Source = new PartSource(stDistributor, stDistributorPartNum, stDistributorPartLink);
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

		// TODO: REMOVE
		//public PartSource Source { get; private set; }

		private List<PartSource> sources = new List<PartSource>();
		public List<PartSource> Sources { get { return sources; } }

		//		public string Distributor { get; private set; }

		//		public string DistributorPartNum { get; private set; }

		//		public string DistributorPartLink { get; private set; }

		private List<String> refs = new List<String>();
		public List<String> Refs { get { return refs; } }

		/// <summary>
		/// Add another component reference to this part.
		/// Keep the list of refdes's in sorted order.
		/// </summary>
		/// <param name="stRef">Part refdes to add</param>
		public void AddRef(string stRef)
		{
			int i = refs.BinarySearch(stRef,new PartRefComparer());
			if (i < 0) i = ~i; // -i - 1; - see documentation of BinarySearch
			refs.Insert(i, stRef);
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
				foreach (string st in refs) {
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
		/// <returns>String - list of fields in CSV format</returns>
		public string InfoLine
		{
			get
			{
				string stInfo = String.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\"",
					RefListString, Refs.Count, Name, Value, Value2,
					Note, Description, Datasheet, Manufacturer, ManufacturerPartNum);
				foreach (PartSource source in Sources) {
					stInfo += $",\"{source.Distributor}\",\"{source.PartNum}\",\"{source.PartLink}\"";
				}
				return stInfo;
			}
		}

		SortedDictionary<String, String> fields = new SortedDictionary<String, String>();
		public IDictionary<String, String> ExtraFields { get { return fields; } }

		public override bool Equals(object obj)
		{
			if (null == obj) {
				return false;
			} else if (Object.ReferenceEquals(this, obj)) {
				return true;
			} else if (!(obj is Part)) {
				return false;
			} else {
				return EqualsPart((Part)obj);
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
			string stPrefix1, stPrefix2;
			int n1, n2;
			int nCompare;
			DivideRef(stRef1, out stPrefix1, out n1);
			DivideRef(stRef2, out stPrefix2, out n2);
			nCompare = String.Compare(stPrefix1, stPrefix2);
			if (nCompare == 0) {
				if (n1 == n2)
					nCompare = 0;
				else
					nCompare = (n1 < n2) ? -1 : 1;
			}
			return nCompare;
		}

		static private char[] digitList = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
		static private void DivideRef(string stRef, out string stPrefix, out int n)
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
		private PartRefComparer refComparer = new PartRefComparer();

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
				//nCompare = String.Compare(part1.Refs[0], part2.Refs[0]);
				nCompare = refComparer.Compare(part1.Refs[0], part2.Refs[0]);
			}
			return nCompare;
		}
	}
}
