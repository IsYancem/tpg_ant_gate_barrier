namespace AntGateBarrier.ScopedServices
{
    public interface IWeightScopedService
    {
        public Task DoWorkAsync(CancellationToken stoppingToken);
    }
}
