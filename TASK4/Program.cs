using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Octokit;
using System;

namespace TASK4
{
    internal class Program
    {
        private const string PagedIssueQuery =
        @"query ($repo_name: String!,  $start_cursor:String) {
          repository(owner: ""dotnet"", name: $repo_name) {
            issues(last: 25, before: $start_cursor)
             {
                totalCount
                pageInfo {
                  hasPreviousPage
                  startCursor
                }
                nodes {
                  title
                  number
                  createdAt
                }
              }
            }
          }
        ";


        public class GraphQLRequest
        {
            [JsonProperty("query")]
            public string Query { get; set; }

            [JsonProperty("variables")]
            public IDictionary<string, object> Variables { get; } = new Dictionary<string, object>();

            public string ToJsonText() =>
                JsonConvert.SerializeObject(this);
        }

        static async Task Main(string[] args)
        {
            string key = "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";

            var client = new GitHubClient(new Octokit.ProductHeaderValue("IssueQueryDemo"))
            {
                Credentials = new Octokit.Credentials(key)
            };

            CancellationTokenSource cancellationSource = new CancellationTokenSource();

            await foreach (var issue in GetIssuesAsync(client, PagedIssueQuery, "docs", cancellationSource.Token))
            {
                Console.WriteLine(issue);
            }

            Console.WriteLine("All issues fetched.");
        }

        private static async IAsyncEnumerable<JObject> GetIssuesAsync(GitHubClient client, string queryText, string repoName, CancellationToken cancel)
        {
            var issueAndPRQuery = new GraphQLRequest
            {
                Query = queryText
            };

            issueAndPRQuery.Variables["repo_name"] = repoName;

            JArray finalResults = new JArray();
            bool hasMorePages = true;
            int pagesReturned = 0;
            int issuesReturned = 0;

            // Stop with 10 pages, because these are large repos:
            while (hasMorePages && (pagesReturned++ < 10))
            {
                var postBody = issueAndPRQuery.ToJsonText();
                var response = await client.Connection.Post<string>(new Uri("https://api.github.com/graphql"),
                    postBody, "application/json", "application/json");

                JObject results = JObject.Parse(response.HttpResponse.Body.ToString());

                int totalCount = (int)issues(results)["totalCount"];
                hasMorePages = (bool)pageInfo(results)["hasPreviousPage"];
                issueAndPRQuery.Variables["start_cursor"] = pageInfo(results)["startCursor"].ToString();
                issuesReturned += issues(results)["nodes"].Count();

                finalResults.Merge(issues(results)["nodes"]);
                var array = issues(results)["nodes"];

                Console.WriteLine($"You have returned {issuesReturned}");

                foreach (var issue in array)
                {
                    yield return (JObject)issue;
                }

                cancel.ThrowIfCancellationRequested();

            }

            JObject issues(JObject result) => (JObject)result["data"]["repository"]["issues"];
            JObject pageInfo(JObject result) => (JObject)issues(result)["pageInfo"];
        }
    }
}
