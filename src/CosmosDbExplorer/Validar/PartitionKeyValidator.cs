using System;
using System.Linq;
using FluentValidation;
using FluentValidation.Validators;
using Newtonsoft.Json.Linq;

namespace CosmosDbExplorer.Validar
{
    public class PartitionKeyValidator<T, TProperty> : PropertyValidator<T, TProperty>
    {
        private static readonly JTokenType[] AcceptedTypes = new[]
        {
            JTokenType.Boolean,
            JTokenType.Integer, JTokenType.Float,
            JTokenType.String,
            JTokenType.Undefined, JTokenType.Null
        };

        public override string Name => "PartitionKeyValidator";

        protected override string GetDefaultMessageTemplate(string errorCode)
            => "Numeric, string, bool, null, Undefined are the only supported types.{Details}";

        public override bool IsValid(ValidationContext<T> context, TProperty value)
        {
            try
            {
                var pk = value as string;

#pragma warning disable CS8604 // Possible null reference argument.
                var token = JToken.Parse(pk);
#pragma warning restore CS8604 // Possible null reference argument.

                if (!AcceptedTypes.Contains(token.Type))
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
