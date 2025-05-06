using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ZoumaFinance
{
    public class CamelCaseModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));
            var model = Activator.CreateInstance(bindingContext.ModelType);
            var properties = bindingContext.ModelType.GetProperties();
            foreach (var property in properties)
            {
                // Try both camelCase and PascalCase
                var camelCaseName = char.ToLowerInvariant(property.Name[0]) + property.Name.Substring(1);
                var pascalCaseName = property.Name;
                // Check if the value exists in the form data
                var value = bindingContext.ValueProvider.GetValue(camelCaseName).FirstValue
                            ?? bindingContext.ValueProvider.GetValue(pascalCaseName).FirstValue;
                if (value != null)
                {
                    try
                    {
                        var convertedValue = Convert.ChangeType(value, property.PropertyType);
                        property.SetValue(model, convertedValue);
                    }
                    catch
                    {
                        // Handle conversion errors if needed
                    }
                }
            }
            bindingContext.Result = ModelBindingResult.Success(model);
            return Task.CompletedTask;
        }
    }
}