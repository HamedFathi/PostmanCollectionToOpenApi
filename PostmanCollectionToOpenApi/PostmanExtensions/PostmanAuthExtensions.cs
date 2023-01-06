// ReSharper disable UnusedMember.Global

namespace PostmanCollectionToOpenApi.PostmanExtensions;

public static class PostmanAuthExtensions
{
    public static IList<OpenApiSecurityRequirement> ToOpenApiSecurityRequirements(this PostmanAuth auth)
    {
        var key = auth.ToOpenApiSecurityScheme();
        return new List<OpenApiSecurityRequirement>
        {
            new()
            {
                {key, new List<string>()}
            }
        };
    }

    public static OpenApiSecurityScheme ToOpenApiSecurityScheme(this PostmanAuth auth)
    {
        if (auth == null) throw new ArgumentNullException(nameof(auth));

        switch (auth.Type)
        {
            case PostmanAuthType.Apikey:
                break;

            case PostmanAuthType.Awsv4:
                break;

            case PostmanAuthType.Basic:
                return new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Query,
                    Type = SecuritySchemeType.Http,
                    Scheme = "basic",
                    Reference = new OpenApiReference
                    {
                        Id = "basicAuth",
                        Type = ReferenceType.SecurityScheme,
                    }
                };

            case PostmanAuthType.Bearer:
                return new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Query,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    Reference = new OpenApiReference
                    {
                        Id = "bearerAuth",
                        Type = ReferenceType.SecurityScheme,
                    }
                };

            case PostmanAuthType.Digest:
                break;

            case PostmanAuthType.Edgegrid:
                break;

            case PostmanAuthType.Hawk:
                break;

            case PostmanAuthType.Noauth:
                break;

            case PostmanAuthType.Ntlm:
                break;

            case PostmanAuthType.Oauth1:
                return new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Query,
                    Type = SecuritySchemeType.Http,
                    Scheme = "oauth1",
                    Reference = new OpenApiReference
                    {
                        Id = "oauth1Auth",
                        Type = ReferenceType.SecurityScheme,
                    }
                };

            case PostmanAuthType.Oauth2:
                return new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Query,
                    Type = SecuritySchemeType.Http,
                    Scheme = "oauth2",
                    Reference = new OpenApiReference
                    {
                        Id = "oauth2Auth",
                        Type = ReferenceType.SecurityScheme,
                    }
                };

            default:
                throw new ArgumentOutOfRangeException();
        }

        return new OpenApiSecurityScheme();
    }
}