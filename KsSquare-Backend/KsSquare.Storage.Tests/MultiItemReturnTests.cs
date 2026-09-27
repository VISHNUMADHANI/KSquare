using KsSquare.Application.Features.Orders;
using KsSquare.Application.Features.Products.Services;
using KsSquare.Domain.Entities;
using System.Text.Json;
namespace KsSquare.Storage.Tests;
public sealed class MultiItemReturnTests
{
 private static CustomerOrder Delivered(){var o=new CustomerOrder(Guid.NewGuid(),Guid.NewGuid(),"hash","Test","test@example.test","1234567890","Test address","[]",250);o.ChangeStatus("Processing");o.ChangeStatus("Shipped");o.ChangeStatus("Delivered");return o;}
 private static OrderItemDto[] Items()=>[new(Guid.NewGuid(),"Chain",3,40,[],true),new(Guid.NewGuid(),"Ring",2,65,[],true)];
 private static ReturnRequestDto Entry()=>new(Guid.NewGuid(),0,"Size or fit","The size does not fit.",DateTimeOffset.UtcNow,"Approved",null,50,null,[],[],[0],[0],[new(0,1)],[new(0,1)]);
 [Fact] public void PartialQuantitiesUsePaidUnitPrices(){var lines=ReturnRules.ValidateLines([new(0,2),new(1,1)],Items(),Delivered(),DateTimeOffset.UtcNow);Assert.Equal(145,ReturnRules.ReturnTotal(lines,Items()));}
 [Theory][InlineData(0)][InlineData(-1)][InlineData(4)] public void InvalidQuantityRejected(int quantity)=>Assert.Throws<CatalogValidationException>(()=>ReturnRules.ValidateLines([new(0,quantity)],Items(),Delivered(),DateTimeOffset.UtcNow));
 [Fact] public void DuplicateOrMissingSelectionsRejected(){Assert.Throws<CatalogValidationException>(()=>ReturnRules.ValidateLines([],Items(),Delivered(),DateTimeOffset.UtcNow));Assert.Throws<CatalogValidationException>(()=>ReturnRules.ValidateLines([new(0,1),new(0,1)],Items(),Delivered(),DateTimeOffset.UtcNow));Assert.Throws<CatalogValidationException>(()=>ReturnRules.ValidateLines([new(9,1)],Items(),Delivered(),DateTimeOffset.UtcNow));}
 [Fact] public void PersonalizedItemsStayExcluded(){var items=Items();items[1]=items[1] with{Details=["Name: AB"]};Assert.Throws<CatalogValidationException>(()=>ReturnRules.ValidateLines([new(0,1),new(1,1)],items,Delivered(),DateTimeOffset.UtcNow));}
 [Fact] public void AdminEditClearsAgreementAndRecordsQuantityHistory(){var lines=new ReturnItemSelection[]{new(1,2)};var r=ReturnRules.Review(Entry(),new("UpdateItems","Customer agreed to change the items.",null,false,0,Items:lines),130,true,Guid.NewGuid(),DateTimeOffset.UtcNow);Assert.Equal("Requested",r.Status);Assert.Null(r.RefundAmount);Assert.Null(r.RefundReference);Assert.Equal(lines,r.History[0].Items);}
 [Theory][InlineData("Declined")][InlineData("Refunded")]public void CompletedReturnsCannotBeEdited(string status)=>Assert.Throws<CatalogConflictException>(()=>ReturnRules.Review(Entry() with{Status=status},new("UpdateItems","Change requested by customer.",null,false,0,Items:[new(1,1)]),65,true,Guid.NewGuid(),DateTimeOffset.UtcNow));
 [Fact] public void RefundCannotExceedSelectedQuantityTotal()=>Assert.Throws<CatalogValidationException>(()=>ReturnRules.Review(Entry(),new("AgreeRefund","Amount agreed with customer.",41,false,0,true),40,true,Guid.NewGuid(),DateTimeOffset.UtcNow));
 [Fact] public void LegacySingleItemJsonUsesAllPurchasedUnits(){var legacy=Entry() with{Items=null,ItemIndices=null,CustomerItems=null};var saved=JsonSerializer.Deserialize<ReturnRequestDto>(JsonSerializer.Serialize(legacy))!;Assert.Equal(3,ReturnRules.SelectedLines(saved,Items())[0].Quantity);}

 [Fact] public void AcceptSavesAmountAndInstructionsInOneDecision(){var r=ReturnRules.Review(Entry() with{Status="Requested"},new("Accept","Send back using the supplied return address.",35,false,0,Items:[new(0,1)]),40,false,Guid.NewGuid(),DateTimeOffset.UtcNow);Assert.Equal("Approved",r.Status);Assert.Equal(35,r.RefundAmount);Assert.Null(r.RefundReference);Assert.Single(r.History);Assert.Equal(1,r.History[0].Items![0].Quantity);}
 [Theory][InlineData(-1)][InlineData(41)][InlineData(0.001)]public void AcceptRejectsInvalidAmounts(decimal amount)=>Assert.Throws<CatalogValidationException>(()=>ReturnRules.Review(Entry(),new("Accept","Return instructions supplied.",amount,false,0),40,true,Guid.NewGuid(),DateTimeOffset.UtcNow));
 [Fact] public void AcceptRequiresRefundAmount()=>Assert.Throws<CatalogValidationException>(()=>ReturnRules.Review(Entry(),new("Accept","Return instructions supplied.",null,false,0),40,true,Guid.NewGuid(),DateTimeOffset.UtcNow));
}