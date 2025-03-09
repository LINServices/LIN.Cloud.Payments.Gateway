using Yarp.ReverseProxy.Health;
using Yarp.ReverseProxy.Model;

namespace LIN.Cloud.Payments.Gateway.Services;

public class HealthPolicy : IActiveHealthCheckPolicy
{
    public string Name => "HealthPolicy";

    public void ProbingCompleted(ClusterState cluster, IReadOnlyList<DestinationProbingResult> probingResults)
    {
        foreach (var result in probingResults)
        {
            var destination = cluster.Destinations[result.Destination.DestinationId];

            // Si la respuesta es exitosa (200-299), marcamos el destino como saludable
            if (result.Response?.IsSuccessStatusCode == true)
            {
                destination.Health.Active = DestinationHealth.Healthy;
            }
            else
            {
                // Si la respuesta no es exitosa o hubo un fallo, marcamos como no saludable
                destination.Health.Active = DestinationHealth.Unhealthy;
            }
        }
    }
}