using System.Collections.Generic;

namespace Nafis.Services.Implementation.DesignPatterns.Validation
{
    /// <summary>
    /// Fluent builder for constructing validation chains.
    ///
    /// This makes it EASY to build validation chains in a readable way:
    ///
    /// OLD WAY (if we didn't have this builder):
    /// var validator1 = new RequiredFieldsValidator();
    /// var validator2 = new BidDatesValidator();
    /// var validator3 = new BidPriceValidator();
    /// validator1.SetNext(validator2);
    /// validator2.SetNext(validator3);
    /// var result = await validator1.ValidateAsync(context);
    ///
    /// NEW WAY (with this builder):
    /// var chain = new ValidationChainBuilder<BidValidationContext>()
    ///     .Add(new RequiredFieldsValidator())
    ///     .Add(new BidDatesValidator())
    ///     .Add(new BidPriceValidator())
    ///     .Build();
    /// var result = await chain.ValidateAsync(context);
    ///
    /// Much cleaner and more readable!
    ///
    /// This is an example of the BUILDER PATTERN, which we've seen before
    /// in the email notification fluent API.
    /// </summary>
    /// <typeparam name="T">The type of validation context</typeparam>
    public class ValidationChainBuilder<T>
    {
        private readonly List<IValidator<T>> _validators;

        public ValidationChainBuilder()
        {
            _validators = new List<IValidator<T>>();
        }

        /// <summary>
        /// Adds a validator to the chain
        /// </summary>
        public ValidationChainBuilder<T> Add(IValidator<T> validator)
        {
            _validators.Add(validator);
            return this;  // Return this for fluent chaining
        }

        /// <summary>
        /// Adds multiple validators to the chain
        /// </summary>
        public ValidationChainBuilder<T> AddRange(params IValidator<T>[] validators)
        {
            _validators.AddRange(validators);
            return this;
        }

        /// <summary>
        /// Builds the validation chain by linking all validators together.
        /// Returns the FIRST validator in the chain.
        /// </summary>
        public IValidator<T> Build()
        {
            if (_validators.Count == 0)
            {
                return null;
            }

            // Link all validators together in order
            for (int i = 0; i < _validators.Count - 1; i++)
            {
                _validators[i].SetNext(_validators[i + 1]);
            }

            // Return the first validator (head of the chain)
            return _validators[0];
        }

        /// <summary>
        /// Gets the number of validators in this chain
        /// </summary>
        public int Count => _validators.Count;

        /// <summary>
        /// Clears all validators from the builder
        /// </summary>
        public ValidationChainBuilder<T> Clear()
        {
            _validators.Clear();
            return this;
        }
    }
}
