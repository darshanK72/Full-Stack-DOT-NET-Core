/*
 * PROBLEM: Article Tag Manager
 *
 * Blog tag deduplication and set algebra across articles.
 *
 * This exercise covers:
 *   ch05 — HashSet, set operations, IEqualityComparer<T>
 */

using System;
using System.Collections.Generic;

namespace PublishingTags
{
    class Article
    {
        public string Title { get; }
        public HashSet<string> Tags { get; }

        public Article(string title, IEnumerable<string> seedTags)
        {
            Title = title;
            Tags = new HashSet<string>(seedTags, StringComparer.OrdinalIgnoreCase);
        }

        public bool AddTag(string tag) => Tags.Add(tag);

        public override string ToString() => $"{Title} [{string.Join(", ", Tags)}]";
    }

    class Subscriber
    {
        public string Name { get; }
        public string Email { get; }

        public Subscriber(string name, string email)
        {
            Name = name;
            Email = email;
        }

        public override string ToString() => $"{Name} <{Email}>";
    }

    sealed class SubscriberByEmailComparer : IEqualityComparer<Subscriber>
    {
        public bool Equals(Subscriber? x, Subscriber? y)
        {
            // TODO: same email case-insensitive
            throw new NotImplementedException();
        }

        public int GetHashCode(Subscriber obj)
        {
            // TODO: hash from email — must match Equals rule
            throw new NotImplementedException();
        }
    }

    static class TagAnalytics
    {
        public static HashSet<string> UnionTags(Article a, Article b)
        {
            // TODO: non-mutating union, OrdinalIgnoreCase
            throw new NotImplementedException();
        }

        public static HashSet<string> SharedTags(Article a, Article b)
        {
            // TODO: intersect
            throw new NotImplementedException();
        }

        public static HashSet<string> TagsOnlyIn(Article a, Article b)
        {
            // TODO: except — tags in a not in b
            throw new NotImplementedException();
        }

        public static void MergeDraftTags(HashSet<string> editorPool, IEnumerable<string> draftTags)
        {
            // TODO: UnionWith in place
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: two articles, union/shared/only-in reports
            // TODO: subscriber comparer vs default HashSet — same email demo
            // TODO: duplicate AddTag("LINQ") when "linq" exists
            throw new NotImplementedException();
        }
    }
}
