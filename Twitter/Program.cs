var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");
var db = postgres.AddDatabase("postgresdb");

builder.AddProject<Projects.Backend>("backend")
    .WithExternalHttpEndpoints()
    .WithReference(db);


builder.Build().Run();