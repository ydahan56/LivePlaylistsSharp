# LivePlaylistsSharp
This is an open-source clone in C# influenced by the [LivePlaylists](https://www.facebook.com/LivePlaylists) project by [Yaniv Lerman](https://www.facebook.com/yaniv.lerman). This is not a replacement of the original work; it's a side project I opened source to let streamers continue enjoy the service and encourage other curious developers to observe the code and learn from it, and perhaps influence others to pull off their own variation of the project.

# How it works?
The program monitors the channel every 30 seconds, it saves an 8 second chunk (128KB in size) of the stream and uploads it to the [AudD.io API](https://docs.audd.io/#recognize) for recognition. If no song was recognized (due to a broadcast), then the method returns without doing nothing and waits for next execution.

# Playlists
- In the end, I decided not to maintain my own playlists. At least for now, because I do not believe it is needed as Mr. Lerman himself is hosting his own playlists with the [PLAYingLIST.net](https://playinglist.net/) project, and because of redundant financial it will cost me. I prefer to put my effort into maintaining the open source project instead of hosting my own playlists and servers.

# Extending channels
Extending support to more channels is quite easy. You create a new *class* with the channel name (and "Channel" suffix) in the "**Channels**" folder and inherit from `Channel`. Then, you supply three properties. A url of the stream (`StreamUrl`), channel name (`ChannelName`) and the id of the spotify playlist (`PlaylistId`) initialized at ctor.

# Credits
[AlekseyMartynov](https://github.com/AlekseyMartynov) - For the Shazam client [shazam-for-real](https://github.com/AlekseyMartynov/shazam-for-real), and the inspiration from the profile picture ;D
