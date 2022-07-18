namespace Pekspro.RadioStorm.UI.Utilities;

internal sealed class SampleData
{
    public static readonly DateTimeOffset SampleTime = new DateTimeOffset(2038, 1, 20, 11, 2, 44, TimeSpan.FromHours(1));

    internal static ChannelData ChannelDataSample(int sampleType = 0)
    {
        if (sampleType % 3 == 0)
        {
            return new ChannelData()
            {
                Title = "P1",
                ChannelColor = "31a1bd",
                ChannelGroupName = "Rikskanal",
                ChannelId = 132,
                ChannelImageHighResolution = "https://static-cdn.sr.se/images/132/2186745_512_512.jpg",
                ChannelImageLowResolution = "https://static-cdn.sr.se/images/132/2186745_512_512.jpg?preset=api-default-square",
                LiveAudioUrl = "https://sverigesradio.se/topsy/direkt/srapi/132.mp3",
                WebPageUri = "https://sverigesradio.se/p1"
            };
        }
        else if (sampleType % 3 == 1)
        {
            return new ChannelData()
            {
                ChannelId = 163,
                Title = "P2",
                ChannelColor = "ff5a00",
                ChannelGroupName = "Rikskanal",
                ChannelImageHighResolution = "https://static-cdn.sr.se/images/163/2186754_512_512.jpg",
                ChannelImageLowResolution = "https://static-cdn.sr.se/images/163/2186754_512_512.jpg?preset=api-default-square",
                LiveAudioUrl = "https://sverigesradio.se/topsy/direkt/srapi/132.mp3",
                WebPageUri = "https://sverigesradio.se/p1"
            };
        }
        else
        {
            return new ChannelData()
            {
                ChannelId = 220,
                Title = "P4 Halland",
                ChannelColor = "c31eaa",
                ChannelGroupName = "Lokal kanal",
                ChannelImageHighResolution = "https://static-cdn.sr.se/images/128/df8f9818-c563-431f-930b-be1a49705008.jpg",
                ChannelImageLowResolution = "https://static-cdn.sr.se/images/128/df8f9818-c563-431f-930b-be1a49705008.jpg?preset=api-default-square",
                LiveAudioUrl = "https://sverigesradio.se/topsy/direkt/srapi/220.mp3",
                WebPageUri = "https://sverigesradio.se/halland/"
            };
        }
    }

    internal static ChannelStatusData ChannelStatusDataSample(int sampleType = 0)
    {
        if (sampleType % 3 == 0)
        {
            return new ChannelStatusData()
            {
                CurrentProgramId = 123,
                CurrentProgram = "Snedtänkt med Kalle Lind",
                CurrentProgramImage = "https://static-cdn.sr.se/images/4747/006ecda4-8378-4824-83c5-1099056e93b0.jpg?preset=api-default-square",
                CurrentProgramDescription = "Podden som pratar om det inga andra poddar pratar om.",
                CurrentStartTime = SampleTime.Date.AddHours(10),
                CurrentEndTime = SampleTime.Date.AddHours(11.5),

                NextProgramId = 456,
                NextProgram = "Nyhetsuppdatering från Ekot",
                NextProgramImage = "https://static-cdn.sr.se/images/5380/0e6902e2-9fd3-48cd-acf3-639228799d2d.jpg?preset=api-default-square",
                NextProgramDescription = "Senaste nytt varje timme från Ekoredaktionen.",
                NextStartTime = SampleTime.Date.AddHours(11.5),
                NextEndTime = SampleTime.Date.AddHours(13.0),
            };
        }
        else if (sampleType % 3 == 1)
        {
            return new ChannelStatusData()
            {
                CurrentProgramId = 123,
                CurrentProgram = "Klassiska konserten i P2",
                CurrentProgramImage = "https://static-cdn.sr.se/images/4427/6481dc6e-06cf-468d-8d16-1eb99e250739.jpg?preset=api-default-square",
                CurrentProgramDescription = "Sveriges största konserthus!",
                CurrentStartTime = SampleTime.Date.AddHours(10),
                CurrentEndTime = SampleTime.Date.AddHours(11.5),

                NextProgramId = 456,
                NextProgram = "Opera i P2",
                NextProgramImage = "https://static-cdn.sr.se/images/2359/185dc9cd-60c1-4608-8a43-9f5dcdb253af.jpg?preset=api-default-square",
                NextProgramDescription = "Hör operaföreställningar från hela världen!",
                NextStartTime = SampleTime.Date.AddHours(11.5),
                NextEndTime = SampleTime.Date.AddHours(13.0),
            };
        }
        else
        {
            return new ChannelStatusData()
            {
                CurrentProgramId = 3135,
                CurrentProgram = "P4 Dans",
                CurrentProgramImage = "https://static-cdn.sr.se/images/3135/988ff02c-e270-4f68-9afd-7d71eb995b60.jpg?preset=api-default-square",
                CurrentProgramDescription = "Programmet som speglar den svenska dansbandskulturen.",
                CurrentStartTime = SampleTime.Date.AddHours(10),
                CurrentEndTime = SampleTime.Date.AddHours(11.5),

                NextProgramId = 5380,
                NextProgram = "Nyhetsuppdatering från Ekot",
                NextProgramImage = "https://static-cdn.sr.se/images/5380/0e6902e2-9fd3-48cd-acf3-639228799d2d.jpg?preset=api-default-square",
                NextProgramDescription = "Senaste nytt varje timme från Ekoredaktionen.",
                NextStartTime = SampleTime.Date.AddHours(11.5),
                NextEndTime = SampleTime.Date.AddHours(13.0),
            };
        }
    }

