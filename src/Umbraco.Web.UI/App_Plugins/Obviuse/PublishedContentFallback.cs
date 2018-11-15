using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.UI.App_Plugins.Obviuse
{

    // composition.Container.Register<IPublishedValueFallback, Yada>();
    public class Yada : IPublishedValueFallback
    {
        public bool TryGetValue(IPublishedProperty property, string culture, string segment, Fallback fallback, object defaultValue, out object value)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue<T>(IPublishedProperty property, string culture, string segment, Fallback fallback, T defaultValue,
            out T value)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(IPublishedElement content, string alias, string culture, string segment, Fallback fallback,
            object defaultValue, out object value)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue<T>(IPublishedElement content, string alias, string culture, string segment, Fallback fallback,
            T defaultValue, out T value)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(IPublishedContent content, string alias, string culture, string segment, Fallback fallback,
            object defaultValue, out object value)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue<T>(IPublishedContent content, string alias, string culture, string segment, Fallback fallback,
            T defaultValue, out T value)
        {
            throw new NotImplementedException();
        }
    }
}
