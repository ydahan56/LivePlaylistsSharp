# LivePlaylistsSharp
This is an open-source clone in C# of the [LivePlaylists](https://www.facebook.com/LivePlaylists) project by [Yaniv Lerman](https://www.facebook.com/yaniv.lerman). This project was written for my own personal educational purpose only, I was bored so decided to write my own version of this cute little app.

# How it works?
The program monitors the channel every 30 seconds, it saves an 8 second chunk (128KB in size) of the stream and uploads it to the [AudD.io API](https://docs.audd.io/#recognize) for recognition. If no song was recognized (due to a broadcast), then the method returns without doing nothing and waits for next execution.

# Playlists
- [Galgalatz - 100 Last Songs](https://open.spotify.com/playlist/5mLHWcR8C3ObKYdKxTyzyY?si=7bbc1536145c40f0)
- [Kan 88 (כאן 88)](https://open.spotify.com/playlist/3SpUq03whlfRMwEHMRulNy?si=a8a4e73a2de84977)

# Extending channels
Extending support to more channels is quite easy. You create a new *class* with the channel name (and "Channel" suffix) in the "**Channels**" folder and inherit from `Channel`. Then, you supply three properties. A url of the stream (`StreamUrl`), channel name (`ChannelName`) and the id of the spotify playlist (`PlaylistId`) initialized at ctor.
