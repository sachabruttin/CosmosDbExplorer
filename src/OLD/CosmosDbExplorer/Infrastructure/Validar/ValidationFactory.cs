using System;
using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;

namespace CosmosDbExplorer.Infrastructure.Validar
{

    public static class ValidationFactory
    {
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IValidator> Validators = new ConcurrentDictionary<RuntimeTypeHandle, IValidator>();

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
}