    internal static ProgramData ProgramDataSample(int sampleType = 0)
    {
        if (sampleType % 3 == 0)
        {
            return new ProgramData()
            {
                ProgramId = 4131,
                Name = "Institutet",
                CategoryId = 3,
                CategoryName = "Kultur/Nöje",
                Description = "Vetenskap, forskning och experiment",
                ProgramImageHighResolution = "https://static-cdn.sr.se/images/4131/e9478015-67ef-4e70-be75-20d0717538f3.jpg",
                ProgramImageLowResolution = "https://static-cdn.sr.se/images/4131/e9478015-67ef-4e70-be75-20d0717538f3.jpg?preset=api-default-square",
                Archived = true,
                HasOnDemand = false,
                HasPod = true,
                BroadcastInfo = "Programmet sänds inte längre.",
                ProgramUri = "https://sverigesradio.se/default.aspx?programid=4131",
                FacebookPageUri = "https://www.facebook.com/InstitutetmedKarinochJesper",
                TwitterPageUri = null,
                ChannelId = 164
            };
        }
        else if (sampleType % 3 == 1)
        {
            return new ProgramData()
            {
                ProgramId = 516,
                Name = "Spanarna",
                CategoryId = 3,
                CategoryName = "Kultur/Nöje",
                Description = "Spanarna är ett program och en podd för dig som gillar samhällsspaning med humor. Tre skarpsynta personligheter försöker avläsa trender i vår vardag och ge oss sina framtidsvisioner.",
                ProgramImageHighResolution = "https://static-cdn.sr.se/images/516/603755d4-225e-47eb-9075-1cfbb998404f.jpg",
                ProgramImageLowResolution = "https://static-cdn.sr.se/images/516/603755d4-225e-47eb-9075-1cfbb998404f.jpg?preset=api-default-square",
                Archived = false,
                HasOnDemand = true,
                HasPod = true,
                BroadcastInfo = "Fredag 15.04",
                ProgramUri = "https://sverigesradio.se/default.aspx?programid=516",
                FacebookPageUri = "https://www.facebook.com/SpanarnaiP1/",
                TwitterPageUri = null,
                ChannelId = 132
            };
        }
        else
        {
            return new ProgramData()
            {
                ProgramId = 3345,
                Name = "Vetenskapsradion Klotet",
                CategoryId = 12,
                CategoryName = "Vetenskap/Miljö",
                Description = "Vetenskapsradions internationella miljöprogram.",
                ProgramImageHighResolution = "https://static-cdn.sr.se/images/3345/64e09762-e392-4834-95ef-c46bd236a77f.jpg",
                ProgramImageLowResolution = "https://static-cdn.sr.se/images/3345/64e09762-e392-4834-95ef-c46bd236a77f.jpg?preset=api-default-square",
                Archived = false,
                HasOnDemand = true,
                HasPod = true,
                BroadcastInfo = "Onsdag kl. 14.04",
                ProgramUri = "https://sverigesradio.se/default.aspx?programid=3345",
                FacebookPageUri = "https://www.facebook.com/Klotet",
                TwitterPageUri = "https://twitter.com/klotet/",
                ChannelId = 132
            };
        }
    }

