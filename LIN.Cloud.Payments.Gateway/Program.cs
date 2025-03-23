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
            LoadBalancingPolicy="RoundRobin",
            Destinations = new Dictionary<string, DestinationConfig>
            {
                { "mercado1", new DestinationConfig() { Address = "https://cloud.mercadopago.linplatform.com", } },
                { "mercadoazure", new DestinationConfig() { Address = "https://linmercadopago-a3dfgvahbpdpabfv.canadacentral-01.azurewebsites.net/" } }
            },
            HealthCheck = new () {
                Active = new(){
                    Enabled = true,
                    Interval = TimeSpan.FromSeconds(2),
                    Timeout = TimeSpan.FromSeconds(2),
                    Policy = "ConsecutiveFailures",
                    Path ="/health"
                },
                Passive = new() {
                    Enabled = false,
                }
            }
        }
    ]);

var app = builder.Build();

app.MapGet("/", () => "Welcome to LIN Cloud Payments Gateway");

app.MapReverseProxy();
app.Run();
