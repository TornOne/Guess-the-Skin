<div align="justify">

# About
Guess the Skin is a guessing game I made for League of Legends skins, inspired by LoLdle. You are displayed a series of skin splash arts that slowly zoom out and you have to guess the skin and the champion as fast as possible.

It integrates with Discord, allowing you to compete against your friends in realtime.

# Setup
* Download the [latest release](https://github.com/TornOne/Guess-the-Skin/releases/latest)
* Install the [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (not the SDK, and not the regular Runtime)
* Get a Discord bot to run the game for you. It should not be too difficult to create one through the [Discord Developer Portal](https://discord.com/developers/applications).  
It should join the server you wish to play in, as well as have permissions to read and write messages in the channel(s) you want to play in.  
[Step 1 of the Getting Started guide](https://discord.com/developers/docs/getting-started#step-1-creating-an-app) might help you, if you're having trouble with this.  
Replace the contents of `Discord Bot/auth` with your bot's token.
* Replace the contents of `Discord Bot/admin` with your Discord User ID. Do the same with `Guess the Skin/admins.txt`. The latter file can have multiple admin IDs on separate lines.  
You can find people's User ID by going to Settings, Advanced, and enabling Developer Mode. You can then right click them and "Copy User ID".

The rest of the steps are optional, but you can do the following in the Guess the Skin folder:
* Run `Skin Collector.exe` to fetch any new skins that have been released. That program may break at any time, so don't count on it working.
* Add any new champions that have been released into `champions.txt`. As of writing, the newest added champion is Smolder.
* Add or remove any champion name aliases from `aliases.txt`. These are alternate spellings that are considered correct. (Note that minor misspellings are accepted regardless of these aliases.)

# Usage
* Run `Discord Bot/Discord Bot.exe` and `Guess the Skin/Guess the Skin.exe`, in either order.
* Send the message `!play` in a Discord channel that the bot has access to.
* If everything went well, the skin guesser UI should appear and the bot should write joining instructions into the channel.
* `!help` will have the bot respond with additional instructions.

Every round, a random skin is picked, and the image starts zooming out from a random point on the image.  
You have 1 minute to guess. The first to guess the skinline gets 1 point, and the first to guess the champion also gets 1 point.  
Every player has a total of 10 / (# of players) guesses, rounded up, for skinlines and champions separately. Minor spelling mistakes are accepted. Guessing one or more words present in the skinline name, but not the entire name, gives you a hint.  
For balancing purposes, "Original" (AKA base) skins of champions require the champion to be guessed first.

</div>
