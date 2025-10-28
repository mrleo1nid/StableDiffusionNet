using System;
using Microsoft.Extensions.Options;
using StableDiffusionNet.Configuration;

namespace StableDiffusionNet.DependencyInjection.Extensions
{
    /// <summary>
    /// Валидатор для StableDiffusionOptions
    /// </summary>
    internal class StableDiffusionOptionsValidator : IValidateOptions<StableDiffusionOptions>
    {
        public ValidateOptionsResult Validate(string? name, StableDiffusionOptions options)
        {
            try
            {
                options.Validate();
                return ValidateOptionsResult.Success;
            }
            catch (Exception ex)
            {
                return ValidateOptionsResult.Fail(ex.Message);
            }
        }
    }
}
