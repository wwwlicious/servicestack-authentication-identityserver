namespace ServiceStack.Authentication.IdentityServer.Extensions
{
    using System;
    using System.Collections.Generic;
    using Web;

    public static class RequestExtensions
    {
        public static IEnumerable<Tuple<string, string>> GetFragments(this IRequest request)
        {
            return GetFragments(request.AbsoluteUri);
        }

        public static IEnumerable<Tuple<string, string>> GetFragments(this string url)
        {
            var uri = new Uri(url);
            var fragment = uri.Fragment;

            if (string.IsNullOrWhiteSpace(fragment))
            {
                yield break;
            }

            if (fragment.StartsWith("#"))
            {
                fragment = fragment.Substring(1);
            }

            var fragments = fragment.Split('&');
            foreach (var frag in fragments)
            {
                var pair = frag.Split('=');
                yield return Tuple.Create(pair[0], pair[1]);
            }
        }
    }
}
