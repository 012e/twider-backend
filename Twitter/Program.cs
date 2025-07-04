var builder = DistributedApplication.CreateBuilder(args);

// Database (PostgreSQL)
var postgres = builder.AddPostgres("database")
    .WithImage("postgres", tag: "16.4")
    .WithEndpoint(name: "postgres", port: 5432, targetPort: 5432)
    .WithBindMount("../Backend/cache/postgres/data", "/var/lib/postgresql/data")
    .WithEnvironment("POSTGRES_DB", "postgres")
    .WithEnvironment("POSTGRES_USER", "user")
    .WithEnvironment("POSTGRES_PASSWORD", "user");

// Keycloak
var keycloak = builder.AddContainer("keycloak", "quay.io/keycloak/keycloak", "23.0.7")
    .WithHttpEndpoint(port: 6969, targetPort: 8080) // Expose Keycloak on port 6969
    .WithExternalHttpEndpoints()
    .WithBindMount("../Backend/cache/keycloak/data", "/opt/keycloak/data")
    .WithEnvironment("KEYCLOAK_ADMIN", "admin")
    .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin")
    .WithArgs("start-dev");

// Minio
var minio = builder.AddContainer("minio", "quay.io/minio/minio")
    .WithEndpoint(name: "console", port: 9001, targetPort: 9001)
    .WithEndpoint(name: "api", port: 9000, targetPort: 9000)
    .WithExternalHttpEndpoints()
    .WithBindMount("../Backend/cache/minio/data", "/data")
    .WithEnvironment("MINIO_ROOT_USER", "minioadmin")
    .WithEnvironment("MINIO_ROOT_PASSWORD", "minioadmin")
    .WithArgs("server", "/data", "--console-address", ":9001");

// Qdrant
var qdrant = builder.AddContainer("qdrant", "qdrant/qdrant", "v1.14.1")
    .WithEndpoint(name: "grpc", port: 6333, targetPort: 6333)
    .WithHttpEndpoint(6334, 6334)
    .WithExternalHttpEndpoints()
    .WithBindMount("../Backend/cache/qdrant", "/qdrant/storage");

// NATS
var nats = builder.AddContainer("nats", "nats", "2.11.4-linux")
    .WithEndpoint(name: "client", port: 4222, targetPort: 4222)
    .WithEndpoint(name: "cluster", port: 6222, targetPort: 6222)
    .WithEndpoint(name: "monitor", port: 8222, targetPort: 8222)
    .WithExternalHttpEndpoints()
    .WithBindMount("../Backend/cache/nats", "/data")
    .WithArgs("-js", "-sd", "/data");

// Application project with environment variables
var myBackendApi = builder.AddProject<Projects.Backend>("twider-backend-api")
    .WithReference(postgres.GetEndpoint("postgres"))
    .WithReference(keycloak.GetEndpoint("http")) // Keycloak exposes HTTP on port 6969
    .WithReference(minio.GetEndpoint("api")) // Minio API port 9000
    .WithReference(qdrant.GetEndpoint("http"))
    .WithReference(nats.GetEndpoint("client"))
    // OAuth Configuration
    .WithEnvironment("OAuth__ClientId", "twider")
    .WithEnvironment("OAuth__Audience", "twider")
    .WithEnvironment("OAuth__AuthorizationUrl", "http://localhost:6969/realms/master/protocol/openid-connect/auth")
    .WithEnvironment("OAuth__Authority", "http://localhost:6969/realms/master")
    .WithEnvironment("OAuth__TokenUrl", "http://localhost:6969/realms/master/protocol/openid-connect/token")
    // Keycloak Configuration
    .WithEnvironment("Keycloak__Url", "http://localhost:6969")
    .WithEnvironment("Keycloak__Realm", "master")
    .WithEnvironment("Keycloak__Username", "admin")
    .WithEnvironment("Keycloak__Password", "admin")
    // Minio Configuration
    .WithEnvironment("Minio__Endpoint", "localhost:9000")
    .WithEnvironment("Minio__AccessKey", "minioadmin")
    .WithEnvironment("Minio__SecretKey", "minioadmin")
    // Qdrant Configuration
    .WithEnvironment("Qdrant__Url", "http://localhost:6333")
    // OpenAI Configuration (you should replace this with your actual API key)
    .WithEnvironment("OpenAI__ApiKey", "your-openai-api-key-here")
    // ML Search Configuration
    .WithEnvironment("MlSearch__BaseUrl", "http://localhost:8000");

builder.Build().Run();