using Nafis.Services.Implementation.DesignPatterns.Validation.Models;
using System.Threading.Tasks;

namespace Nafis.Services.Implementation.DesignPatterns.Validation
{
    /// <summary>
    /// Interface for all validators in the Chain of Responsibility pattern.
    ///
    /// CHAIN OF RESPONSIBILITY PATTERN:
    /// Think of it like a security checkpoint at an airport:
    /// - First checkpoint: Check passport (Validator 1)
    /// - Second checkpoint: Check boarding pass (Validator 2)
    /// - Third checkpoint: Security scan (Validator 3)
    ///
    /// If ANY checkpoint fails, you don't proceed to the next.
    /// If ALL checkpoints pass, you board the plane!
    ///
    /// In code:
    /// - Each validator checks ONE thing
    /// - If it fails, the chain stops and returns error
    /// - If it passes, it calls the NEXT validator in the chain
    /// - If there's no next validator and everything passed, validation succeeds!
    ///
    /// Benefits:
    /// - Single Responsibility: Each validator has ONE job
    /// - Flexible: Can reorder validators or add new ones easily
    /// - Testable: Test each validator independently
    /// - Reusable: Same validator can be used in different chains
    /// </summary>
    /// <typeparam name="T">The type of object being validated</typeparam>
    public interface IValidator<T>
    {
        /// <summary>
        /// Validates the given object
        /// </summary>
        /// <param name="context">The object to validate</param>
        /// <returns>Validation result indicating success or failure</returns>
        Task<ValidationResult> ValidateAsync(T context);

        /// <summary>
        /// Sets the next validator in the chain
        /// </summary>
        /// <param name="nextValidator">The validator to call if this one passes</param>
        /// <returns>The next validator (for fluent chaining)</returns>
        IValidator<T> SetNext(IValidator<T> nextValidator);

        /// <summary>
        /// Gets the name of this validator (for debugging and logging)
        /// </summary>
        string ValidatorName { get; }
    }
}
