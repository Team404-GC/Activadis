namespace Activadis.API.Middlewares
{
    public class LogEndpointMiddleware
    {
        private readonly RequestDelegate Request;

        public LogEndpointMiddleware(RequestDelegate request)
        {
            Request = request;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await Request(context);

            Endpoint? endpoint = context.GetEndpoint();
            if (endpoint is null)
                return;

            string name = context.Request.Path.Value ?? "?";
            string method = context.Request.Method;
            if (!name.StartsWith('/'))
                name = '/' + name;

            bool succeeded = context.Response.StatusCode >= 200 && context.Response.StatusCode < 300;
            string status = succeeded ? "SUCCEEDED" : "FAILED";

            ConsoleColor original = Console.ForegroundColor;
            if (method == "OPTIONS")
            {
                if (succeeded)
                    return;

                Console.ForegroundColor = ConsoleColor.Red;
            }

            if (name != "?")
            {
                List<int> ignoreIndexes = [];
                if (endpoint is RouteEndpoint route)
                    foreach ((int index, string item) in (route.RoutePattern.RawText ?? "").Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Index())
                        if (item.StartsWith('{') && item.EndsWith('}'))
                            ignoreIndexes.Add(index);

                List<string> parts = [.. name.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)];
                bool isUpper = parts[0].StartsWith(char.ToUpperInvariant(parts[0][0]));

                if (parts.Where((x, i) => !ignoreIndexes.Contains(i) && !x.StartsWith(isUpper ? char.ToUpperInvariant(x[0]) : char.ToLowerInvariant(x[0]))).Any())
                {
                    List<string> faulty = [..parts.Where(x => !x.StartsWith(isUpper ? char.ToUpperInvariant(x[0]) : char.ToLowerInvariant(x[0]))).Select(x => $"'{x}'")];
                    string warning = faulty.Count > 2 ? $"{string.Join(", ", faulty.SkipLast(1))} AND {faulty.Last()}" : string.Join(" AND ", faulty);
                    string word = faulty.Count >= 2 ? "ARE" : "IS";
                    warning += !isUpper ? $" {word} CAPITALIZED UNLIKE THE CONTROLLER: '{parts[0]}'" : $" {word} NOT CAPITALIZED LIKE THE CONTROLLER: '{parts[0]}'";

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"\nEndpoint Warning:\t {method} {name} | {warning}\n");
                    Console.ForegroundColor = original;
                }
            }

            string message = $"Endpoint Log:\t\t {method} {name} | {context.Response.StatusCode} {status}";
            Console.WriteLine(message);
            Console.ForegroundColor = original;
        }
    }
}
