namespace AntGateBarrier.ScopedServices
{
    public interface IVehicleScopedService
    {
        public Task DoWorkAsync(CancellationToken stoppingToken);
    }
}
