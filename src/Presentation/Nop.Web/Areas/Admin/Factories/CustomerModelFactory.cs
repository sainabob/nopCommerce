﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Services.Affiliates;
using Nop.Services.Authentication.External;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Areas.Admin.Models.ShoppingCart;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.DataTables;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the customer model factory implementation
    /// </summary>
    public partial class CustomerModelFactory : ICustomerModelFactory
    {
        #region Fields

        private readonly AddressSettings _addressSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly GdprSettings _gdprSettings;
        private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IAddressAttributeModelFactory _addressAttributeModelFactory;
        private readonly IAffiliateService _affiliateService;
        private readonly IAuthenticationPluginManager _authenticationPluginManager;
        private readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerAttributeParser _customerAttributeParser;
        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IGdprService _gdprService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IGeoLookupService _geoLookupService;
        private readonly ILocalizationService _localizationService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IOrderService _orderService;
        private readonly IPictureService _pictureService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IRewardPointService _rewardPointService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly ITaxService _taxService;
        private readonly MediaSettings _mediaSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly TaxSettings _taxSettings;

        #endregion

        #region Ctor

        public CustomerModelFactory(AddressSettings addressSettings,
            CustomerSettings customerSettings,
            DateTimeSettings dateTimeSettings,
            GdprSettings gdprSettings,
            IAclSupportedModelFactory aclSupportedModelFactory,
            IAddressAttributeFormatter addressAttributeFormatter,
            IAddressAttributeModelFactory addressAttributeModelFactory,
            IAffiliateService affiliateService,
            IAuthenticationPluginManager authenticationPluginManager,
            IBackInStockSubscriptionService backInStockSubscriptionService,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICustomerActivityService customerActivityService,
            ICustomerAttributeParser customerAttributeParser,
            ICustomerAttributeService customerAttributeService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IGdprService gdprService,
            IGenericAttributeService genericAttributeService,
            IGeoLookupService geoLookupService,
            ILocalizationService localizationService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IOrderService orderService,
            IPictureService pictureService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductAttributeFormatter productAttributeFormatter,
            IRewardPointService rewardPointService,
            IStoreContext storeContext,
            IStoreService storeService,
            ITaxService taxService,
            MediaSettings mediaSettings,
            RewardPointsSettings rewardPointsSettings,
            TaxSettings taxSettings)
        {
            _addressSettings = addressSettings;
            _customerSettings = customerSettings;
            _dateTimeSettings = dateTimeSettings;
            _gdprSettings = gdprSettings;
            _aclSupportedModelFactory = aclSupportedModelFactory;
            _addressAttributeFormatter = addressAttributeFormatter;
            _addressAttributeModelFactory = addressAttributeModelFactory;
            _affiliateService = affiliateService;
            _authenticationPluginManager = authenticationPluginManager;
            _backInStockSubscriptionService = backInStockSubscriptionService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _customerActivityService = customerActivityService;
            _customerAttributeParser = customerAttributeParser;
            _customerAttributeService = customerAttributeService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _gdprService = gdprService;
            _genericAttributeService = genericAttributeService;
            _geoLookupService = geoLookupService;
            _localizationService = localizationService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _orderService = orderService;
            _pictureService = pictureService;
            _priceCalculationService = priceCalculationService;
            _priceFormatter = priceFormatter;
            _productAttributeFormatter = productAttributeFormatter;
            _rewardPointService = rewardPointService;
            _storeContext = storeContext;
            _storeService = storeService;
            _taxService = taxService;
            _mediaSettings = mediaSettings;
            _rewardPointsSettings = rewardPointsSettings;
            _taxSettings = taxSettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare the reward points model to add to the customer
        /// </summary>
        /// <param name="model">Reward points model to add to the customer</param>
        protected virtual void PrepareAddRewardPointsToCustomerModel(AddRewardPointsToCustomerModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.Message = _localizationService.GetResource("Admin.Customers.Customers.SomeComment");
            model.ActivatePointsImmediately = true;
            model.StoreId = _storeContext.CurrentStore.Id;

            //prepare available stores
            _baseAdminModelFactory.PrepareStores(model.AvailableStores, false);
        }

        /// <summary>
        /// Prepare customer associated external authorization models
        /// </summary>
        /// <param name="models">List of customer associated external authorization models</param>
        /// <param name="customer">Customer</param>
        protected virtual void PrepareAssociatedExternalAuthModels(IList<CustomerAssociatedExternalAuthModel> models, Customer customer)
        {
            if (models == null)
                throw new ArgumentNullException(nameof(models));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            foreach (var record in customer.ExternalAuthenticationRecords)
            {
                var method = _authenticationPluginManager.LoadPluginBySystemName(record.ProviderSystemName);
                if (method == null)
                    continue;

                models.Add(new CustomerAssociatedExternalAuthModel
                {
                    Id = record.Id,
                    Email = record.Email,
                    ExternalIdentifier = !string.IsNullOrEmpty(record.ExternalDisplayIdentifier)
                        ? record.ExternalDisplayIdentifier : record.ExternalIdentifier,
                    AuthMethodName = method.PluginDescriptor.FriendlyName
                });
            }
        }

        /// <summary>
        /// Prepare customer attribute models
        /// </summary>
        /// <param name="models">List of customer attribute models</param>
        /// <param name="customer">Customer</param>
        protected virtual void PrepareCustomerAttributeModels(IList<CustomerModel.CustomerAttributeModel> models, Customer customer)
        {
            if (models == null)
                throw new ArgumentNullException(nameof(models));

            //get available customer attributes
            var customerAttributes = _customerAttributeService.GetAllCustomerAttributes();
            foreach (var attribute in customerAttributes)
            {
                var attributeModel = new CustomerModel.CustomerAttributeModel
                {
                    Id = attribute.Id,
                    Name = attribute.Name,
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = _customerAttributeService.GetCustomerAttributeValues(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var attributeValueModel = new CustomerModel.CustomerAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.Name,
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(attributeValueModel);
                    }
                }

                //set already selected attributes
                if (customer != null)
                {
                    var selectedCustomerAttributes = _genericAttributeService
                        .GetAttribute<string>(customer, NopCustomerDefaults.CustomCustomerAttributes);
                    switch (attribute.AttributeControlType)
                    {
                        case AttributeControlType.DropdownList:
                        case AttributeControlType.RadioList:
                        case AttributeControlType.Checkboxes:
                        {
                            if (!string.IsNullOrEmpty(selectedCustomerAttributes))
                            {
                                //clear default selection
                                foreach (var item in attributeModel.Values)
                                    item.IsPreSelected = false;

                                //select new values
                                var selectedValues = _customerAttributeParser.ParseCustomerAttributeValues(selectedCustomerAttributes);
                                foreach (var attributeValue in selectedValues)
                                    foreach (var item in attributeModel.Values)
                                        if (attributeValue.Id == item.Id)
                                            item.IsPreSelected = true;
                            }
                        }
                        break;
                        case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //do nothing
                            //values are already pre-set
                        }
                        break;
                        case AttributeControlType.TextBox:
                        case AttributeControlType.MultilineTextbox:
                        {
                            if (!string.IsNullOrEmpty(selectedCustomerAttributes))
                            {
                                var enteredText = _customerAttributeParser.ParseValues(selectedCustomerAttributes, attribute.Id);
                                if (enteredText.Any())
                                    attributeModel.DefaultValue = enteredText[0];
                            }
                        }
                        break;
                        case AttributeControlType.Datepicker:
                        case AttributeControlType.ColorSquares:
                        case AttributeControlType.ImageSquares:
                        case AttributeControlType.FileUpload:
                        default:
                            //not supported attribute control types
                            break;
                    }
                }

                models.Add(attributeModel);
            }
        }

        /// <summary>
        /// Prepare address model
        /// </summary>
        /// <param name="model">Address model</param>
        /// <param name="address">Address</param>
        protected virtual void PrepareAddressModel(AddressModel model, Address address)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            //set some of address fields as enabled and required
            model.FirstNameEnabled = true;
            model.FirstNameRequired = true;
            model.LastNameEnabled = true;
            model.LastNameRequired = true;
            model.EmailEnabled = true;
            model.EmailRequired = true;
            model.CompanyEnabled = _addressSettings.CompanyEnabled;
            model.CompanyRequired = _addressSettings.CompanyRequired;
            model.CountryEnabled = _addressSettings.CountryEnabled;
            model.CountryRequired = _addressSettings.CountryEnabled; //country is required when enabled
            model.StateProvinceEnabled = _addressSettings.StateProvinceEnabled;
            model.CityEnabled = _addressSettings.CityEnabled;
            model.CityRequired = _addressSettings.CityRequired;
            model.CountyEnabled = _addressSettings.CountyEnabled;
            model.CountyRequired = _addressSettings.CountyRequired;
            model.StreetAddressEnabled = _addressSettings.StreetAddressEnabled;
            model.StreetAddressRequired = _addressSettings.StreetAddressRequired;
            model.StreetAddress2Enabled = _addressSettings.StreetAddress2Enabled;
            model.StreetAddress2Required = _addressSettings.StreetAddress2Required;
            model.ZipPostalCodeEnabled = _addressSettings.ZipPostalCodeEnabled;
            model.ZipPostalCodeRequired = _addressSettings.ZipPostalCodeRequired;
            model.PhoneEnabled = _addressSettings.PhoneEnabled;
            model.PhoneRequired = _addressSettings.PhoneRequired;
            model.FaxEnabled = _addressSettings.FaxEnabled;
            model.FaxRequired = _addressSettings.FaxRequired;

            //prepare available countries
            _baseAdminModelFactory.PrepareCountries(model.AvailableCountries);

            //prepare available states
            _baseAdminModelFactory.PrepareStatesAndProvinces(model.AvailableStates, model.CountryId);

            //prepare custom address attributes
            _addressAttributeModelFactory.PrepareCustomAddressAttributes(model.CustomAddressAttributes, address);
        }

        /// <summary>
        /// Prepare HTML string address
        /// </summary>
        /// <param name="model">Address model</param>
        /// <param name="address">Address</param>
        protected virtual void PrepareModelAddressHtml(AddressModel model, Address address)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var addressHtmlSb = new StringBuilder("<div>");

            if (_addressSettings.CompanyEnabled && !string.IsNullOrEmpty(model.Company))
                addressHtmlSb.AppendFormat("{0}<br />", WebUtility.HtmlEncode(model.Company));

            if (_addressSettings.StreetAddressEnabled && !string.IsNullOrEmpty(model.Address1))
                addressHtmlSb.AppendFormat("{0}<br />", WebUtility.HtmlEncode(model.Address1));

            if (_addressSettings.StreetAddress2Enabled && !string.IsNullOrEmpty(model.Address2))
                addressHtmlSb.AppendFormat("{0}<br />", WebUtility.HtmlEncode(model.Address2));

            if (_addressSettings.CityEnabled && !string.IsNullOrEmpty(model.City))
                addressHtmlSb.AppendFormat("{0},", WebUtility.HtmlEncode(model.City));

            if (_addressSettings.CountyEnabled && !string.IsNullOrEmpty(model.County))
                addressHtmlSb.AppendFormat("{0},", WebUtility.HtmlEncode(model.County));

            if (_addressSettings.StateProvinceEnabled && !string.IsNullOrEmpty(model.StateProvinceName))
                addressHtmlSb.AppendFormat("{0},", WebUtility.HtmlEncode(model.StateProvinceName));

            if (_addressSettings.ZipPostalCodeEnabled && !string.IsNullOrEmpty(model.ZipPostalCode))
                addressHtmlSb.AppendFormat("{0}<br />", WebUtility.HtmlEncode(model.ZipPostalCode));

            if (_addressSettings.CountryEnabled && !string.IsNullOrEmpty(model.CountryName))
                addressHtmlSb.AppendFormat("{0}", WebUtility.HtmlEncode(model.CountryName));

            var customAttributesFormatted = _addressAttributeFormatter.FormatAttributes(address?.CustomAttributes);
            if (!string.IsNullOrEmpty(customAttributesFormatted))
            {
                //already encoded
                addressHtmlSb.AppendFormat("<br />{0}", customAttributesFormatted);
            }

            addressHtmlSb.Append("</div>");

            model.AddressHtml = addressHtmlSb.ToString();
        }

        /// <summary>
        /// Prepare reward points search model
        /// </summary>
        /// <param name="searchModel">Reward points search model</param>
        /// <param name="customer">Customer</param>
        /// <returns>Reward points search model</returns>
        protected virtual CustomerRewardPointsSearchModel PrepareRewardPointsSearchModel(CustomerRewardPointsSearchModel searchModel, Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            searchModel.CustomerId = customer.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();
            searchModel.Grid = PrepareRewardPointGridModel(searchModel);

            return searchModel;
        }

        /// <summary>
        /// Prepare datatables model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>Datatables model</returns>
        protected virtual DataTablesModel PrepareCustomerAddressGridModel(CustomerAddressSearchModel searchModel)
        {
            //prepare common properties
            var model = new DataTablesModel
            {
                Name = "customer-addresses-grid",
                UrlRead = new DataUrl("AddressesSelect", "Customer", null),
                UrlDelete = new DataUrl("AddressDelete", "Customer", new RouteValueDictionary { [nameof(searchModel.CustomerId)] = searchModel.CustomerId }),
                Length = searchModel.PageSize,
                LengthMenu = searchModel.AvailablePageSizes
            };

            //prepare filters to search
            model.Filters = new List<FilterParameter>
            {
                new FilterParameter(nameof(searchModel.CustomerId), searchModel.CustomerId)
            };

            //prepare model columns
            model.ColumnCollection = new List<ColumnProperty>
            {
                new ColumnProperty(nameof(AddressModel.FirstName))
                {
                    Title = _localizationService.GetResource("Admin.Address.Fields.FirstName")
                },
                new ColumnProperty(nameof(AddressModel.LastName))
                {
                    Title = _localizationService.GetResource("Admin.Address.Fields.LastName")
                },
                new ColumnProperty(nameof(AddressModel.Email))
                {
                    Title = _localizationService.GetResource("Admin.Address.Fields.Email")
                },
                new ColumnProperty(nameof(AddressModel.PhoneNumber))
                {
                    Title = _localizationService.GetResource("Admin.Address.Fields.PhoneNumber")
                },
                new ColumnProperty(nameof(AddressModel.FaxNumber))
                {
                    Title = _localizationService.GetResource("Admin.Address.Fields.FaxNumber")
                },
                new ColumnProperty(nameof(AddressModel.AddressHtml))
                {
                    Title = _localizationService.GetResource("Admin.Address"),
                    Encode = false
                },
                new ColumnProperty(nameof(AddressModel.Id))
                {
                    Title = _localizationService.GetResource("Admin.Common.Edit"),
                    Width = "100",
                    ClassName =  StyleColumn.ButtonStyle,
                    Render = new RenderButtonEdit(new DataUrl("~/Admin/Customer/AddressEdit?customerId=" + searchModel.CustomerId + "&addressid=", true))
                },
                new ColumnProperty(nameof(AddressModel.Id))
                {
                    Title = _localizationService.GetResource("Admin.Common.Delete"),
                    Width = "100",
                    Render = new RenderButtonRemove(_localizationService.GetResource("Admin.Common.Delete")) { Style = StyleButton.Default },
                    ClassName =  StyleColumn.ButtonStyle
                }
            };

            return model;
        }

        /// <summary>
        /// Prepare customer address search model
        /// </summary>
        /// <param name="searchModel">Customer address search model</param>
        /// <param name="customer">Customer</param>
        /// <returns>Customer address search model</returns>
        protected virtual CustomerAddressSearchModel PrepareCustomerAddressSearchModel(CustomerAddressSearchModel searchModel, Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            searchModel.CustomerId = customer.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();
            searchModel.Grid = PrepareCustomerAddressGridModel(searchModel);

            return searchModel;
        }

        /// <summary>
        /// Prepare customer order search model
        /// </summary>
        /// <param name="searchModel">Customer order search model</param>
        /// <param name="customer">Customer</param>
        /// <returns>Customer order search model</returns>
        protected virtual CustomerOrderSearchModel PrepareCustomerOrderSearchModel(CustomerOrderSearchModel searchModel, Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            searchModel.CustomerId = customer.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();
            searchModel.Grid = PrepareOrderGridModel(searchModel);

            return searchModel;
        }

        /// <summary>
        /// Prepare customer shopping cart search model
        /// </summary>
        /// <param name="searchModel">Customer shopping cart search model</param>
        /// <param name="customer">Customer</param>
        /// <returns>Customer shopping cart search model</returns>
        protected virtual CustomerShoppingCartSearchModel PrepareCustomerShoppingCartSearchModel(CustomerShoppingCartSearchModel searchModel,
            Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            searchModel.CustomerId = customer.Id;

            //prepare available shopping cart types (search shopping cart by default)
            searchModel.ShoppingCartTypeId = (int)ShoppingCartType.ShoppingCart;
            _baseAdminModelFactory.PrepareShoppingCartTypes(searchModel.AvailableShoppingCartTypes, false);

            //prepare page parameters
            searchModel.SetGridPageSize();
            searchModel.Grid = PrepareCustomerShoppingCartGridModel(searchModel);

            return searchModel;
        }

        /// <summary>
        /// Prepare customer activity log search model
        /// </summary>
        /// <param name="searchModel">Customer activity log search model</param>
        /// <param name="customer">Customer</param>
        /// <returns>Customer activity log search model</returns>
        protected virtual CustomerActivityLogSearchModel PrepareCustomerActivityLogSearchModel(CustomerActivityLogSearchModel searchModel, Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            searchModel.CustomerId = customer.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();
            searchModel.Grid = PrepareCustomerActivityLogGridModel(searchModel);

            return searchModel;
        }

        /// <summary>
        /// Prepare customer back in stock subscriptions search model
        /// </summary>
        /// <param name="searchModel">Customer back in stock subscriptions search model</param>
        /// <param name="customer">Customer</param>
        /// <returns>Customer back in stock subscriptions search model</returns>
        protected virtual CustomerBackInStockSubscriptionSearchModel PrepareCustomerBackInStockSubscriptionSearchModel(
            CustomerBackInStockSubscriptionSearchModel searchModel, Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            searchModel.CustomerId = customer.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();
            searchModel.Grid = PrepareCustomerBackInStockSubscriptionGridModel(searchModel);

            return searchModel;
        }

        /// <summary>
        /// Prepare customer back in stock subscriptions search model
        /// </summary>
        /// <param name="searchModel">Customer back in stock subscriptions search model</param>
        /// <param name="customer">Customer</param>
        /// <returns>Customer back in stock subscriptions search model</returns>
        protected virtual CustomerAssociatedExternalAuthRecordsSearchModel PrepareCustomerAssociatedExternalAuthRecordsSearchModel(
            CustomerAssociatedExternalAuthRecordsSearchModel searchModel, Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            searchModel.CustomerId = customer.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();
            //prepare external authentication records
            PrepareAssociatedExternalAuthModels(searchModel.AssociatedExternalAuthRecords, customer);
            searchModel.Grid = PrepareCustomerAssociatedExternalAuthRecordsGridModel(searchModel);

            return searchModel;
        }

        /// <summary>
        /// Prepare datatables model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>Datatables model</returns>
        protected virtual DataTablesModel PrepareCustomerGridModel(CustomerSearchModel searchModel)
        {
            //prepare common properties
            var model = new DataTablesModel
            {
                Name = "customers-grid",
                UrlRead = new DataUrl("CustomerList", "Customer", null),
                SearchButtonId = "search-customers",
                Length = searchModel.PageSize,
                LengthMenu = searchModel.AvailablePageSizes
            };

            //prepare filters to search
            model.Filters = new List<FilterParameter>
            {
                new FilterParameter(nameof(searchModel.SelectedCustomerRoleIds)),
                new FilterParameter(nameof(searchModel.SearchEmail)),
                new FilterParameter(nameof(searchModel.SearchUsername)),
                new FilterParameter(nameof(searchModel.SearchFirstName)),
                new FilterParameter(nameof(searchModel.SearchLastName)),
                new FilterParameter(nameof(searchModel.SearchDayOfBirth)),
                new FilterParameter(nameof(searchModel.SearchMonthOfBirth)),
                new FilterParameter(nameof(searchModel.SearchCompany)),
                new FilterParameter(nameof(searchModel.SearchPhone)),
                new FilterParameter(nameof(searchModel.SearchZipPostalCode)),
                new FilterParameter(nameof(searchModel.SearchIpAddress)),
            };

            //prepare model columns
            var columnsProperty = new List<ColumnProperty>();
            columnsProperty.Add(new ColumnProperty(nameof(CustomerModel.Id))
            {
                IsMasterCheckBox = true,
                Render = new RenderCheckBox("checkbox_customers"),
                ClassName = StyleColumn.CenterAll,
                Width = "50",
            });
            columnsProperty.Add(new ColumnProperty(nameof(CustomerModel.Email))
            {
                Title = _localizationService.GetResource("Admin.Customers.Customers.Fields.Email"),
                Width = "200"
            });
            if (searchModel.AvatarEnabled)
            {
                columnsProperty.Add(new ColumnProperty(nameof(CustomerModel.AvatarUrl))
                {
                    Title = _localizationService.GetResource("Admin.Customers.Customers.Fields.Avatar"),
                    Width = "100",
                    Render = new RenderPicture()
                });
            }
            if (searchModel.UsernamesEnabled)
            {
                columnsProperty.Add(new ColumnProperty(nameof(CustomerModel.Username))
                {
                    Title = _localizationService.GetResource("Admin.Customers.Customers.Fields.Username"),
                    Width = "200"
                });
            }
            columnsProperty.Add(new ColumnProperty(nameof(CustomerModel.FullName))
            {
                Title = _localizationService.GetResource("Admin.Customers.Customers.Fields.FullName"),
                Width = "200"
            });
            columnsProperty.Add(new ColumnProperty(nameof(CustomerModel.CustomerRoleNames))
            {
                Title = _localizationService.GetResource("Admin.Customers.Customers.Fields.CustomerRoles"),
                Width = "200"
            });
            if (searchModel.CompanyEnabled)
            {
                columnsProperty.Add(new ColumnProperty(nameof(CustomerModel.Company))
                {
                    Title = _localizationService.GetResource("Admin.Customers.Customers.Fields.Company"),
                    Width = "200"
                });
            }
            if (searchModel.PhoneEnabled)
            {
                columnsProperty.Add(new ColumnProperty(nameof(CustomerModel.Phone))
                {
                    Title = _localizationService.GetResource("Admin.Customers.Customers.Fields.Phone"),
                    Width = "200"
                });
            }
            if (searchModel.ZipPostalCodeEnabled)
            {
                columnsProperty.Add(new ColumnProperty(nameof(CustomerModel.ZipPostalCode))
                {
                    Title = _localizationService.GetResource("Admin.Customers.Customers.Fields.ZipPostalCode"),
                    Width = "200"
                });
            }
            columnsProperty.Add(new ColumnProperty(nameof(CustomerModel.Active))
            {
                Title = _localizationService.GetResource("Admin.Customers.Customers.Fields.Active"),
                Width = "100",
                ClassName = StyleColumn.CenterAll,
                Render = new RenderBoolean()
            });

            columnsProperty.Add(new ColumnProperty(nameof(CustomerModel.CreatedOn))
            {
                Title = _localizationService.GetResource("Admin.Customers.Customers.Fields.CreatedOn"),
                Width = "200",
                Render = new RenderDate()
            });
            columnsProperty.Add(new ColumnProperty(nameof(CustomerModel.LastActivityDate))
            {
                Title = _localizationService.GetResource("Admin.Customers.Customers.Fields.LastActivityDate"),
                Width = "200",
                Render = new RenderDate()
            });
            columnsProperty.Add(new ColumnProperty(nameof(CustomerModel.Id))
            {
                Title = _localizationService.GetResource("Admin.Common.Edit"),
                Width = "100",
                ClassName = StyleColumn.ButtonStyle,
                Render = new RenderButtonEdit(new DataUrl("Edit"))
            });
            
            model.ColumnCollection = columnsProperty;

            return model;
        }

        /// <summary>
        /// Prepare datatables model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>Datatables model</returns>
        protected virtual DataTablesModel PrepareOnlineCustomerGridModel(OnlineCustomerSearchModel searchModel)
        {
            //prepare common properties
            var model = new DataTablesModel
            {
                Name = "onlinecustomers-grid",
                UrlRead = new DataUrl("List", "OnlineCustomer", null),
                Length = searchModel.PageSize,
                LengthMenu = searchModel.AvailablePageSizes
            };
            
            //prepare model columns
            model.ColumnCollection = new List<ColumnProperty>
            {
                new ColumnProperty(nameof(OnlineCustomerModel.CustomerInfo))
                {
                    Title = _localizationService.GetResource("Admin.Customers.OnlineCustomers.Fields.CustomerInfo"),
                    Width = "100",
                    Render = new RenderLink(new DataUrl("~/Admin/Customer/Edit", nameof(CustomerModel.Id)))
                },
                new ColumnProperty(nameof(OnlineCustomerModel.LastIpAddress))
                {
                    Title = _localizationService.GetResource("Admin.Customers.OnlineCustomers.Fields.IPAddress"),
                    Width = "100"                   
                },
                new ColumnProperty(nameof(OnlineCustomerModel.Location))
                {
                    Title = _localizationService.GetResource("Admin.Customers.OnlineCustomers.Fields.Location"),
                    Width = "100"
                },
                new ColumnProperty(nameof(OnlineCustomerModel.LastActivityDate))
                {
                    Title = _localizationService.GetResource("Admin.Customers.OnlineCustomers.Fields.LastActivityDate"),
                    Width = "200",
                    Render = new RenderDate()
                },
                new ColumnProperty(nameof(OnlineCustomerModel.LastVisitedPage))
                {
                    Title = _localizationService.GetResource("Admin.Customers.OnlineCustomers.Fields.LastVisitedPage"),
                    Width = "100"
                }
            };

            return model;
        }

        /// <summary>
        /// Prepare datatables model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>Datatables model</returns>
        protected virtual DataTablesModel PrepareGdprLogGridModel(GdprLogSearchModel searchModel)
        {
            //prepare common properties
            var model = new DataTablesModel
            {
                Name = "log-grid",
                UrlRead = new DataUrl("GdprLogList", "Customer", null),
                SearchButtonId = "search-log",
                Length = searchModel.PageSize,
                LengthMenu = searchModel.AvailablePageSizes
            };

            //prepare filters to search
            model.Filters = new List<FilterParameter>
            {
                new FilterParameter(nameof(searchModel.SearchRequestTypeId)),
                new FilterParameter(nameof(searchModel.SearchEmail))                
            };

            //prepare model columns
            model.ColumnCollection = new List<ColumnProperty>
            {                
                new ColumnProperty(nameof(GdprLogModel.CustomerInfo))
                {
                    Title = _localizationService.GetResource("Admin.Customers.GdprLog.Fields.CustomerInfo")
                },
                new ColumnProperty(nameof(GdprLogModel.RequestType))
                {
                    Title = _localizationService.GetResource("Admin.Customers.GdprLog.Fields.RequestType")
                },
                new ColumnProperty(nameof(GdprLogModel.RequestDetails))
                {
                    Title = _localizationService.GetResource("Admin.Customers.GdprLog.Fields.RequestDetails")
                },
                new ColumnProperty(nameof(GdprLogModel.CreatedOn))
                {
                    Title = _localizationService.GetResource("Admin.Customers.GdprLog.Fields.CreatedOn"),
                    Render = new RenderDate()
                }                
            };
            
            return model;
        }

        /// <summary>
        /// Prepare datatables model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>Datatables model</returns>
        protected virtual DataTablesModel PrepareCustomerShoppingCartGridModel(CustomerShoppingCartSearchModel searchModel)
        {
            var stores = _storeService.GetAllStores();

            //prepare common properties
            var model = new DataTablesModel
            {
                Name = "currentshoppingcart-grid",
                UrlRead = new DataUrl("GetCartList", "Customer", null),
                Length = searchModel.PageSize,
                LengthMenu = searchModel.AvailablePageSizes,

                //prepare filters to search
                Filters = new List<FilterParameter>
                {
                    new FilterParameter(nameof(searchModel.CustomerId), searchModel.CustomerId),
                    new FilterParameter(nameof(CustomerShoppingCartSearchModel.ShoppingCartTypeId), nameof(CustomerShoppingCartSearchModel))
                },

                //prepare model columns
                ColumnCollection = new List<ColumnProperty>
                {
                    new ColumnProperty(nameof(ShoppingCartItemModel.ProductName))
                    {
                        Title = _localizationService.GetResource("Admin.CurrentCarts.Product"),
                        Width = "500",
                        Render = new RenderCustom("renderProductName")
                    },
                    new ColumnProperty(nameof(ShoppingCartItemModel.Quantity))
                    {
                        Title = _localizationService.GetResource("Admin.CurrentCarts.Quantity"),
                        Width = "200"
                    },
                    new ColumnProperty(nameof(ShoppingCartItemModel.UnitPrice))
                    {
                        Title = _localizationService.GetResource("Admin.CurrentCarts.UnitPrice"),
                        Width = "200"
                    },
                    new ColumnProperty(nameof(ShoppingCartItemModel.Total))
                    {
                        Title = _localizationService.GetResource("Admin.CurrentCarts.Total"),
                        Width = "200"
                    },
                    new ColumnProperty(nameof(ShoppingCartItemModel.Store))
                    {
                        Title = _localizationService.GetResource("Admin.CurrentCarts.Store"),
                        Width = "200",
                        Visible = stores.Count > 1
                    },
                    new ColumnProperty(nameof(ShoppingCartItemModel.UpdatedOn))
                    {
                        Title = _localizationService.GetResource("Admin.CurrentCarts.UpdatedOn"),
                        Width = "200",
                        Render = new RenderDate()
                    }
                }
            };

            return model;
        }

        /// <summary>
        /// Prepare datatables model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>Datatables model</returns>
        protected virtual DataTablesModel PrepareCustomerAssociatedExternalAuthRecordsGridModel(CustomerAssociatedExternalAuthRecordsSearchModel searchModel)
        {
            //prepare common properties
            var model = new DataTablesModel
            {
                Name = "externalauthrecords-grid",
                Paging = false,
                ServerSide = false,
                Length = searchModel.PageSize,
                LengthMenu = searchModel.AvailablePageSizes,

                //prepare model columns
                ColumnCollection = new List<ColumnProperty>
                {
                    new ColumnProperty(nameof(CustomerAssociatedExternalAuthModel.AuthMethodName))
                    {
                        Title = _localizationService.GetResource("Admin.Customers.Customers.AssociatedExternalAuth.Fields.AuthMethodName"),
                        Width = "100"
                    },
                    new ColumnProperty(nameof(CustomerAssociatedExternalAuthModel.Email))
                    {
                        Title = _localizationService.GetResource("Admin.Customers.Customers.AssociatedExternalAuth.Fields.Email"),
                        Width = "100"
                    },
                    new ColumnProperty(nameof(CustomerAssociatedExternalAuthModel.ExternalIdentifier))
                    {
                        Title = _localizationService.GetResource("Admin.Customers.Customers.AssociatedExternalAuth.Fields.ExternalIdentifier"),
                        Width = "300"
                    }
                },
                //prepare grid data
                Data = JsonConvert.SerializeObject(searchModel.AssociatedExternalAuthRecords.Select(externalAuthRecord => new
                {
                    AuthMethodName = JavaScriptEncoder.Default.Encode(externalAuthRecord.AuthMethodName),
                    Email = JavaScriptEncoder.Default.Encode(externalAuthRecord.Email),
                    ExternalIdentifier = JavaScriptEncoder.Default.Encode(externalAuthRecord.ExternalIdentifier)
                }).ToList())
            };

            return model;
        }

        /// <summary>
        /// Prepare datatables model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>Datatables model</returns>
        protected virtual DataTablesModel PrepareCustomerBackInStockSubscriptionGridModel(CustomerBackInStockSubscriptionSearchModel searchModel)
        {
            var stores = _storeService.GetAllStores();

            //prepare common properties
            var model = new DataTablesModel
            {
                Name = "backinstock-subscriptions-grid",
                UrlRead = new DataUrl("BackInStockSubscriptionList", "Customer", null),
                Length = searchModel.PageSize,
                LengthMenu = searchModel.AvailablePageSizes,

                //prepare filters to search
                Filters = new List<FilterParameter>
                {
                    new FilterParameter(nameof(searchModel.CustomerId), searchModel.CustomerId)
                },

                //prepare model columns
                ColumnCollection = new List<ColumnProperty>
                {
                    new ColumnProperty(nameof(CustomerBackInStockSubscriptionModel.StoreName))
                    {
                        Title = _localizationService.GetResource("Admin.Customers.Customers.BackInStockSubscriptions.Store"),
                        Width = "200",
                        Visible = stores.Count > 1
                    },
                    new ColumnProperty(nameof(CustomerBackInStockSubscriptionModel.ProductName))
                    {
                        Title = _localizationService.GetResource("Admin.Customers.Customers.BackInStockSubscriptions.Product"),
                        Width = "300",
                        Render = new RenderLink(new DataUrl("~/Admin/Product/Edit/", nameof(CustomerBackInStockSubscriptionModel.ProductId)))
                    },
                    new ColumnProperty(nameof(CustomerBackInStockSubscriptionModel.CreatedOn))
                    {
                        Title = _localizationService.GetResource("Admin.Customers.Customers.BackInStockSubscriptions.CreatedOn"),
                        Width = "200",
                        Render = new RenderDate()
                    }
                }
            };

            return model;
        }

        /// <summary>
        /// Prepare datatables model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>Datatables model</returns>
        protected virtual DataTablesModel PrepareCustomerActivityLogGridModel(
            CustomerActivityLogSearchModel searchModel)
        {
            //prepare common properties
            var model = new DataTablesModel
            {
                Name = "activitylog-grid",
                UrlRead = new DataUrl("ListActivityLog", "Customer", null),
                Length = searchModel.PageSize,
                LengthMenu = searchModel.AvailablePageSizes,

                //prepare filters to search
                Filters = new List<FilterParameter>
                {
                    new FilterParameter(nameof(searchModel.CustomerId), searchModel.CustomerId)
                },

                //prepare model columns
                ColumnCollection = new List<ColumnProperty>
                {
                    new ColumnProperty(nameof(CustomerActivityLogModel.ActivityLogTypeName))
                    {
                        Title = _localizationService.GetResource(
                            "Admin.Customers.Customers.ActivityLog.ActivityLogType"),
                        Width = "300"
                    },
                    new ColumnProperty(nameof(CustomerActivityLogModel.IpAddress))
                    {
                        Title = _localizationService.GetResource("Admin.Customers.Customers.ActivityLog.IpAddress"),
                        Width = "100"
                    },
                    new ColumnProperty(nameof(CustomerActivityLogModel.Comment))
                    {
                        Title = _localizationService.GetResource("Admin.Customers.Customers.ActivityLog.Comment")
                    },
                    new ColumnProperty(nameof(CustomerActivityLogModel.CreatedOn))
                    {
                        Title = _localizationService.GetResource("Admin.Customers.Customers.ActivityLog.CreatedOn"),
                        Width = "200",
                        Render = new RenderDate()
                    }
                }
            };

            return model;
        }

        /// <summary>
        /// Prepare datatables model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>Datatables model</returns>
        protected virtual DataTablesModel PrepareOrderGridModel(CustomerOrderSearchModel searchModel)
        {
            //prepare common properties
            var model = new DataTablesModel
            {
                Name = "order-grid",
                UrlRead = new DataUrl("OrderList", "Customer", new RouteValueDictionary { [nameof(searchModel.CustomerId)] = searchModel.CustomerId }),
                Length = searchModel.PageSize,
                LengthMenu = searchModel.AvailablePageSizes,
            };

            //prepare model columns
            model.ColumnCollection = new List<ColumnProperty>
            {
                new ColumnProperty(nameof(CustomerOrderModel.CustomOrderNumber))
                {
                    Title = _localizationService.GetResource("Admin.Customers.Customers.Orders.CustomOrderNumber"),
                    Width = "200"
                },
                new ColumnProperty(nameof(CustomerOrderModel.OrderTotal))
                {
                    Title = _localizationService.GetResource("Admin.Customers.Customers.Orders.OrderTotal"),
                    Width = "200"
                },
                new ColumnProperty(nameof(CustomerOrderModel.OrderStatus))
                {
                    Title = _localizationService.GetResource("Admin.Customers.Customers.Orders.OrderStatus"),
                    Width = "200",
                    Render = new RenderCustom("renderColumnOrderStatus")
                },
                new ColumnProperty(nameof(CustomerOrderModel.PaymentStatus))
                {
                    Title = _localizationService.GetResource("Admin.Orders.Fields.PaymentStatus"),
                    Width = "200"
                },
                new ColumnProperty(nameof(CustomerOrderModel.ShippingStatus))
                {
                    Title = _localizationService.GetResource("Admin.Orders.Fields.ShippingStatus"),
                    Width = "200"
                },
                new ColumnProperty(nameof(CustomerOrderModel.StoreName))
                {
                    Title = _localizationService.GetResource("Admin.Orders.Fields.Store"),
                    Width = "200",
                    Visible = _storeService.GetAllStores().Count > 1
                },
                new ColumnProperty(nameof(CustomerOrderModel.CreatedOn))
                {
                    Title = _localizationService.GetResource("Admin.System.Log.Fields.CreatedOn"),
                    Width = "200",
                    Render = new RenderDate()
                },
                new ColumnProperty(nameof(CustomerOrderModel.Id))
                {
                    Title = _localizationService.GetResource("Admin.Common.View"),
                    Width = "100",
                    ClassName = StyleColumn.ButtonStyle,
                    Render = new RenderButtonEdit(new DataUrl("~/Admin/Order/Edit/"))
                }
            };
            return model;
        }

        /// <summary>
        /// Prepare datatables model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>Datatables model</returns>
        protected virtual DataTablesModel PrepareRewardPointGridModel(CustomerRewardPointsSearchModel searchModel)
        {
            //prepare common properties
            var model = new DataTablesModel
            {
                Name = "customer-rewardpoints-grid",
                UrlRead = new DataUrl("RewardPointsHistorySelect", "Customer", new RouteValueDictionary { [nameof(searchModel.CustomerId)] = searchModel.CustomerId }),
                Length = searchModel.PageSize,
                LengthMenu = searchModel.AvailablePageSizes
            };

            //prepare model columns
            model.ColumnCollection = new List<ColumnProperty>
            {
                new ColumnProperty(nameof(CustomerRewardPointsModel.StoreName))
                {
                    Title = _localizationService.GetResource("Admin.Customers.Customers.RewardPoints.Fields.Store"),
                    Visible = _storeService.GetAllStores().Count > 1
                },
                new ColumnProperty(nameof(CustomerRewardPointsModel.Points))
                {
                    Title = _localizationService.GetResource("Admin.Customers.Customers.RewardPoints.Fields.Points")
                },
                new ColumnProperty(nameof(CustomerRewardPointsModel.PointsBalance))
                {
                    Title = _localizationService.GetResource("Admin.Customers.Customers.RewardPoints.Fields.PointsBalance")
                },
                new ColumnProperty(nameof(CustomerRewardPointsModel.Message))
                {
                    Title = _localizationService.GetResource("Admin.Customers.Customers.RewardPoints.Fields.Message")
                },
                new ColumnProperty(nameof(CustomerRewardPointsModel.CreatedOn))
                {
                    Title = _localizationService.GetResource("Admin.Customers.Customers.RewardPoints.Fields.CreatedDate"),
                    Render = new RenderDate()
                },
                new ColumnProperty(nameof(CustomerRewardPointsModel.EndDate))
                {
                    Title = _localizationService.GetResource("Admin.Customers.Customers.RewardPoints.Fields.EndDate"),
                    Render = new RenderDate()
                }
            };

            return model;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare customer search model
        /// </summary>
        /// <param name="searchModel">Customer search model</param>
        /// <returns>Customer search model</returns>
        public virtual CustomerSearchModel PrepareCustomerSearchModel(CustomerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            searchModel.AvatarEnabled = _customerSettings.AllowCustomersToUploadAvatars;
            searchModel.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
            searchModel.CompanyEnabled = _customerSettings.CompanyEnabled;
            searchModel.PhoneEnabled = _customerSettings.PhoneEnabled;
            searchModel.ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled;

            //search registered customers by default
            var registeredRole = _customerService.GetCustomerRoleBySystemName(NopCustomerDefaults.RegisteredRoleName);
            if (registeredRole != null)
                searchModel.SelectedCustomerRoleIds.Add(registeredRole.Id);

            //prepare available customer roles
            _aclSupportedModelFactory.PrepareModelCustomerRoles(searchModel);

            //prepare page parameters
            searchModel.SetGridPageSize();
            searchModel.Grid = PrepareCustomerGridModel(searchModel);

            return searchModel;
        }

        /// <summary>
        /// Prepare paged customer list model
        /// </summary>
        /// <param name="searchModel">Customer search model</param>
        /// <returns>Customer list model</returns>
        public virtual CustomerListModel PrepareCustomerListModel(CustomerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter customers
            int.TryParse(searchModel.SearchDayOfBirth, out var dayOfBirth);
            int.TryParse(searchModel.SearchMonthOfBirth, out var monthOfBirth);

            //get customers
            var customers = _customerService.GetAllCustomers(customerRoleIds: searchModel.SelectedCustomerRoleIds.ToArray(),
                email: searchModel.SearchEmail,
                username: searchModel.SearchUsername,
                firstName: searchModel.SearchFirstName,
                lastName: searchModel.SearchLastName,
                dayOfBirth: dayOfBirth,
                monthOfBirth: monthOfBirth,
                company: searchModel.SearchCompany,
                phone: searchModel.SearchPhone,
                zipPostalCode: searchModel.SearchZipPostalCode,
                ipAddress: searchModel.SearchIpAddress,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = new CustomerListModel().PrepareToGrid(searchModel, customers, () =>
            {
                return customers.Select(customer =>
                {
                    //fill in model values from the entity
                    var customerModel = customer.ToModel<CustomerModel>();

                    //convert dates to the user time
                    customerModel.Email = customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest");
                    customerModel.FullName = _customerService.GetCustomerFullName(customer);
                    customerModel.Company = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.CompanyAttribute);
                    customerModel.Phone = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.PhoneAttribute);
                    customerModel.ZipPostalCode = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.ZipPostalCodeAttribute);

                    customerModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(customer.CreatedOnUtc, DateTimeKind.Utc);
                    customerModel.LastActivityDate = _dateTimeHelper.ConvertToUserTime(customer.LastActivityDateUtc, DateTimeKind.Utc);

                    //fill in additional values (not existing in the entity)
                    customerModel.CustomerRoleNames = string.Join(", ", customer.CustomerRoles.Select(role => role.Name));
                    if (_customerSettings.AllowCustomersToUploadAvatars)
                    {
                        var avatarPictureId = _genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.AvatarPictureIdAttribute);
                        customerModel.AvatarUrl = _pictureService.GetPictureUrl(avatarPictureId, _mediaSettings.AvatarPictureSize,
                            _customerSettings.DefaultAvatarEnabled, defaultPictureType: PictureType.Avatar);
                    }

                    return customerModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare customer model
        /// </summary>
        /// <param name="model">Customer model</param>
        /// <param name="customer">Customer</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Customer model</returns>
        public virtual CustomerModel PrepareCustomerModel(CustomerModel model, Customer customer, bool excludeProperties = false)
        {
            if (customer != null)
            {
                //fill in model values from the entity
                model = model ?? new CustomerModel();

                model.Id = customer.Id;
                model.DisplayVatNumber = _taxSettings.EuVatEnabled;
                model.AllowSendingOfWelcomeMessage = customer.IsRegistered() &&
                    _customerSettings.UserRegistrationType == UserRegistrationType.AdminApproval;
                model.AllowReSendingOfActivationMessage = customer.IsRegistered() && !customer.Active &&
                    _customerSettings.UserRegistrationType == UserRegistrationType.EmailValidation;
                model.GdprEnabled = _gdprSettings.GdprEnabled;

                //whether to fill in some of properties
                if (!excludeProperties)
                {
                    model.Email = customer.Email;
                    model.Username = customer.Username;
                    model.VendorId = customer.VendorId;
                    model.AdminComment = customer.AdminComment;
                    model.IsTaxExempt = customer.IsTaxExempt;
                    model.Active = customer.Active;
                    model.FirstName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.FirstNameAttribute);
                    model.LastName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.LastNameAttribute);
                    model.Gender = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.GenderAttribute);
                    model.DateOfBirth = _genericAttributeService.GetAttribute<DateTime?>(customer, NopCustomerDefaults.DateOfBirthAttribute);
                    model.Company = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.CompanyAttribute);
                    model.StreetAddress = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.StreetAddressAttribute);
                    model.StreetAddress2 = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.StreetAddress2Attribute);
                    model.ZipPostalCode = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.ZipPostalCodeAttribute);
                    model.City = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.CityAttribute);
                    model.County = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.CountyAttribute);
                    model.CountryId = _genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.CountryIdAttribute);
                    model.StateProvinceId = _genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.StateProvinceIdAttribute);
                    model.Phone = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.PhoneAttribute);
                    model.Fax = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.FaxAttribute);
                    model.TimeZoneId = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.TimeZoneIdAttribute);
                    model.VatNumber = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.VatNumberAttribute);
                    model.VatNumberStatusNote = _localizationService.GetLocalizedEnum((VatNumberStatus)_genericAttributeService
                        .GetAttribute<int>(customer, NopCustomerDefaults.VatNumberStatusIdAttribute));
                    model.CreatedOn = _dateTimeHelper.ConvertToUserTime(customer.CreatedOnUtc, DateTimeKind.Utc);
                    model.LastActivityDate = _dateTimeHelper.ConvertToUserTime(customer.LastActivityDateUtc, DateTimeKind.Utc);
                    model.LastIpAddress = customer.LastIpAddress;
                    model.LastVisitedPage = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.LastVisitedPageAttribute);
                    model.SelectedCustomerRoleIds = customer.CustomerCustomerRoleMappings.Select(mapping => mapping.CustomerRoleId).ToList();
                    model.RegisteredInStore = _storeService.GetAllStores()
                        .FirstOrDefault(store => store.Id == customer.RegisteredInStoreId)?.Name ?? string.Empty;

                    //prepare model affiliate
                    var affiliate = _affiliateService.GetAffiliateById(customer.AffiliateId);
                    if (affiliate != null)
                    {
                        model.AffiliateId = affiliate.Id;
                        model.AffiliateName = _affiliateService.GetAffiliateFullName(affiliate);
                    }

                    //prepare model newsletter subscriptions
                    if (!string.IsNullOrEmpty(customer.Email))
                    {
                        model.SelectedNewsletterSubscriptionStoreIds = _storeService.GetAllStores()
                            .Where(store => _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, store.Id) != null)
                            .Select(store => store.Id).ToList();
                    }
                }
                //prepare reward points model
                model.DisplayRewardPointsHistory = _rewardPointsSettings.Enabled;
                if (model.DisplayRewardPointsHistory)
                    PrepareAddRewardPointsToCustomerModel(model.AddRewardPoints);

                //prepare nested search models
                PrepareRewardPointsSearchModel(model.CustomerRewardPointsSearchModel, customer);
                PrepareCustomerAddressSearchModel(model.CustomerAddressSearchModel, customer);
                PrepareCustomerOrderSearchModel(model.CustomerOrderSearchModel, customer);
                PrepareCustomerShoppingCartSearchModel(model.CustomerShoppingCartSearchModel, customer);
                PrepareCustomerActivityLogSearchModel(model.CustomerActivityLogSearchModel, customer);
                PrepareCustomerBackInStockSubscriptionSearchModel(model.CustomerBackInStockSubscriptionSearchModel, customer);
                PrepareCustomerAssociatedExternalAuthRecordsSearchModel(model.CustomerAssociatedExternalAuthRecordsSearchModel, customer);
            }
            else
            {
                //whether to fill in some of properties
                if (!excludeProperties)
                {
                    //precheck Registered Role as a default role while creating a new customer through admin
                    var registeredRole = _customerService.GetCustomerRoleBySystemName(NopCustomerDefaults.RegisteredRoleName);
                    if (registeredRole != null)
                        model.SelectedCustomerRoleIds.Add(registeredRole.Id);
                }
            }

            model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;
            model.GenderEnabled = _customerSettings.GenderEnabled;
            model.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
            model.CompanyEnabled = _customerSettings.CompanyEnabled;
            model.StreetAddressEnabled = _customerSettings.StreetAddressEnabled;
            model.StreetAddress2Enabled = _customerSettings.StreetAddress2Enabled;
            model.ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled;
            model.CityEnabled = _customerSettings.CityEnabled;
            model.CountyEnabled = _customerSettings.CountyEnabled;
            model.CountryEnabled = _customerSettings.CountryEnabled;
            model.StateProvinceEnabled = _customerSettings.StateProvinceEnabled;
            model.PhoneEnabled = _customerSettings.PhoneEnabled;
            model.FaxEnabled = _customerSettings.FaxEnabled;

            //set default values for the new model
            if (customer == null)
            {
                model.Active = true;
                model.DisplayVatNumber = false;
            }

            //prepare available vendors
            _baseAdminModelFactory.PrepareVendors(model.AvailableVendors,
                defaultItemText: _localizationService.GetResource("Admin.Customers.Customers.Fields.Vendor.None"));

            //prepare model customer attributes
            PrepareCustomerAttributeModels(model.CustomerAttributes, customer);

            //prepare model stores for newsletter subscriptions
            model.AvailableNewsletterSubscriptionStores = _storeService.GetAllStores().Select(store => new SelectListItem
            {
                Value = store.Id.ToString(),
                Text = store.Name,
                Selected = model.SelectedNewsletterSubscriptionStoreIds.Contains(store.Id)
            }).ToList();

            //prepare model customer roles
            _aclSupportedModelFactory.PrepareModelCustomerRoles(model);

            //prepare available time zones
            _baseAdminModelFactory.PrepareTimeZones(model.AvailableTimeZones, false);

            //prepare available countries and states
            if (_customerSettings.CountryEnabled)
            {
                _baseAdminModelFactory.PrepareCountries(model.AvailableCountries);
                if (_customerSettings.StateProvinceEnabled)
                    _baseAdminModelFactory.PrepareStatesAndProvinces(model.AvailableStates, model.CountryId == 0 ? null : (int?)model.CountryId);
            }

            return model;
        }

        /// <summary>
        /// Prepare paged reward points list model
        /// </summary>
        /// <param name="searchModel">Reward points search model</param>
        /// <param name="customer">Customer</param>
        /// <returns>Reward points list model</returns>
        public virtual CustomerRewardPointsListModel PrepareRewardPointsListModel(CustomerRewardPointsSearchModel searchModel, Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            //get reward points history
            var rewardPoints = _rewardPointService.GetRewardPointsHistory(customer.Id,
                showNotActivated: true,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = new CustomerRewardPointsListModel().PrepareToGrid(searchModel, rewardPoints, () =>
            {
                return rewardPoints.Select(historyEntry =>
                {
                    //fill in model values from the entity        
                    var rewardPointsHistoryModel = historyEntry.ToModel<CustomerRewardPointsModel>();

                    //convert dates to the user time
                    var activatingDate = _dateTimeHelper.ConvertToUserTime(historyEntry.CreatedOnUtc, DateTimeKind.Utc);
                    rewardPointsHistoryModel.CreatedOn = activatingDate;
                    rewardPointsHistoryModel.PointsBalance = historyEntry.PointsBalance.HasValue ? historyEntry.PointsBalance.ToString() :
                        string.Format(_localizationService.GetResource("Admin.Customers.Customers.RewardPoints.ActivatedLater"), activatingDate);
                    rewardPointsHistoryModel.EndDate = !historyEntry.EndDateUtc.HasValue ? null :
                        (DateTime?)_dateTimeHelper.ConvertToUserTime(historyEntry.EndDateUtc.Value, DateTimeKind.Utc);

                    //fill in additional values (not existing in the entity)
                    rewardPointsHistoryModel.StoreName = _storeService.GetStoreById(historyEntry.StoreId)?.Name ?? "Unknown";

                    return rewardPointsHistoryModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare paged customer address list model
        /// </summary>
        /// <param name="searchModel">Customer address search model</param>
        /// <param name="customer">Customer</param>
        /// <returns>Customer address list model</returns>
        public virtual CustomerAddressListModel PrepareCustomerAddressListModel(CustomerAddressSearchModel searchModel, Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            //get customer addresses
            var addresses = customer.Addresses
                .OrderByDescending(address => address.CreatedOnUtc).ThenByDescending(address => address.Id).ToList()
                .ToPagedList(searchModel);

            //prepare list model
            var model = new CustomerAddressListModel().PrepareToGrid(searchModel, addresses, () =>
            {
                return addresses.Select(address =>
                {
                    //fill in model values from the entity        
                    var addressModel = address.ToModel<AddressModel>();
                    addressModel.CountryName = address.Country?.Name;
                    addressModel.StateProvinceName = address.StateProvince?.Name;

                    //fill in additional values (not existing in the entity)
                    PrepareModelAddressHtml(addressModel, address);

                    return addressModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare customer address model
        /// </summary>
        /// <param name="model">Customer address model</param>
        /// <param name="customer">Customer</param>
        /// <param name="address">Address</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Customer address model</returns>
        public virtual CustomerAddressModel PrepareCustomerAddressModel(CustomerAddressModel model,
            Customer customer, Address address, bool excludeProperties = false)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (address != null)
            {
                //fill in model values from the entity
                model = model ?? new CustomerAddressModel();

                //whether to fill in some of properties
                if (!excludeProperties)
                    model.Address = address.ToModel(model.Address);
            }

            model.CustomerId = customer.Id;

            //prepare address model
            PrepareAddressModel(model.Address, address);

            return model;
        }

        /// <summary>
        /// Prepare paged customer order list model
        /// </summary>
        /// <param name="searchModel">Customer order search model</param>
        /// <param name="customer">Customer</param>
        /// <returns>Customer order list model</returns>
        public virtual CustomerOrderListModel PrepareCustomerOrderListModel(CustomerOrderSearchModel searchModel, Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            //get customer orders
            var orders = _orderService.SearchOrders(customerId: customer.Id,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = new CustomerOrderListModel().PrepareToGrid(searchModel, orders, () =>
            {
                return orders.Select(order =>
                {
                    //fill in model values from the entity
                    var orderModel = order.ToModel<CustomerOrderModel>();

                    //convert dates to the user time
                    orderModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc);

                    //fill in additional values (not existing in the entity)
                    orderModel.StoreName = _storeService.GetStoreById(order.StoreId)?.Name ?? "Unknown";
                    orderModel.OrderStatus = _localizationService.GetLocalizedEnum(order.OrderStatus);
                    orderModel.PaymentStatus = _localizationService.GetLocalizedEnum(order.PaymentStatus);
                    orderModel.ShippingStatus = _localizationService.GetLocalizedEnum(order.ShippingStatus);
                    orderModel.OrderTotal = _priceFormatter.FormatPrice(order.OrderTotal, true, false);

                    return orderModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare paged customer shopping cart list model
        /// </summary>
        /// <param name="searchModel">Customer shopping cart search model</param>
        /// <param name="customer">Customer</param>
        /// <returns>Customer shopping cart list model</returns>
        public virtual CustomerShoppingCartListModel PrepareCustomerShoppingCartListModel(CustomerShoppingCartSearchModel searchModel,
            Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            //get customer shopping cart
            var shoppingCart = customer.ShoppingCartItems
                .Where(item => item.ShoppingCartTypeId == searchModel.ShoppingCartTypeId).ToList()
                .ToPagedList(searchModel);

            //prepare list model

            var pageList = shoppingCart.Select(item =>
            {
                //fill in model values from the entity
                var shoppingCartItemModel = item.ToModel<ShoppingCartItemModel>();

                //fill in additional values (not existing in the entity)
                shoppingCartItemModel.ProductName = item.Product.Name;
                shoppingCartItemModel.Store = _storeService.GetStoreById(item.StoreId)?.Name ?? "Unknown";
                shoppingCartItemModel.AttributeInfo =
                    _productAttributeFormatter.FormatAttributes(item.Product, item.AttributesXml);
                shoppingCartItemModel.UnitPrice = _priceFormatter.FormatPrice(
                    _taxService.GetProductPrice(item.Product, _priceCalculationService.GetUnitPrice(item), out var _));
                shoppingCartItemModel.Total = _priceFormatter.FormatPrice(
                    _taxService.GetProductPrice(item.Product, _priceCalculationService.GetSubTotal(item), out _));
                //convert dates to the user time
                shoppingCartItemModel.UpdatedOn =
                    _dateTimeHelper.ConvertToUserTime(item.UpdatedOnUtc, DateTimeKind.Utc);

                return shoppingCartItemModel;
            }).ToList().ToPagedList(searchModel);

            var model = new CustomerShoppingCartListModel().PrepareToGrid(searchModel, pageList, () => pageList);

            return model;
        }

        /// <summary>
        /// Prepare paged customer activity log list model
        /// </summary>
        /// <param name="searchModel">Customer activity log search model</param>
        /// <param name="customer">Customer</param>
        /// <returns>Customer activity log list model</returns>
        public virtual CustomerActivityLogListModel PrepareCustomerActivityLogListModel(CustomerActivityLogSearchModel searchModel, Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            //get customer activity log
            var activityLog = _customerActivityService.GetAllActivities(customerId: customer.Id,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var pageList = activityLog.Select(logItem =>
            {
                //fill in model values from the entity
                var customerActivityLogModel = logItem.ToModel<CustomerActivityLogModel>();

                //fill in additional values (not existing in the entity)
                customerActivityLogModel.ActivityLogTypeName = logItem.ActivityLogType.Name;

                //convert dates to the user time
                customerActivityLogModel.CreatedOn =
                    _dateTimeHelper.ConvertToUserTime(logItem.CreatedOnUtc, DateTimeKind.Utc);

                return customerActivityLogModel;
            }).ToList().ToPagedList(searchModel);

            //prepare list model
            var model = new CustomerActivityLogListModel().PrepareToGrid(searchModel, pageList, () => pageList);
            
            return model;
        }

        /// <summary>
        /// Prepare paged customer back in stock subscriptions list model
        /// </summary>
        /// <param name="searchModel">Customer back in stock subscriptions search model</param>
        /// <param name="customer">Customer</param>
        /// <returns>Customer back in stock subscriptions list model</returns>
        public virtual CustomerBackInStockSubscriptionListModel PrepareCustomerBackInStockSubscriptionListModel(
            CustomerBackInStockSubscriptionSearchModel searchModel, Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            //get customer back in stock subscriptions
            var subscriptions = _backInStockSubscriptionService.GetAllSubscriptionsByCustomerId(customer.Id,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var pageList = subscriptions.Select(subscription =>
            {
                //fill in model values from the entity
                var subscriptionModel = subscription.ToModel<CustomerBackInStockSubscriptionModel>();

                //convert dates to the user time
                subscriptionModel.CreatedOn =
                    _dateTimeHelper.ConvertToUserTime(subscription.CreatedOnUtc, DateTimeKind.Utc);

                //fill in additional values (not existing in the entity)
                subscriptionModel.StoreName = _storeService.GetStoreById(subscription.StoreId)?.Name ?? "Unknown";
                subscriptionModel.ProductName = subscription.Product?.Name ?? "Unknown";

                return subscriptionModel;
            }).ToList().ToPagedList(searchModel);

            var model = new CustomerBackInStockSubscriptionListModel().PrepareToGrid(searchModel, pageList, () => pageList);

            return model;
        }

        /// <summary>
        /// Prepare online customer search model
        /// </summary>
        /// <param name="searchModel">Online customer search model</param>
        /// <returns>Online customer search model</returns>
        public virtual OnlineCustomerSearchModel PrepareOnlineCustomerSearchModel(OnlineCustomerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();
            searchModel.Grid = PrepareOnlineCustomerGridModel(searchModel);

            return searchModel;
        }

        /// <summary>
        /// Prepare paged online customer list model
        /// </summary>
        /// <param name="searchModel">Online customer search model</param>
        /// <returns>Online customer list model</returns>
        public virtual OnlineCustomerListModel PrepareOnlineCustomerListModel(OnlineCustomerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter customers
            var lastActivityFrom = DateTime.UtcNow.AddMinutes(-_customerSettings.OnlineCustomerMinutes);

            //get online customers
            var customers = _customerService.GetOnlineCustomers(customerRoleIds: null,
                 lastActivityFromUtc: lastActivityFrom,
                 pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = new OnlineCustomerListModel().PrepareToGrid(searchModel, customers, () =>
            {
                return customers.Select(customer =>
                {
                    //fill in model values from the entity
                    var customerModel = customer.ToModel<OnlineCustomerModel>();

                    //convert dates to the user time
                    customerModel.LastActivityDate = _dateTimeHelper.ConvertToUserTime(customer.LastActivityDateUtc, DateTimeKind.Utc);

                    //fill in additional values (not existing in the entity)
                    customerModel.CustomerInfo = customer.IsRegistered()
                        ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest");
                    customerModel.LastIpAddress = _customerSettings.StoreIpAddresses
                        ? customer.LastIpAddress : _localizationService.GetResource("Admin.Customers.OnlineCustomers.Fields.IPAddress.Disabled");
                    customerModel.Location = _geoLookupService.LookupCountryName(customer.LastIpAddress);
                    customerModel.LastVisitedPage = _customerSettings.StoreLastVisitedPage
                        ? _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.LastVisitedPageAttribute)
                        : _localizationService.GetResource("Admin.Customers.OnlineCustomers.Fields.LastVisitedPage.Disabled");

                    return customerModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare GDPR request (log) search model
        /// </summary>
        /// <param name="searchModel">GDPR request search model</param>
        /// <returns>GDPR request search model</returns>
        public virtual GdprLogSearchModel PrepareGdprLogSearchModel(GdprLogSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare request types
            _baseAdminModelFactory.PrepareGdprRequestTypes(searchModel.AvailableRequestTypes);

            //prepare page parameters
            searchModel.SetGridPageSize();
            searchModel.Grid = PrepareGdprLogGridModel(searchModel);

            return searchModel;
        }

        /// <summary>
        /// Prepare paged GDPR request list model
        /// </summary>
        /// <param name="searchModel">GDPR request search model</param>
        /// <returns>GDPR request list model</returns>
        public virtual GdprLogListModel PrepareGdprLogListModel(GdprLogSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var customerId = 0;
            var customerInfo = "";
            if (!string.IsNullOrEmpty(searchModel.SearchEmail))
            {
                var customer = _customerService.GetCustomerByEmail(searchModel.SearchEmail);
                if (customer != null)
                    customerId = customer.Id;
                else
                {
                    customerInfo = searchModel.SearchEmail;
                }
            }
            //get requests
            var gdprLog = _gdprService.GetAllLog(
                customerId: customerId,
                customerInfo: customerInfo,
                requestType: searchModel.SearchRequestTypeId > 0 ? (GdprRequestType?)searchModel.SearchRequestTypeId : null,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            //prepare list model
            var model = new GdprLogListModel().PrepareToGrid(searchModel, gdprLog, () =>
            {
                return gdprLog.Select(log =>
                {
                    //fill in model values from the entity
                    var customer = _customerService.GetCustomerById(log.CustomerId);

                    var requestModel = log.ToModel<GdprLogModel>();

                    //fill in additional values (not existing in the entity)
                    requestModel.CustomerInfo = customer != null && !customer.Deleted && !string.IsNullOrEmpty(customer.Email) ? customer.Email : log.CustomerInfo;
                    requestModel.RequestType = _localizationService.GetLocalizedEnum(log.RequestType);
                    requestModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(log.CreatedOnUtc, DateTimeKind.Utc);

                    return requestModel;
                });
            });

            return model;
        }

        #endregion
    }
}