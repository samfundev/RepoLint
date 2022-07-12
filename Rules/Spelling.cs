using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using AngleSharp.Common;
using AngleSharp.Dom;

namespace RepoLint.Rules
{
	internal class Spelling : HTMLRule
	{
		static Spelling()
		{
			var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "dictionary.txt");
			var fileText = System.IO.File.ReadAllText(filePath);
			corrections = fileText.Split('\n').Select(line => line.Split("->")).ToDictionary(a => a[0], a => a[1]);
		}

		static readonly Dictionary<string, string> corrections;

		protected override void HTML(IDocument document)
		{
			if (File.Name.Contains("translated") || File.Name.Contains("all languages condensed (")) return;

			foreach (var word in Regex.Split(document.Body.TextContent, "[^\\w']+"))
			{
				if (word.ToUpper() == word)
				{
					continue;
				}

				if (corrections.TryGetValue(word.ToLowerInvariant(), out string correction))
				{
					foreach (var element in document.Body.QuerySelectorAll("*"))
					{
						var textOfElement = element.ChildNodes.OfType<IText>().Select(m => m.Text).FirstOrDefault();
						if (string.IsNullOrWhiteSpace(textOfElement))
						{
							continue;
						}

						if (Regex.IsMatch(textOfElement, $"\\b{Regex.Escape(word)}\\b", RegexOptions.IgnoreCase))
						{
							ReportElement($"\"{word}\" might be spelled \"{correction}\".", element);
						}
					}

					break;
				}
			}
		}
	}
}