﻿using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Messages;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Messages;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.DataTables;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the message template model factory implementation
    /// </summary>
    public partial class MessageTemplateModelFactory : IMessageTemplateModelFactory
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
        private readonly IStoreService _storeService;

        #endregion

        #region Ctor

        public MessageTemplateModelFactory(CatalogSettings catalogSettings,
            IBaseAdminModelFactory baseAdminModelFactory,
            ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            IMessageTemplateService messageTemplateService,
            IMessageTokenProvider messageTokenProvider,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
            IStoreService storeService)
        {
            _catalogSettings = catalogSettings;
            _baseAdminModelFactory = baseAdminModelFactory;
            _localizationService = localizationService;
            _localizedModelFactory = localizedModelFactory;
            _messageTemplateService = messageTemplateService;
            _messageTokenProvider = messageTokenProvider;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
            _storeService = storeService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare datatables model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>Datatables model</returns>
        protected virtual DataTablesModel PrepareMessageTemplateGridModel(MessageTemplateSearchModel searchModel)
        {
            //prepare common properties
            var model = new DataTablesModel
            {
                Name = "templates-grid",
                UrlRead = new DataUrl("List", "MessageTemplate", null),
                SearchButtonId = "search-templates",
                Length = searchModel.PageSize,
                LengthMenu = searchModel.AvailablePageSizes
            };

            //prepare filters to search
            model.Filters = new List<FilterParameter>
            {
                new FilterParameter(nameof(searchModel.SearchStoreId))
            };

            //prepare model columns
            model.ColumnCollection = new List<ColumnProperty>
            {
                new ColumnProperty(nameof(MessageTemplateModel.Name))
                {
                    Title = _localizationService.GetResource("Admin.ContentManagement.MessageTemplates.Fields.Name")
                },
                new ColumnProperty(nameof(MessageTemplateModel.Subject))
                {
                    Title = _localizationService.GetResource("Admin.ContentManagement.MessageTemplates.Fields.Subject")
                },
                new ColumnProperty(nameof(MessageTemplateModel.IsActive))
                {
                    Title = _localizationService.GetResource("Admin.ContentManagement.MessageTemplates.Fields.IsActive"),
                    Width = "100",
                    ClassName =  StyleColumn.CenterAll,
                    Render = new RenderBoolean()
                },
                new ColumnProperty(nameof(MessageTemplateModel.ListOfStores))
                {
                    Title = _localizationService.GetResource("Admin.ContentManagement.MessageTemplates.Fields.LimitedToStores"),
                    Width = "300"
                },
                new ColumnProperty(nameof(MessageTemplateModel.Id))
                {
                    Title = _localizationService.GetResource("Admin.Common.Edit"),
                    Width = "100",
                    ClassName =  StyleColumn.ButtonStyle,
                    Render = new RenderButtonEdit(new DataUrl("Edit"))
                }
            };

            return model;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare message template search model
        /// </summary>
        /// <param name="searchModel">Message template search model</param>
        /// <returns>Message template search model</returns>
        public virtual MessageTemplateSearchModel PrepareMessageTemplateSearchModel(MessageTemplateSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available stores
            _baseAdminModelFactory.PrepareStores(searchModel.AvailableStores);

            searchModel.HideStoresList = _catalogSettings.IgnoreStoreLimitations || searchModel.AvailableStores.SelectionIsNotPossible();

            //prepare page parameters
            searchModel.SetGridPageSize();
            searchModel.Grid = PrepareMessageTemplateGridModel(searchModel);

            return searchModel;
        }

        /// <summary>
        /// Prepare paged message template list model
        /// </summary>
        /// <param name="searchModel">Message template search model</param>
        /// <returns>Message template list model</returns>
        public virtual MessageTemplateListModel PrepareMessageTemplateListModel(MessageTemplateSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get message templates
            var messageTemplates = _messageTemplateService
                .GetAllMessageTemplates(storeId: searchModel.SearchStoreId).ToPagedList(searchModel);

            //prepare store names (to avoid loading for each message template)
            var stores = _storeService.GetAllStores().Select(store => new { store.Id, store.Name }).ToList();

            //prepare list model
            var model = new MessageTemplateListModel().PrepareToGrid(searchModel, messageTemplates, () =>
            {
                return messageTemplates.Select(messageTemplate =>
                {
                    //fill in model values from the entity
                    var messageTemplateModel = messageTemplate.ToModel<MessageTemplateModel>();

                    //fill in additional values (not existing in the entity)
                    var storeNames = stores.Select(store => store.Name);
                    if (messageTemplate.LimitedToStores)
                    {
                        _storeMappingSupportedModelFactory.PrepareModelStores(messageTemplateModel, messageTemplate, false);
                        storeNames = stores
                            .Where(store => messageTemplateModel.SelectedStoreIds.Contains(store.Id)).Select(store => store.Name);
                    }

                    messageTemplateModel.ListOfStores = string.Join(", ", storeNames);

                    return messageTemplateModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare message template model
        /// </summary>
        /// <param name="model">Message template model</param>
        /// <param name="messageTemplate">Message template</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Message template model</returns>
        public virtual MessageTemplateModel PrepareMessageTemplateModel(MessageTemplateModel model,
            MessageTemplate messageTemplate, bool excludeProperties = false)
        {
            Action<MessageTemplateLocalizedModel, int> localizedModelConfiguration = null;

            if (messageTemplate != null)
            {
                //fill in model values from the entity
                model = model ?? messageTemplate.ToModel<MessageTemplateModel>();

                //define localized model configuration action
                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.BccEmailAddresses = _localizationService.GetLocalized(messageTemplate, entity => entity.BccEmailAddresses, languageId, false, false);
                    locale.Subject = _localizationService.GetLocalized(messageTemplate, entity => entity.Subject, languageId, false, false);
                    locale.Body = _localizationService.GetLocalized(messageTemplate, entity => entity.Body, languageId, false, false);
                    locale.EmailAccountId = _localizationService.GetLocalized(messageTemplate, entity => entity.EmailAccountId, languageId, false, false);

                    //prepare available email accounts
                    _baseAdminModelFactory.PrepareEmailAccounts(locale.AvailableEmailAccounts,
                        defaultItemText: _localizationService.GetResource("Admin.ContentManagement.MessageTemplates.Fields.EmailAccount.Standard"));
                };
            }

            model.SendImmediately = !model.DelayBeforeSend.HasValue;
            model.HasAttachedDownload = model.AttachedDownloadId > 0;

            var allowedTokens = string.Join(", ", _messageTokenProvider.GetListOfAllowedTokens(_messageTokenProvider.GetTokenGroups(messageTemplate)));
            model.AllowedTokens = $"{allowedTokens}{Environment.NewLine}{Environment.NewLine}" +
                $"{_localizationService.GetResource("Admin.ContentManagement.MessageTemplates.Tokens.ConditionalStatement")}{Environment.NewLine}";

            //prepare localized models
            if (!excludeProperties)
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

            //prepare available email accounts
            _baseAdminModelFactory.PrepareEmailAccounts(model.AvailableEmailAccounts);

            //prepare available stores
            _storeMappingSupportedModelFactory.PrepareModelStores(model, messageTemplate, excludeProperties);

            return model;
        }

        /// <summary>
        /// Prepare test message template model
        /// </summary>
        /// <param name="model">Test message template model</param>
        /// <param name="messageTemplate">Message template</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>Test message template model</returns>
        public virtual TestMessageTemplateModel PrepareTestMessageTemplateModel(TestMessageTemplateModel model,
            MessageTemplate messageTemplate, int languageId)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (messageTemplate == null)
                throw new ArgumentNullException(nameof(messageTemplate));

            model.Id = messageTemplate.Id;
            model.LanguageId = languageId;

            //filter tokens to the current template
            var subject = _localizationService.GetLocalized(messageTemplate, entity => entity.Subject, languageId);
            var body = _localizationService.GetLocalized(messageTemplate, entity => entity.Body, languageId);
            model.Tokens = _messageTokenProvider.GetListOfAllowedTokens()
                .Where(token => subject.Contains(token) || body.Contains(token)).ToList();

            return model;
        }

        #endregion
    }
}