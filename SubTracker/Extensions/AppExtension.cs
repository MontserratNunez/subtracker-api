using Asp.Versioning;
using Microsoft.Extensions.Options;
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
                            var name = $"SubTracker API - {apiVersion.GroupName.ToUpperInvariant()}";
                            opt.SwaggerEndpoint(url, name);
                        }
                    }

                    opt.HeadContent = @"
                    <script>
                        document.addEventListener('DOMContentLoaded', function() {
                            var checkExist = setInterval(function() {
                                if (window.ui) {
                                    clearInterval(checkExist);
                                    
                                    var config = window.ui.getConfigs();
                                    config.requestInterceptor = function(req) {
                                        function getCookie(name) {
                                            var value = '; ' + document.cookie;
                                            var parts = value.split('; ' + name + '=');
                                            if (parts.length === 2) return parts.pop().split(';').shift();
                                        }
                                        var token = getCookie('XSRF-TOKEN');
                                        if (token && req.method !== 'GET') {
                                            req.headers['X-XSRF-TOKEN'] = decodeURIComponent(token);
                                        }
                                        return req;
                                    };
                                }
                            }, 100);
                        });
                    </script>";
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
