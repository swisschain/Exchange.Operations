using MatchingEngine.Client.Contracts.Incoming;

namespace Operations.DomainService.Model
{
    public class CreateMarketOrderResponse : OperationResponse
    {
        public CreateMarketOrderResponse()
        {

        }

        public CreateMarketOrderResponse(MarketOrderResponse response)
        {
            Id = response.Id;
            Status = response.Status;
            Reason = response.StatusReason;
            Price = decimal.Parse(response.Price);
        }

        public decimal Price { get; set; }
    }
}
