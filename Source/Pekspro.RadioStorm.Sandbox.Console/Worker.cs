using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Pekspro.RadioStorm.CacheDatabase;
using Pekspro.RadioStorm.CacheDatabase.Models;
using Pekspro.RadioStorm.DataFetcher;
using Pekspro.RadioStorm.Options;
using Pekspro.RadioStorm.Settings.SynchronizedSettings;
using Pekspro.RadioStorm.Settings.SynchronizedSettings.Favorite;
using Pekspro.RadioStorm.Settings.SynchronizedSettings.FileProvider.Graph;
using Pekspro.RadioStorm.UI.Model.Channel;
using Pekspro.RadioStorm.Utilities;

namespace Pekspro.RadioStorm.Sandbox.Console;

public sealed class Worker : BackgroundService
{
    public Worker(IServiceProvider serviceProvider, IHostApplicationLifetime hostApplicationLifetime)
    {
        ServiceProvider = serviceProvider;
        HostApplicationLifetime = hostApplicationLifetime;
    }

    public IServiceProvider ServiceProvider { get; }
    public IHostApplicationLifetime HostApplicationLifetime { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        {
            //IGraphHelper graphSignInHelper = ServiceProvider.GetRequiredService<IGraphHelper>();
            //await graphSignInHelper.InitAsync();

            IMessenger messenger = ServiceProvider.GetRequiredService<IMessenger>();
            messenger.Register<ProviderStateChangedEventArgs>(this, (a, b) =>
            {
                Log("New Graph state: " + b.NewState);
            });
        }  
        
        {

            Bootstrap.Bootstrap bootstrap = ServiceProvider.GetRequiredService<Bootstrap.Bootstrap>();
            await bootstrap.SetupAsync();
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            PrepareMenu();
            System.Console.WriteLine("1. Check Graph status");
            System.Console.WriteLine("2. Sign in into Graph");
            System.Console.WriteLine("3. Sign out from Graph");
            System.Console.WriteLine("4. Fetch channels");
            System.Console.WriteLine("5. Fetch episodes");
            System.Console.WriteLine("6. Prefetch");
            System.Console.WriteLine("7. Benchmark: Insert");
            System.Console.WriteLine("a. Download images");

            try
            {
                ConsoleKeyInfo key = GetKey();

                switch (key.KeyChar)
                {
                    case '1':
                        await GraphStatusAsync(stoppingToken);
                        break;
                    case '2':
                        await GraphSignInAsync(stoppingToken);
                        break;
                    case '3':
                        await GraphSignOutAsync(stoppingToken);
                        break;
                    case '4':
                        await FetchChannelsAsync(stoppingToken);
                        break;
                    case '5':
                        await FetchEpisodesAsync(stoppingToken);
                        break;
                    case '6':
                        await PrefetchAsync(stoppingToken);
                        break;
                    case '7':
                        await BenchmarkInsertAsync(stoppingToken);
                        break;
                    case 'a':
                        await DownloadImagesAsync(stoppingToken);
                        break;
                }
            }
            catch (EscapeException)
            {
                Log();
                Log("Bye, bye!");
                Log();

                HostApplicationLifetime.StopApplication();
                return;
            }
            catch (Exception e)
            {
                Log();
                Log("Something bad happen: " + e.Message);
                Log();

                HostApplicationLifetime.StopApplication();
                return;
            }
        }
    }

    private async Task GraphStatusAsync(CancellationToken stoppingToken)
    {
        System.Console.Clear();

        IGraphHelper graphSignInHelper = ServiceProvider.GetRequiredService<IGraphHelper>();
        StorageLocations storageLocations = ServiceProvider.GetRequiredService<IOptions<StorageLocations>>().Value;
        Log();
        Log($"Cache file: {Path.Combine(storageLocations.LocalSettingsPath, CacheSettings.CacheFileName)}");
        if (graphSignInHelper.IsSignedIn)
        {
            Log("Your are signed in.");

            var client = await graphSignInHelper.GetClientAsync();

            // var me = await client.Me.Request().GetAsync(stoppingToken);

            // Log($"Your ID: {me.Id}");
        }
        else
        {
            Log("Your are not signed in.");
        }
    }

