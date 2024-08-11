﻿using DotNetEnv;
using FluentScheduler;
using LivePlaylistsClone.Channels;
using LivePlaylistsClone.Playlists;
using System;
using System.Diagnostics;

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

// get current instance
var instance = Process.GetCurrentProcess();

// print pid
Console.WriteLine($"Agent Running... PID {instance.Id}");

// idle..
Console.ReadKey();
