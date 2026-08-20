// To clear the interview, you must complete at least one bug fix and two tasks.
// A single task may involve multiple functions; sub-parts like 2.1 and 2.2 are considered one task.
// Please provide a verbal walkthrough of your thought process while writing the code.

/*<bug-task1>
 * We are building the back-end for an online gaming platform. The system tracks
 * players and their match history. Each match result records the outcome from
 * that player's perspective.
 *
 * Definitions:
 * - A "player" has: playerId, username.
 * - A "match result" has: playerId, opponentId, outcome, score, timestamp.
 * - Outcome is one of: WIN, LOSS, DRAW.
 * - "GameManager" manages players and match results and provides player statistics.
 *
 * To begin with, we present you with two tasks:
 * 1-1) Read through and understand the code below. Feel free to run it.
 * 1-2) The test for GameManager is not passing due to a bug in the code.
 *      Make the necessary changes to GameManager to fix the bug.
 </bug-task1>
 */

/*<task2>
 * We are extending the platform to support recording match results
 * and computing per-player score statistics.
 *
 * Each MatchResult represents one player's experience of a single match:
 * - playerId    : the player this record belongs to
 * - opponentId  : the opponent in that match
 * - outcome     : one of WIN, LOSS, DRAW
 * - score       : the player's score in that match
 * - timestamp   : when the match was played
 *
 * 2.1) The addMatchResult method should store a match result.
 *      If the playerId does not refer to a known player, ignore the result.
 *
 * 2.2) The getAverageScoreByOutcome method should return a Map mapping each
 *      outcome (WIN, LOSS, DRAW) to the player's average score for that outcome.
 *      Only outcomes the player has at least one result for should appear.
 *      If the player has no match results at all, return an empty map.
 </task2>
 */

/*<task3>
 * We want to summarize the match history between two specific players.
 *
 * HeadToHead fields:
 * - winsPlayer1         : number of matches the first player won
 * - winsPlayer2         : number of matches the second player won
 * - draws               : number of draws
 * - totalMatches        : total matches played between them
 * - lastResult          : outcome of the most recent match from player1's perspective
 *                         (null if they have never played)
 * - lastMatchTimestamp  : timestamp of the most recent match (null if never played)
 *
 * 3) The getHeadToHead method takes two player IDs and returns a HeadToHead
 *    summarizing all matches between them.
 *    If they have never faced each other, return a HeadToHead with all numeric
 *    fields set to 0, lastResult null, and lastMatchTimestamp null.
 </task3>
 */