    internal static EpisodeData EpisodeDataSample(int sampleType = 0)
    {
        if (sampleType % 3 == 0)
        {
            return new EpisodeData()
            {
                AudioDownloadDuration = 3974,
                AudioDownloadUrl = "https://sverigesradio.se/topsy/ljudfil/srapi/3818113.mp3",
                AudioStreamWithMusicDuration = 0,
                AudioStreamWithMusicExpireDate = null,
                AudioStreamWithMusicUrl = null,
                AudioStreamWithoutMusicDuration = 3974,
                AudioStreamWithoutMusicUrl = "https://sverigesradio.se/topsy/ljudfil/srapi/3818113.mp3",
                Description = "Är rymden verkligen oändlig eller beskrivs den bättre som en kantig slang? Skulle människan kunna leva för evigt och när i så fall utvecklar vi vingar? Vad har det att göra med krypen som har sex med sin brorsa inuti sin mamma? Och hur kan det finnas olika stora oändligheter? Svaren får du i Institutets andra program. Repris från 20110620. Programledare: Karin Gyllenklev och Jesper Rönndahl.",
                EpisodeId = 49265,
                EpisodeImage = "https://static-cdn.sr.se/images/4131/3146930_2048_1152.jpg?preset=api-default-square",
                ProgramId = 4131,
                ProgramName = "Institutet",
                PublishDate = new DateTimeOffset(new DateTime(2011, 6, 20)),
                Title = "Oändligheten"
            };
        }
        else if (sampleType % 3 == 1)
        {
            return new EpisodeData()
            {
                AudioDownloadDuration = 4588,
                AudioDownloadUrl = "https://sverigesradio.se/topsy/ljudfil/srapi/3790229.mp3",
                AudioStreamWithMusicDuration = 0,
                AudioStreamWithMusicExpireDate = null,
                AudioStreamWithMusicUrl = null,
                AudioStreamWithoutMusicDuration = 4588,
                AudioStreamWithoutMusicUrl = "https://sverigesradio.se/topsy/ljudfil/srapi/3790229.mp3",
                Description = "Av någon anledning är det ryssarna som har stått för den knasigaste forskningen genom historien, från att transplantera en liten hund på en stor hund, till att inseminera kvinnor med apsperma - och vilja snabba på växthuseffekten för att kunna odla bananer i Sibirien. Men dom har kanske också gett oss lösningen på klimatkrisen i form av en slags filt, och framför (nästan) allt det periodiska systemet! Dessutom: maneterna som fyllde Svarta havet. Programledare: Karin Gyllenklev och Jesper Rönndahl.",
                EpisodeId = 49771,
                EpisodeImage = "https://static-cdn.sr.se/images/4131/3146930_2048_1152.jpg?preset=api-default-square",
                ProgramId = 4131,
                ProgramName = "Institutet",
                PublishDate = new DateTimeOffset(new DateTime(2012, 2, 25)),
                Title = "Rysk forskning"
            };
        }
        else
        {
            return new EpisodeData()
            {
                AudioDownloadDuration = 3206,
                AudioDownloadUrl = "https://sverigesradio.se/topsy/ljudfil/srapi/3776081.mp3",
                AudioStreamWithMusicDuration = 0,
                AudioStreamWithMusicExpireDate = null,
                AudioStreamWithMusicUrl = null,
                AudioStreamWithoutMusicDuration = 3206,
                AudioStreamWithoutMusicUrl = "https://sverigesradio.se/topsy/ljudfil/srapi/3776081.mp3",
                Description = "Du ska inte lita på någon. Inte på vittnen, inte på svampar, inte på din syster - om hon nu verkligen ÄR din syster! Lita inte heller på dom som säger att du måste öva dig för att bli bra på gitarr, lita inte på dina ögon och framförallt: lita inte på nånting som din hjärna säger. Repris från 20110613. Programledare: Karin Gyllenklev och Jesper Rönndahl.",
                EpisodeId = 50012,
                EpisodeImage = "https://static-cdn.sr.se/images/4131/3146930_2048_1152.jpg?preset=api-default-square",
                ProgramId = 4131,
                ProgramName = "Institutet",
                PublishDate = new DateTimeOffset(new DateTime(2011, 6, 13)),
                Title = "Lita inte på nån!"
            };
        }
    }

