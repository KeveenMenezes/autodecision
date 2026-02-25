namespace AutodecisionMultipleFlagsProcessor.DTOs
{
    public class AppServiceBaseResponse
    {
        public bool IsSuccess { get; set; }

        public ErrorDetail ErrorDetail { get; set; }

    }

    public class AppServiceBaseResponse<TData> : AppServiceBaseResponse
    {
        public TData Data { get; set; }
    }

    public class ErrorDetail
    {
        public string Description { get; set; }

        public string HttpCode { get; set; }

        public ErrorDetail(string description, string httpCode)
        {
            Description = description;
            HttpCode = httpCode;
        }
    }
}
