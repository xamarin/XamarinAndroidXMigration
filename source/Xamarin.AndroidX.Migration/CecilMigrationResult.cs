using System;

namespace Xamarin.AndroidX.Migration
{
	[Flags]
	public enum CecilMigrationResult
	{
		Skipped = 0,

		ContainedSupport = 1 << 0,
		PotentialJni = 1 << 1,
		ContainedJni = 1 << 2,
	}
}
