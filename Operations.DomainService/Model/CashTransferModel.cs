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
        /// Client's account.
        /// </summary>
        public ulong AccountId { get; set; }

        /// <summary>
        /// Source wallet.
        /// </summary>
        public ulong FromWalletId { get; set; }

        /// <summary>
        /// Target wallet.
        /// </summary>
        public ulong ToWalletId { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        public CashTransferModel()
        {
        }

        public CashTransferModel(string asset, decimal volume, ulong accountId, ulong fromWalletId, ulong toWalletId, string description)
        {
            Asset = asset;
            Volume = volume;
            AccountId = accountId;
            FromWalletId = fromWalletId;
            ToWalletId = toWalletId;
            Description = description;
        }
    }
}
