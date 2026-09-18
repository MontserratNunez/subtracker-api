using Microsoft.AspNetCore.Mvc;
using SubTracker.Core.Application;
using SubTracker.Extensions;
using SubTracker.Infrastructure.Identity;
using SubTracker.Infrastructure.Persistence;
using SubTracker.Infrastructure.Shared;
using System.Text.Json.Serialization;

namespace SubTracker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            })
            .AddJsonOptions(opt =>
            {
                opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            builder.Services.AddPersistenceLayerIoc(builder.Configuration);
            builder.Services.AddApplicationLayerIoc();
            builder.Services.AddSharedLayerIoc(builder.Configuration);
            builder.Services.AddIdentityLayerIocForApi(builder.Configuration);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddHealthChecks();
            builder.Services.AddAppiVersioningExtension();
            builder.Services.AddSwaggerExtension();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwaggerExtension(app);
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHealthChecks("/health");

            app.MapControllers();

            app.Run();
        }
    }
}
