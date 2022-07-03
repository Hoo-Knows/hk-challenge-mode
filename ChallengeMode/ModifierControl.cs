﻿using System.Collections;
using System.Collections.Generic;
using Modding;
using UnityEngine;
using Random = System.Random;

namespace ChallengeMode
{
	public class ModifierControl : MonoBehaviour
	{
		private Modifier[] activeModifiers;
		private Random random;
		private bool active;

		public bool displayed;

		private string currentScene;

		private readonly List<string> sceneBlacklist = new List<string>()
		{
			"Knight Pickup", "Pre_Menu_Intro", "Menu_Title", "End_Credits", "Menu_Credits", "Cutscene_Boss_Door",
			"PermaDeath_Unlock", "GG_Unlock", "GG_End_Sequence", "End_Game_Completion", "BetaEnd", "PermaDeath",
			"GG_Entrance_Cutscene", "GG_Boss_Door_Entrance", "Intro_Cutscene_Prologue", "Opening_Sequence",
			"Prologue_Excerpt", "Intro_Cutscene", "Cinematic_Stag_travel", "PermaDeath", "Cinematic_Ending_A",
			"Cinematic_Ending_B", "Cinematic_Ending_C", "Cinematic_Ending_D", "Cinematic_Ending_E",
			"Cinematic_MrMushroom"
		};
		private readonly List<string> GGSceneBlacklist = new List<string>()
		{
			"GG_Atrium", "GG_Atrium_Roof", "GG_Unlock_Wastes", "GG_Blue_Room", "GG_Workshop", "GG_Land_Of_Storms",
			"GG_Engine", "GG_Engine_Prime", "GG_Unn", "GG_Engine_Root", "GG_Wyrm", "GG_Spa", "GG_Boss_Door_Entrance",
			"GG_End_Sequence", "GG_Waterways"
		};

		public void Initialize(int numModifiers, string currentScene)
		{
			Unload();
			
			this.currentScene = currentScene;

			//Scene checks
			if(ChallengeMode.Settings.everywhereOption && sceneBlacklist.Contains(currentScene)) return;
			if(!ChallengeMode.Settings.everywhereOption && 
				(currentScene.ToUpper().Substring(0, 2) != "GG" || GGSceneBlacklist.Contains(currentScene))) return;

			activeModifiers = new Modifier[numModifiers];
			random = new Random();

			int index = 0;
			//Select modifiers
			if(ChallengeMode.scenesU.Contains(currentScene))
			{
				int i = ChallengeMode.scenesU.IndexOf(currentScene);
				activeModifiers[index] = ChallengeMode.modifiersU[i];
				index++;
			}
			if(ChallengeMode.Settings.modifierOption)
			{
				activeModifiers[index] = ChallengeMode.modifiers[ChallengeMode.Settings.modifierValue];
				index++;
			}
			//Keep track of loops, if it hits 1000 then force break to prevent infinite loop
			int loops = 0;
			for(int i = index; i < numModifiers; i++)
			{
				Modifier modifier = SelectModifier();

				if(modifier != null)
				{
					activeModifiers[i] = modifier;
					//Frail Shell must appear before High Stress and Poor Memory
					if(modifier.ToString() == "ChallengeMode_Frail Shell")
					{
						for(int j = 0; j < numModifiers; j++)
						{
							if(activeModifiers[j] != null &&
								(activeModifiers[j].ToString() == "ChallengeMode_High Stress" ||
								activeModifiers[j].ToString() == "ChallengeMode_Poor Memory"))
							{
								(activeModifiers[i], activeModifiers[j]) = (activeModifiers[j], activeModifiers[i]);
								break;
							}
						}
					}
				}
				else i--;

				//Scuffed way to prevent infinite loops
				loops++;
				if(loops > 200)
				{
					ChallengeMode.Instance.Log("Spent too long finding a modifier for " + currentScene + ", breaking");
					break;
				}
			}

			GameManager.instance.OnFinishedEnteringScene += OnFinishedEnteringScene;
		}

