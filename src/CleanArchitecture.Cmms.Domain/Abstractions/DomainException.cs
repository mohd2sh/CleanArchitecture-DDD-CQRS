namespace CleanArchitecture.Cmms.Domain.Abstractions
{
    /// <summary>
    /// Exception thrown when domain invariants are violated.
    /// </summary>
    public class DomainException : Exception
    {
        /// <summary>
        /// The structured error information associated with this exception.
        /// </summary>
        public DomainError Error { get; }

        /// <summary>
        /// Creates a domain exception with a structured DomainError object.
        /// </summary>
        /// <param name="error">The structured error information</param>
        public DomainException(DomainError error) : base(error.Message)
        {
            Error = error;
        }

        /// <summary>
        /// Creates a domain exception with a structured DomainError object and inner exception.
        /// </summary>
        /// <param name="error">The structured error information</param>
        /// <param name="innerException">The inner exception</param>
        public DomainException(DomainError error, Exception? innerException) : base(error.Message, innerException)
        {
            Error = error;
        }
    }
}
