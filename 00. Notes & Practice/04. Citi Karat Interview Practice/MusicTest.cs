
/*<bug-task1>
 * We are building the analytics back-end for a music streaming platform. The system
 * tracks songs and play events. A play is "completed" when listenedSeconds >= the
 * song's duration, and "skipped" otherwise.
 *
 * 1-1) Read through and understand the code below. Feel free to run it.
 * 1-2) The test for MusicLibrary is not passing due to a bug in the code.
 *      Make the necessary changes to MusicLibrary to fix the bug.
 *</bug-task1>
 */
/*<task2>
 * We've discovered that the catalog has accumulated duplicate Song records from
 * multiple data feeds. We need to deduplicate.
 * Two songs are duplicates if they have the same title (case-insensitive), the same artist (case-insensitive), and the same durationSeconds.
 * You may assume all songIds are unique.
 * For each group of duplicates, we keep ONE song: the one with the smallest songId.
 * The DedupResult class is provided below:
 *   songId:     the songId of the song that was kept
 *   mergedIds:  list of songIds merged into it,
                sorted ascending (does NOT include songId itself)
 * Add functions to MusicLibrary:
 *  2.1) findDuplicateGroups():
 *    Return the list of DedupResult objects, sorted by songId ascending, for all groups of duplicates.
 *  2.2) getMergedCountByArtist():
 *    Return a Map<String, Integer> mapping an artist to the total number of duplicate
 *    songs that would be merged away (i.e. removed) for that artist across ALL of its
 *    duplicate groups.
 *      - For each duplicate group, the number merged away = (group size - 1).
 *      - Group these counts by the artist of the KEPT song (the one with the smallest
 *        songId), using that kept song's original artist string as the map key.
 *      - If several duplicate groups map to the same artist key, SUM their counts.
 *      - Only artists that have at least one duplicate appear in the result.
 *      - If the catalog has no duplicates at all, return an empty map.
 *To assist you in testing the new functions, we have provided the testDedup and testMergedCountByArtist tests.
 *</task2>
 */
