using System;
using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;

namespace CosmosDbExplorer.Infrastructure
{
    public class ValidationTemplate : IDataErrorInfo, INotifyDataErrorInfo
    {
        private INotifyPropertyChanged _target;
        private IValidator _validator;
        private ValidationResult _validationResult;
        private static ConcurrentDictionary<RuntimeTypeHandle, IValidator> _validators = new ConcurrentDictionary<RuntimeTypeHandle, IValidator>();

        public ValidationTemplate(INotifyPropertyChanged target)
        {
            _target = target;
            _validator = GetValidator(target.GetType());
            _validationResult = _validator.Validate(target);
            target.PropertyChanged += Validate;
        }

        static IValidator GetValidator(Type modelType)
        {
            if (!_validators.TryGetValue(modelType.TypeHandle, out var validator))
            {
                var typeName = string.Format("{0}.{1}Validator", modelType.Namespace, modelType.Name);
                var type = modelType.Assembly.GetType(typeName, true);
                _validators[modelType.TypeHandle] = validator = (IValidator)Activator.CreateInstance(type);
            }
            return validator;
        }

        void Validate(object sender, PropertyChangedEventArgs e)
        {
            _validationResult = _validator.Validate(_target);
            foreach (var error in _validationResult.Errors)
            {
                RaiseErrorsChanged(error.PropertyName);
            }
        }

        public IEnumerable GetErrors(string propertyName)
        {
            return _validationResult.Errors
                                   .Where(x => x.PropertyName == propertyName)
                                   .Select(x => x.ErrorMessage);
        }

        public bool HasErrors
        {
            get { return _validationResult.Errors.Count > 0; }
        }

        public string Error
        {
            get
            {
                var strings = _validationResult.Errors.Select(x => x.ErrorMessage).ToArray();
                return string.Join(Environment.NewLine, strings);
            }
        }

        public string this[string columnName]
        {
            get
            {
                var strings = _validationResult.Errors.Where(x => x.PropertyName == columnName)
                                              .Select(x => x.ErrorMessage)
                                              .ToArray();

                return string.Join(Environment.NewLine, strings);
            }
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }
}
