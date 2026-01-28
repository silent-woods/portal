using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using App.Web.Framework.Models;

namespace App.Web.Framework.Mvc.ModelBinding.Binders
{
    /// <summary>
    /// Represents a model binder provider for CustomProperties
    /// </summary>
    public class CustomPropertiesModelBinderProvider : IModelBinderProvider
    {
        IModelBinder IModelBinderProvider.GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.PropertyName == nameof(BaseNopModel.CustomProperties) && context.Metadata.ModelType == typeof(Dictionary<string, string>))
                return new CustomPropertiesModelBinder();
            
            return null;
        }
    }
}
