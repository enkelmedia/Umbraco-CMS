﻿using System;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using Newtonsoft.Json.Serialization;

namespace Obviuse.Utilities.WebApi.Attributes
{
    public class CamelCaseController : Attribute, IControllerConfiguration
    {
        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            var formatter = controllerSettings.Formatters.OfType<JsonMediaTypeFormatter>().Single();
            controllerSettings.Formatters.Remove(formatter);

            formatter = new JsonMediaTypeFormatter
            {
                SerializerSettings = { ContractResolver = new CamelCasePropertyNamesContractResolver() }
            };

            controllerSettings.Formatters.Add(formatter);

        }
    }
}
