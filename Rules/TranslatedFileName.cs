using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RepoLint.Rules
{
	internal class TranslatedFileName : Rule
	{
		public TranslatedFileName() : base(".html") { }

		static TranslatedFileName()
		{
			moduleNames = Repository.Modules?.ConvertAll(module => module.FileName ?? module.Name);
		}

		static readonly List<string> moduleNames;

		protected override void Lint()
		{
			var filename = File.GetNameWithoutExtension();

			if (!Regex.IsMatch(filename, @"([A-Z\d❖][!-~]*) translated ") && !Regex.IsMatch(filename, @"\(.+ — .+\)")) return;

			var moduleName = Regex.Match(filename, @"^(.+) translated").Groups[1].ToString();

			if (moduleNames != null && !moduleNames.Contains(moduleName))
			{
				Report($"No original module named \"{moduleName}\" found");
			}

			if (!Regex.IsMatch(filename, "^" + moduleName + @" translated \(.+ — .+\)( [^()]+)?( \([^()]+\))?$"))
			{
				Report("Invalid file name: expected \"(.+) translated \\(.+ — .+\\)( [^()]+)?( \\([^()]+\\))?.html\"");
			}
		}
	}
}