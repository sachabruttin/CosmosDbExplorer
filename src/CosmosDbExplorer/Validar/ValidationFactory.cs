using System;
using System.Collections.Concurrent;
using System.ComponentModel;

using FluentValidation;

namespace CosmosDbExplorer.Validar
{
#pragma warning disable CS8600 // Possible null reference assignment.
#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8604 // Possible null reference argument.

    public static class ValidationFactory
    {
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IValidator> Validators = new();

        public static IValidator<T> GetValidator<T>()
            where T : INotifyPropertyChanged
        {
            var modelType = typeof(T);
            var modelTypeHandle = modelType.TypeHandle;
            if (!Validators.TryGetValue(modelTypeHandle, out var validator))
            {
                var typeName = $"{modelType.Namespace}.{modelType.Name}Validator";
                var type = modelType.Assembly.GetType(typeName, true);
                Validators[modelTypeHandle] = validator = (IValidator)Activator.CreateInstance(type);
            }

            return (IValidator<T>)validator;
        }
    }

#pragma warning restore CS8600 // Possible null reference assignment.
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8604 // Possible null reference argument.

}