    private async Task GraphSignInAsync(CancellationToken stoppingToken)
    {
        System.Console.Clear();
        Log();

        IGraphHelper graphSignInHelper = ServiceProvider.GetRequiredService<IGraphHelper>();
        if (graphSignInHelper.IsSignedIn)
        {
            Log("Your are already signed in.");
        }
        else
        {
            Log("Your are not signed in. Trying sign in via cache...");

            await graphSignInHelper.SignInViaCacheAsync();

            if (graphSignInHelper.IsSignedIn)
            {
                Log("Signed in via cache successfully.");
            }
            else
            {
                Log("Signing in via user...");

                await graphSignInHelper.SignIn();

                if (graphSignInHelper.IsSignedIn)
                {
                    Log("Signed in successfully.");
                }
                else
                {
                    Log("Signed in failed.");
                }
            }
        }
    }

    private async Task GraphSignOutAsync(CancellationToken stoppingToken)
    {
        System.Console.Clear();
        Log();

        IGraphHelper graphSignInHelper = ServiceProvider.GetRequiredService<IGraphHelper>();
        if (graphSignInHelper.IsSignedIn)
        {
            Log("Your are signed in. Signing out...");

            await graphSignInHelper.SignOut();

            Log("Your are now signed out.");
        }
        else
        {
            Log("Your are not signed in.");

        }
    }

    private async Task FetchChannelsAsync(CancellationToken stoppingToken)
    {
        System.Console.Clear();
        Log();

        IDataFetcher dataFetcher = ServiceProvider.GetRequiredService<IDataFetcher>();
        IDateTimeProvider dateTimeProvider = ServiceProvider.GetRequiredService<IDateTimeProvider>();
        bool allowCache = GetBool("Allow cache (y/n): ");

        Log("Gettings channels...");

        var channels = await dataFetcher.GetChannelsAsync(true, stoppingToken);

        Log($"Got {channels.Count()} channels...");

        foreach (var channel in channels)
        {
            Log($"Fetching {channel.Title}...");

            var songs = await dataFetcher.GetChannelSongListAsync(channel.ChannelId, allowCache, stoppingToken);
            var scheduledEpisodes = await dataFetcher.GetScheduledEpisodeListAsync(channel.ChannelId, DateOnly.FromDateTime(dateTimeProvider.SwedishNow), allowCache, stoppingToken);

            Log($"{channel.Title} har {songs?.Count ?? 0} songs and {scheduledEpisodes?.Count ?? 00} scheduled episodes today.");
        }
    }

    private async Task FetchEpisodesAsync(CancellationToken stoppingToken)
    {
        System.Console.Clear();
        Log();

        IDataFetcher dataFetcher = ServiceProvider.GetRequiredService<IDataFetcher>();
        bool allowCache = GetBool("Allow cache (y/n): ");
        int programId = GetInteger("Program id (0 for all): ");

        IList<ProgramData> programs;

        Log("Gettings program...");

        if (programId == 0)
        {
            programs = await dataFetcher.GetProgramsAsync(allowCache, stoppingToken);

            Log($"Got {programs.Count} programs.");
        }
        else
        {
            var program = await dataFetcher.GetProgramAsync(programId, allowCache, stoppingToken);
            programs = new ProgramData[] { program };

            Log($"Got program {program.Name}.");
        }

        foreach (var program in programs)
        {
            Log($"Fetching {program.Name} episodes...");

            var episodes = await dataFetcher.GetEpisodesAsync(program.ProgramId, false, allowCache, stoppingToken);

            Log($"{program.Name} has {episodes.Episodes.Length} episodes...");

            foreach (var episode in episodes.Episodes)
            {
                var songs = await dataFetcher.GetEpisodeSongListAsync(episode.EpisodeId, allowCache, stoppingToken);

                Log($"{program.Name} has {episode.Title} has {songs.Count} songs.");
            }
        }
    }


    private async Task PrefetchAsync(CancellationToken stoppingToken)
    {
        ICachePrefetcher cachePrefetcher = ServiceProvider.GetRequiredService<ICachePrefetcher>();

        await cachePrefetcher.PrefetchAsync(stoppingToken);
    }


