namespace Xamarin.AndroidX.Migration.Cecil
{
	public struct AssemblyPair
	{
		public string Source;
		public string Destination;

		public AssemblyPair(string source, string destination)
		{
			Source = source;
			Destination = destination;
		}

		public override bool Equals(object obj) =>
			obj is AssemblyPair other &&
			Source == other.Source &&
			Destination == other.Destination;

		public override int GetHashCode() =>
			(Source, Destination).GetHashCode();

		public override string ToString() =>
			$"{Source} => {Destination}";

		public void Deconstruct(out string source, out string destination)
		{
			source = Source;
			destination = Destination;
		}

		public static implicit operator (string Source, string Destination) (AssemblyPair value) =>
			(value.Source, value.Destination);

		public static implicit operator AssemblyPair((string Source, string Destination) value) =>
			new AssemblyPair(value.Source, value.Destination);
	}
}
