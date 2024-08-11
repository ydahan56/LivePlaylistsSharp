using DotNetEnv;
using FluentScheduler;
using LivePlaylistsClone.Channels;
using LivePlaylistsClone.Playlists;
using System;

Env.Load();

JobManager.Initialize(
    new Channel(
        new GlglzChannel(), [
            new SpotifyPlaylist("5mLHWcR8C3ObKYdKxTyzyY")
        ]
    ),
    new Channel(
        new Kan88Channel(), [
            new SpotifyPlaylist("3SpUq03whlfRMwEHMRulNy")
        ]
    )
);

Console.ReadKey();
