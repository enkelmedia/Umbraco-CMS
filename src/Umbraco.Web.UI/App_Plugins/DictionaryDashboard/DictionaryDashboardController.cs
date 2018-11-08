using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using Newtonsoft.Json;
using Obviuse.Utilities.WebApi.Attributes;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Obviuse.Core.Controllers.Backoffice
{
	[PluginController("Obviuse")]
	[CamelCaseController]
	public class DictionaryDashboardController : UmbracoAuthorizedApiController
	{
	    private readonly ILocalizationService _localizationService;

	    public DictionaryDashboardController(ILocalizationService localizationService)
	    {
	        _localizationService = localizationService;
	    }

		[HttpGet]
		public HttpResponseMessage GetAll()
		{

			var localizationService = _localizationService;
			var translations = new List<IDictionaryItem>();
			
			var allLangs = localizationService.GetAllLanguages();

			// getting all translations
			foreach (var root in localizationService.GetRootDictionaryItems())
			{
				translations.Add(root);

				foreach (var items in localizationService.GetDictionaryItemDescendants(root.Key))
				{
					translations.Add(items);
				}
			}

			var languages = new List<Language>();
			foreach (var lang in allLangs)
			{
				var l = new Language();
				l.Name = lang.CultureInfo.DisplayName;
				l.Code = lang.CultureInfo.Name;
				l.LanguageId = lang.Id;
				
				foreach (var item in translations)
				{
					var dicVal = new DictionaryValue()
					{
						Id = item.Id,
						Key = item.ItemKey
					};
					
					var transItem = item.Translations.FirstOrDefault(x => x.LanguageId == lang.Id);
					dicVal.Value = transItem != null ? transItem.Value : "";
					l.Dictionaries.Add(dicVal);

				}

				languages.Add(l);
			}

			return Request.CreateResponse(HttpStatusCode.OK, languages);
		}

	    [HttpPost]
	    public HttpResponseMessage Save(DictionaryItemUpdateViewModel item)
	    {
            var localizationService = _localizationService;
	        var dicItem = localizationService.GetDictionaryItemById(item.Id);

		    foreach (var trans in item.Translations)
		    {
			    dicItem.Translations.First(x => x.LanguageId == trans.LanguageId).Value = trans.Value;
		    }

            localizationService.Save(dicItem);

            return Request.CreateResponse(HttpStatusCode.OK, item);
        }

    }

	public class Language
	{
		public Language()
		{
			Dictionaries = new List<DictionaryValue>();
		}
		public string Name { get; set; }
		public string Code { get; set; }
		public int LanguageId { get; set; }
		public List<DictionaryValue> Dictionaries { get; set; }
	}

	public class DictionaryValue
	{
		public int Id { get; set; }
		public string Value { get; set; }
		public string Key { get; set; }
	}

	public class DictionaryItemUpdateViewModel
	{
		public int Id { get; set; }
		public string Key { get; set; }
		public List<TranslationUpdateViewModel> Translations { get; set; }
	}

	public class TranslationUpdateViewModel
	{
		public int LanguageId { get; set; }
		public string Value { get; set; }
	}

}
