namespace ProductsService.Domain.Exceptions
{
    public class ApplicationValidationException : Exception
    {
        public IDictionary<string, string[]> Errors { get; }

        public ApplicationValidationException(IDictionary<string, string[]> errors)
            : base("Validation failed")
        {
            Errors = errors;
        }
    }
}