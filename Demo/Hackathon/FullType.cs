﻿namespace Hackathon
{
	public struct FullType
	{
		public static FullType Empty = new FullType(string.Empty, string.Empty, string.Empty);

		public string Assembly;
		public string Namespace;
		public string Name;

		public FullType(string ns, string n)
		{
			Assembly = string.Empty;
			Namespace = ns;
			Name = n;
		}

		public FullType(string assembly, string ns, string n)
		{
			Assembly = assembly;
			Namespace = ns;
			Name = n;
		}

		public string FullName =>
			$"{Namespace}.{Name}";

		public bool IsEmpty =>
			Namespace == string.Empty || Name == string.Empty;

		public override bool Equals(object obj) =>
			obj is FullType other && Assembly == other.Assembly && Namespace == other.Namespace && Name == other.Name;

		public override int GetHashCode() =>
			(Assembly, Namespace, Name).GetHashCode();

		public override string ToString() =>
			FullName;

		public void Deconstruct(out string c, out string ns, out string n)
		{
			c = Assembly;
			ns = Namespace;
			n = Name;
		}

		public static implicit operator (string Assembly, string Namespace, string Name) (FullType value) =>
			(value.Assembly, value.Namespace, value.Name);

		public static implicit operator FullType((string Assembly, string Namespace, string Name) value) =>
			new FullType(value.Assembly, value.Namespace, value.Name);
	}
}
