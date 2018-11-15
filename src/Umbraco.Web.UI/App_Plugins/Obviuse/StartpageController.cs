using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Collections;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.PublishedModels;

namespace Umbraco.Web.UI.App_Plugins.Obviuse
{

    //public class SomethingElse
    //{
    //    public void Yada()
    //    {

    //        var umbContext = Current.UmbracoContext;

    //        var cacheHelper = Current.ApplicationCache;

    //        var logger = Current.Logger;

            


    //        var test1 = umbContext.Application;
    //        var test2 = cacheHelper.RequestCache;
    //        var test3 = logger.ToString();
    //    }
    //}

    public class StartpageController : RenderMvcController
    {
        private readonly IMeetingsService _meetingsService;
        private readonly UmbracoHelper _umbracoHelper;

        public StartpageController(IMeetingsService meetingsService, UmbracoHelper umbracoHelper)
        {
            _meetingsService = meetingsService;
            _umbracoHelper = umbracoHelper;
        }

        public ActionResult Index(ContentModel<Startpage> model)
        {
            var vm = new StartPageViewModel(model.Content);

            vm.Meetings = _meetingsService.GetMeetings();
            vm.SiteName = _umbracoHelper.ContentAtRoot().First().Name;

            return base.Index(vm);
        }
    }

    /*
     *
            var contentFromCache = Umbraco.ContentAtRoot().First();

            var con = Current.Services.ContentService.GetById(1071);
            vm.CustomProperty = con.Name + " " + contentFromCache.Name;

            vm.CustomProperty += ", service: " + _contentService.GetById(1071);
     *
     */

    public class StartPageViewModel : ContentModel<Startpage>
    {
        public StartPageViewModel(Startpage content) : base(content)
        {

        }

        public IEnumerable<Meeting> Meetings { get; set; }

        public string SiteName { get; set; }

    }

    public class ContactFormSurfaceController : SurfaceController
    {
        private readonly IContentService _contentService;
        private readonly UmbracoHelper _umbracoHelper;

        public ContactFormSurfaceController(IContentService contentService, UmbracoHelper umbracoHelper)
        {
            _contentService = contentService;
            _umbracoHelper = umbracoHelper;
        }

        [ChildActionOnly]
        public ActionResult RenderForm()
        {
            var viewModel = new ContactFormModel {};
            return PartialView("ContactForm", viewModel);
        }

        [HttpPost]
        public ActionResult HandleForm(ContactFormModel model)
        {
            var leadsContainer = _umbracoHelper.GetStartpage().FirstChild<Leads>();

            var content = _contentService.Create(model.Name, leadsContainer.Key, Lead.ModelTypeAlias);
            content.SetValue("phone",model.Phone);
            _contentService.SaveAndPublish(content);

            return RedirectToCurrentUmbracoPage();
        }
    }

    public class ContactFormModel
    {
        public string Name { get; set; }
        public string Phone { get; set; }
    }

    public static class UmbracoHelperExtentions
    {
        public static Startpage GetStartpage(this UmbracoHelper helper)
        {
            return new Startpage(helper.ContentAtRoot().First());
        }
    }

}
