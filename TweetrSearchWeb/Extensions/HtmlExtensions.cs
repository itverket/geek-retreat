using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace TweetrSearchWeb.Extensions
{
    public static class HtmlExtensions
    {
        private const string UrlRegEx = @"((http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)";
        private const string UserNameRegEx = "(@)((?:[a-zæøå0-9-_]*))";
        private const string HashTagRegEx = "(#)((?:[a-zæøå0-9-_]*))";

        public static MvcHtmlString DisplayWithLinksFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            var content = GetContent(htmlHelper, expression);
            var withUrls = ReplaceUrlsWithLinks(content);
            var withUserNames = ReplaceUserNamesWithLinks(withUrls);
            var withHashTags = ReplaceHashTagssWithLinks(withUserNames);
            
            return MvcHtmlString.Create(withHashTags);
        }

        private static string ReplaceUrlsWithLinks(string input)
        {
            var rx = new Regex(UrlRegEx, RegexOptions.IgnoreCase);
            var result = rx.Replace(input, delegate(Match match)
            {
                var url = match.ToString();
                return string.Format("<a href=\"{0}\">{0}</a>", url);
            });
            return result;
        }

        private static string ReplaceUserNamesWithLinks(string input)
        {
            var rx = new Regex(UserNameRegEx, RegexOptions.IgnoreCase);
            var result = rx.Replace(input, delegate (Match match)
            {
                var username = match.ToString().Replace("@",string.Empty);
                if (string.IsNullOrEmpty(username))
                {
                    return match.ToString();
                }
                return string.Format("<a href=\"#\" onclick=\"document.getElementById('username').value='{0}'; document.getElementById('hashTags').value=null; document.forms[0].submit(); return false;\">@{0}</a>", username);
            });
            return result;
        }
        private static string ReplaceHashTagssWithLinks(string input)
        {
            var rx = new Regex(HashTagRegEx, RegexOptions.IgnoreCase);
            var result = rx.Replace(input, delegate (Match match)
            {
                var hashTag = match.ToString().Replace("#",string.Empty);
                if (string.IsNullOrEmpty(hashTag))
                {
                    return match.ToString();
                }
                return string.Format("<a href=\"#\" onclick=\"document.getElementById('hashTags').value='{0}'; document.getElementById('username').value=null; document.forms[0].submit(); return false;\">#{0}</a>", hashTag);
            });
            return result;
        }

        private static string GetContent<TModel, TProperty>(HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            var func = expression.Compile();
            return func(htmlHelper.ViewData.Model).ToString();
        }
    }
}