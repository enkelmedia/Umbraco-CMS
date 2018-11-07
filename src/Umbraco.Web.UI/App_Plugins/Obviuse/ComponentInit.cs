using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Components;
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
