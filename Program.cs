using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AngleSharp.Dom;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace RepoLint
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
			Console.OutputEncoding = System.Text.Encoding.UTF8;

			var treatAsRepo = args.Any(arg => arg == "--repo");
			args = args.Where(arg => arg != "--repo").ToArray();
			if (args.Length == 1)
			{
				var path = args[0];
				if (Directory.Exists(path))
				{
					ScanRepo(path);
					return;
				}
				else if (File.Exists(path))
				{
					if (new[] { ".zip", ".rar", ".7z" }.Contains(Path.GetExtension(path)))
						ScanArchive(path);
					else
						ScanFile(path, GetRules(treatAsRepo ? new[] { "FourIndentHTML" } : new[] { "FourIndentHTML", "ExpectedFiles", "ParentFolder" }), treatAsRepo ? "." : path, true);

					return;
				}
			}

			Console.Error.WriteLine("Pass either the repo or file to scan.");
			Environment.ExitCode = 2;
		}

		private static void ScanArchive(string archivePath)
		{
			string destDirectory = Path.GetRandomFileName();

			try
			{
				var reader = ArchiveFactory.Open(archivePath);
				var entries = reader.Entries.Where(entry => !entry.Key.StartsWith("__MACOSX/"));

				if (entries.Sum(entry => entry.Size) > 100000000)
					throw new Exception("Archive is too big.");

				if (entries.Count() > 200)
					throw new Exception("Archive has too many files.");

				foreach (var entry in entries)
				{
					entry.WriteToDirectory(destDirectory, new ExtractionOptions()
					{
						Overwrite = true,
						ExtractFullPath = true
					});
				}

				LintDirectory(destDirectory, GetRules(new[] { "FourIndentHTML" }));
			}
			catch (Exception exception)
			{
				Console.Error.WriteLine("Failed to scan archive: " + exception.Message);
				Environment.ExitCode = 2;
			}
			finally
			{
				if (Directory.Exists(destDirectory))
					Directory.Delete(destDirectory, true);
			}
		}

		private static readonly string[] Folders = new[] { "HTML", "JSON" };
		private static void ScanRepo(string repoDir)
		{
			foreach (var folder in Folders)
			{
				var newPath = Path.Combine(repoDir, folder);
				if (!Directory.Exists(newPath))
				{
					Console.WriteLine($"Can't find standard repo folder \"{folder}\", skipping.");
					continue;
				}

				LintDirectory(newPath, GetRules(new[] { "FourIndentHTML", "W3CValidator" }), rootPath: repoDir);
			}
		}

		private static void LintDirectory(string path, Rule[] rules, string rootPath = null)
		{
			if (rootPath == null)
				rootPath = path;

			var directories = Directory.EnumerateDirectories(path).Where(directory => new DirectoryInfo(directory).Name != "__MACOSX").ToArray();
			if (directories.Length == 1 && Directory.GetFiles(path).Length == 0)
			{
				LintDirectory(directories[0], rules, directories[0]);
				return;
			}

			foreach (string file in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
			{
				ScanFile(file, rules, rootPath);
			}
		}

		private static void ScanFile(string file, Rule[] rules, string rootPath, bool singleFile = false)
		{
			string extension = Path.GetExtension(file);
			var applicableRules = rules.Where(rule => rule.Extensions.Contains(extension)).ToArray();
			if (applicableRules.Length == 0)
				return;

			List<string> problems = new List<string>();
			long problemCount = 0;
			foreach (Rule rule in applicableRules)
			{
				rule.AddProblems(file, rootPath, problems, singleFile, ref problemCount);

				// Stop after 15 problems, since we only display 15.
				if (problems.Count > 15)
					break;
			}

			if (problems.Count == 0)
				return;

			Console.WriteLine($"{(rootPath != file ? Path.GetRelativePath(rootPath, file) : Path.GetFileName(file))}: ({problemCount} problem{(problemCount != 1 ? "s" : "")})");
			for (int i = 0; i < Math.Min(15, problems.Count); i++)
				Console.WriteLine("    " + problems[i]);

			if (problems.Count > 15)
				Console.WriteLine("    ... 15 limit ...");
		}

		private static Rule[] GetRules(string[] ignoredRules) => GetRules(name => !ignoredRules.Contains(name));
		private static Rule[] GetRules(string onlyRule) => GetRules(name => name == onlyRule);

		private static Rule[] GetRules(Func<string, bool> ruleFilter) => Assembly
				.GetExecutingAssembly()
				.GetTypes()
				.Where(type => typeof(Rule).IsAssignableFrom(type) && !type.IsAbstract && ruleFilter(type.Name))
				.Select(type => (Rule) Activator.CreateInstance(type))
				.ToArray();
	}
}
