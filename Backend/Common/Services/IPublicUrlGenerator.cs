namespace Backend.Common.Services;

public interface IPublicUrlGenerator
{
    Task<string> GenerateUrlAsync(string bucket, string path);
}