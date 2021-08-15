using System;
using System.Collections.Generic;
using System.Text;
using NetToBOM.Properties;

namespace NetToBOM
{
	class PartSource : IEquatable<PartSource> // TODO: Is IEquatable needed?
	{
		public PartSource(string stDistributor, string stPartNum, string stPartLink)
		{
			Distributor = stDistributor;
			PartNum = stPartNum;
			PartLink = stPartLink;
		}

		public string Distributor { get; private set; }

		public string PartNum { get; private set; }

		public string PartLink { get; private set; }

		public override bool Equals(object obj)
		{
			if (null == obj) {
				return false;
			} else if (Object.ReferenceEquals(this, obj)) {
				return true;
			} else if (!(obj is PartSource)) {
				return false;
			} else {
				return EqualsPartSource((PartSource)obj);
			}
		}

		public bool Equals(PartSource other)
		{
			if (null == other) {
				return false;
			} else if (Object.ReferenceEquals(this, other)) {
				return true;
			} else {
				return EqualsPartSource(other);
			}
		}

		private bool EqualsPartSource(PartSource other)
		{
			return Distributor == other.Distributor && PartNum == other.PartNum && PartLink == other.PartLink;
		}

		public override int GetHashCode()
		{
			string stHash = $"{base.ToString()} {Distributor} {PartNum} {PartLink}";
			return stHash.GetHashCode();
		}

		public override string ToString()
		{
			return $"{base.ToString()} {Distributor} {PartNum}";
		}
	}
}
