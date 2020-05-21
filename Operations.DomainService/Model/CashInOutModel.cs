namespace Operations.DomainService.Model
{
    /// <summary>
    /// Represents cash in or out model.
    /// </summary>
    public class CashInOutModel
    {
        /// <summary>
        /// Client's account.
        /// </summary>
        public ulong AccountId { get; set; }

        /// <summary>
        /// Client's wallet.
        /// </summary>
        public ulong WalletId { get; set; }

        /// <summary>
        /// Asset that has to be transferred.
        /// </summary>
        public string Asset { get; set; }

        /// <summary>
        /// Amount of the asset that has to be transferred.
        /// </summary>
        public decimal Volume { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        public CashInOutModel()
        {
        }

        public CashInOutModel(string asset, decimal volume, ulong accountId, ulong walletId, string description)
        {
            Asset = asset;
            Volume = volume;
            AccountId = accountId;
            WalletId = walletId;
            Description = description;
        }
    }
}
