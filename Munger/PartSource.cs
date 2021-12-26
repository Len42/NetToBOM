namespace NetToBOM
{
	class PartSource
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
