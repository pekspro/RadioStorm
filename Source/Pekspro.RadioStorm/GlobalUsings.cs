﻿global using System;
global using System.Buffers.Binary;
global using System.Collections.Generic;
global using System.Diagnostics;
global using System.IO;
global using System.IO.Compression;
global using System.Linq;
global using System.Net.Http;
global using System.Security.Cryptography;
global using System.Text.RegularExpressions;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Text.Json;
global using System.Timers;
global using CommunityToolkit.Mvvm.Messaging;
global using EFCore.BulkExtensions;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Infrastructure;
global using Microsoft.EntityFrameworkCore.Migrations;
global using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Pekspro.RadioStorm.Audio;
global using Pekspro.RadioStorm.Audio.Message;
global using Pekspro.RadioStorm.Audio.Models;
global using Pekspro.RadioStorm.Bootstrap;
global using Pekspro.RadioStorm.Bootstrap.Message;
global using Pekspro.RadioStorm.CacheDatabase;
global using Pekspro.RadioStorm.CacheDatabase.Models;
global using Pekspro.RadioStorm.DataFetcher;
global using Pekspro.RadioStorm.Downloads;
global using Pekspro.RadioStorm.GeneralDatabase;
global using Pekspro.RadioStorm.GeneralDatabase.Models;
global using Pekspro.RadioStorm.Logging;
global using Pekspro.RadioStorm.Options;
global using Pekspro.RadioStorm.Settings;
global using Pekspro.RadioStorm.Settings.SynchronizedSettings;
global using Pekspro.RadioStorm.Settings.SynchronizedSettings.Base;
global using Pekspro.RadioStorm.Settings.SynchronizedSettings.EpisodesSortOrder;
global using Pekspro.RadioStorm.Settings.SynchronizedSettings.Favorite;
global using Pekspro.RadioStorm.Settings.SynchronizedSettings.ListenState;
global using Pekspro.RadioStorm.Settings.SynchronizedSettings.RecentHistory;
global using Pekspro.RadioStorm.Utilities;
global using Pekspro.SwedRadio;
