using BackupService.Application.Services;
using BackupService.Application.Settings;
using Common.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BackupService.Application;

public static class ServiceRegistration
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplicationLayer(IConfiguration configuration)
        {
            services.Configure<BackupSettings>(configuration.GetSection(nameof(BackupSettings)));
            services.Configure<ConnectionStrings>(configuration.GetSection(nameof(ConnectionStrings)));

            services.AddScoped<ISqlBackupService, SqlBackupService>();

            return services;
        }
    }
}
