using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Backend.Common.Helpers.Types;

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