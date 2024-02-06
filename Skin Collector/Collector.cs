using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;

static class Collector {
	static void Main() {
		HttpClient client = new() {
			BaseAddress = new Uri("https://leagueoflegends.fandom.com")
		};

		ReadOnlySpan<char> championsTable = client.GetStringAsync("/wiki/List_of_champions").Result.AsSpan().Slice("<tbody>", "</tbody>");
		List<Range> championRanges = championsTable.Split("</tr>");
		Champion[] champions = new Champion[championRanges.Count - 2];
		for (int i = 1; i < championRanges.Count - 1; i++) {
			ReadOnlySpan<char> championSpan = championsTable[championRanges[i]];
			champions[i - 1] = new Champion(WebUtility.HtmlDecode(championSpan.Slice("data-champion=\"", "\"").ToString()), championSpan.Slice("href=\"", "\"").ToString() + "/Cosmetics");
		}

		foreach (Champion champion in champions) {
			ReadOnlySpan<char> skinHTML = client.GetStringAsync(champion.url).Result.AsSpan().Slice("<span class=\"mw-headline\" id=\"Available\">Available</span>", "<span class=\"mw-headline\" id=\"Eternals\">Eternals</span>");
			List<Range> skinRanges = skinHTML.Split("data-skin=\"");
			for (int i = 1; i < skinRanges.Count; i++) {
				ReadOnlySpan<char> skinSpan = skinHTML[skinRanges[i]];
				string skinName = WebUtility.HtmlDecode(skinSpan[..skinSpan.IndexOf("\"")].ToString());
				string skinURL = skinSpan.Slice("href=\"", "\"").ToString();
				string fullName = $"{skinName} {champion.name}";
				string fileName = $"Skins/{fullName}.jpg";
				if (!File.Exists(fileName)) {
					Console.WriteLine($"Downloading {fullName}");
					File.WriteAllBytes(fileName, client.GetByteArrayAsync(skinURL).Result);
					Thread.Sleep(1000);
				}
			}
		}

		Console.WriteLine("Successfully finished, press Enter to close...");
		Console.ReadLine();
	}

	static ReadOnlySpan<char> Slice(this ReadOnlySpan<char> text, string from, string to) {
		text = text[(text.IndexOf(from) + from.Length)..];
		return text[..text.IndexOf(to)];
	}

	static List<Range> Split(this ReadOnlySpan<char> text, string separator) {
		List<Range> ranges = new(256);
		int lastIndex = 0;
		while (true) {
			int index = lastIndex + text[lastIndex..].IndexOf(separator);
			if (index < lastIndex) {
				ranges.Add(new Range(lastIndex, text.Length));
				return ranges;
			} else {
				ranges.Add(new Range(lastIndex, index));
				lastIndex = index + separator.Length;
			}
		}
	}
}

class Champion(string name, string url) {
	public readonly string name = name, url = url;
}