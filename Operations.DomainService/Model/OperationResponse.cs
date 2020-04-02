using System;
using MatchingEngine.Client.Contracts.Incoming;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Operations.DomainService.Model
{
    /// <summary>
    /// Represents response of operation.
    /// </summary>
    public class OperationResponse
    {
        public OperationResponse()
        {
        }

        public OperationResponse(Response response)
        {
            Id = response.Id;
            Status = response.Status;
            Reason = response.StatusReason;
        }

        /// <summary>
        /// The unique identifier of operation.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The status of operation.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public Status Status { get; set; }

        /// <summary>
        /// The error status message of operation.
        /// </summary>
        public string Reason { get; set; }
    }
}
