using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Sdk;

namespace Xamarin.AndroidX.Migration.Tests
{
	public class CecilfyDataAttribute : DataAttribute
	{
		public CecilfyDataAttribute(string input)
		{
			Input = input;
			HasOutput = false;
			HasExpectedResult = false;
		}

		public CecilfyDataAttribute(string input, string output)
		{
			Input = input;
			Output = output;
			HasOutput = true;
			HasExpectedResult = false;
		}

		public CecilfyDataAttribute(string input, CecilMigrationResult expectedResult)
		{
			Input = input;
			HasOutput = false;
			ExpectedResult = expectedResult;
			HasExpectedResult = true;
		}

		public CecilfyDataAttribute(string input, string output, CecilMigrationResult expectedResult)
		{
			Input = input;
			Output = output;
			HasOutput = true;
			ExpectedResult = expectedResult;
			HasExpectedResult = true;
		}

		public string Input { get; }

		public string Output { get; }

		public bool HasOutput { get; }

		public CecilMigrationResult ExpectedResult { get; }

		public bool HasExpectedResult { get; }

		public override IEnumerable<object[]> GetData(MethodInfo testMethod)
		{
			foreach (var migrator in Enum.GetValues(typeof(AvailableMigrators)))
			{
				var args = new List<object> { migrator, Input };
				if (HasOutput)
					args.Add(Output);
				if (HasExpectedResult)
					args.Add(ExpectedResult);
				yield return args.ToArray();
			}
		}
	}
}
