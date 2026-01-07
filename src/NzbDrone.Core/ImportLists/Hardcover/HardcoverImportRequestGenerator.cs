using System.Linq;
using NLog;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Hardcover
{
    public class HardcoverImportRequestGenerator : IImportListRequestGenerator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public HardcoverImportSettings Settings { get; set; }

        public int MaxPages { get; set; } = 1;
        public int PageSize { get; set; } = 200;

        public ImportListPageableRequestChain GetListItems()
        {
            var pageableRequests = new ImportListPageableRequestChain();
            var apiKey = NormalizeApiKey(Settings.ApiKey);
            var hasLists = Settings.ListIds != null && Settings.ListIds.Any();
            var hasStatuses = Settings.BookStatusIds != null && Settings.BookStatusIds.Any();

            // Generate request for lists if configured
            if (hasLists)
            {
                Logger.Debug("Hardcover: Fetching list books from user lists");

                var listQuery = @"{
  ""query"": ""query ListBooks { me { lists { slug name list_books { book { id title contributions { author { id name } } } } } } }""
}";

                var listRequest = BuildGraphQlRequest(apiKey, listQuery);
                pageableRequests.Add(new[] { new ImportListRequest(listRequest) });
            }

            // Generate request for user book statuses if configured
            if (hasStatuses)
            {
                var statusIds = string.Join(",", Settings.BookStatusIds);
                Logger.Debug("Hardcover: Fetching user books with status IDs: {0}", statusIds);

                var statusQuery = "{\"query\": \"query UserBooks { me { user_books(where: {user_book_status: {id: {_in: [ " + statusIds + " ] } } }) { book { id title contributions { author { id name } } } } } }\"}";

                var statusRequest = BuildGraphQlRequest(apiKey, statusQuery);
                pageableRequests.Add(new[] { new ImportListRequest(statusRequest) });
            }

            return pageableRequests;
        }

        private HttpRequest BuildGraphQlRequest(string apiKey, string graphQlBody)
        {
            var request = new HttpRequestBuilder($"{Settings.BaseUrl.TrimEnd('/')}/v1/graphql")
                .Post()
                .Accept(HttpAccept.Json)
                .SetHeader("Authorization", $"Bearer {apiKey}")
                .SetHeader("X-Api-Key", apiKey)
                .SetHeader("User-Agent", "Readarr (Hardcover Import)")
                .SetHeader("Content-Type", "application/json")
                .KeepAlive()
                .Build();

            request.SetContent(graphQlBody);
            return request;
        }

        private string NormalizeApiKey(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return string.Empty;
            }

            var trimmed = apiKey.Trim();
            const string bearerPrefix = "bearer ";

            if (trimmed.StartsWith(bearerPrefix, System.StringComparison.OrdinalIgnoreCase))
            {
                return trimmed.Substring(bearerPrefix.Length).Trim();
            }

            return trimmed;
        }
    }
}
