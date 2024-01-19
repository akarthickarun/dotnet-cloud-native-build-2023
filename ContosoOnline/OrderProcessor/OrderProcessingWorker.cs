public class OrderProcessingWorker(ILogger<OrderProcessingWorker> logger,
                                   IServiceScopeFactory serviceScopeFactory,
                                   IConfiguration configuration)
    : BackgroundService
{
    private TimeSpan CheckOrderInterval => TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation($"Worker running at: {DateTime.UtcNow}");
            logger.LogInformation($"Using Order URL: {configuration["ORDERS_URL"]} and Products URL: {configuration["PRODUCTS_URL"]}");

            await using var scope = serviceScopeFactory.CreateAsyncScope();

            var request = scope.ServiceProvider.GetRequiredService<OrderProcessingRequest>();

            try
            {
                await request.ProcessOrdersAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // We're shutting down
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting orders");
            }

            await Task.Delay(CheckOrderInterval, stoppingToken);
        }
    }
}
