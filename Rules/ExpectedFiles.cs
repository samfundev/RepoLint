using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace RepoLint.Rules
{
	internal class ExpectedFiles : Rule
	{
		public ExpectedFiles() : base(".html") { }

		static ExpectedFiles()
		{
			moduleNames = Repository.Modules?.ConvertAll(module => module.Name);
		}

		static readonly List<string> moduleNames;

		protected override void Lint()
		{
			var moduleName = Regex.Match(File.GetNameWithoutExtension(), @"^([A-Z\d❖][!-~]* ?)+").Value.Trim();
			var files = new List<string>();

			if (moduleNames != null && moduleNames.Contains(moduleName))
				return;

			foreach (var file in Directory.EnumerateFiles(RootPath, moduleName + "*.*", SearchOption.AllDirectories))
			{
				if (Path.GetFullPath(file) == File.FullName)
					continue;

				files.Add(file);

				if (files.Count == 2)
					break;
			}

			if (files.Count < 2)
			{
				var found = files.Count == 1 ? "only found: " + Path.GetRelativePath(RootPath, files[0]) : "found nothing.";
				Report($"Expected 2 other files (Component SVG and JSON) with the same name \"{moduleName}\", " + found);
			}
		}
	}
}