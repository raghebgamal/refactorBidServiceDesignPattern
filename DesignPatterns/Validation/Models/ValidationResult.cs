using Nafes.CrossCutting.Common.OperationResponse;
using System.Collections.Generic;
using System.Linq;

namespace Nafis.Services.Implementation.DesignPatterns.Validation.Models
{
    /// <summary>
    /// Result of a validation operation.
    ///
    /// Think of this like a test result:
    /// - IsValid = Did you pass the test? (true/false)
    /// - Errors = What did you get wrong? (list of errors)
    /// - ErrorCode = What kind of error? (for API responses)
    ///
    /// This is similar to ValidationResult in FluentValidation library.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Indicates if the validation passed
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// List of validation error messages
        /// </summary>
        public List<string> Errors { get; set; }

        /// <summary>
        /// HTTP error code (for API responses)
        /// </summary>
        public HttpErrorCode? HttpErrorCode { get; set; }

        /// <summary>
        /// Application-specific error code
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Name of the validator that failed (for debugging)
        /// </summary>
        public string FailedValidatorName { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ValidationResult()
        {
            Errors = new List<string>();
        }

        /// <summary>
        /// Creates a successful validation result
        /// </summary>
        public static ValidationResult Success()
        {
            return new ValidationResult
            {
                IsValid = true,
                Errors = new List<string>()
            };
        }

        /// <summary>
        /// Creates a failed validation result with a single error
        /// </summary>
        public static ValidationResult Failure(string errorMessage, string errorCode = null, HttpErrorCode? httpErrorCode = null)
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = new List<string> { errorMessage },
                ErrorCode = errorCode,
                HttpErrorCode = httpErrorCode
            };
        }

        /// <summary>
        /// Creates a failed validation result with multiple errors
        /// </summary>
        public static ValidationResult Failure(List<string> errors, string errorCode = null, HttpErrorCode? httpErrorCode = null)
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = errors,
                ErrorCode = errorCode,
                HttpErrorCode = httpErrorCode
            };
        }

        /// <summary>
        /// Adds an error to the result
        /// </summary>
        public ValidationResult AddError(string errorMessage)
        {
            IsValid = false;
            Errors.Add(errorMessage);
            return this;
        }

        /// <summary>
        /// Combines multiple validation results
        /// </summary>
        public static ValidationResult Combine(params ValidationResult[] results)
        {
            if (results == null || results.Length == 0)
                return Success();

            var allErrors = results.SelectMany(r => r.Errors).ToList();

            if (allErrors.Count == 0)
                return Success();

            return Failure(allErrors);
        }

        /// <summary>
        /// Gets the first error message (useful for displaying)
        /// </summary>
        public string FirstError => Errors.FirstOrDefault();

        /// <summary>
        /// Gets all errors as a single string (useful for logging)
        /// </summary>
        public string AllErrorsAsString => string.Join("; ", Errors);
    }
}
