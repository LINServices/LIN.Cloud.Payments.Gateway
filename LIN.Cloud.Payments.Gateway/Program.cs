using LIN.Cloud.Payments.Gateway.Services;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Health;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IActiveHealthCheckPolicy, HealthPolicy>();

builder.Services.AddReverseProxy()
    .LoadFromMemory(
    [
        new RouteConfig()
        {
            RouteId = "mercado_pago_production",
            ClusterId = "mercado_cluster",
            Match = new RouteMatch() { Path = "/MercadoPago/{**catch-all}" },
            Transforms = new List<Dictionary<string, string>>
            {
                new() { { "RequestHeader", "x-gateway" }, { "Set", "MercadoPago" } }
            }
        }
    ],
    [
        new ClusterConfig()
        {
            ClusterId = "mercado_cluster",
            Destinations = new Dictionary<string, DestinationConfig>
            {
                { "mercado1", new DestinationConfig() { Address = "https://www.cloud.mercadopago.linplatform.com", } },
                { "mercadoazure", new DestinationConfig() { Address = "https://linmercadopago-a3dfgvahbpdpabfv.canadacentral-01.azurewebsites.net/" } }
            },
            HealthCheck = new () {
                Active = new(){
                    Enabled = true,
                    Interval = TimeSpan.FromSeconds(5),
                    Timeout = TimeSpan.FromSeconds(2),
                    Policy = "HealthPolicy",
                    Path ="/health"
                },
                Passive = new() {
                    Enabled = false,
                }
            }
        }
    ]);

var app = builder.Build();

app.MapReverseProxy();
app.Run();
