﻿using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Serialization;
using Umbraco.Core.Services;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Web.PropertyEditors
{
    [DataEditor(
        Constants.PropertyEditors.Aliases.EmailAddress,
        EditorType.PropertyValue | EditorType.MacroParameter,
        "Email address",
        "email",
        Icon = "icon-message")]
    public class EmailAddressPropertyEditor : DataEditor
    {
        private readonly IIOHelper _ioHelper;

        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public EmailAddressPropertyEditor(
            ILoggerFactory loggerFactory,
            IIOHelper ioHelper,
            IDataTypeService dataTypeService,
            ILocalizationService localizationService,
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer)
            : base(loggerFactory, dataTypeService, localizationService, localizedTextService, shortStringHelper, jsonSerializer)
        {
            _ioHelper = ioHelper;
        }

        protected override IDataValueEditor CreateValueEditor()
        {
            var editor = base.CreateValueEditor();
            //add an email address validator
            editor.Validators.Add(new EmailValidator());
            return editor;
        }

        protected override IConfigurationEditor CreateConfigurationEditor()
        {
            return new EmailAddressConfigurationEditor(_ioHelper);
        }
    }
}
