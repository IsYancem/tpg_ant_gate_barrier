namespace AntGateBarrier.ScopedServices
{
    public interface IAnntScopedService
    {
        public Task DoWorkAsync(CancellationToken stoppingToken);
    }
}
