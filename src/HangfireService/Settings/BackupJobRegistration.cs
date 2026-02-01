using Hangfire;
using HangfireService.Features.Commands;
using HangfireService.Features.Jobs;
using Microsoft.Extensions.Options;

namespace HangfireService.Settings;

public static class BackupJobRegistration
{
    extension(IApplicationBuilder app)
    {
        public IApplicationBuilder RegisterRecurringBackupJobs()
        {
            using var scope = app.ApplicationServices.CreateScope();
            var settings = scope.ServiceProvider.GetRequiredService<IOptions<BackupJobSettings>>().Value;

            if (!settings.Enabled)
            {
                return app;
            }

            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            var recurring = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

            recurring.AddOrUpdate<HangfireMediatorExecutor>(
                settings.JobStartName,
                x => x.ExecuteNamed(settings.JobStartName, new StartSqlBackupJobCommand()),
                settings.CronExpression);

            logger.LogInformation(
                "Registered backup job {JobName} with cron {CronExpression}",
                settings.JobStartName,
                settings.CronExpression);

            return app;
        }
    }
}