    private async Task BenchmarkInsertAsync(CancellationToken stoppingToken)
    {
        System.Console.Clear();
        Log();

        var episodeSongItems = new List<EpisodeSongListItemData>();
        for (int i = 0; i < 10000; i++)
        {
            episodeSongItems.Add(new EpisodeSongListItemData
            {
                EpisodeId = i,
                Artist = "Artist " + i,
                Title = "Title " + i,
                AlbumName = "Alb " + i,
                EpisodeSongListItemDataId = i
            });
        };

        System.Console.WriteLine($"Inserting {episodeSongItems.Count} items... Make sure logging is disabled.");

        Stopwatch stopwatchBatch, stopwatchNormal;

        ICacheDatabaseContextFactory factory = ServiceProvider.GetRequiredService<ICacheDatabaseContextFactory>();
        {
            using var contextBatch = factory.Create();
            await contextBatch.EpisodeSongListItemData.ExecuteDeleteAsync(stoppingToken);
            
            stopwatchBatch = Stopwatch.StartNew();
            await contextBatch.BulkInsertAsync(episodeSongItems, cancellationToken: stoppingToken);
            stopwatchBatch.Stop();

            await contextBatch.EpisodeSongListItemData.ExecuteDeleteAsync(stoppingToken);
        }

        {
            using var contextNormal = factory.Create();

            stopwatchNormal = Stopwatch.StartNew();
            contextNormal.EpisodeSongListItemData.AddRange(episodeSongItems);
            await contextNormal.SaveChangesAsync(stoppingToken);
            stopwatchNormal.Stop();

            await contextNormal.EpisodeSongListItemData.ExecuteDeleteAsync(stoppingToken);
        }

        await Task.Delay(1000);

        System.Console.WriteLine($"{stopwatchBatch.ElapsedMilliseconds} ms with BatchInsert.");
        System.Console.WriteLine($"{stopwatchNormal.ElapsedMilliseconds} ms with normal insert.");
    }

    private async Task DownloadImagesAsync(CancellationToken stoppingToken)
    {
        System.Console.Clear();
        Log();

        string basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "RadioStormImages");
        
        IDataFetcher dataFetcher = ServiceProvider.GetRequiredService<IDataFetcher>();

        // Download channel images
        string channelImagePath = Path.Combine(basePath, "Channel");
        Directory.CreateDirectory(channelImagePath);

        var channels = await dataFetcher.GetChannelsAsync(true);
        foreach (var channel in channels)
        {
            string imageUri = channel.ChannelImageHighResolution;

            if (imageUri is not null)
            {
                Uri uri = new Uri(imageUri);
                string targetFileName = $"{channel.Title.Trim()}";

                targetFileName = string.Join("_", targetFileName.Split(Path.GetInvalidFileNameChars()));

                await DownloadImageAsync(uri, Path.Combine(channelImagePath, $"{targetFileName}{Path.GetExtension(uri.LocalPath)}"));
            }
        }

        // Download program images
        string programImagePath = Path.Combine(basePath, "Program");
        Directory.CreateDirectory(programImagePath);

