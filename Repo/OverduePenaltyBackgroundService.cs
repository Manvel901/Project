using Diplom.Abstract;

namespace Diplom.Repo
{
    public class OverduePenaltyBackgroundService:BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OverduePenaltyBackgroundService> _logger;

        public OverduePenaltyBackgroundService(IServiceProvider serviceProvider, ILogger<OverduePenaltyBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var penaltyService = scope.ServiceProvider.GetRequiredService<IPenaltyService>();
                        var createdCount = penaltyService.CheckAndCreateOverduePenalties();

                        if (createdCount > 0)
                        {
                            _logger.LogInformation("Automatically created {Count} overdue penalties", createdCount);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in overdue penalty background service");
                }

                // Проверяем каждые 24 часа
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}

