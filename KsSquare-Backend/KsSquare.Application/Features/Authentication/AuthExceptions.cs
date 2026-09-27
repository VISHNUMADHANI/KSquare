namespace KsSquare.Application.Features.Authentication;

public sealed class AuthValidationException(string message) : Exception(message);
public sealed class AuthConflictException(string message) : Exception(message);
public sealed class AuthUnauthorizedException(string message) : Exception(message);
