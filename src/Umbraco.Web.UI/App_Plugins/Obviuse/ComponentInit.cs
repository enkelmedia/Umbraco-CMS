using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Obviuse.Core.Controllers.Backoffice;
using Umbraco.Core.Components;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.UI.App_Plugins.Obviuse.Controllers.Api;

namespace Umbraco.Web.UI.App_Plugins.Obviuse
{
    public class ComponentInit : UmbracoComponentBase, IUmbracoUserComponent
    {

        public void Initialize(IMarkusService service)
        {
            service.DoStuff();
            var test = "hej";
        }

        public override void Compose(Composition composition)
        {
            var halloj = "ddd";
            composition.Container.Register<IMarkusService, MarkusXmlService>();
            composition.Container.Register<StartpageController>();
            composition.Container.Register<ContactFormSurfaceController>();
            composition.Container.Register<MeetingsController>();
            composition.Container.Register<DictionaryDashboardController>();

            composition.Container.Register<IPublishedValueFallback, Yada>();
        }
    }

    public class Yada : IPublishedValueFallback {
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

    public interface IMarkusService
    {
        void DoStuff();
    }

    public class MarkusXmlService : IMarkusService
    {
        public void DoStuff()
        {
            var foo = "bar";
        }
    }

}
