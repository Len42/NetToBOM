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
			// BUG: nodeLibPart may be null due to KiCad fubar.
			// So don't rely on it for the part name or description.
			Lib = nodeLib.Attributes.GetNamedItem("lib").Value;
			Name = nodeLib.Attributes.GetNamedItem("part").Value;
			Value = nodePart.SelectSingleNode("value").InnerText;
			Description=nodeLib.Attributes.GetNamedItem("description").Value;
			// Other fields
			XmlNode nodeFields = nodePart.SelectSingleNode("./fields");
			if (nodeFields != null) {
				foreach (XmlNode nodeField in nodeFields.ChildNodes) {
					string stName = nodeField.Attributes.GetNamedItem("name").Value;
					string stValue = nodeField.InnerText;
					// Some fields are special.
					if (stName == "Note")
						Note = stValue;
					else if (stName == "Value2")
						Value2 = stValue;
					else
						fields.Add(stName, stValue);
				}
			}
		}

		public string Lib { get; private set; }

		public string Name { get; private set; }

		public string Value { get; private set; }

		public string Value2 { get; private set; }

		public string Description { get; private set; }

		public string Note { get; private set; }

		private List<String> refs = new List<String>();
		public List<String> Refs { get { return refs; } }

		/// <summary>
		/// Add another part reference to this part.
		/// Keep the list of refdes's in sorted order.
		/// </summary>
		/// <param name="stRef">Part refdes to add</param>
		public void AddRef(string stRef)
		{
			int i = refs.BinarySearch(stRef,new PartRefComparer());
			if (i < 0) i = ~i;// -i - 1; - see documentation of BinarySearch
			refs.Insert(i, stRef);
		}

		//public int RefCount { get { return refs.Count; } }

		public string GetRefListString()
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
				Part other = (Part)obj;
				// Don't bother comparing Description because it's determined by Lib & Part
				return Lib == other.Lib && Name == other.Name && Value == other.Value && Value2 == other.Value2 && Note == other.Note;
			}
		}

		public bool Equals(Part other)
		{
			if (null == other) {
				return false;
			} else if (Object.ReferenceEquals(this, other)) {
				return true;
			} else {
				// Don't bother comparing Description because it's determined by Lib & Part
				return Lib == other.Lib && Name == other.Name && Value == other.Value && Value2 == other.Value2 && Note == other.Note;
			}
		}

		public override int GetHashCode()
		{
			string stHash = String.Format("{0} {1} {2} {3} {4}", Name, Lib, Value, Value2, Note);
			// Don't bother hashing Description because it's determined by Lib & Part
			return stHash.GetHashCode();
		}

		public override string ToString()
		{
			string stObj = base.ToString();
			return String.Format("{0} {1} {2} {3}", stObj, Name, Value, Lib);
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
			if (part1.Refs.Count == 0 && part2.Refs.Count == 0) {
				// just in case
				nCompare = String.Compare(part1.Name, part2.Name);
			} else if (part1.Refs.Count == 0) {
				nCompare = (part2.Refs.Count == 0) ? 0 : -1;
			} else if (part2.Refs.Count == 0) {
				nCompare = (part1.Refs.Count == 0) ? 0 : -1;
			} else {
				//nCompare = String.Compare(part1.Refs[0], part2.Refs[0]);
				nCompare = refComparer.Compare(part1.Refs[0], part2.Refs[0]);
			}
			return nCompare;
		}
	}
}
