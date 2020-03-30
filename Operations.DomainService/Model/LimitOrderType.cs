namespace Operations.DomainService.Model
{
    /// <summary>
    /// Specifies a limit order type.
    /// </summary>
    public enum LimitOrderType
    {
        /// <summary>
        /// Unspecified limit order type.
        /// </summary>
        None,

        /// <summary>
        /// Sell limit order type.
        /// </summary>
        Sell,

        /// <summary>
        /// Buy limit order type.
        /// </summary>
        Buy
    }
}
