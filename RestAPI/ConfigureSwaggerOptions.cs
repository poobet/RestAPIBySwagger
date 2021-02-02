using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;
using System.Reflection;

namespace RestAPI
{
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        readonly IApiVersionDescriptionProvider provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) =>
            this.provider = provider;

        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(
                    description.GroupName,
                    new OpenApiInfo()
                    {
                        Title = $"Document API V{description.ApiVersion}",
                        Version = description.ApiVersion.ToString(),
                        Description = "By. Poobet S.",
                        License = new OpenApiLicense()
                        {
                            Name = "Poobet"
                        },

                    });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
                //options.OperationFilter<RemoveVersionParameterFilter>();
                //options.DocumentFilter<ReplaceVersionWithExactValueInPathFilter>();
            }

        }
        //public class RemoveVersionParameterFilter : IOperationFilter
        //{
        //    public void Apply(OpenApiOperation operation, OperationFilterContext context)
        //    {
        //        var versionParameter = operation.Parameters.Single(p => p.Name == "version");
        //        operation.Parameters.Remove(versionParameter);
        //    }
        //}
        //public class ReplaceVersionWithExactValueInPathFilter : IDocumentFilter
        //{
        //    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        //    {
        //        var paths = new OpenApiPaths();
        //        foreach (var path in swaggerDoc.Paths)
        //        {
        //            paths.Add(path.Key.Replace("{version}", swaggerDoc.Info.Version), path.Value);
        //        }
        //        swaggerDoc.Paths = paths;
        //    }
        //}
    }
}
