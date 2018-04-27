using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation.Validators;
using Newtonsoft.Json.Linq;

namespace CosmosDbExplorer.Infrastructure.Validar
{
    public class PartitionKeyValidator : PropertyValidator
    {
        private static readonly JTokenType[] _acceptedTypes = new[]
        {
            JTokenType.Boolean,
            JTokenType.Integer, JTokenType.Float,
            JTokenType.String,
            JTokenType.Undefined, JTokenType.Null
        };

        public PartitionKeyValidator()
            : base("Numeric, string, bool, null, Undefined are the only supported types.{Details}")
        {}

        protected override bool IsValid(PropertyValidatorContext context)
        {
            try
            {
                var pk = context.PropertyValue as string;

                var token = JToken.Parse(pk);

                if (!_acceptedTypes.Contains(token.Type))
                {
                    context.MessageFormatter.AppendArgument("Details", null);
                    return false;
                }
            }
            catch (Exception ex)
            {
                context.MessageFormatter.AppendArgument("Details", "\n" + ex.Message);
                return false;
            }

            return true;
        }
    }
}
