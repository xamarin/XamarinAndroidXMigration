﻿using System;

namespace Hackathon
{
	[Flags]
	public enum MigrationResult
	{
		Skipped = 0,

		ContainedSupport = 1 << 0,
		PotentialJni = 1 << 1,
		ContainedJni = 1 << 2,
	}
}
