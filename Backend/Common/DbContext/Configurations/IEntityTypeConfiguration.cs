using Microsoft.EntityFrameworkCore;

namespace Backend.Common.DbContext.Configurations;

public interface IEntityTypeConfiguration<T> : Microsoft.EntityFrameworkCore.IEntityTypeConfiguration<T> where T : class
{
}

public static class ModelBuilderExtensions
{
    public static void ApplyConfigurationsFromNamespace(this ModelBuilder modelBuilder, System.Reflection.Assembly assembly, string namespaceName)
    {
        var applyGenericMethod = typeof(ModelBuilder).GetMethod(
            nameof(ModelBuilder.ApplyConfiguration), 
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

        var configurationTypes = assembly.GetTypes()
            .Where(type => !string.IsNullOrEmpty(type.Namespace) && 
                   type.Namespace.StartsWith(namespaceName) &&
                   type.GetInterfaces().Any(i => i.IsGenericType && 
                   i.GetGenericTypeDefinition() == typeof(Microsoft.EntityFrameworkCore.IEntityTypeConfiguration<>)))
            .ToList();

        foreach (var configurationType in configurationTypes)
        {
            var entityType = configurationType.GetInterfaces()
                .First(i => i.IsGenericType && 
                       i.GetGenericTypeDefinition() == typeof(Microsoft.EntityFrameworkCore.IEntityTypeConfiguration<>))
                .GetGenericArguments()[0];
                
            var configuration = Activator.CreateInstance(configurationType);
            
            var method = applyGenericMethod.MakeGenericMethod(entityType);
            method.Invoke(modelBuilder, new[] { configuration });
        }
    }
}