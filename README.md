# LivePlaylistsSharp
This is an open-source clone in C# influenced by the [LivePlaylists](https://www.facebook.com/LivePlaylists) project by [Yaniv Lerman](https://www.facebook.com/yaniv.lerman). This is not a replacement of the original work; it's a side project I opened source to let streamers continue enjoy the service and encourage other curious developers to observe the code and learn from it, and perhaps influence others to pull off their own variation of the project.

# How it works? Strategy explained...
The program monitors the channel every fixed interval in seconds; it then saves a chunk of 128KB in size (00:08 seconds in length) of the stream and then uploads it to [Shazam](https://github.com/AlekseyMartynov/shazam-for-real) for recognition (this strategy is used in order to reduce API rate limits and cost for fallback). After a few tries, if not recognition was successful, we fallback to [AudD.io API](https://docs.audd.io/#recognize) for recognition as a backup strategy. If no song was recognized (due to a broadcast) after all strategies were executed, then the method returns without doing anything and waits for next execution. Back to the starting point

# Playlists
- In the end, I decided not to maintain my own playlists. At least for now, because I do not believe it is needed as Mr. Lerman himself is hosting his own playlists with the [PLAYingLIST.net](https://playinglist.net/) project, and because of redundant financial it will cost me. I prefer to put my effort into maintaining the open source project instead of hosting my own playlists and servers.

# Extending channels
Extending support to more channels is quite easy. You create a new *class* with the channel name (and "Channel" suffix) in the "**Channels**" folder and inherit from `Channel`. Then, you supply three properties. A url of the stream (`StreamUrl`), channel name (`ChannelName`) and the id of the spotify playlist (`PlaylistId`) initialized at ctor.

# Roadmap
- I was thinking of going hard and developing a special algorithm that will analyze the sound waves of the audio binary file in order to detect certain patterns and extract interesting parameters, like if the current stream is a broadcast, breaking news, traffic highlights, or a song. This is a clever strategy that could eliminate the need for a redundant API call, which could be cost-friendly. I have been reading a little here and there, doing some small research, but I think ffmpeg could be of use. This is something that I'm still not certain about, but it's just an idea.I was thinking of going hard and developing a special algorithm that will analyze the sound waves of the audio binary file in order to detect certain patterns and extract interesting parameters, like if the current stream is a broadcast, breaking news, traffic highlights, or a song. This is a clever strategy that could eliminate the need for a redundant API call, which could be cost-friendly. I have been reading a little here and there, doing some small research, but I think ffmpeg could be of use. This is something that I'm still not certain about, but it's just an idea.
- Implement [Odesli](https://odesli.co/); this provides direct link support to all major streaming providers. This eliminates the need to perform a redundant search and directly paste the song to the playlist without heading. 

# Credits
[AlekseyMartynov](https://github.com/AlekseyMartynov) - For the Shazam client [shazam-for-real](https://github.com/AlekseyMartynov/shazam-for-real), and the inspiration from the profile picture.
