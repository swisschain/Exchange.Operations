namespace Operations.DomainService.Model
{
    /// <summary>
    /// Represents cash operation.
    /// </summary>
    public class CashOperationModel
    {
        public string ClientId { get; set; }

        public string Symbol { get; set; }

        public decimal Amount { get; set; }
    }
}