    internal static SongListItemData SongListItemDataSample(int sampleType = 0)
    {
        if (sampleType % 3 == 0)
        {
            return new SongListItemData()
            {
                Title = "Infinity",
                AlbumName = "The Album",
                Composer = "Smith Jones",
                Artist = "Singeling",
                PublishDate = new DateTimeOffset(new DateTime(2021, 11, 6, 19, 12, 45), TimeSpan.FromHours(1))

            };
        }
        else if (sampleType % 3 == 1)
        {
            return new SongListItemData()
            {
                Title = "Zero",
                AlbumName = "Numbers",
                Composer = "Archimelody",
                Artist = "Division",
                PublishDate = new DateTimeOffset(new DateTime(2021, 11, 1, 10, 12, 45), TimeSpan.FromHours(1))
            };
        }
        else
        {
            return new SongListItemData()
            {
                Title = "Pi",
                AlbumName = "Fractions for everyone",
                Composer = "Euler",
                Artist = "Harmony",
                PublishDate = new DateTimeOffset(new DateTime(2021, 11, 4, 14, 44, 44), TimeSpan.FromHours(1))
            };
        }
    }

    internal static ScheduledEpisodeListItemData ScheduledEpisodeListItemDataSample(int sampleType = 0)
    {
        if (sampleType % 3 == 0)
        {
            return new ScheduledEpisodeListItemData()
            {
                Title = "Sagor i Barnradion",
                ProgramName = "Sagor i Barnradion",
                Description = "Bra sagor och serier för yngre barn varje vardag.",
                StartTimeUtc = new DateTimeOffset(new DateTime(2021, 11, 6, 19, 12, 45), TimeSpan.FromHours(sampleType))
            };
        }
        else if (sampleType % 3 == 1)
        {
            return new ScheduledEpisodeListItemData()
            {
                Title = "Kulturnytt",
                ProgramName = "Kulturnytt i P1",
                Description = "Nyhetssändning från kulturredaktionen P1, med reportage, nyheter och recensioner.",
                StartTimeUtc = new DateTimeOffset(new DateTime(2021, 11, 6, 19, 12, 45), TimeSpan.FromHours(sampleType))
            };
        }
        else
        {
            return new ScheduledEpisodeListItemData()
            {
                Title = "Radioföljetongen",
                ProgramName = "Ljudböcker från Radioföljetongen & Radionovellen",
                Description = "Ljudböcker från Sveriges Radio sedan 1939 – utvalda romaner och specialskrivna radionoveller.",
                StartTimeUtc = new DateTimeOffset(new DateTime(2021, 11, 6, 19, 12, 45), TimeSpan.FromHours(sampleType))
            };
        }
    }

    internal static Download DownloadSample(int sampleType = 0)
    {
        if (sampleType % 4 == 0)
        {
            return new Download("49771.mp3", 4131, 49771)
            {
                BytesToDownload = 5 * 1024 * 1024,
                BytesDownloaded = 7 * 1024 * 1024 * 3 / 7,
                Status = DownloadDataStatus.Downloading
            };
        }
        else if (sampleType % 4 == 1)
        {
            return new Download("49265.mp3", 4131, 49265)
            {
                BytesToDownload = 5 * 1024 * 1024,
                BytesDownloaded = 5 * 1024 * 1024,
                Status = DownloadDataStatus.Done
            };
        }
        else if (sampleType % 4 == 2)
        {
            return new Download("49771.mp3", 4131, 49771)
            {
                BytesToDownload = 5 * 1024 * 1024,
                BytesDownloaded = 7 * 1024 * 1024 * 7 / 3,
                Status = DownloadDataStatus.Downloading
            };
        }
        else
        {
            return new Download("50012.mp3", 4131, 50012)
            {
                BytesToDownload = 5 * 1024 * 1024,
                BytesDownloaded = 7 * 1024 * 1024 * 7 / 3,
                Status = DownloadDataStatus.Starting
            };
        }
    }
}
