namespace KsSquare.Application.Abstractions.Authentication;
public sealed class OtpDeliveryException : Exception
{
 public OtpDeliveryException() : base("We could not send your verification email. Please wait a minute and try again.") {}
}