/*<task4>
 * We want to identify players in the best recent form.
 *
 * Points system:
 * - WIN  = 3 points
 * - DRAW = 1 point
 * - LOSS = 0 points
 *
 * 4) The getRecentForm method takes a number n and returns a List<int[]> of
 *    [playerId, formPoints] — one per qualifying player.
 *    For each player:
 *    - Take only their last n match results, ordered by timestamp ascending.
 *    - Calculate formPoints from those n matches.
 *    - Only include players who have at least n match results.
 *    Sort by formPoints descending; use playerId ascending as a tiebreaker.
 *    If n <= 0, return an empty list.
 </task4>
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetQuestions
{
        // Enum & Data Classes
        enum Outcome
        {
                WIN, LOSS, DRAW
        }

        class Player
        {
                public int playerId;
                public string username;

                public Player(int playerId, string username)
                {
                        this.playerId = playerId;
                        this.username = username;
                }
        }

        class HeadToHead
        {
                public int winsPlayer1;
                public int winsPlayer2;
                public int draws;
                public int totalMatches;
                public Outcome? lastResult;
                public long? lastMatchTimestamp;

                public HeadToHead(int winsPlayer1, int winsPlayer2, int draws, int totalMatches,
                           Outcome? lastResult, long? lastMatchTimestamp)
                {
                        this.winsPlayer1 = winsPlayer1;
                        this.winsPlayer2 = winsPlayer2;
                        this.draws = draws;
                        this.totalMatches = totalMatches;
                        this.lastResult = lastResult;
                        this.lastMatchTimestamp = lastMatchTimestamp;
                }
        }

        class MatchResult : IComparable<MatchResult>
        {
                public int playerId;
                public int opponentId;
                public Outcome outcome;
                public int score;
                public long timestamp;

                public MatchResult(int playerId, int opponentId, Outcome outcome, int score, long timestamp)
                {
                        this.playerId = playerId;
                        this.opponentId = opponentId;
                        this.outcome = outcome;
                        this.score = score;
                        this.timestamp = timestamp;
                }

                public int CompareTo(MatchResult other)
                {
                        return this.timestamp.CompareTo(other.timestamp);
                }
        }

        class PlayerStats
        {
                public int totalMatches;
                public int wins;
                public double winRate;

                public PlayerStats(int totalMatches, int wins, double winRate)
                {
                        this.totalMatches = totalMatches;
                        this.wins = wins;
                        this.winRate = winRate;
                }
        }

        // GameManager
        class GameManager
        {
                public Dictionary<int, Player> players = new Dictionary<int, Player>();
                public List<MatchResult> matchResults = new List<MatchResult>();

                public void AddPlayer(Player player)
                {
                        players[player.playerId] = player;
                }

                /**
                 * BUG 1-2: getPlayerStatistics has a logic error.
                 * totalMatches is computed incorrectly, causing wrong stats to be returned.
                 */
                public PlayerStats GetPlayerStatistics(int playerId)
                {
                        List<MatchResult> playerMatches = matchResults
                                .Where(m => m.playerId == playerId)
                                .ToList();

                        int totalMatches = playerMatches.Count();
                        int wins = playerMatches
                                .Where(m => m.outcome == Outcome.WIN)
                                .Count();
                        double winRate = totalMatches > 0 ? (double)wins / totalMatches : 0.0;

                        return new PlayerStats(totalMatches, wins, winRate);
                }

                /**
                 * TASK 2.1: Store a match result. Ignore if playerId is not a known player.
                 */
                public void AddMatchResult(MatchResult result)
                {
                        // TODO: implement
                        if (players.ContainsKey(result.playerId))
                        {
                                matchResults.Add(result);
                        }
                }

                /**
                 * TASK 2.2: Return a Map of each Outcome -> average score for that player.
                 * Only include outcomes the player has at least one result for.
                 * Return an empty map if the player has no results.
                 */
                public Dictionary<Outcome, double> GetAverageScoreByOutcome(int playerId)
                {
                        return matchResults
                                .Where(mr => mr.playerId == playerId)
                                .GroupBy(g => g.outcome)
                                .Where(g => g.Any())
                                .ToDictionary(g => g.Key, g => g.Average(mr => mr.score));

                }

                /**
                 * TASK 3: Return a HeadToHead summary for all matches between player1 and player2.
                 * If they have never faced each other, return a HeadToHead with all zeros and nulls.
                 */
                public HeadToHead GetHeadToHead(int playerId1, int playerId2)
                {
                        // TODO: implement
                        //return new HeadToHead(0, 0, 0, 0, null, null);

                        var playerMatchResults = matchResults
                            .Where(mr => mr.playerId == playerId1 && mr.opponentId == playerId2);

                        if (playerMatchResults.Count() == 0) return new HeadToHead(0, 0, 0, 0, null, null);

                        int player1Win = playerMatchResults
                                .Where(mr => mr.outcome == Outcome.WIN)
                                .Count();
                        int player2Win = playerMatchResults
                                .Where(mr => mr.outcome == Outcome.LOSS)
                                .Count();
                        int draws = playerMatchResults
                                .Where(mr => mr.outcome == Outcome.DRAW)
                                .Count();

                        int totalMatches = playerMatchResults.Count();
                        var latestMatch = playerMatchResults
                                .OrderByDescending(mr => mr.timestamp)
                                .Select(mr => new { mr.outcome, mr.timestamp })
                                .FirstOrDefault();

                        Outcome? lastResult = latestMatch?.outcome;
                        long? lastMatchTimestamp = latestMatch?.timestamp;

                        return new HeadToHead(player1Win, player2Win, draws, totalMatches, lastResult, lastMatchTimestamp);

                }

                /**
                 * TASK 4: Return a List<int[]> of [playerId, formPoints] for each player
                 * with at least n match results, sorted by formPoints desc, playerId asc.
                 * WIN=3pts, DRAW=1pt, LOSS=0pts. Return empty list if n <= 0.
                 */
                public List<int[]> GetRecentForm(int n)
                {
                        // TODO: implement
                        //return new List<int[]>();
                        if (n <= 0) return new List<int[]>();

                        return players.Keys
                            .Select(playerId =>
                            {
                                    var matches = matchResults
                                            .Where(mr => mr.playerId == playerId)
                                            .OrderBy(mr => mr.timestamp)
                                            .ToList();
                                    return new { playerId, matches };
                            })
                            .Where(x => x.matches.Count >= n)
                            .Select(x =>
                            {
                                    List<MatchResult> lastNMatches = x.matches
                                            .Skip(x.matches.Count - n).ToList();

                                    int formPoints = lastNMatches.Sum(m =>
                                    m.outcome == Outcome.WIN ? 3 :
                                    m.outcome == Outcome.DRAW ? 1 : 0);
                                    return new int[]{
                                x.playerId,
                                formPoints
                                    };
                            })
                            .OrderByDescending(x => x[1])
                            .ThenBy(x => x[0])
                            .ToList();
                }
        }

        // Test Runner
        public class Program
        {
                static int passed = 0, failed = 0;

                public static void Main(string[] args)
                {
                        Console.WriteLine("=== GAMING PLATFORM TEST SUITE ===\n");

                        // <bug-task1>
                        RunTest("BUG 1-2: Player Statistics", () =>
                        {
                                GameManager gm = new GameManager();
                                gm.AddPlayer(new Player(1, "player1"));
                                gm.AddPlayer(new Player(2, "player2"));

                                gm.matchResults.Add(new MatchResult(1, 2, Outcome.WIN, 80, 1000));
                                gm.matchResults.Add(new MatchResult(1, 2, Outcome.LOSS, 50, 2000));
                                gm.matchResults.Add(new MatchResult(1, 2, Outcome.DRAW, 60, 3000));
                                gm.matchResults.Add(new MatchResult(1, 2, Outcome.WIN, 90, 4000));

                                PlayerStats stats = gm.GetPlayerStatistics(1);
                                Check(stats.totalMatches == 4,
                            "BUG 1-2: Player 1 totalMatches should be 4, got: " + stats.totalMatches);
                                Check(stats.wins == 2,
                            "BUG 1-2: Player 1 wins should be 2, got: " + stats.wins);
                                Check(Math.Abs(stats.winRate - 0.5) < 0.0001,
                            "BUG 1-2: Player 1 winRate should be 0.5, got: " + stats.winRate);

                                gm.matchResults.Add(new MatchResult(2, 1, Outcome.DRAW, 60, 1000));
                                gm.matchResults.Add(new MatchResult(2, 1, Outcome.DRAW, 60, 2000));

                                PlayerStats stats2 = gm.GetPlayerStatistics(2);
                                Check(stats2.totalMatches == 2,
                            "BUG 1-2: Player 2 totalMatches should be 2, got: " + stats2.totalMatches);
                                Check(stats2.wins == 0,
                            "BUG 1-2: Player 2 wins should be 0, got: " + stats2.wins);
                                Check(Math.Abs(stats2.winRate - 0.0) < 0.0001,
                            "BUG 1-2: Player 2 winRate should be 0.0, got: " + stats2.winRate);
                        });
                        // </bug-task1>

                        // <task2>
                        RunTest("TASK 2.1: Add Match Result", () =>
                        {
                                GameManager gm = new GameManager();
                                gm.AddPlayer(new Player(1, "player1"));
                                gm.AddPlayer(new Player(2, "player2"));

                                gm.AddMatchResult(new MatchResult(1, 2, Outcome.WIN, 80, 1000));
                                gm.AddMatchResult(new MatchResult(2, 1, Outcome.LOSS, 50, 1000));
                                gm.AddMatchResult(new MatchResult(99, 1, Outcome.WIN, 100, 2000)); // unknown — ignored

                                Check(gm.matchResults.Count == 2,
                            "Task 2.1: Expected 2 stored results, got: " + gm.matchResults.Count);
                        });

                        RunTest("TASK 2.2: Average Score By Outcome", () =>
                        {
                                GameManager gm = new GameManager();
                                gm.AddPlayer(new Player(1, "player1"));
                                gm.AddPlayer(new Player(2, "player2"));
                                gm.AddPlayer(new Player(3, "player3"));

                                gm.AddMatchResult(new MatchResult(1, 2, Outcome.WIN, 80, 1000));
                                gm.AddMatchResult(new MatchResult(2, 1, Outcome.LOSS, 50, 1000));
                                gm.AddMatchResult(new MatchResult(1, 2, Outcome.WIN, 90, 2000));
                                gm.AddMatchResult(new MatchResult(2, 1, Outcome.LOSS, 60, 2000));
                                gm.AddMatchResult(new MatchResult(1, 2, Outcome.DRAW, 70, 3000));
                                gm.AddMatchResult(new MatchResult(2, 1, Outcome.DRAW, 70, 3000));

                                Dictionary<Outcome, double> avg1 = gm.GetAverageScoreByOutcome(1);
                                Check(Math.Abs(avg1.GetValueOrDefault(Outcome.WIN, -1.0) - 85.0) < 0.0001,
                            "Task 2.2: Player 1 avg WIN should be 85.0, got: " + avg1.GetValueOrDefault(Outcome.WIN, -1.0));
                                Check(Math.Abs(avg1.GetValueOrDefault(Outcome.DRAW, -1.0) - 70.0) < 0.0001,
                            "Task 2.2: Player 1 avg DRAW should be 70.0, got: " + avg1.GetValueOrDefault(Outcome.DRAW, -1.0));
                                Check(!avg1.ContainsKey(Outcome.LOSS),
                            "Task 2.2: Player 1 should have no LOSS entry");

                                Dictionary<Outcome, double> avg2 = gm.GetAverageScoreByOutcome(2);
                                Check(Math.Abs(avg2.GetValueOrDefault(Outcome.LOSS, -1.0) - 55.0) < 0.0001,
                            "Task 2.2: Player 2 avg LOSS should be 55.0, got: " + avg2.GetValueOrDefault(Outcome.LOSS, -1.0));
                                Check(Math.Abs(avg2.GetValueOrDefault(Outcome.DRAW, -1.0) - 70.0) < 0.0001,
                            "Task 2.2: Player 2 avg DRAW should be 70.0, got: " + avg2.GetValueOrDefault(Outcome.DRAW, -1.0));
                                Check(!avg2.ContainsKey(Outcome.WIN),
                            "Task 2.2: Player 2 should have no WIN entry");

                                Check(gm.GetAverageScoreByOutcome(3).Count == 0,
                            "Task 2.2: Player 3 with no results should return empty map");
                        });
                        // </task2>

                        // <task3>
                        RunTest("TASK 3: Head To Head", () =>
                        {
                                GameManager gm = new GameManager();
                                gm.AddPlayer(new Player(1, "player1"));
                                gm.AddPlayer(new Player(2, "player2"));
                                gm.AddPlayer(new Player(3, "player3"));

                                gm.AddMatchResult(new MatchResult(1, 2, Outcome.WIN, 80, 1000));
                                gm.AddMatchResult(new MatchResult(2, 1, Outcome.LOSS, 50, 1000));
                                gm.AddMatchResult(new MatchResult(1, 2, Outcome.LOSS, 60, 2000));
                                gm.AddMatchResult(new MatchResult(2, 1, Outcome.WIN, 90, 2000));
                                gm.AddMatchResult(new MatchResult(1, 2, Outcome.DRAW, 70, 3000));
                                gm.AddMatchResult(new MatchResult(2, 1, Outcome.DRAW, 70, 3000));
                                gm.AddMatchResult(new MatchResult(1, 2, Outcome.WIN, 85, 4000));
                                gm.AddMatchResult(new MatchResult(2, 1, Outcome.LOSS, 65, 4000));

                                HeadToHead h2h = gm.GetHeadToHead(1, 2);
                                Check(h2h.winsPlayer1 == 2,
                            "Task 3: winsPlayer1 should be 2, got: " + h2h.winsPlayer1);
                                Check(h2h.winsPlayer2 == 1,
                            "Task 3: winsPlayer2 should be 1, got: " + h2h.winsPlayer2);
                                Check(h2h.draws == 1,
                            "Task 3: draws should be 1, got: " + h2h.draws);
                                Check(h2h.totalMatches == 4,
                            "Task 3: totalMatches should be 4, got: " + h2h.totalMatches);
                                Check(h2h.lastResult == Outcome.WIN,
                            "Task 3: lastResult should be WIN, got: " + h2h.lastResult);
                                Check(h2h.lastMatchTimestamp == 4000L,
                            "Task 3: lastMatchTimestamp should be 4000, got: " + h2h.lastMatchTimestamp);

                                HeadToHead h2hReverse = gm.GetHeadToHead(2, 1);
                                Check(h2hReverse.winsPlayer1 == 1,
                            "Task 3 (reverse): winsPlayer1 should be 1, got: " + h2hReverse.winsPlayer1);
                                Check(h2hReverse.winsPlayer2 == 2,
                            "Task 3 (reverse): winsPlayer2 should be 2, got: " + h2hReverse.winsPlayer2);
                                Check(h2hReverse.lastResult == Outcome.LOSS,
                            "Task 3 (reverse): lastResult should be LOSS, got: " + h2hReverse.lastResult);

                                HeadToHead h2hEmpty = gm.GetHeadToHead(1, 3);
                                Check(h2hEmpty.totalMatches == 0, "Task 3: never-played totalMatches should be 0");
                                Check(h2hEmpty.lastResult == null, "Task 3: never-played lastResult should be null");
                                Check(h2hEmpty.lastMatchTimestamp == null, "Task 3: never-played timestamp should be null");
                        });
                        // </task3>

                        // <task4>
                        RunTest("TASK 4: Recent Form — Case 1", () =>
                        {
                                GameManager gm = new GameManager();
                                gm.AddPlayer(new Player(1, "player1"));
                                gm.AddPlayer(new Player(2, "player2"));
                                gm.AddPlayer(new Player(3, "player3"));

                                // Player 1: W W W  → last 2 = W W = 6 pts
                                gm.AddMatchResult(new MatchResult(1, 2, Outcome.WIN, 80, 1000));
                                gm.AddMatchResult(new MatchResult(1, 2, Outcome.WIN, 80, 2000));
                                gm.AddMatchResult(new MatchResult(1, 2, Outcome.WIN, 80, 3000));

                                // Player 2: L L W  → last 2 = L W = 3 pts
                                gm.AddMatchResult(new MatchResult(2, 1, Outcome.LOSS, 50, 1000));
                                gm.AddMatchResult(new MatchResult(2, 1, Outcome.LOSS, 50, 2000));
                                gm.AddMatchResult(new MatchResult(2, 1, Outcome.WIN, 80, 3000));

                                // Player 3: only 1 match — excluded when n=2
                                gm.AddMatchResult(new MatchResult(3, 1, Outcome.WIN, 90, 1000));

                                List<int[]> form = gm.GetRecentForm(2);
                                Check(form.Count == 2,
                            "Task 4 Case1: Expected 2 entries, got: " + form.Count);
                                Check(form[0][0] == 1 && form[0][1] == 6,
                            "Task 4 Case1: Entry 0 should be [playerId=1, form=6], got: ["
                                    + form[0][0] + "," + form[0][1] + "]");
                                Check(form[1][0] == 2 && form[1][1] == 3,
                            "Task 4 Case1: Entry 1 should be [playerId=2, form=3], got: ["
                                    + form[1][0] + "," + form[1][1] + "]");
                        });

                        RunTest("TASK 4: Recent Form — Case 2 (tiebreaker & n<=0)", () =>
                        {
                                GameManager gm = new GameManager();
                                gm.AddPlayer(new Player(1, "player1"));
                                gm.AddPlayer(new Player(2, "player2"));

                                // Both players: last 3 = W D L = 4 pts → tiebreaker by playerId asc
                                gm.AddMatchResult(new MatchResult(1, 2, Outcome.WIN, 80, 1000));
                                gm.AddMatchResult(new MatchResult(1, 2, Outcome.DRAW, 60, 2000));
                                gm.AddMatchResult(new MatchResult(1, 2, Outcome.LOSS, 40, 3000));

                                gm.AddMatchResult(new MatchResult(2, 1, Outcome.WIN, 80, 1000));
                                gm.AddMatchResult(new MatchResult(2, 1, Outcome.DRAW, 60, 2000));
                                gm.AddMatchResult(new MatchResult(2, 1, Outcome.LOSS, 40, 3000));

                                List<int[]> form = gm.GetRecentForm(3);
                                Check(form.Count == 2,
                            "Task 4 Case2: Expected 2 entries, got: " + form.Count);
                                Check(form[0][0] == 1,
                            "Task 4 Case2: Tiebreaker — playerId 1 should come first, got: " + form[0][0]);
                                Check(form[0][1] == 4,
                            "Task 4 Case2: formPoints should be 4, got: " + form[0][1]);

                                Check(gm.GetRecentForm(0).Count == 0,
                            "Task 4 Case2: n=0 should return empty list");
                                Check(gm.GetRecentForm(-1).Count == 0,
                            "Task 4 Case2: n=-1 should return empty list");
                        });
                        // </task4>

                        Console.WriteLine("\nResults: " + passed + " passed, " + failed + " failed");
                }

                static void Check(bool condition, string msg)
                {
                        if (!condition) throw new Exception(msg);
                }

                static void RunTest(string name, Action test)
                {
                        try
                        {
                                test();
                                passed++;
                                Console.WriteLine("PASS: " + name);
                        }
                        catch (Exception e)
                        {
                                failed++;
                                Console.WriteLine("FAIL: " + name);
                                Console.WriteLine("      Error Detail: " + e.Message);
                                Console.WriteLine("-------------------------------------------");
                        }
                }
        }
}
