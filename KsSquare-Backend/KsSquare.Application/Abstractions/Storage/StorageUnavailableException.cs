namespace KsSquare.Application.Abstractions.Storage;

public sealed class StorageUnavailableException(string message, Exception innerException) : Exception(message, innerException);
