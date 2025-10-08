using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EduTrack.WebApp.ModelBinders;

/// <summary>
/// Model binder provider for DateTimeOffset with Persian date support
/// </summary>
public class PersianDateTimeOffsetModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (context.Metadata.ModelType == typeof(DateTimeOffset) || 
            context.Metadata.ModelType == typeof(DateTimeOffset?))
        {
            return new PersianDateTimeOffsetModelBinder();
        }

        return null;
    }
}
