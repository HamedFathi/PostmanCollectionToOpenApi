namespace PostmanCollectionToOpenApi.OpenApiExtensions
{
    internal static class OpenApiSchemaExtensions
    {
        internal static OpenApiSchema ToOpenApiSchema(this string jsObjectOrArrayString, Dictionary<string, string>? variables = null)
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

        internal static OpenApiSchema ToOpenApiSchema(this JToken jToken, Dictionary<string, string>? variables = null)
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
                schema = schema.OpenApiSchemaMaker(jsonDetail, jsonDetails, variables);
            }
            return schema;
        }

        private static OpenApiSchema ConvertToOpenApiSchema(this OpenApiValueType openApiValueType, string value, Dictionary<string, string>? variables = null)
        {
            var (type, format) = openApiValueType.GetTypeAndFormat();
            return new OpenApiSchema()
            {
                Example = value.ToExample(variables),
                Type = type,
                Format = format
            };
        }

        private static OpenApiSchema OpenApiSchemaMaker(this OpenApiSchema openApiSchema, JTokenDetail jTokenDetail,
            IList<JTokenDetail> jTokenDetails, Dictionary<string, string>? variables)
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
                            var jTokenDetailArray = jTokenDetails.First(x =>
                                x.SharedKey == jTokenDetail.SharedKey && !x.IsObjectOrArray);
                            var arraySchema = new OpenApiSchema
                            {
                                Type = "array",
                                Example = jTokenDetailArray.JTokenString.ToExample(variables),
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
                            var jTokenDetailObject = jTokenDetails.First(x =>
                                x.SharedKey == jTokenDetail.SharedKey && !x.IsObjectOrArray);
                            // The place you add the final primitive value.
                            currentSchema.Properties.Add(name, new OpenApiSchema
                            {
                                Type = type,
                                Format = format,
                                Example = jTokenDetailObject.JTokenString.ToExample(variables),
                                Nullable = isNullable
                            });
                        }
                    }
                }
                // Parent
                else
                {
                    //var parentText = GetParentJsonString(jsonDetail, jToken);

                    // Parent is an array
                    if (isArray)
                    {
                        // Parent does not exist
                        if (!currentSchema.Properties.ContainsKey(name))
                        {
                            var jTokenDetailArray = jTokenDetails.First(x =>
                                x.SharedKey == jTokenDetail.SharedParentKey && x.JTokenType == JTokenType.Array);
                            var arraySchema = new OpenApiSchema
                            {
                                Type = "array",
                                Example = jTokenDetailArray.JTokenString.ToExample(variables)
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
                            var jTokenDetailObject = jTokenDetails.First(x =>
                                x.SharedKey == jTokenDetail.SharedParentKey && x.JTokenType == JTokenType.Object);
                            var objectSchema = new OpenApiSchema
                            {
                                Type = "object",
                                Example = jTokenDetailObject.JTokenString.ToExample(variables)
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