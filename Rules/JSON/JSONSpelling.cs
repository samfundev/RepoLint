using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace RepoLint.Rules
{
	internal class JSONSpelling : JSONRule
	{
		static JSONSpelling()
		{
			var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "dictionary.txt");
			var fileText = System.IO.File.ReadAllText(filePath);
			corrections = fileText.Split('\n').Select(line => line.Split("->")).ToDictionary(a => a[0], a => a[1]);
		}

		static readonly Dictionary<string, string> corrections;

		protected override void JSON(Dictionary<string, JsonElement> json)
		{
			if (!json.TryGetValue("Description", out JsonElement element)) return;
			var description = element.GetString();

			foreach (var word in Regex.Split(description, "[^\\w']+"))
			{
				if (word.ToUpper() == word)
				{
					continue;
				}

				if (corrections.TryGetValue(word.ToLowerInvariant(), out string correction))
				{
					Report($"\"{word}\" might be spelled \"{correction}\".");
				}
			}
		}
	}
}