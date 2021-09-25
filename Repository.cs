using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;

static class Repository
{
	static Repository()
	{
		try
		{
			var httpClient = new HttpClient();
			var response = httpClient.GetAsync("https://ktane.timwi.de/json/raw").Result;
			if (!response.IsSuccessStatusCode)
				return;

			var jsonString = response.Content.ReadAsStringAsync().Result;
			Modules = JsonSerializer.Deserialize<WebsiteJSON>(jsonString).KtaneModules;
		}
		catch
		{
			Console.WriteLine("Failed to load the repository. Some rules will not work.");
		}
	}

	public static readonly List<WebsiteJSON.KtaneModule> Modules;

	public class WebsiteJSON
	{
		public List<KtaneModule> KtaneModules { get; set; }

		public class KtaneModule
		{
			public string Name { get; set; }
			public string FileName { get; set; }
		}
	}
}