using Asp.Versioning;
using Microsoft.OpenApi;

namespace SubTracker.Extensions
{
    public static class AppExtension
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S3267:Loops should be simplified with \"LINQ\" expressions", Justification = "<Pending>")]
        public static void UseSwaggerExtension(this IApplicationBuilder app, IEndpointRouteBuilder routeBuilder) {
            app.UseSwagger();
            app.UseSwaggerUI(opt =>
            {
                try
                {
                    var versionDescriptions = routeBuilder.DescribeApiVersions();
                    if (versionDescriptions != null && versionDescriptions.Any())
                    {
                        foreach (var apiVersion in versionDescriptions)
                        {
                            var url = $"/swagger/{apiVersion.GroupName}/swagger.json";
                            var name = $"RealEstate API - {apiVersion.GroupName.ToUpperInvariant()}";
                            opt.SwaggerEndpoint(url, name);
                        }
                    }
                }
                catch (System.Reflection.ReflectionTypeLoadException ex)
                {
                    foreach (var loaderException in ex.LoaderExceptions)
                    {
                        var errMsg = $"[DIAGNÓSTICO API] Error de Carga: {loaderException?.Message}";
                        System.Diagnostics.Debug.WriteLine(errMsg);
                        Console.WriteLine(errMsg);
                    }
                    throw;
                }
            });
        }
    }
}
