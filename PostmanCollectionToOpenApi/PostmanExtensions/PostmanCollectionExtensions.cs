// ReSharper disable UnusedMember.Global

namespace PostmanCollectionToOpenApi.PostmanExtensions;

public static class PostmanCollectionExtensions
{
    public static IList<PostmanItems> FlattenItem(this PostmanCollection postmanCollection)
    {
        if (postmanCollection == null) throw new ArgumentNullException(nameof(postmanCollection));

        var result = postmanCollection.Item.FlattenByStack(x => x.Item)
                .Where(x => x.Request != null || x.Response != null)
            ;
        return result.ToList();
    }

    public static Dictionary<string, IList<PostmanItems>> FlattenItemWithParents(this PostmanCollection postmanCollection, string noParentSymbol = "$", string separator = " > ")
    {
        if (postmanCollection == null) throw new ArgumentNullException(nameof(postmanCollection));
        if (string.IsNullOrWhiteSpace(separator))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(separator));
        if (string.IsNullOrWhiteSpace(noParentSymbol))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(noParentSymbol));

        var result = new Dictionary<string, IList<PostmanItems>>();
        var tempResult = new List<Tuple<string, PostmanItems>>();
        var stack = new Stack<PostmanItems>();
        var parents = new Stack<string>();
        var parentsCounter = new Stack<int>();

        var items = postmanCollection.Item;
        items.Reverse();
        foreach (var item in items)
            stack.Push(item);

        while (stack.Count > 0)
        {
            var current = stack.Pop();

            // Parent
            if (current.Response == null && current.Request == null)
            {
                parents.Push(current.Name);
            }

            // Child
            else
            {
                var p = parents.Count == 0 ? noParentSymbol : parents.Reverse().Aggregate((a, b) => a + separator + b);
                tempResult.Add(new Tuple<string, PostmanItems>(p, current));
            }

            // Coordinator
            if (parentsCounter.Count > 0)
            {
                var number = parentsCounter.Pop() - 1;
                parentsCounter.Push(number);
                if (parentsCounter.Peek() == 0)
                {
                    parentsCounter.Pop();
                    parents.Pop();
                }
            }

            // Nested
            var children = current.Item;
            if (children != null)
            {
                parentsCounter.Push(children.Count);
                children.Reverse();
                foreach (var child in children)
                {
                    stack.Push(child);
                }
            }
        }

        foreach (var tr in tempResult)
        {
            if (result.ContainsKey(tr.Item1))
            {
                result[tr.Item1].Add(tr.Item2);
            }
            else
            {
                result.Add(tr.Item1, new List<PostmanItems>());
                result[tr.Item1].Add(tr.Item2);
            }
        }

        return result;
    }

    public static IList<PostmanAuth?> FlattenRequestAuth(this PostmanCollection postmanCollection)
    {
        if (postmanCollection == null) throw new ArgumentNullException(nameof(postmanCollection));

        var result = postmanCollection.Item.FlattenByStack(x => x.Item)
                .Where(x => x.Request?.RequestClass.Auth != null)
                .Select(x => x.Request?.RequestClass.Auth)
            ;
        return result.ToList();
    }

    public static IList<string> GetTags(this PostmanCollection postmanCollection, string noParentSymbol = "$", string separator = " > ")
    {
        return postmanCollection.FlattenItemWithParents(noParentSymbol, separator)
            .Where(x => x.Key != noParentSymbol)
            .Select(x => x.Key).OrderBy(x => x).ToList();
    }

    public static OpenApiDocument ToOpenApiDocument(this string postmanCollectionJson, PostmanToOpenApiSettings? settings = null)
    {
        if (string.IsNullOrWhiteSpace(postmanCollectionJson))
        {
            throw new ArgumentException($"'{nameof(postmanCollectionJson)}' cannot be null or whitespace.", nameof(postmanCollectionJson));
        }
        var postmanCollection = PostmanCollection.FromJson(postmanCollectionJson);
        return postmanCollection.ToOpenApiDocument(settings);
    }

    public static OpenApiDocument ToOpenApiDocument(this PostmanCollection postmanCollection, PostmanToOpenApiSettings? settings = null)
    {
        if (postmanCollection is null)
        {
            throw new ArgumentNullException(nameof(postmanCollection));
        }
        settings ??= new PostmanToOpenApiSettings();

        var openApiDocument = new OpenApiDocument
        {
            Info = postmanCollection.Info.ToOpenApiInfo(settings),
            Servers = postmanCollection.ToOpenApiServers(settings),
            Tags = postmanCollection.ToOpenApiTags(),
            Paths = postmanCollection.ToOpenApiPaths(settings)
        };

        var mainAuth = postmanCollection.Auth;
        if (mainAuth != null)
        {
            openApiDocument.SecurityRequirements = mainAuth.ToOpenApiSecurityRequirements();
        }
        var auth = GetOpenApiSecuritySchemes(postmanCollection);
        if (auth.Count > 0)
        {
            openApiDocument.Components = new OpenApiComponents
            {
                SecuritySchemes = auth.ToDictionary(x => $"{x.Scheme.ToLower()}Auth", y => y)
            };
        }

        return openApiDocument;
    }

    public static OpenApiPaths ToOpenApiPaths(this PostmanCollection postmanCollection, PostmanToOpenApiSettings settings)
    {
        var result = new OpenApiPaths();
        var list = postmanCollection.FlattenItemWithParents();

        foreach (var data in list)
        {
            var tags = data.Key == "$"
                ? null
                : data.Key.Split(new[] { " > " }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => new OpenApiTag { Name = x }).ToList();
            var variables = postmanCollection.Variable.MergeVariables(settings.PostmanVariables);
            foreach (var pk in data.Value.GetRequestPathKeys(variables))
            {
                var key = pk.Key;
                var items = pk.Value.DistinctBy(x => x.Request?.RequestClass.Method);

                var openApiPathItem = new OpenApiPathItem
                {
                    Operations = new Dictionary<OperationType, OpenApiOperation>()
                };

                foreach (var op in items)
                {
                    if (op.Request == null) continue;
                    var opType = op.Request.Value.RequestClass.Method.GetOperationType();
                    openApiPathItem.Operations.Add(opType, new OpenApiOperation
                    {
                        Description = op.Description?.String,
                        Summary = op.Name,
                        RequestBody = op.GetOpenApiRequestBody(variables),
                        Parameters = op.GetOpenApiParameters(variables),
                        Responses = op.GetOpenApiResponses(variables),
                        Security = op.Request?.RequestClass.Auth?.ToOpenApiSecurityRequirements(),
                        Tags = tags
                    });
                }

                if (result.ContainsKey(key))
                {
                    MergeAllAvailableOperationsForSameKey(result, key, openApiPathItem);
                }
                else
                {
                    result.Add(key, openApiPathItem);
                }
            }
        }

        return result;
    }

    public static IList<OpenApiServer> ToOpenApiServers(this PostmanCollection postmanCollection, PostmanToOpenApiSettings? settings)
    {
        if (postmanCollection is null)
        {
            throw new ArgumentNullException(nameof(postmanCollection));
        }

        settings ??= new PostmanToOpenApiSettings();

        var items = postmanCollection.FlattenItem();
        var variables = postmanCollection.Variable.MergeVariables(settings.PostmanVariables);

        var result = new List<OpenApiServer>();
        var uniqueUrls = new HashSet<string>();

        var requestUrls = items
                .Select(x => x.Request.GetRawUrl()?.ReplaceVariables(variables))
            ;

        var responseUrls = items
                .SelectMany(x => x.Response)
                .Select(y => y.GetRawUrl()?.ReplaceVariables(variables))
            ;

        var list = requestUrls.Concat(responseUrls);
        foreach (var item in list)
        {
            if (item != null && !string.IsNullOrWhiteSpace(item))
            {
                var uri = item.GetHostWithScheme();
                var isUnique = uniqueUrls.Add(uri);
                if (isUnique)
                {
                    result.Add(new OpenApiServer
                    {
                        Url = uri
                    });
                }
            }
        }

        return result;
    }

    public static IList<OpenApiTag> ToOpenApiTags(this PostmanCollection postmanCollection)
    {
        var result = new List<OpenApiTag>();
        var tags = postmanCollection.GetTags();

        foreach (var tag in tags)
        {
            if (!string.IsNullOrWhiteSpace(tag))
            {
                result.Add(new OpenApiTag
                {
                    Name = tag
                });
            }
        }
        return result;
    }

    private static IList<OpenApiSecurityScheme> GetOpenApiSecuritySchemes(PostmanCollection postmanCollection)
    {
        var auth = new List<OpenApiSecurityScheme>();
        if (postmanCollection.Auth != null)
        {
            auth.Add(postmanCollection.Auth.ToOpenApiSecurityScheme());
        }

        foreach (var reqAuth in postmanCollection.FlattenRequestAuth())
        {
            if (reqAuth != null)
            {
                var scheme = reqAuth.ToOpenApiSecurityScheme();
                if (auth.All(x => x.Scheme != scheme.Scheme))
                {
                    auth.Add(scheme);
                }
            }
        }

        return auth;
    }

    private static OperationType GetOperationType(this string operationType)
    {
        if (string.IsNullOrWhiteSpace(operationType))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(operationType));

        return operationType.ToLower() switch
        {
            "delete" => OperationType.Delete,
            "get" => OperationType.Get,
            "head" => OperationType.Head,
            "options" => OperationType.Options,
            "patch" => OperationType.Patch,
            "post" => OperationType.Post,
            "put" => OperationType.Put,
            "trace" => OperationType.Trace,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static void MergeAllAvailableOperationsForSameKey(OpenApiPaths? result, string key, OpenApiPathItem? openApiPathItem)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;
        if (result == null || openApiPathItem == null || result.Count == 0 || !result.ContainsKey(key)) return;

        var oldOpenApiPathItem = result[key];
        if (oldOpenApiPathItem.Operations.Count < openApiPathItem.Operations.Count)
        {
            result[key] = openApiPathItem;
        }

        foreach (var oldOp in oldOpenApiPathItem.Operations)
        {
            if (!result[key].Operations.ContainsKey(oldOp.Key))
            {
                result[key].AddOperation(oldOp.Key, oldOp.Value);
            }
        }

        foreach (var newOp in openApiPathItem.Operations)
        {
            if (!result[key].Operations.ContainsKey(newOp.Key))
            {
                result[key].AddOperation(newOp.Key, newOp.Value);
            }
        }
    }
}