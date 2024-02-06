using System;
using System.Collections.Generic;
using System.IO;

class Skin {
	static readonly HashSet<string> champions = new(File.ReadAllLines("champions.txt"));
	static readonly List<string> guesses;
	public static readonly Dictionary<string, GuessType> guessTypes = [];
	public static readonly Dictionary<string, string> guessFullNames = [];
	public static Skin[] skins;

	static Skin() {
		foreach (string line in File.ReadAllLines("aliases.txt")) {
			string[] pair = line.Split(" = ");
			guessTypes[pair[0]] = GuessType.Champion;
			guessFullNames[pair[0]] = pair[1];
		}

		FileInfo[] files = new DirectoryInfo("Skins").GetFiles();
		skins = new Skin[files.Length];
		for (int i = 0; i < files.Length; i++) {
			skins[i] = new Skin(files[i]);
		}
		guesses = new List<string>(guessTypes.Keys);

		//Randomize
		for (int i = 0; i < skins.Length - 1; i++) {
			int j = Random.Shared.Next(i, skins.Length);
			if (j != i) {
				(skins[j], skins[i]) = (skins[i], skins[j]);
			}
		}
	}

	public readonly string skinName, championName, fullName, path;

	Skin(FileInfo file) {
		path = file.FullName;

		fullName = file.Name[..^(file.Extension.Length)];
		//string fullNameLower = fullName.ToLower();
		//guessTypes[fullNameLower] = NameType.FullName;
		//guessFullNames[fullNameLower] = fullName;

		foreach (string champion in champions) {
			if (fullName.EndsWith(champion)) {
				championName = champion;
				string championNameLower = championName.ToLower();
				guessTypes[championNameLower] = GuessType.Champion;
				guessFullNames[championNameLower] = championName;

				skinName = fullName[..^(champion.Length + 1)];
				string skinNameLower = skinName.ToLower();
				guessTypes[skinNameLower] = GuessType.Skinline;
				guessFullNames[skinNameLower] = skinName;
				return;
			}
		}
		throw new Exception("No matching champion found");
	}

	public static List<string> BestGuess(string guess) {
		List<string> bestGuesses = [];
		int bestDistance = int.MaxValue;

		foreach (string validGuess in guesses) {
			int distance = Distance(guess, validGuess);
			if (distance < bestDistance) {
				bestDistance = distance;
				bestGuesses.Clear();
				bestGuesses.Add(validGuess);
			} else if (distance == bestDistance) {
				bestGuesses.Add(validGuess);
			}
		}

		return bestGuesses;
	}

	//https://en.wikipedia.org/wiki/Damerau%E2%80%93Levenshtein_distance#Distance_with_adjacent_transpositions
	static int Distance(string a, string b) {
		Dictionary<char, int> da = [];
		int[,] d = new int[a.Length + 2, b.Length + 2];
		int maxdist = a.Length + b.Length;

		d[0, 0] = maxdist;
		for (int i = 0; i <= a.Length; i++) {
			d[i + 1, 0] = maxdist;
			d[i + 1, 1] = i;
		}
		for (int j = 0; j <= b.Length; j++) {
			d[0, j + 1] = maxdist;
			d[1, j + 1] = j;
		}

		for (int i = 1; i <= a.Length; i++) {
			int db = 0;
			for (int j = 1; j <= b.Length; j++) {
				if (!da.TryGetValue(b[j - 1], out int k)) {
					k = 0;
				}
				int l = db;
				int cost;
				if (a[i - 1] == b[j - 1]) {
					cost = 0;
					db = j;
				} else {
					cost = 1;
				}
				d[i + 1, j + 1] =
					Math.Min(d[i, j] + cost, //substitution
					Math.Min(d[i + 1, j] + 1, //insertion
					Math.Min(d[i, j + 1] + 1, //deletion
					d[k, l] + (i - k - 1) + 1 + (j - l - 1)))); //transposition
			}
			da[a[i - 1]] = i;
		}
		return d[a.Length + 1, b.Length + 1];
	}
}

enum GuessType {
	Champion,
	Skinline,
	FullName
}
