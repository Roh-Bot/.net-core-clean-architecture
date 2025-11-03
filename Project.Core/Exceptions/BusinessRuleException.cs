namespace Project.Core.Exceptions;

/// <summary>
/// Represents a domain or business rule violation that occurred
/// while executing an application use case.
/// </summary>
public class BusinessRuleException : Exception
{
    /// <summary>
    /// Optional machine-readable error code (e.g., "PAYMENT_INVALID_AMOUNT").
    /// </summary>
    public string? ErrorCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessRuleException"/> class.
    /// </summary>
    public BusinessRuleException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessRuleException"/> class with a specified error message.
    /// </summary>
    public BusinessRuleException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessRuleException"/> class with a specified 
    /// error message and inner exception.
    /// </summary>
    public BusinessRuleException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance with an error code and message.
    /// </summary>
    public BusinessRuleException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}