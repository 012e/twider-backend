using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Result<T, E>
{
    public T? Value { get; }
    public E? Error { get; }
    public bool IsSuccess { get; }
    public bool IsFailed => !IsSuccess;

    protected Result(T value)
    {
        Value = value;
        IsSuccess = true;
    }

    protected Result(E error)
    {
        Error = error;
        IsSuccess = false;
    }

    public static Result<T, E> Ok(T value) => new(value);
    public static Result<T, E> Fail(E error) => new(error);

}

public class ApiResult<T> : Result<T, ProblemDetails>
{
    private ApiResult(T value) : base(value) { }
    private ApiResult(ProblemDetails error) : base(error) { }

    public static implicit operator ApiResult<T>(T value) => new(value);
    public static implicit operator ApiResult<T>(ProblemDetails error) => new(error);

    public new static ApiResult<T> Ok(T value) => new(value);
    public new static ApiResult<T> Fail(ProblemDetails error) => new(error);

    public static implicit operator ApiResult<T>(ApiResult<object> result)
    {
        if (result.IsSuccess)
        {
            if (result.Value is T value)
                return new ApiResult<T>(value);

            throw new InvalidCastException($"Cannot convert value of type '{result.Value?.GetType()}' to '{typeof(T)}'.");
        }
        else
        {
            return new ApiResult<T>(result.Error!);
        }
    }
}

public static class ApiResult
{
    public static ApiResult<T> Ok<T>(T value) => ApiResult<T>.Ok(value);
    public static ApiResult<object> Fail(ProblemDetails error) => ApiResult<object>.Fail(error);
}


public static class ApiResultExtensions
{
    public static IResult ToIResult<T>(this ApiResult<T> result)
    {
        if (result.IsSuccess && result.Value is not null)
        {
            return Results.Ok(result.Value);
        }
        else if (result.IsFailed && result.Error is not null)
        {
            return Results.Problem(
                title: result.Error.Title,
                detail: result.Error.Detail,
                statusCode: result.Error.Status,
                type: result.Error.Type,
                instance: result.Error.Instance
            );
        }
        else
        {
            return Results.StatusCode(500);
        }
    }
}

public class ResultJsonConverter<T, E> : JsonConverter<Result<T, E>>
{
    public override Result<T, E>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // For this example, we're focusing on the writing part
        // Implementation of deserialization would depend on how you want to handle incoming JSON
        throw new NotImplementedException("Deserialization is not implemented for Result<T, E>");
    }

    public override void Write(Utf8JsonWriter writer, Result<T, E> value, JsonSerializerOptions options)
    {
        if (value.IsSuccess && value.Value != null)
        {
            // Directly serialize just the Value when successful
            JsonSerializer.Serialize(writer, value.Value, options);
        }
        else if (value.IsFailed && value.Error != null)
        {
            // Directly serialize just the Error when failed
            JsonSerializer.Serialize(writer, value.Error, options);
        }
        else
        {
            // Handle edge case where both Value and Error are null
            writer.WriteNullValue();
        }
    }
}

public class ApiResultJsonConverter<T> : JsonConverter<ApiResult<T>>
{
    public override ApiResult<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // For this example, we're focusing on the writing part
        throw new NotImplementedException("Deserialization is not implemented for ApiResult<T>");
    }

    public override void Write(Utf8JsonWriter writer, ApiResult<T> value, JsonSerializerOptions options)
    {
        if (value.IsSuccess && value.Value != null)
        {
            // Directly serialize just the Value when successful
            JsonSerializer.Serialize(writer, value.Value, options);
        }
        else if (value.IsFailed && value.Error != null)
        {
            // Directly serialize just the Error when failed
            JsonSerializer.Serialize(writer, value.Error, options);
        }
        else
        {
            // Handle edge case where both Value and Error are null
            writer.WriteNullValue();
        }
    }
}


// Custom JsonConverterFactory for Result types
public class ResultJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
            return false;

        var genericTypeDef = typeToConvert.GetGenericTypeDefinition();
        return genericTypeDef == typeof(Result<,>) || genericTypeDef == typeof(ApiResult<>);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (!typeToConvert.IsGenericType)
            return null;

        var genericTypeDef = typeToConvert.GetGenericTypeDefinition();
        var args = typeToConvert.GetGenericArguments();

        if (genericTypeDef == typeof(Result<,>))
        {
            var converterType = typeof(ResultJsonConverter<,>).MakeGenericType(args);
            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }
        else if (genericTypeDef == typeof(ApiResult<>))
        {
            var converterType = typeof(ApiResultJsonConverter<>).MakeGenericType(args);
            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }

        return null;
    }
}

// Extension method to easily register the converters
public static class ResultJsonConverterExtensions
{
    public static JsonSerializerOptions AddResultConverters(this JsonSerializerOptions options)
    {
        options.Converters.Add(new ResultJsonConverterFactory());
        return options;
    }
}
