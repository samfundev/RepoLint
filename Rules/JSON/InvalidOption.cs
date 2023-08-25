using System;
using System.Collections.Generic;
using System.Text.Json;
using AngleSharp.Text;

namespace RepoLint.Rules
{
	internal class InvalidOption : JSONRule
	{
		private readonly Dictionary<string, string[]> enumFields = new Dictionary<string, string[]>() {
			{ "Type", new[] { "Regular", "Needy", "Holdable", "Widget" } },
			{ "License", new[] { "OpenSource", "Republishable", "Restricted" } },
			{ "Compatibility", new[] { "Untested", "Unplayable", "Problematic", "Compatible" } },
			{ "DefuserDifficulty", new[] { "Trivial", "VeryEasy", "Easy", "Medium", "Hard", "VeryHard", "Extreme" } },
			{ "ExpertDifficulty", new[] { "Trivial", "VeryEasy", "Easy", "Medium", "Hard", "VeryHard", "Extreme" } },
			{ "RuleSeedSupport", new[] { "NotSupported", "Supported" } },
			{ "MysteryModule", new[] { "NoConflict", "MustNotBeHidden", "MustNotBeKey", "MustNotBeHiddenOrKey", "RequiresAutoSolve" } }
		};

		protected override void JSON(Dictionary<string, JsonElement> json)
		{
			foreach (var pair in enumFields)
			{
				if (json.TryGetValue(pair.Key, out JsonElement element) && element.ValueKind == JsonValueKind.String && !pair.Value.Contains(element.GetString()))
				{
					Report($"\"{element}\" is not a valid option for {pair.Key}.");
				}
			}
		}
	}
}