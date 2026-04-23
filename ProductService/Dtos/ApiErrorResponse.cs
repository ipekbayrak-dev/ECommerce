namespace ProductService.Dtos
{
    public class ApiErrorResponse
    {
        public required string Message { get; set; }
        public required string CorrelationId { get; set; }
        public DateTime TimestampUtc { get; set; }

        public static ApiErrorResponse Create(string message, string correlationId)
        {
            return new ApiErrorResponse
            {
                Message = message,
                CorrelationId = correlationId,
                TimestampUtc = DateTime.UtcNow
            };
        }
    }
}
