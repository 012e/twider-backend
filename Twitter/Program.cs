var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");
var db = postgres.AddDatabase("postgresdb");
var keycloak = builder
    .AddContainer("keycloak", "quay.io/keycloak/keycloak")
    .WithArgs("start-dev")
    .WithHttpEndpoint(port: 8080, targetPort: 6969, name: "keycloak");

builder.AddProject<Projects.Backend>("backend")
    .WithExternalHttpEndpoints()
    .WithReference(db)
    .WithReference(keycloak.GetEndpoint("keycloak"));


builder.Build().Run();