		public Modifier SelectModifier()
		{
			Modifier modifier = ChallengeMode.modifiers[random.Next(0, ChallengeMode.modifiers.Count)];

			if(CheckValidModifier(modifier))
			{
				//ChallengeMode.Instance.Log(modifier.ToString() + " is valid");
				return modifier;
			}
			ChallengeMode.Instance.Log(modifier.ToString() + " is not valid");
			return null;
		}

		private bool CheckValidModifier(Modifier modifier)
		{
			if(modifier == null) return false;

			//High Stress
			if(modifier.ToString() == "ChallengeMode_High Stress")
			{
				if(!ChallengeMode.Settings.highStressOption) return false;
			}

			foreach(Modifier m in activeModifiers)
			{
				if(m != null && (m.GetCodeBlacklist().Contains(modifier.ToString()) || 
					modifier.GetCodeBlacklist().Contains(m.ToString()))) return false;
				if(m != null && m.ToString() == modifier.ToString()) return false;
			}
			if(ChallengeMode.Settings.logicOption)
			{
				foreach(Modifier m in activeModifiers)
				{
					if(m != null && (m.GetBalanceBlacklist().Contains(modifier.ToString()) ||
						modifier.GetBalanceBlacklist().Contains(m.ToString()))) return false;
				}
			}
			
			return true;
		}

		private void OnFinishedEnteringScene()
		{
			StartCoroutine(StartModifiers());
		}

		private IEnumerator StartModifiers()
		{
			displayed = false;

			//Used to make sure modifiers don't activate twice
			if(active) yield break;
			active = true;

			//ChallengeMode.Instance.Log("Started modifiers for " + currentScene);
			if(ChallengeMode.Settings.slowdownOption) Time.timeScale = 0.2f;

			for(int i = 0; i < activeModifiers.Length; i++)
			{
				if(activeModifiers[i] == null) continue;
				
				Modifier modifier = activeModifiers[i];
				try
				{
					//ChallengeMode.Instance.Log("Starting " + modifier.ToString().Split(new char[] { '_' })[1]);
					modifier.StartEffect();
				}
				catch
				{
					ChallengeMode.Instance.Log("Failed to start " + modifier.ToString().Split(new char[] { '_' })[1] + 
						" for " + currentScene);
				}
			}
			StartCoroutine(DisplayModifiers());
			yield return new WaitForSecondsRealtime(2f);
			if(!GameManager.instance.isPaused) Time.timeScale = 1f;
			GameManager.instance.OnFinishedEnteringScene -= OnFinishedEnteringScene;
			yield break;
		}

		private IEnumerator DisplayModifiers()
		{
			//Reflection magic to award achievements
			AchievementHandler ah = GameManager.instance.GetComponent<AchievementHandler>();
			AchievementHandler.AchievementAwarded aa =
				ReflectionHelper.GetField<AchievementHandler, AchievementHandler.AchievementAwarded>(ah, "AwardAchievementEvent");

			for(int i = 0; i < activeModifiers.Length; i++)
			{
				//Pause to give time for achievements to disappear
				if(i % 3 == 0 && i != 0)
				{
					yield return new WaitForSecondsRealtime(3f);
				}
				if(activeModifiers[i] == null) break;
				Modifier modifier = activeModifiers[i];
				aa.Invoke(modifier.ToString());
				//Wait between modifiers
				yield return new WaitForSecondsRealtime(0.75f);
			}
			displayed = true;
			yield break;
		}

		private void StopModifiers()
		{
			if(activeModifiers != null)
			{
				foreach(Modifier modifier in activeModifiers)
				{
					if(modifier != null)
					{
						//ChallengeMode.Instance.Log("Stopping " + modifier.ToString().Split(new char[] { '_' })[1]);
						try
						{
							modifier.StopEffect();
						}
						catch
						{
							ChallengeMode.Instance.Log("Failed to stop " + modifier.ToString().Split(new char[] { '_' })[1] + 
								" for " + currentScene);
						}
					}
				}
			}
			activeModifiers = null;
			active = false;
		}

		public void Unload()
		{
			StopModifiers();

			GameManager.instance.OnFinishedEnteringScene -= OnFinishedEnteringScene;
			StopAllCoroutines();
		}
	}
}