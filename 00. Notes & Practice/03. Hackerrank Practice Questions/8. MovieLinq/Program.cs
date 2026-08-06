using System;
using System.Collections.Generic;
using System.Linq;

namespace Solution
{
    public class Solution
    {
        public static int HighestRating(List<Movie> movies)
        {
            return movies.Max(movie => movie.Rating);
        }

        public static int LowestRating(List<Movie> movies)
        {
            return movies.Min(movie => movie.Rating);
        }

        public static int AverageRating(List<Movie> movies)
        {
            return (int)Math.Round(movies.Average(movie => movie.Rating));
        }

        public static Dictionary<string, Movie> HighestRatingForEachGenre(List<Movie> movies)
        {
            var highestRatingsByGenre = movies
                .GroupBy(movie => movie.Genre)
                .ToDictionary(
                    group => group.Key,
                    group => group.OrderByDescending(movie => movie.Rating).First()
                )
                .OrderBy(pair => pair.Key) // Order by genre alphabetically
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            return highestRatingsByGenre;
        }

        public static void Main()
        {
            int countOfMovies = int.Parse(Console.ReadLine());

            var movies = new List<Movie>();
            for (int i = 0; i < countOfMovies; i++)
            {
                string str = Console.ReadLine();
                string[] strArr = str.Split(' ');

                movies.Add(new Movie
                {
                    Title = strArr[0],
                    Genre = strArr[1],
                    Rating = int.Parse(strArr[2])
                });
            }

            Console.WriteLine($"Highest rating is {HighestRating(movies)}");
            Console.WriteLine($"Lowest rating is {LowestRating(movies)}");
            Console.WriteLine($"Average rating is {AverageRating(movies)}");
            foreach (var mv in HighestRatingForEachGenre(movies))
            {
                Console.WriteLine($"The highest rating for genre {mv.Key} is {mv.Value.Rating}. Movie's title is {mv.Value.Title}");
            }
        }
    }

    public class Movie
    {
        public string Title { get; set; }
        public string Genre { get; set; }
        public int Rating { get; set; }
    }
}
