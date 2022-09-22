﻿using AngleSharp;
using AngleSharp.Html.Parser;
using NovelParserBLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NovelParserBLL.Parsers.Kemono
{
    internal class KemonoParser : INovelParser
    {
        private readonly string kemonoUrl = "https://kemono.party/";
        private readonly string urlPattern = @"https:\/\/kemono\.party\/patreon\/user\/\d*";
        private readonly HtmlParser parser = new HtmlParser();

        public Task ParseAndLoadChapters(Novel novel, SortedList<int, Chapter> chapters, bool includeImages, Action<int, int> setProgress, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<Novel?> ParseAsync(string novelUrl, CancellationToken cancellationToken)
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(novelUrl);

            var coverUrl = kemonoUrl + document.QuerySelector(".fancy-image__image")?.GetAttribute("src");
            var paginator = document.QuerySelector(".paginator > small")?.TextContent ?? "of 0";

            var countPosts = int.Parse(paginator.Substring(paginator.IndexOf("of") + 2).Trim());

            var posts = document.QuerySelectorAll(".post-card__heading > a").Select(el => new { Href = el.GetAttribute("href"), Title = el.TextContent });


            return null;
        }

        public bool ValidateUrl(string url)
        {
            return !string.IsNullOrEmpty(PrepareUrl(url));
        }

        private string PrepareUrl(string url)
        {
            return Regex.Match(url, urlPattern).Value;
        }
    }
}
