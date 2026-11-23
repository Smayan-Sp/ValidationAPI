using Validation_Management_API.Data;
using Validation_Management_API.Models;
using Microsoft.EntityFrameworkCore;
using Validation_Management_API.Models.Enums;

namespace Validation_Management_API.BackgroundServices
{
    public class OverdueTasks : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OverdueTasks> _logger;

        public OverdueTasks(IServiceScopeFactory scopeFactory, ILogger<OverdueTasks> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromHours(24));

            do
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    List<ProjectTask> taskList = await db.ProjectTasks
                        .Where(t => t.Status != ProjectTaskStatus.Completed && !t.IsOverdue && t.PlannedEndDate < DateTime.UtcNow)
                        .ToListAsync();

                    List<int> overdueTasks = [];

                    foreach (var task in taskList)
                    {
                        task.IsOverdue = true;
                        overdueTasks.Add(task.Id);
                    }

                    if (overdueTasks.Count > 0)
                    {
                        _logger.LogInformation("Found {Count} overdue tasks listed here: {Tasks}", overdueTasks.Count, string.Join(", ", overdueTasks));
                        await db.SaveChangesAsync();
                    }
                    else
                        _logger.LogInformation("Found 0 overdue tasks");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Overdue task marking failed");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));

        }

    }
}
