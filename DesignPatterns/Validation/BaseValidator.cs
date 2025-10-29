using Nafis.Services.Implementation.DesignPatterns.Validation.Models;
using System.Threading.Tasks;

namespace Nafis.Services.Implementation.DesignPatterns.Validation
{
    /// <summary>
    /// Base class for all validators implementing the Chain of Responsibility pattern.
    ///
    /// This class handles the CHAINING LOGIC so each validator doesn't have to.
    ///
    /// How it works:
    /// 1. Validator checks its ONE rule
    /// 2. If rule fails → return error, stop chain
    /// 3. If rule passes → call next validator in chain (if any)
    /// 4. If no next validator → validation complete, success!
    ///
    /// Example Chain:
    /// RequiredFieldsValidator → DateRangeValidator → AuthorizationValidator
    ///         ↓                        ↓                      ↓
    ///    Check fields exist      Check dates valid      Check user allowed
    ///
    /// If RequiredFieldsValidator fails, the other two NEVER run (fast fail).
    /// </summary>
    /// <typeparam name="T">The type of object being validated</typeparam>
    public abstract class BaseValidator<T> : IValidator<T>
    {
        /// <summary>
        /// The next validator in the chain
        /// </summary>
        protected IValidator<T> NextValidator { get; private set; }

        /// <summary>
        /// Name of this validator (for debugging/logging)
        /// </summary>
        public abstract string ValidatorName { get; }

        /// <summary>
        /// Main validation method that implements the chaining logic.
        /// This is SEALED (cannot be overridden) to ensure all validators follow the same pattern.
        /// </summary>
        public async Task<ValidationResult> ValidateAsync(T context)
        {
            // STEP 1: Run THIS validator's rules
            var result = await ValidateInternalAsync(context);

            // STEP 2: If validation failed, stop here and return error
            if (!result.IsValid)
            {
                result.FailedValidatorName = ValidatorName;
                return result;
            }

            // STEP 3: If validation passed and there's a next validator, run it
            if (NextValidator != null)
            {
                return await NextValidator.ValidateAsync(context);
            }

            // STEP 4: No next validator and we passed = SUCCESS!
            return ValidationResult.Success();
        }

        /// <summary>
        /// Sets the next validator in the chain.
        /// Returns the next validator to enable fluent chaining:
        /// validator1.SetNext(validator2).SetNext(validator3)...
        /// </summary>
        public IValidator<T> SetNext(IValidator<T> nextValidator)
        {
            NextValidator = nextValidator;
            return nextValidator;  // Return next for fluent chaining
        }

        /// <summary>
        /// Internal validation method that each validator must implement.
        /// This is where you put your specific validation rules.
        /// </summary>
        /// <param name="context">The object to validate</param>
        /// <returns>ValidationResult indicating pass/fail</returns>
        protected abstract Task<ValidationResult> ValidateInternalAsync(T context);

        #region Helper Methods for Common Validations

        /// <summary>
        /// Helper to create a failure result with standard error codes
        /// </summary>
        protected ValidationResult Fail(string errorMessage, string errorCode = null)
        {
            return ValidationResult.Failure(errorMessage, errorCode, Nafes.CrossCutting.Common.OperationResponse.HttpErrorCode.InvalidInput);
        }

        /// <summary>
        /// Helper to check if a string is null or empty
        /// </summary>
        protected bool IsNullOrEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Helper to check if an object is null
        /// </summary>
        protected bool IsNull(object value)
        {
            return value == null;
        }

        #endregion
    }
}
