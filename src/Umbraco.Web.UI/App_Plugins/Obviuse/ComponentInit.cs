using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClientDependency.Core;
using Obviuse.Core.Controllers.Backoffice;
using Umbraco.Core.Components;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Web.PublishedModels;
using Umbraco.Web.UI.App_Plugins.Obviuse.Controllers.Api;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.UI.App_Plugins.Obviuse
{

    public class DemoComponent : UmbracoComponentBase, IUmbracoUserComponent
    {
        public override void Compose(Composition composition)
        {
            composition.Container.Register<IMeetingsService, MeetingsXmlService>();
            composition.Container.Register<StartpageController>();
            composition.Container.Register<ContactFormSurfaceController>();
        }
    }

    public class EventHandlerComponent : UmbracoComponentBase, IUmbracoUserComponent
    {
        public void Initialize()
        {
            ContentService.Trashing +=
                delegate (IContentService sender, MoveEventArgs<IContent> args)
            {
                foreach (var entity in args.MoveInfoCollection)
                {
                    if (entity.Entity.ContentType.Alias.Equals(Offices.ModelTypeAlias))
                    {
                        args.Messages.Add(new EventMessage("No-no", "Can't remove a container", EventMessageType.Error));
                        args.Cancel = true;
                    }
                }
            };
        }
    }

    /* From component

        composition.Container.Register<ContactFormSurfaceController>();
            composition.Container.Register<MeetingsController>();
            composition.Container.Register<DictionaryDashboardController>();        

     * public void Initialize(IMeetingsService service)
        {
            
        }
     *
     */


    public interface IMeetingsService
    {
        IEnumerable<Meeting> GetMeetings();
    }



    public class MeetingsXmlService : IMeetingsService
    {
        public IEnumerable<Meeting> GetMeetings()
        {
            return new List<Meeting>()
            {
                new Meeting() { Description = "Yearly meeting", DateTime = new DateTime(2018,12,01,15,00,00) },
                new Meeting() { Description = "Umbraco Festival", DateTime = new DateTime(2018,11,16,08,30,00) }
            };
        }
    }

    public class Meeting
    {
        public string Description { get; set; }
        public DateTime DateTime { get; set; }
    }



}
