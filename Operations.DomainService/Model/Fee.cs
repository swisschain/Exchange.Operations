using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Operations.DomainService.Model
{
    /// <summary>
    /// Represents fee
    /// </summary>
    public class Fee
    {
        /// <summary>
        /// Asset identifier
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// Fee type
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public FeeType Type { get; set; }

        /// <summary>
        /// Source wallet identifier
        /// </summary>
        public string SourceWalletId { get; set; }

        /// <summary>
        /// Target wallet identifier
        /// </summary>
        public string TargetWalletId { get; set; }

        /// <summary>
        /// Size
        /// </summary>
        public decimal Size { get; set; }

        /// <summary>
        /// Type of size
        /// </summary>
        public int? SizeType { get; set; }
    }
}
