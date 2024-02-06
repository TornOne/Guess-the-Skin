using System;
using System.Collections.Generic;
using System.IO;

class Skin {
	#region Champions
	static readonly HashSet<string> champions = [
		"Annie",
		"Olaf",
		"Galio",
		"Twisted Fate",
		"Xin Zhao",
		"Urgot",
		"LeBlanc",
		"Vladimir",
		"Fiddlesticks",
		"Kayle",
		"Master Yi",
		"Alistar",
		"Ryze",
		"Sion",
		"Sivir",
		"Soraka",
		"Teemo",
		"Tristana",
		"Warwick",
		"Nunu & Willump",
		"Miss Fortune",
		"Ashe",
		"Tryndamere",
		"Jax",
		"Morgana",
		"Zilean",
		"Singed",
		"Evelynn",
		"Twitch",
		"Karthus",
		"Cho'Gath",
		"Amumu",
		"Rammus",
		"Anivia",
		"Shaco",
		"Dr. Mundo",
		"Sona",
		"Kassadin",
		"Irelia",
		"Janna",
		"Gangplank",
		"Corki",
		"Karma",
		"Taric",
		"Veigar",
		"Trundle",
		"Swain",
		"Caitlyn",
		"Blitzcrank",
		"Malphite",
		"Katarina",
		"Nocturne",
		"Maokai",
		"Renekton",
		"Jarvan IV",
		"Elise",
		"Orianna",
		"Wukong",
		"Brand",
		"Lee Sin",
		"Vayne",
		"Rumble",
		"Cassiopeia",
		"Skarner",
		"Heimerdinger",
		"Nasus",
		"Nidalee",
		"Udyr",
		"Poppy",
		"Gragas",
		"Pantheon",
		"Ezreal",
		"Mordekaiser",
		"Yorick",
		"Akali",
		"Kennen",
		"Garen",
		"Leona",
		"Malzahar",
		"Talon",
		"Riven",
		"Kog'Maw",
		"Shen",
		"Lux",
		"Xerath",
		"Shyvana",
		"Ahri",
		"Graves",
		"Fizz",
		"Volibear",
		"Rengar",
		"Varus",
		"Nautilus",
		"Viktor",
		"Sejuani",
		"Fiora",
		"Ziggs",
		"Lulu",
		"Draven",
		"Hecarim",
		"Kha'Zix",
		"Darius",
		"Jayce",
		"Lissandra",
		"Diana",
		"Quinn",
		"Syndra",
		"Aurelion Sol",
		"Kayn",
		"Zoe",
		"Zyra",
		"Kai'Sa",
		"Seraphine",
		"Gnar",
		"Zac",
		"Yasuo",
		"Vel'Koz",
		"Taliyah",
		"Camille",
		"Akshan",
		"Bel'Veth",
		"Braum",
		"Jhin",
		"Kindred",
		"Zeri",
		"Jinx",
		"Tahm Kench",
		"Briar",
		"Viego",
		"Senna",
		"Lucian",
		"Zed",
		"Kled",
		"Ekko",
		"Qiyana",
		"Vi",
		"Aatrox",
		"Nami",
		"Azir",
		"Yuumi",
		"Samira",
		"Thresh",
		"Illaoi",
		"Rek'Sai",
		"Ivern",
		"Kalista",
		"Bard",
		"Rakan",
		"Xayah",
		"Ornn",
		"Sylas",
		"Neeko",
		"Aphelios",
		"Rell",
		"Pyke",
		"Vex",
		"Yone",
		"Sett",
		"Lillia",
		"Gwen",
		"Renata Glasc",
		"Nilah",
		"K'Sante",
		"Smolder",
		"Milio",
		"Hwei",
		"Naafiri"
	];
	#endregion
	static readonly List<string> guesses;
	public static readonly Dictionary<string, GuessType> guessTypes = [];
	public static readonly Dictionary<string, string> guessFullNames = [];
	public static Skin[] skins;

	static Skin() {
		foreach (KeyValuePair<string, string> pair in new Dictionary<string, string>() {
			{ "tf", "Twisted Fate" },
			{ "fiddle", "Fiddlesticks" },
			{ "yi", "Master Yi" },
			{ "nunu", "Nunu & Willump" },
			{ "mf", "Miss Fortune" },
			{ "chogath", "Cho'Gath" },
			{ "mundo", "Dr. Mundo" },
			{ "jarvan", "Jarvan IV" },
			{ "lee", "Lee Sin" },
			{ "heimer", "Heimerdinger" },
			{ "morde", "Mordekaiser" },
			{ "kogmaw", "Kog'Maw" },
			{ "khazix", "Kha'Zix" },
			{ "aurelion", "Aurelion Sol" },
			{ "asol", "Aurelion Sol" },
			{ "kaisa", "Kai'Sa" },
			{ "velkoz", "Vel'Koz" },
			{ "belveth", "Bel'Veth" },
			{ "tahm", "Tahm Kench" },
			{ "reksai", "Rek'Sai" },
			{ "renata", "Renata Glasc" },
			{ "ksante", "K'Sante" }
		}) {
			guessTypes[pair.Key] = GuessType.Champion;
			guessFullNames[pair.Key] = pair.Value;
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
