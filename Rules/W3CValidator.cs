using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RepoLint.Rules
{
	internal class W3CValidator : Rule
	{
		public W3CValidator() : base(".html") { }

		private readonly string[] ignoredRules = new[] {
			"Consider adding a “lang” attribute to the “html” start tag to declare the language of this document.",
			"This document appears to be written in *. Consider adding “lang=\"*\"” (or variant) to the “html” start tag.",
			"Bad value “*” for attribute “*” on element “*”: Illegal character in path segment: space is not allowed.",
			"An “img” element must have an “alt” attribute, except under certain conditions. For details, consult guidance on providing text alternatives for images.",
			"A table row was * columns wide, which is less than the column count established by the first row (*).",

			"CSS: “aspect-ratio”: “*” is not a “aspect-ratio” value." // Related issues: https://github.com/w3c/css-validator/issues/366 and https://github.com/w3c/css-validator/issues/287#issuecomment-1011074600
		};

		protected override void Lint()
		{
			foreach (W3CMessage message in W3C(File.FullName).OrderBy(message => message.Line))
			{
				if (ignoredRules.Any(pattern => message.Message.Like(pattern)))
					continue;

				ReportLine(message.Message, message.Line);
			}
		}

		private static W3CMessage[] W3C(string path)
		{
			var jarPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "vnu.jar");
			Process process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "java",
					Arguments = $"-jar {jarPath} --format json --stdout \"{path}\"",
					UseShellExecute = false,
					RedirectStandardOutput = true,
					WindowStyle = ProcessWindowStyle.Hidden,
					CreateNoWindow = true
				}
			};

			process.Start();
			var result = process.StandardOutput.ReadToEnd();
			process.Dispose();

			if (string.IsNullOrEmpty(result))
				return new W3CMessage[] { };

			return JsonSerializer.Deserialize<Output>(result).Messages;
		}

		private class Output
		{
			[JsonPropertyName("messages")]
			public W3CMessage[] Messages { get; set; }
		}

		private class W3CMessage
		{
			[JsonPropertyName("firstLine")]
			public int? FirstLine { get; set; }
			[JsonPropertyName("lastLine")]
			public int LastLine { get; set; }
			[JsonPropertyName("message")]
			public string Message { get; set; }

			public int Line => FirstLine ?? LastLine;
		}
	}
}