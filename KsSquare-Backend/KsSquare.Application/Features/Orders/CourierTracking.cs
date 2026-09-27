using KsSquare.Application.Features.Products.Services;
namespace KsSquare.Application.Features.Orders;
public static class CourierTracking {
 public static string? Url(string? courier,string? number){if(string.IsNullOrWhiteSpace(number))return null;var n=Uri.EscapeDataString(number);return courier switch{
 "USPS"=>"https://tools.usps.com/go/TrackConfirmAction?tLabels="+n,
 "UPS"=>"https://www.ups.com/track?loc=en_US&tracknum="+n,
 "DHL"=>"https://www.dhl.com/global-en/home/tracking.html?tracking-id="+n,
 "FedEx"=>"https://www.fedex.com/fedextrack/?trknbr="+n,
 "Aramex"=>"https://www.aramex.com/track/results?ShipmentNumber="+n,_=>null};}
 public static (string Courier,string Number) Validate(string? courier,string? number){var n=number?.Trim()??"";if(Url(courier,"test")==null)throw new CatalogValidationException("Choose USPS, UPS, DHL, FedEx or Aramex.");if(!System.Text.RegularExpressions.Regex.IsMatch(n,@"\A[A-Za-z0-9-]{5,80}\z"))throw new CatalogValidationException("Enter a tracking number of 5 to 80 letters, digits or hyphens.");return(courier!,n);}
}
