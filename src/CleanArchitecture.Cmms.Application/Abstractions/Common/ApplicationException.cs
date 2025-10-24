namespace CleanArchitecture.Cmms.Application.Abstractions.Common
{
    public class ApplicationException : Exception
    {

        public Error Error { get; }

        /// <summary>
        /// Creates a domain exception with a structured DomainError object.
        /// </summary>
        /// <param name="error">The structured error information</param>
        public ApplicationException(Error error) : base(error.Message)
        {
            Error = error;
        }

        public ApplicationException(Error error, Exception? innerException) : base(error.Message, innerException)
        {
            Error = error;
        }
    }
}
