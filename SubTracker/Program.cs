using RealEstateApi.Extensions;
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
        public async static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers()
            .AddJsonOptions(opt =>
            {
                opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

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

            await app.RunAsync();
        }
    }
}
