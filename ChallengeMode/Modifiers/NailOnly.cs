﻿using System.Collections.Generic;

namespace ChallengeMode.Modifiers
{
	class NailOnly : Modifier
	{
		public override void StartEffect()
		{
			On.HeroController.CanCast += OnCanCast;
		}

		private bool OnCanCast(On.HeroController.orig_CanCast orig, HeroController self)
		{
			return false;
		}

		public override void StopEffect()
		{
			On.HeroController.CanCast -= OnCanCast;
		}

		public override string ToString()
		{
			return "ChallengeMode_Nail Only";
		}

		public override List<string> GetBlacklistedModifiers()
		{
			return new List<string>()
			{
				"ChallengeMode_Nail Only", "ChallengeMode_Soul Master", "ChallengeMode_Past Regrets",
				"ChallengeMode_Speedrunner's Curse", "ChallengeMode_Nailmaster"
			};
		}
	}
}