        var programs = await dataFetcher.GetProgramsAsync(true);
        foreach (var program in programs)
        {
            string imageUri = program.ProgramImageHighResolution;

            if (imageUri is not null)
            {
                Uri uri = new Uri(imageUri);
                string targetFileName = $"{program.Name.Trim()}";
                
                targetFileName = string.Join("_", targetFileName.Split(Path.GetInvalidFileNameChars()));
                
                await DownloadImageAsync(uri, Path.Combine(programImagePath, $"{targetFileName}{Path.GetExtension(uri.LocalPath)}"));
            }
        }
    }

    private async Task DownloadImageAsync(Uri url, string fileName)
    {
        using var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync(url);
        using var stream = await response.Content.ReadAsStreamAsync();
        using var fileStream = File.Create(fileName);
        await stream.CopyToAsync(fileStream);
    }

    private async Task RunUglyCode(CancellationToken stoppingToken)
    {
        WeakReferenceMessenger.Default.Register<ChannelFavoriteChangedMessage>(this, (r, m) =>
        {
            // Handle the message here, with r being the recipient and m being the
            // input message. Using the recipient passed as input makes it so that
            // the lambda expression doesn't capture "this", improving performance.

            Log($"ID: {m.Id} Added: {m.IsAdded}");
        });

        IChannelModelFactory channelModelFactory = ServiceProvider.GetRequiredService<IChannelModelFactory>();

        var channel1 = channelModelFactory.Create(new ChannelData()
        {
            ChannelId = 1
        });
        var channel2 = channelModelFactory.Create(new ChannelData()
        {
            ChannelId = 2
        });


        //var sharedSettingsManager = ServiceProvider.GetRequiredService<SharedSettingsManager>();
        //sharedSettingsManager.Init(true);
        //await sharedSettingsManager.ReadLocalSettingsAsync();
        //RadioStormSandboxToolExtensions.SetupFakeRoamingFileProviders(ServiceProvider);

        //var bootstrap = ServiceProvider.GetRequiredService<Bootstrap.Bootstrap>();
        //await bootstrap.SetupAsync(false);

        //var channelFav = ServiceProvider.GetRequiredService<IChannelFavoriteList>();
        //channelFav.SetFavorite(1, false);
        //channelFav.SetFavorite(1, true);

        //return;

        //RecentPlayedManager recentPlayedManager = new RecentPlayedManager();
        //recentPlayedManager.Init(true);
        //await recentPlayedManager.ReadLocalSettingsAsync();
        //recentPlayedManager.AddOrUpdate(true, 1);
        //recentPlayedManager.AddOrUpdate(false, 2);
        //recentPlayedManager.AddOrUpdate(true, 3);
        //await recentPlayedManager.SaveIfDirtyAsync();

        /*
        ListenStateManager listenStateManager = new ListenStateManager();
        listenStateManager.Init(true);
        await listenStateManager.ReadLocalSettingsAsync();
        listenStateManager.SetListenLength(123, 456);
        await listenStateManager.SaveDirtySlotsAsync();
        */

        //EpisodesSortOrderManager episodesSortOrderManager = new EpisodesSortOrderManager();
        //episodesSortOrderManager.Init(true);
        //await episodesSortOrderManager.ReadLocalSettingsAsync();
        //episodesSortOrderManager.SetFavorite(1, true, FavoriteListChangeSource.ContextMenu);
        //episodesSortOrderManager.SetFavorite(2, false, FavoriteListChangeSource.Selection);
        //episodesSortOrderManager.SetFavorite(3, true, FavoriteListChangeSource.Swipe);
        //await episodesSortOrderManager.SaveIfDirtyAsync();


        /*FavoriteManager favoriteManager = new FavoriteManager();
        favoriteManager.Init(true);
        await favoriteManager.ReadLocalSettingsAsync();

        favoriteManager.Channels.SetFavorite(1, true, FavoriteListChangeSource.ContextMenu);
        favoriteManager.Programs.SetFavorite(11, true, FavoriteListChangeSource.Selection);

        await favoriteManager.Channels.SaveIfDirtyAsync();
        await favoriteManager.Programs.SaveIfDirtyAsync();*/

        /*
        FavoriteList favoriteList = new FavoriteList();
        favoriteList.Init("favorites", true, "Favorites");
        await favoriteList.ReadLocalSettingsAsync();
        favoriteList.SetFavorite(1, true, FavoriteListChangeSource.ContextMenu);
        favoriteList.SetFavorite(2, false, FavoriteListChangeSource.Selection);
        favoriteList.SetFavorite(3, true, FavoriteListChangeSource.Swipe);
        await favoriteList.SaveIfDirtyAsync();
        */


        var dataFetcher = ServiceProvider.GetService<IDataFetcher>();
        var c = await dataFetcher.GetChannelsAsync(true);
        bool c2 = c.Any(b => b.Title == null);
        var p = await dataFetcher.GetProgramsAsync(true);
        await dataFetcher.FetchAllProgramsAndAllEpisodesAsync();

        //CacheDatabaseHelper helper = new();
        //await helper.MigrateAsync();

        //CacheDatabaseManager cacheDatabaseManager = new();
        //var chan =  await cacheDatabaseManager.GetChannels();


        //var ch = await DataFetcher.GetChannels();

        //while (!stoppingToken.IsCancellationRequested)
        //{
        //    _logger.LogInformation("Worker running at: {time}", dateTimeProvider.OffsetNow);
        //    await Task.Delay(1000, stoppingToken);
        //}
    }

    /*

    static async Task TestChannels()
    {
        var manager = new CacheDatabaseManager();

        ChannelData channelData = new ChannelData()
        {
            ChannelId = 76,
            Title = "je"
        };

        await manager.InsertOrUpdateChannel(channelData);

        ChannelData[] channelDatas = new ChannelData[]
        {
            new ChannelData()
            {
                ChannelId = 701,
                Title = "Åäö 701"
            },
            new ChannelData()
            {
                ChannelId = 702,
                Title = "Åäö 702"
            },
            new ChannelData()
            {
                ChannelId = 703,
                Title = "Åäö 703"
            }
        };

        await manager.InsertOrUpdateChannels(channelDatas);
        var p1 = await manager.GetChannel(-1);
        var p701 = await manager.GetChannel(701);
        var progs = await manager.GetChannels();

        var syncTimeProg = await manager.GetLatestListSyncTime(ListSyncStatusData.ListType.Channels);
        var syncTimeChan = await manager.GetLatestListSyncTime(ListSyncStatusData.ListType.Channels);

        var syncTimeEp = await manager.GetEpisodeListSyncStatus(2);
        var syncTimeEpX = await manager.GetEpisodeListSyncStatus(2222);


        var songList = new List<ChannelSongListItemData>()
        {
            new ChannelSongListItemData()
            {
                ChannelSongListItemDataId = 761,
                Title = "Song 761",
            },
            new ChannelSongListItemData()
            {
                ChannelSongListItemDataId = 762,
                Title = "Song 762",
            },
            new ChannelSongListItemData()
            {
                ChannelSongListItemDataId = 763,
                Title = "Song 763",
            }
        };

        await manager.InsertOrUpdateChannelSongListItems(76, songList);

        songList = new List<ChannelSongListItemData>()
        {
            new ChannelSongListItemData()
            {
                ChannelSongListItemDataId = 771,
                Title = "Song 771",
            },
            new ChannelSongListItemData()
            {
                ChannelSongListItemDataId = 772,
                Title = "Song 772",
            }
        };

        await manager.InsertOrUpdateChannelSongListItems(77, songList);

        var list76 = await manager.GetChannelsSongListItems(76);
        var list76status = await manager.GetLatestChannelSongListSyncTime(76);
        await manager.DeleteChannelSongList(77);


        ChannelStatusData channelStatusData = new ChannelStatusData()
        {
            ChannelId = 76,
            CurrentProgram = "hello"
        };

        await manager.InsertOrUpdateChannelStatus(channelStatusData);

        ChannelStatusData[] channelStatusesData = new ChannelStatusData[]
        {
            new ChannelStatusData()
            {
                ChannelId = 701,
                CurrentProgram = "Åäö 701"
            },
            new ChannelStatusData()
            {
                ChannelId = 702,
                CurrentProgram = "Åäö 702"
            },
            new ChannelStatusData()
            {
                ChannelId = 703,
                CurrentProgram = "Åäö 703"
            }
        };

        await manager.InsertOrUpdateChannelStatuses(channelStatusesData);

        var status1 = await manager.GetChannelStatusData(701);
        var status2 = await manager.GetChannelStatusData(-701);
        var statuses = await manager.GetChannelStatusesData();

        List<ScheduledEpisodeListItemData> scheduleds = new List<ScheduledEpisodeListItemData>()
        {
            new ScheduledEpisodeListItemData()
            {
                ScheduledEpisodeDataId = 761,
                Title = "hi 1"
            },
            new ScheduledEpisodeListItemData()
            {
                ScheduledEpisodeDataId = 762,
                Title = "hi 2"
            }
        };

        await manager.InsertOrUpdateScheduledEpisodeListItemData(76, DateTime.Today, scheduleds);

        scheduleds = new List<ScheduledEpisodeListItemData>()
        {
            new ScheduledEpisodeListItemData()
            {
                ScheduledEpisodeDataId = 771,
                Title = "hi 1"
            },
            new ScheduledEpisodeListItemData()
            {
                ScheduledEpisodeDataId = 772,
                Title = "hi 2"
            }
        };
        await manager.InsertOrUpdateScheduledEpisodeListItemData(77, DateTime.Today, scheduleds);

        var c1 = await manager.GetScheduledEpisodListItemDataItems(76, DateTime.Today);
        var c2 = await manager.GetScheduledEpisodListItemDataItems(76, DateTime.Today.AddDays(1));
        var c3 = await manager.GetScheduledEpisodListItemDataItems(77, DateTime.Today);
        
        var d1 = await manager.GetLatestScheduledEpisodListSyncTime(76, DateTime.Today);
        var d2 = await manager.GetLatestScheduledEpisodListSyncTime(76, DateTime.Today.AddDays(1));
        var d3 = await manager.GetLatestScheduledEpisodListSyncTime(77, DateTime.Today);

        await manager.DeleteObseleteScheduledEpisodeListsAsync();
        await manager.DeleteObseleteSongListsAsync();
    }


    static async Task TestPrograms()
    {
        var manager = new CacheDatabaseManager();

        ProgramData programData = new ProgramData()
        {
            ProgramId = 76,
            Name = "je"
        };

        await manager.InsertOrUpdateProgram(programData);

        ProgramData[] programDatas = new ProgramData[]
        {
            new ProgramData()
            {
                ProgramId = 701,
                Name = "Åäö 701"
            },
            new ProgramData()
            {
                ProgramId = 702,
                Name = "Åäö 702"
            },
            new ProgramData()
            {
                ProgramId = 703,
                Name = "Åäö 703"
            }
        };

        await manager.InsertOrUpdatePrograms(programDatas);
        var p1 = await manager.GetProgram(-1);
        var p701 = await manager.GetProgram(701);
        var progs = await manager.GetPrograms();

        var syncTimeProg = await manager.GetLatestListSyncTime(ListSyncStatusData.ListType.Programs);
        var syncTimeChan = await manager.GetLatestListSyncTime(ListSyncStatusData.ListType.Channels);

        var syncTimeEp = await manager.GetEpisodeListSyncStatus(2);
        var syncTimeEpX = await manager.GetEpisodeListSyncStatus(2222);
    }


    static async Task TestEpisodes()
    {
        EpisodeData episodeData = new EpisodeData()
        {
            EpisodeId = 76,
            Title = "je"
        };

        var manager = new CacheDatabaseManager();
        await manager.InsertOrUpdateEpisode(episodeData);

        episodeData = new EpisodeData()
        {
            EpisodeId = 79,
            Title = "pe"
        };

        await manager.InsertOrUpdateEpisode(episodeData);

        var episodes = new List<EpisodeData>()
        {
            new EpisodeData()
            {
                EpisodeId = 701,
                ProgramId = 2,
                AudioDownloadUrl = "a701",
                PublishDate = new DateTimeOffset(new DateTime(2021, 07, 01))
            },
            new EpisodeData()
            {
                EpisodeId = 702,
                ProgramId = 2,
                AudioDownloadUrl = "a702",
                PublishDate = new DateTimeOffset(new DateTime(2021, 07, 02))
            },
            new EpisodeData()
            {
                EpisodeId = 703,
                ProgramId = 2,
                AudioDownloadUrl = "a703",
                PublishDate = new DateTimeOffset(new DateTime(2021, 07, 03))
            }
        };

        EpisodeListSyncStatusData episodeListSyncStatusData = new EpisodeListSyncStatusData()
        {
            ProgramId = 2,
            Status = EpisodeListSyncStatusData.SyncStatus.FullySynchronized
        };

        await manager.InsertOrUpdateEpisodes(2, episodes, episodeListSyncStatusData);

        await manager.InsertOrUpdateEpisodes(episodes);
        var e1 = await manager.GetEpisode(76);
        var e2 = await manager.GetEpisode(67);

        var p10 = await manager.GetPreviousEpisode(2, new DateTimeOffset(new DateTime(2021, 07, 01)));
        var p11 = await manager.GetPreviousEpisode(2, new DateTimeOffset(new DateTime(2021, 07, 02)));
        var p12 = await manager.GetPreviousEpisode(2, new DateTimeOffset(new DateTime(2021, 07, 03)));
        var p13 = await manager.GetPreviousEpisode(2, new DateTimeOffset(new DateTime(2021, 07, 04)));

        var n10 = await manager.GetNextEpisode(2, new DateTimeOffset(new DateTime(2021, 07, 01)));
        var n11 = await manager.GetNextEpisode(2, new DateTimeOffset(new DateTime(2021, 07, 02)));
        var n12 = await manager.GetNextEpisode(2, new DateTimeOffset(new DateTime(2021, 07, 03)));
        var n13 = await manager.GetNextEpisode(2, new DateTimeOffset(new DateTime(2021, 07, 04)));
        
        var match1 = await manager.GetFirstMatchingEpisode(new int?[] { 2, 3 }, new int[] { 701 }, true);
        var latest = await manager.GetLatestEpisode(2);
        var eplist = await manager.GetEpisodes(2);
        var eplist2 = await manager.GetEpisodes(new int[] { 701, 703, 705 });
        // var match1 = await manager.GetFirstMatchingEpisode(new int?[] { 2 }, new int[] {}, true);


        var songList = new List<EpisodeSongListItemData>()
        {
            new EpisodeSongListItemData()
            {
                EpisodeSongListItemDataId = 761,
                Title = "Song 761",
            },
            new EpisodeSongListItemData()
            {
                EpisodeSongListItemDataId = 762,
                Title = "Song 762",
            },
            new EpisodeSongListItemData()
            {
                EpisodeSongListItemDataId = 763,
                Title = "Song 763",
            }
        };

        await manager.InsertOrUpdateEpisodesSongListItems(76, songList);

        songList = new List<EpisodeSongListItemData>()
        {
            new EpisodeSongListItemData()
            {
                EpisodeSongListItemDataId = 771,
                Title = "Song 771",
            },
            new EpisodeSongListItemData()
            {
                EpisodeSongListItemDataId = 772,
                Title = "Song 772",
            }
        };

        await manager.InsertOrUpdateEpisodesSongListItems(77, songList);

        var list76 = await manager.GetEpisodesSongListItems(76);
        var list76status = await manager.GetLatestEpisodeSongListSyncTime(76);
        await manager.DeleteEpisodeSongList(77);

        await manager.DeleteObseleteEpisodesAsync(new List<int>()
        {
            2,

        }, new List<int>()
        {
            702
        });

    }
     
     */

    #region Tools

    private void PrepareMenu()
    {
        System.Console.WriteLine();
        System.Console.WriteLine();

        System.Console.WriteLine("What do you want do do?");
        System.Console.WriteLine();
    }

    private void Log(string text = null)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            System.Console.WriteLine();
            return;
        }

        System.Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " " + text);
    }

    private ILogger NullLogger
    {
        get
        {
            return new NullLogger<Worker>();
        }
    }

    private ConsoleKeyInfo GetKey()
    {
        var key = System.Console.ReadKey(true);

        if (key.Key == ConsoleKey.Escape)
        {
            throw new EscapeException();
        }

        return key;
    }

    private bool GetBool(string message)
    {
        while (true)
        {
            Log(message);

            var key = GetKey();

            if (key.KeyChar == '0' || key.KeyChar == 'n' || key.KeyChar == 'N')
            {
                return false;
            }

            if (key.KeyChar == '1' || key.KeyChar == 'y' || key.KeyChar == 'Y')
            {
                return true;
            }
        }
    }

    private int GetInteger(string message, Func<int, bool> validator = null)
    {
        while (true)
        {
            Log(message);

            var line = System.Console.ReadLine();

            if (int.TryParse(line.Trim(), out int res))
            {
                if (validator is null || validator(res))
                {
                    return res;
                }
            }
        }
    }

    public sealed class EscapeException : Exception
    {
        public EscapeException()
        {

        }
    }

    #endregion
}
