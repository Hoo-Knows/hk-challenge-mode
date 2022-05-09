﻿using System.Collections.Generic;

namespace ChallengeMode.Modifiers
{
	class NailmasterU : Modifier
	{
		public override void StartEffect()
		{
			On.HeroController.CanCast += OnCanCast;
			On.HeroController.CanAttack += OnCanAttack;
		}

		private bool OnCanCast(On.HeroController.orig_CanCast orig, HeroController self)
		{
			return false;
		}

		private bool OnCanAttack(On.HeroController.orig_CanAttack orig, HeroController self)
		{
			return false;
		}

		public override void StopEffect()
		{
			On.HeroController.CanCast -= OnCanCast;
			On.HeroController.CanAttack -= OnCanAttack;
		}

		public override string ToString()
		{
			return "ChallengeMode_Nailmaster";
		}

		public override List<string> GetBlacklistedModifiers()
		{
			return new List<string>()
			{
				"ChallengeMode_Nailmaster", "ChallengeMode_Nail Only", "ChallengeMode_Soul Master",
				"ChallengeMode_Past Regrets", "ChallengeMode_Speedrunner's Curse", "ChallengeMode_A Fool's Errand"
			};
		}
	}
}
