namespace PostmanCollectionToOpenApi.OpenApiExtensions
{
    public static class OpenApiSchemaExtensions
    {
        public static OpenApiSchema ConvertToOpenApiSchema(this OpenApiValueType openApiValueType, string value, Dictionary<string, string>? variables = null)
        {
            variables ??= new Dictionary<string, string>();
            var (type, format) = openApiValueType.GetTypeAndFormat();
            return new OpenApiSchema
            {
                Example = value.ToExample(variables),
                Type = type,
                Format = format
            };
        }

        public static OpenApiSchema ToOpenApiSchema(this string jsObjectOrArrayString, Dictionary<string, string>? variables = null)
        {
            variables ??= new Dictionary<string, string>();
            var isArray = jsObjectOrArrayString.IsArrayText();
            var isObject = jsObjectOrArrayString.IsJsonText();

            // Is simple value
            if (!isObject && !isArray)
            {
                jsObjectOrArrayString = jsObjectOrArrayString.ReplaceVariables(variables);
                var schemaValue = jsObjectOrArrayString.ToOpenApiValueType().ConvertToOpenApiSchema(jsObjectOrArrayString, variables);
                return schemaValue;
            }
            var rootName = Constants.Separator;
            var json = jsObjectOrArrayString.ConvertJavascriptObjectOrArrayToJson(rootName).ReplaceVariables(variables);
            var jToken = JToken.Parse(json);
            var schema = jToken.ToOpenApiSchema(variables);
            return isArray ? schema.Properties.First().Value : schema;
        }

        public static OpenApiSchema ToOpenApiSchema(this JToken jToken, Dictionary<string, string>? variables = null)
        {
            variables ??= new Dictionary<string, string>();
            var schema = new OpenApiSchema
            {
                Type = "object",
                Example = jToken.ToString().ToExample(variables)
            };
            var jsonDetails = jToken.GetJTokenDetails();
            var filteredJsonDetails = jsonDetails.Where(x => !x.IsObjectOrArray).ToList();
            foreach (var jsonDetail in filteredJsonDetails)
            {
                schema = schema.OpenApiSchemaMaker(jsonDetail, jsonDetails, jToken, variables);
            }
            return schema;
        }

        private static string? GetParentJsonString(JTokenDetail jTokenDetail, JToken jToken)
        {
            string? parentText = null;
            if (jTokenDetail.Path.Length > 1 && jToken is JObject obj)
            {
                var key = jTokenDetail.Path.Take(jTokenDetail.Path.Length - 1)
                    .Aggregate((a, b) => $"{a}.{b}").RemoveArrayIndex();
                parentText = obj.SelectToken(key)?.ToString();
            }

            if (jTokenDetail.Path.Length > 1 && jToken is JArray arr)
            {
                var key = jTokenDetail.Path.Take(jTokenDetail.Path.Length - 1)
                    .Aggregate((a, b) => $"{a}.{b}").RemoveArrayIndex();
                parentText = arr.SelectToken(key)?.ToString();
            }

            return parentText;
        }

        private static OpenApiSchema OpenApiSchemaMaker(this OpenApiSchema openApiSchema, JTokenDetail jTokenDetail,
            IList<JTokenDetail> jTokenDetails, JToken jToken, Dictionary<string, string>? variables = null)
        {
            variables ??= new Dictionary<string, string>();
            var currentSchema = openApiSchema;
            for (var i = 0; i < jTokenDetail.Path.Length; i++)
            {
                var pathDetail = jTokenDetail.Path[i].GetPathDetail();
                var name = pathDetail.name;
                var index = pathDetail.index;
                var isChild = i + 1 == jTokenDetail.Path.Length; // Is child
                var isArray = index != -1; // path related to an array or object
                var (type, format) = jTokenDetail.Value.ConvertToOpenApiValueType().GetTypeAndFormat();
                var isNullable = type == null;
                type ??= "object";
                // Child, the place to add value
                if (isChild)
                {
                    // Child in an Array
                    if (isArray)
                    {
                        // Array does not exist and we should create it first.
                        if (!currentSchema.Properties.ContainsKey(name))
                        {
                            var exampleItems = jTokenDetails.Where(x =>
                                x.SharedKey == jTokenDetail.SharedKey && !x.IsObjectOrArray)
                                .Select(x => x.JTokenString.ToExample(variables))
                                .ToList();
                            var example = new OpenApiArray();
                            example.AddRange(exampleItems);
                            var arraySchema = new OpenApiSchema
                            {
                                Type = "array",
                                Example = example,
                                Items = new OpenApiSchema
                                {
                                    Type = type,
                                    Format = format,
                                    Nullable = isNullable,
                                    Example = jTokenDetail.Value == null
                                        ? null
                                        : jTokenDetail.JTokenString.ToExample(variables)
                                }
                            };
                            currentSchema.Properties.Add(name, arraySchema);
                        }

                        // Array is there we should just add value.
                        // Nothing to do!
                    }
                    // Child in an Object
                    else
                    {
                        if (!currentSchema.Properties.ContainsKey(name))
                        {
                            var example = jTokenDetail.JTokenString.ToExample(variables);
                            // The place you add the final primitive value.
                            currentSchema.Properties.Add(name, new OpenApiSchema
                            {
                                Type = type,
                                Format = format,
                                Example = example,
                                Nullable = isNullable
                            });
                        }
                    }
                }
                // Parent
                else
                {
                    var parentText = GetParentJsonString(jTokenDetail, jToken);
                    // Parent is an array
                    if (isArray)
                    {
                        var example = parentText!.ToExample(variables);
                        // Parent does not exist
                        if (!currentSchema.Properties.ContainsKey(name))
                        {
                            var arraySchema = new OpenApiSchema
                            {
                                Type = "array",
                                Example = example
                            };
                            currentSchema.Properties.Add(name, arraySchema);
                            currentSchema = currentSchema.Properties[name].Items =
                                new OpenApiSchema { Type = "object" };
                        }
                        // Parent exists
                        else
                        {
                            currentSchema = currentSchema.Properties[name].Items;
                        }
                    }
                    // Parent is an object
                    else
                    {
                        // Object does not exist. we need to create it first.
                        if (!currentSchema.Properties.Keys.Contains(name))
                        {
                            var objectSchema = new OpenApiSchema
                            {
                                Type = "object",
                                Example = string.IsNullOrEmpty(parentText)
                                    ? null
                                    : JToken.Parse(parentText!).ToOpenApiObject()
                            };
                            currentSchema.Properties.Add(name, objectSchema);
                            currentSchema = objectSchema;
                        }
                        // Object exists. we should assign the existing object then.
                        else
                        {
                            currentSchema = currentSchema.Properties[name];
                        }
                    }
                }
            }

            return openApiSchema;
        }
    }
}