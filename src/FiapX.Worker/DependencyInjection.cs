using FiapX.Application.Interfaces.DataSources;
using FiapX.Application.Interfaces.Services;
using FiapX.Infrastructure.DataSources;
using FiapX.Infrastructure.DbContexts;
using FiapX.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace FiapX.Worker
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureForWorker(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString(Configuration.ConnectionString)
                ?? configuration.GetConnectionString("Default");

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null
                    );
                });
            });

            services.AddScoped<IVideoDataSource, VideoDataSource>();
            services.AddScoped<IUserDataSource, UserDataSource>();
            services.AddScoped<IStorageService, LocalStorageService>();
            services.AddScoped<IVideoProcessingService, FFmpegVideoProcessingService>();
            services.AddScoped<INotificationService, EmailNotificationService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddSingleton<IMessageQueueService, RabbitMQService>();

            return services;
        }
    }
}
