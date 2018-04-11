﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;

namespace CosmosDbExplorer.Infrastructure.Validar
{
    public class ValidationTemplate<T> : IDataErrorInfo, INotifyDataErrorInfo
        where T : INotifyPropertyChanged
    {
        private readonly INotifyPropertyChanged _target;
        private readonly IValidator _validator;
        private ValidationResult _validationResult;

        public ValidationTemplate(T target)
        {
            _target = target;
            _validator = ValidationFactory.GetValidator<T>();
            _validationResult = _validator.Validate(target);
            target.PropertyChanged += Validate;
        }

        private void Validate(object sender, PropertyChangedEventArgs e)
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

        public bool HasErrors => _validationResult.Errors.Count > 0;

        public string Error
        {
            get
            {
                var strings = _validationResult.Errors.Select(x => x.ErrorMessage)
                                              .ToArray();
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

        private void RaiseErrorsChanged(string propertyName)
        {
            var handler = ErrorsChanged;
            handler?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }
}
