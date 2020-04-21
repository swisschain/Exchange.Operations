namespace Operations.DomainService.Model
{
    /// <summary>
    /// Represents cash transfer operation.
    /// </summary>
    public class CashTransferModel
    {
        /// <summary>
        /// Asset that has to be transferred.
        /// </summary>
        public string Asset { get; set; }

        /// <summary>
        /// Amount of the asset that has to be transferred.
        /// </summary>
        public decimal Volume { get; set; }

        /// <summary>
        /// Source wallet.
        /// </summary>
        public string FromWallet { get; set; }

        /// <summary>
        /// Target wallet.
        /// </summary>
        public string ToWallet { get; set; }
    }
}
