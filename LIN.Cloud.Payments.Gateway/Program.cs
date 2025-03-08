using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

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
                { "mercado1", new DestinationConfig() { Address = "https://www.cloud.mercadopago.linplatform.com" } }
            },
            HealthCheck = new () {
                Active = new(){
                    Enabled = true,
                    Interval = TimeSpan.FromSeconds(120),
                    Timeout = TimeSpan.FromSeconds(10),
                    Policy = "ConsecutiveFailures",
                    Path ="/health"
                },
                Passive = new() {
                    Enabled = false,
                },
            }
        }
    ]);

var app = builder.Build();

app.MapReverseProxy();
app.Run();
