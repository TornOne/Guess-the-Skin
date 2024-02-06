using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

class UI {
	static Action<string, string>? ProcessMessage;

	[STAThread]
	static void Main() {
		List<Player> players = [];

		TcpListener tcpListener = new(System.Net.IPAddress.Any, 5814);
		tcpListener.Start();
		TcpClient discordClient = tcpListener.AcceptTcpClient();
		NetworkStream discordStream = discordClient.GetStream();
		_ = DiscordConnection(discordStream);
		tcpListener.Stop();

		void Send(string message) => discordStream.Write(Encoding.UTF8.GetBytes(message));

		const int width = 1215;
		const int height = 717;
		DockPanel windowContent = new() { Background = Brushes.Black };
		Grid displayArea = AddElement(new Grid() { Width = width, Height = height, ClipToBounds = true }, windowContent, Dock.Top);
		Image skinDisplay = AddElement(new Image(), displayArea);
		Grid playerGrid = AddElement(new Grid(), windowContent, Dock.Bottom);
		playerGrid.RowDefinitions.Add(new RowDefinition());
		playerGrid.RowDefinitions.Add(new RowDefinition());
		playerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
		Button nextButton = AddElement(new Button { Content = "Next skin", FontSize = 36, Background = Brushes.Black, Foreground = Brushes.White }, playerGrid);
		Grid.SetRowSpan(nextButton, 2);

		int skinIndex = 0;
		Skin currentSkin = null!;
		bool skinGuessed = false;
		bool championGuessed = false;
		bool roundInProgress = false;

		void Next() {
			if (roundInProgress) {
				roundInProgress = false;
			} else {
				roundInProgress = true;
				skinGuessed = false;
				championGuessed = false;
				currentSkin = Skin.skins[skinIndex++];
				players.ForEach(player => {
					player.championGuessesLeft = 10 / players.Count + (10 % players.Count == 0 ? 0 : 1);
					player.skinGuessesLeft = player.championGuessesLeft;
				});

				Task.Run(async () => {
					double xStart = Random.Shared.NextDouble() * width;
					double yStart = Random.Shared.NextDouble() * height;
					const int fps = 50;
					const int roundTime = 60;
					const int frames = fps * roundTime;
					for (int i = 1; i < frames && roundInProgress; i++) {
						double scale = (double)frames / i;
						skinDisplay.Dispatcher.Invoke(() => {
							if (i == 1) {
								skinDisplay.Source = new BitmapImage(new Uri(currentSkin.path));
							}
							skinDisplay.RenderTransform = new ScaleTransform(scale, scale, xStart, yStart);
						});
						await Task.Delay(1000 / fps);
					}
					skinDisplay.Dispatcher.Invoke(() => skinDisplay.RenderTransform = Transform.Identity);
					roundInProgress = false;
					Send($"Round over, the answer was\n{currentSkin.fullName}");
				});

			}
		}

		void RemovePlayer(Player player) {
			int removeIndex = players.IndexOf(player);
			players.RemoveAt(removeIndex);
			displayArea.Dispatcher.Invoke(() => {
				playerGrid.ColumnDefinitions.Remove(playerGrid.ColumnDefinitions[removeIndex]);
				playerGrid.Children.Remove(player.nameButton);
				playerGrid.Children.Remove(player.scoreBox);
			});
		}

		nextButton.Click += (_, _) => Next();
		List<string> adminPermissions = new(File.ReadAllLines("admins.txt"));

		ProcessMessage = (userId, message) => {
			#region Various commands
			if (message == "!help") {
				Send("""
					Possible commands:
					!join [Name] - joins the game as Name
					!leave - leaves the game
					!kick [Name] - removes Name from the game
					!next - ends round / starts next round
					!giveadmin [userId] - gives admin permissions to that user
					Any other text is a case-insensitive guess of the champion or skinline
					""");
				return;
			}

			if (message.StartsWith("!giveadmin ") && adminPermissions.Contains(userId)) {
				adminPermissions.Add(message[10..]);
				return;
			}

			if (message == "!next" && adminPermissions.Contains(userId)) {
				Next();
				return;
			}

			if (message.StartsWith("!join ")) {
				if (players.Exists(player => player.id == userId)) {
					Send("You have already joined");
					return;
				}

				string name = message.Split(' ', 2)[1];
				if (name.Length < 3 || name.Length > 6) {
					Send("Name must be 3-6 characters");
					return;
				} else if (players.Exists(player => player.name == name)) {
					Send("Name is already taken");
					return;
				}

				Player newPlayer = new(userId, name);
				players.Add(newPlayer);
				displayArea.Dispatcher.Invoke(() => {
					playerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
					newPlayer.scoreBox = AddElement(new TextBox { Text = "0", Background = Brushes.Black, Foreground = Brushes.White, FontSize = 36, TextAlignment = TextAlignment.Center }, playerGrid, 1, players.Count - 1);
					newPlayer.scoreBox.TextChanged += (_, _) => {
						if (int.TryParse(newPlayer.scoreBox.Text, out int value)) {
							newPlayer.points = value;
						}
					};
					newPlayer.nameButton = AddElement(new Button { FontSize = 36, Background = Brushes.Black, Foreground = Brushes.White }, playerGrid, 0, players.Count - 1);
					newPlayer.nameButton.Content = newPlayer.name;
					newPlayer.nameButton.Click += (_, _) => {
						newPlayer.points++;
						newPlayer.scoreBox.Text = newPlayer.points.ToString();
					};

					Grid.SetColumn(nextButton, players.Count);
				});
				return;
			}

			if (message.StartsWith("!kick ") && adminPermissions.Contains(userId)) {
				string kickName = message[6..];
				Player? kickPlayer = players.Find(player => player.name == kickName);
				if (kickPlayer is null) {
					Send("No player with such a name");
					return;
				}

				RemovePlayer(kickPlayer);
				return;
			}

			Player? player = players.Find(player => player.id == userId);

			if (message == "!leave") {
				if (player is null) {
					Send("You have not yet joined");
					return;
				}

				RemovePlayer(player);
				return;
			}
			#endregion

			#region Guesses
			if (!roundInProgress || player is null) {
				return;
			}

			//The message is the guess verbatim
			string guess = message.ToLower();

			//If the guess was partially correct (with no spelling errors, because we can't check for those for non-correct guesses), give a hint
			string skinline = currentSkin.skinName.ToLower();
			if (!skinGuessed && skinline.StartsWith(guess + ' ')) {
				Send($"Partially correct skinline guess:\n{currentSkin.skinName[..guess.Length]} \\_\\_\\_\\_\\_\\_");
				return;
			} else if (!skinGuessed && skinline.EndsWith(' ' + guess)) {
				Send($"Partially correct skinline guess:\n\\_\\_\\_\\_\\_\\_ {currentSkin.skinName[^guess.Length..]}");
				return;
			} else if (!skinGuessed && skinline.Contains(' ' + guess + ' ')) {
				Send($"Partially correct skinline guess:\n\\_\\_\\_\\_\\_\\_ {currentSkin.skinName.Substring(skinline.IndexOf(guess), guess.Length)} \\_\\_\\_\\_\\_\\_");
				return;
			}

			//Get a best-guess estimate what skin or champion the user tried to guess
			List<string> bestGuesses = Skin.BestGuess(guess);
			GuessType guessType = Skin.guessTypes[bestGuesses[Random.Shared.Next(bestGuesses.Count)]];
			if (guessType == GuessType.Champion) {
				if (player.championGuessesLeft == 0) {
					Send($"Sorry, out of champion guesses, {player.name}");
					return;
				}
				player.championGuessesLeft--;
			} else {
				if (player.skinGuessesLeft == 0) {
					Send($"Sorry, out of skin guesses, {player.name}");
					return;
				}
				player.skinGuessesLeft--;
			}
			//No cheating by guessing something that's close to multiple answers
			if (bestGuesses.Count != 1) {
				return;
			}

			string fullGuess = Skin.guessFullNames[bestGuesses[0]];
			if (!championGuessed && guessType == GuessType.Champion && currentSkin.championName == fullGuess) {
				championGuessed = true;
				if (skinGuessed) {
					roundInProgress = false;
				}
				player.points++;
				player.UpdatePoints();
				Send($"{player.name} guessed the champion\n");
			} else if (!skinGuessed && guessType == GuessType.Skinline && currentSkin.skinName == fullGuess && (fullGuess != "Original" || championGuessed)) {
				skinGuessed = true;
				if (championGuessed) {
					roundInProgress = false;
				}
				player.points++;
				player.UpdatePoints();
				Send($"{player.name} guessed the skin\n");
			}
			#endregion
		};

		new Application().Run(new Window {
			Title = "Guess the Skin",
			Content = windowContent,
			UseLayoutRounding = true,
			SizeToContent = SizeToContent.WidthAndHeight,
			ResizeMode = ResizeMode.NoResize
		});
	}

	static T AddElement<T>(T element, DockPanel parent, Dock dock) where T : UIElement {
		parent.Children.Add(element);
		DockPanel.SetDock(element, dock);
		return element;
	}

	static T AddElement<T>(T element, Grid grid, int row = 0, int column = 0) where T : UIElement {
		grid.Children.Add(element);
		Grid.SetRow(element, row);
		Grid.SetColumn(element, column);
		return element;
	}

	static async Task DiscordConnection(NetworkStream stream) {
		byte[] buffer = new byte[1024];

		while (true) {
			int readBytes = await stream.ReadAsync(buffer);
			string[] parts = Encoding.UTF8.GetString(buffer, 0, readBytes).Split(' ', 2);
			ProcessMessage?.Invoke(parts[0], parts[1]);
		}
	}
}

class Player(string id, string name) {
	public readonly string id = id, name = name;
	public int points = 0;
	public int championGuessesLeft = 0;
	public int skinGuessesLeft = 0;
	public TextBox scoreBox = null!;
	public Button nameButton = null!;

	public void UpdatePoints() => scoreBox.Dispatcher.Invoke(() => scoreBox.Text = points.ToString());
}