/*<task3>
 * 3) getSongReports() returns a Map from songId to SongReport for EVERY song in
 *    the catalog, regardless of whether it has been played. A song that has never
 *    been played gets a report of 0 / 0 / 0.
 *    - totalPlays      : total number of play events for that song (any status)
 *    - completedPlays  : number of those plays that were COMPLETED
 *    - uniqueListeners : number of distinct users who played that song (any status)
 </task3>
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetQuestions
{
    class Song
    {
        public int songId;
        public string title;
        public string artist;
        public int durationSeconds;
        public string description;
        public string album;
        public int? releaseYear;
        public string genre;

        public Song(int songId, string title, string artist, int durationSeconds)
        {
            this.songId = songId;
            this.title = title;
            this.artist = artist;
            this.durationSeconds = durationSeconds;
            this.description = null;
            this.album = null;
            this.releaseYear = null;
            this.genre = null;
        }

        public Song(int songId, string title, string artist, int durationSeconds,
                    string description, string album, int? releaseYear, string genre)
        {
            this.songId = songId;
            this.title = title;
            this.artist = artist;
            this.durationSeconds = durationSeconds;
            this.description = description;
            this.album = album;
            this.releaseYear = releaseYear;
            this.genre = genre;
        }
    }

    class DedupResult
    {
        public int songId;
        public List<int> mergedIds;

        public DedupResult(int songId, List<int> mergedIds)
        {
            this.songId = songId;
            this.mergedIds = mergedIds;
        }
    }

    class SongReport
    {
        public int totalPlays;
        public int completedPlays;
        public int uniqueListeners;

        public SongReport(int totalPlays, int completedPlays, int uniqueListeners)
        {
            this.totalPlays = totalPlays;
            this.completedPlays = completedPlays;
            this.uniqueListeners = uniqueListeners;
        }
    }

    class PlayEvent
    {
        public int playId;
        public int userId;
        public int songId;
        public int listenedSeconds;

        public PlayEvent(int playId, int userId, int songId, int listenedSeconds)
        {
            this.playId = playId;
            this.userId = userId;
            this.songId = songId;
            this.listenedSeconds = listenedSeconds;
        }
    }

    class ListenerStats
    {
        public int totalPlays;
        public int uniqueSongs;
        public double completionRate;

        public ListenerStats(int totalPlays, int uniqueSongs, double completionRate)
        {
            this.totalPlays = totalPlays;
            this.uniqueSongs = uniqueSongs;
            this.completionRate = completionRate;
        }
    }

    class MusicLibrary
    {
        public Dictionary<int, Song> songs;
        public List<PlayEvent> playEvents;

        public MusicLibrary()
        {
            songs = new Dictionary<int, Song>();
            playEvents = new List<PlayEvent>();
        }

        public void AddSong(Song song)
        {
            songs[song.songId] = song;
        }

        public void AddPlayEvent(PlayEvent evt)
        {
            playEvents.Add(evt);
        }

        private bool IsCompleted(PlayEvent evt)
        {
            // A play is completed when listenedSeconds reaches the song's duration.
            Song song = songs[evt.songId];
            return evt.listenedSeconds >= song.durationSeconds;
        }

        public ListenerStats GetListenerStats(int userId)
        {
            /*
                Return statistics for a single user:
                * totalPlays:     total number of play events by this user
                * uniqueSongs:    number of distinct songs this user has played (including skipped plays)
                * completionRate: fraction of this user's plays that were completed,
                                  expressed as a value between 0.0 and 1.0
            */
            List<PlayEvent> userEvents = playEvents
                    .Where(e => e.userId == userId)
                    .ToList();

            List<PlayEvent> completedEvents = new List<PlayEvent>();
            foreach (PlayEvent e in userEvents)
            {
                if (IsCompleted(e))
                {
                    completedEvents.Add(e);
                }
            }

            int totalPlays = userEvents.Count;

            int uniqueSongs = userEvents
                    .Select(e => e.songId)
                    .Distinct()
                    .Count();

            double completionRate;
            if (totalPlays == 0)
            {
                completionRate = 0.0;
            }
            else
            {
                completionRate = (double)completedEvents.Count / userEvents.Count;
            }

            return new ListenerStats(totalPlays, uniqueSongs, completionRate);
        }

        // TASK 2.1: Return duplicate groups, kept song = smallest songId.
        public List<DedupResult> FindDuplicateGroups()
        {
            List<DedupResult> list = new List<DedupResult>();
            return list;
        }

        // TASK 2.2: Return artist -> total duplicate songs merged away for that artist.
        public Dictionary<string, int> GetMergedCountByArtist()
        {
            // TODO: implement
            return new Dictionary<string, int>();
        }

        // TASK 3: Return songId -> SongReport for EVERY song in the catalog.
        public Dictionary<int, SongReport> GetSongReports()
        {
            // TODO: implement
            return new Dictionary<int, SongReport>();
        }

    }

    public class Main
    {
        static int passed = 0, failed = 0;

        public static void Main(string[] args)
        {
            Console.WriteLine("\n=== MUSIC LIBRARY — TEST SUITE ===\n");

            Run("BUG 1-2: Listener stats",              TestGetListenerStats);
            Run("TASK 2.1: Find duplicate groups",      TestDedup);
            Run("TASK 2.2: Merged count by artist",     TestMergedCountByArtist);
            Run("TASK 3:   Song reports",               TestSongReports);

            Console.WriteLine("\n--------------------------------------------------");
            Console.WriteLine("Results: " + passed + " passed, " + failed
                    + " failed out of " + (passed + failed));
        }
        //<bug-task1>
        public static void TestGetListenerStats()
        {
            Console.WriteLine("Running testGetListenerStats");
            MusicLibrary lib = new MusicLibrary();

            // Catalog: 3 songs.
            lib.AddSong(new Song(101, "Song A", "Artist 1", 180));
            lib.AddSong(new Song(102, "Song B", "Artist 2", 200));
            lib.AddSong(new Song(103, "Song C", "Artist 1", 240));

            // User 1: 4 plays total
            //   - song 101 completed (180 of 180)
            //   - song 102 completed (220 of 200)
            //   - song 101 skipped  (50 of 180)
            //   - song 103 skipped  (100 of 240)
            // Expected: totalPlays = 4, uniqueSongs = 3, completionRate = 0.5
            lib.AddPlayEvent(new PlayEvent(1, 1, 101, 180));
            lib.AddPlayEvent(new PlayEvent(2, 1, 102, 220));
            lib.AddPlayEvent(new PlayEvent(3, 1, 101, 50));
            lib.AddPlayEvent(new PlayEvent(4, 1, 103, 100));

            // User 2: 2 plays total
            //   - song 102 completed (200 of 200)
            //   - song 103 completed (250 of 240)
            // Expected: totalPlays = 2, uniqueSongs = 2, completionRate = 1.0
            lib.AddPlayEvent(new PlayEvent(5, 2, 102, 200));
            lib.AddPlayEvent(new PlayEvent(6, 2, 103, 250));

            ListenerStats statsUser1 = lib.GetListenerStats(1);
            if (!(statsUser1.totalPlays == 4))
                throw new Exception("totalPlays should be 4, was " + statsUser1.totalPlays);
            if (!(statsUser1.uniqueSongs == 3))
                throw new Exception("uniqueSongs should be 3, was " + statsUser1.uniqueSongs);
            if (!(Math.Abs(statsUser1.completionRate - 0.5) < 1e-4))
                throw new Exception("completionRate should be 0.5, was " + statsUser1.completionRate);

            ListenerStats statsUser2 = lib.GetListenerStats(2);
            if (!(statsUser2.totalPlays == 2))
                throw new Exception("totalPlays should be 2, was " + statsUser2.totalPlays);
            if (!(statsUser2.uniqueSongs == 2))
                throw new Exception("uniqueSongs should be 2, was " + statsUser2.uniqueSongs);
            if (!(Math.Abs(statsUser2.completionRate - 1.0) < 1e-4))
                throw new Exception("completionRate should be 1.0, was " + statsUser2.completionRate);

            ListenerStats statsUser3 = lib.GetListenerStats(3);
            if (!(statsUser3.totalPlays == 0))
                throw new Exception("totalPlays should be 0, was " + statsUser3.totalPlays);
            if (!(statsUser3.uniqueSongs == 0))
                throw new Exception("uniqueSongs should be 0, was " + statsUser3.uniqueSongs);
            if (!(Math.Abs(statsUser3.completionRate - 0.0) < 1e-4))
                throw new Exception("completionRate should be 0.0, was " + statsUser3.completionRate);
        }
        //</bug-task1>
        //<task2>
        public static void TestDedup()
        {
            Console.WriteLine("Running testDedup");
            MusicLibrary lib = new MusicLibrary();

            // Group A: songs 1, 2, 3 share ("song a", "artist 1", 180).
            lib.AddSong(new Song(1, "Song A",  "Artist 1", 180, null, null, null, "rock"));
            lib.AddSong(new Song(2, "song a",  "ARTIST 1", 180, "Studio version", "First Album", null, "rock"));
            lib.AddSong(new Song(3, "SONG A",  "artist 1", 180, "Live", "Live Album", null, null));
            lib.AddSong(new Song(30, "Song A", "Artist 1", 999));
            lib.AddSong(new Song(60, "Unrelated", "Nobody", 180));
            lib.AddSong(new Song(61, "Also Unrelated", "Else", 180));

            // Group B: songs 10, 11 share ("song b", "artist 2", 200).
            lib.AddSong(new Song(10, "Song B", "Artist 2", 200, null, "B Album", null, null));
            lib.AddSong(new Song(11, "SONG B", "Artist 2", 200, "some description", null, null, "pop"));
            lib.AddSong(new Song(40, "Song B", "Other Artist", 200));
            lib.AddSong(new Song(50, "Different Title", "Artist 2", 200));

            // Group C: songs 20, 21 share ("song c", "artist 3", 240). Duplicate — smallest songId wins.
            lib.AddSong(new Song(20, "Song C", "Artist 3", 240));
            lib.AddSong(new Song(21, "song c", "Artist 3", 240));

            // Group D: songs 8, 5, 7 share ("late group", "late artist", 50). Duplicate — smallest songId wins.
            lib.AddSong(new Song(8, "Late Group", "Late Artist", 50));
            lib.AddSong(new Song(5, "late group", "LATE ARTIST", 50));
            lib.AddSong(new Song(7, "LATE GROUP", "late artist", 50));

            // Unique song — not a duplicate.
            lib.AddSong(new Song(99, "Only Song", "Solo Artist", 300));

            // Identify songs to merge.
            List<DedupResult> results = lib.FindDuplicateGroups();

            if (!(results.Count == 4))
                throw new Exception("results size should be 4, was " + results.Count);

            if (!(results[0].songId == 1))
                throw new Exception("results[0].songId should be 1, was " + results[0].songId);
            if (!(results[0].mergedIds.SequenceEqual(new List<int> { 2, 3 })))
                throw new Exception("results[0].mergedIds should be [2, 3], was " + string.Join(", ", results[0].mergedIds));

            if (!(results[1].songId == 5))
                throw new Exception("results[1].songId should be 5, was " + results[1].songId);
            if (!(results[1].mergedIds.SequenceEqual(new List<int> { 7, 8 })))
                throw new Exception("results[1].mergedIds should be [7, 8], was " + string.Join(", ", results[1].mergedIds));

            if (!(results[2].songId == 10))
                throw new Exception("results[2].songId should be 10, was " + results[2].songId);
            if (!(results[2].mergedIds.SequenceEqual(new List<int> { 11 })))
                throw new Exception("results[2].mergedIds should be [11], was " + string.Join(", ", results[2].mergedIds));

            if (!(results[3].songId == 20))
                throw new Exception("results[3].songId should be 20, was " + results[3].songId);
            if (!(results[3].mergedIds.SequenceEqual(new List<int> { 21 })))
                throw new Exception("results[3].mergedIds should be [21], was " + string.Join(", ", results[3].mergedIds));
        }

        public static void TestMergedCountByArtist()
        {
            Console.WriteLine("Running testMergedCountByArtist");
            MusicLibrary lib = new MusicLibrary();

            // Group A: 1, 2, 3 duplicates → kept song 1 ("Artist 1"), merged away = 2
            lib.AddSong(new Song(1, "Song A", "Artist 1", 180));
            lib.AddSong(new Song(2, "song a", "ARTIST 1", 180));
            lib.AddSong(new Song(3, "SONG A", "artist 1", 180));

            // Group A2: 40, 41 also "Artist 1" (different title/duration) → kept 40 ("Artist 1"), merged away = 1
            // "Artist 1" total across both groups = 2 + 1 = 3
            lib.AddSong(new Song(40, "Song X", "Artist 1", 300));
            lib.AddSong(new Song(41, "song x", "artist 1", 300));

            // Group B: 10, 11 duplicates → kept 10 ("Artist 2"), merged away = 1
            lib.AddSong(new Song(10, "Song B", "Artist 2", 200));
            lib.AddSong(new Song(11, "SONG B", "Artist 2", 200));

            // Unique song — no duplicate, must not appear.
            lib.AddSong(new Song(99, "Only Song", "Solo Artist", 300));

            Dictionary<string, int> counts = lib.GetMergedCountByArtist();

            // Only artists with duplicates appear.
            if (!(counts.Count == 2))
                throw new Exception("counts size should be 2, was " + counts.Count);
            if (!(GetOrDefault(counts, "Artist 1", -1) == 3))
                throw new Exception("Artist 1 merged count should be 3 (summed across groups), was " + GetOrDefault(counts, "Artist 1", -1));
            if (!(GetOrDefault(counts, "Artist 2", -1) == 1))
                throw new Exception("Artist 2 merged count should be 1, was " + GetOrDefault(counts, "Artist 2", -1));
            if (counts.ContainsKey("Solo Artist"))
                throw new Exception("Solo Artist has no duplicates and should not appear");
        }
        //</task2>
        //<task3>
        public static void TestSongReports()
        {
            Console.WriteLine("Running testSongReports");
            MusicLibrary lib = new MusicLibrary();

            lib.AddSong(new Song(101, "Song A", "Artist 1", 180));
            lib.AddSong(new Song(102, "Song B", "Artist 2", 200));
            lib.AddSong(new Song(103, "Song C", "Artist 1", 240));  // never played → 0/0/0

            // song 101: user 1 completed (180), user 1 skipped (50), user 2 completed (200)
            //   totalPlays = 3, completedPlays = 2, uniqueListeners = 2
            lib.AddPlayEvent(new PlayEvent(1, 1, 101, 180));
            lib.AddPlayEvent(new PlayEvent(2, 1, 101, 50));
            lib.AddPlayEvent(new PlayEvent(3, 2, 101, 200));

            // song 102: user 1 completed (220)
            //   totalPlays = 1, completedPlays = 1, uniqueListeners = 1
            lib.AddPlayEvent(new PlayEvent(4, 1, 102, 220));

            Dictionary<int, SongReport> reports = lib.GetSongReports();

            // Every song must appear.
            if (!(reports.Count == 3))
                throw new Exception("reports size should be 3, was " + reports.Count);

            // song 101
            if (!(reports[101].totalPlays == 3))
                throw new Exception("song101 totalPlays should be 3, was " + reports[101].totalPlays);
            if (!(reports[101].completedPlays == 2))
                throw new Exception("song101 completedPlays should be 2, was " + reports[101].completedPlays);
            if (!(reports[101].uniqueListeners == 2))
                throw new Exception("song101 uniqueListeners should be 2, was " + reports[101].uniqueListeners);

            // song 102
            if (!(reports[102].totalPlays == 1))
                throw new Exception("song102 totalPlays should be 1, was " + reports[102].totalPlays);
            if (!(reports[102].completedPlays == 1))
                throw new Exception("song102 completedPlays should be 1, was " + reports[102].completedPlays);
            if (!(reports[102].uniqueListeners == 1))
                throw new Exception("song102 uniqueListeners should be 1, was " + reports[102].uniqueListeners);

            // song 103 — never played → 0 / 0 / 0
            if (!(reports[103].totalPlays == 0))
                throw new Exception("song103 totalPlays should be 0, was " + reports[103].totalPlays);
            if (!(reports[103].completedPlays == 0))
                throw new Exception("song103 completedPlays should be 0, was " + reports[103].completedPlays);
            if (!(reports[103].uniqueListeners == 0))
                throw new Exception("song103 uniqueListeners should be 0, was " + reports[103].uniqueListeners);
        }
        //</task3>

        static int GetOrDefault(Dictionary<string, int> map, string key, int defaultValue)
        {
            return map.TryGetValue(key, out int value) ? value : defaultValue;
        }

        static void Run(string name, Action test)
        {
            try
            {
                test();
                passed++;
                Console.WriteLine("  PASS: " + name);
            }
            catch (Exception e)
            {
                failed++;
                Console.WriteLine("  FAIL: " + name + " -> " + e);
            }
        }

    }
}
