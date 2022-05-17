using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net.NetworkInformation;

namespace HealthCheckAPI
{
    public class ICMPHealthCheck : IHealthCheck
    {
        private readonly string Host;
        private readonly int HealthRoundtripTime;

        public ICMPHealthCheck(string host, int healthyRoundtripTime)
        {
            Host = host;
            HealthRoundtripTime = healthyRoundtripTime;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            HealthCheckResult result;

            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(Host);
                switch (reply.Status)
                {
                    case IPStatus.Success:
                        var msg = $"ICMP to {Host} took {reply.RoundtripTime} ms.";
                        result = (reply.RoundtripTime > HealthRoundtripTime)
                            ? HealthCheckResult.Degraded(msg)
                            : HealthCheckResult.Healthy(msg);
                        break;
                    default:
                        var err = $"ICMP to {Host} failed: {reply.Status}";
                        result = HealthCheckResult.Unhealthy(err);
                        break;
                }
            }
            catch (Exception e)
            {
                var err = $"ICMP to {Host} failed: {e.Message}";
                result = HealthCheckResult.Unhealthy(err);
            }

            return result;
        }
    }
}
