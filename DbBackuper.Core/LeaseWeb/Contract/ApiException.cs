using System.Runtime.Serialization;

namespace DbBackuper.Core.LeaseWeb.Contract;

public class ApiException : Exception
{
    public ApiException(ErrorDetails? errorDetails)
    {
        ErrorDetails = errorDetails;
    }

    protected ApiException(SerializationInfo info, StreamingContext context, ErrorDetails? errorDetails) : base(info, context)
    {
        ErrorDetails = errorDetails;
    }

    public ApiException(string? message, ErrorDetails? errorDetails) : base(message)
    {
        ErrorDetails = errorDetails;
    }

    public ApiException(string? message, Exception? innerException, ErrorDetails? errorDetails) : base(message, innerException)
    {
        ErrorDetails = errorDetails;
    }

    public ErrorDetails? ErrorDetails { get; set; }

}