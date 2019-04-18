﻿using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Services.Catalog;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.DataTables;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the category model factory implementation
    /// </summary>
    public partial class CategoryModelFactory : ICategoryModelFactory
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICategoryService _categoryService;
        private readonly IDiscountService _discountService;
        private readonly IDiscountSupportedModelFactory _discountSupportedModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IProductService _productService;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
        private readonly IUrlRecordService _urlRecordService;

        #endregion

        #region Ctor

        public CategoryModelFactory(CatalogSettings catalogSettings,
            IAclSupportedModelFactory aclSupportedModelFactory,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICategoryService categoryService,
            IDiscountService discountService,
            IDiscountSupportedModelFactory discountSupportedModelFactory,
            ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            IProductService productService,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
            IUrlRecordService urlRecordService)
        {
            _catalogSettings = catalogSettings;
            _aclSupportedModelFactory = aclSupportedModelFactory;
            _baseAdminModelFactory = baseAdminModelFactory;
            _categoryService = categoryService;
            _discountService = discountService;
            _discountSupportedModelFactory = discountSupportedModelFactory;
            _localizationService = localizationService;
            _localizedModelFactory = localizedModelFactory;
            _productService = productService;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
            _urlRecordService = urlRecordService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare category product search model
        /// </summary>
        /// <param name="searchModel">Category product search model</param>
        /// <param name="category">Category</param>
        /// <returns>Category product search model</returns>
        protected virtual CategoryProductSearchModel PrepareCategoryProductSearchModel(CategoryProductSearchModel searchModel, Category category)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (category == null)
                throw new ArgumentNullException(nameof(category));

            searchModel.CategoryId = category.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();
            searchModel.Grid = PrepareCategoryProductGridModel(searchModel);

            return searchModel;
        }

        /// <summary>
        /// Prepare datatables model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>Datatables model</returns>
        protected virtual DataTablesModel PrepareCategoryProductGridModel(CategoryProductSearchModel searchModel)
        {
            //prepare common properties
            var model = new DataTablesModel
            {
                Name = "products-grid",
                UrlRead = new DataUrl("ProductList", "Category", null),
                UrlDelete = new DataUrl("ProductDelete", "Category", null),
                Length = searchModel.PageSize,
                LengthMenu = searchModel.AvailablePageSizes,

                //prepare filters to search
                Filters = new List<FilterParameter>
                {
                    new FilterParameter(nameof(searchModel.CategoryId), searchModel.CategoryId)
                },

                //prepare model columns
                ColumnCollection = new List<ColumnProperty>
                {
                    new ColumnProperty(nameof(CategoryProductModel.ProductName))
                    {
                        Title = _localizationService.GetResource("Admin.Catalog.Categories.Products.Fields.Product")
                    },
                    new ColumnProperty(nameof(CategoryProductModel.IsFeaturedProduct))
                    {
                        Title = _localizationService.GetResource("Admin.Catalog.Categories.Products.Fields.IsFeaturedProduct"),
                        Width = "150",
                        ClassName = StyleColumn.CenterAll,
                        Render = new RenderBoolean()
                    },
                    new ColumnProperty(nameof(CategoryProductModel.DisplayOrder))
                    {
                        Title = _localizationService.GetResource("Admin.Catalog.Categories.Products.Fields.DisplayOrder"),
                        Width = "150"
                    },
                    new ColumnProperty(nameof(CategoryProductModel.Id))
                    {
                        Title = _localizationService.GetResource("Admin.Common.View"),
                        Width = "150",
                        ClassName = StyleColumn.ButtonStyle,
                        Render = new RenderButtonView(new DataUrl("~/Admin/Product/Edit/", nameof(CategoryProductModel.ProductId)))
                    },
                    new ColumnProperty(nameof(CategoryProductModel.Id))
                    {
                        Title = _localizationService.GetResource("Admin.Common.Edit"),
                        Width = "100",
                        ClassName =  StyleColumn.ButtonStyle,
                        Render = new RenderButtonEdit(new DataUrl("Edit"))
                    },
                    new ColumnProperty(nameof(CategoryProductModel.Id))
                    {
                        Title = _localizationService.GetResource("Admin.Common.Delete"),
                        Width = "100",
                        Render = new RenderButtonRemove(_localizationService.GetResource("Admin.Common.Delete")){ Style = StyleButton.Default },
                        ClassName = StyleColumn.ButtonStyle
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
        protected virtual DataTablesModel PrepareAddProductGridModel(AddProductToCategorySearchModel searchModel)
        {
            //prepare common properties
            var model = new DataTablesModel
            {
                Name = "products-grid",
                UrlRead = new DataUrl("ProductAddPopupList", "Category", null),
                SearchButtonId = "search-products",
                Length = searchModel.PageSize,
                LengthMenu = searchModel.AvailablePageSizes
            };

            //prepare filters to search
            model.Filters = new List<FilterParameter>
            {
                new FilterParameter(nameof(searchModel.SearchProductName)),
                new FilterParameter(nameof(searchModel.SearchCategoryId)),
                new FilterParameter(nameof(searchModel.SearchManufacturerId)),
                new FilterParameter(nameof(searchModel.SearchStoreId)),
                new FilterParameter(nameof(searchModel.SearchVendorId)),
                new FilterParameter(nameof(searchModel.SearchProductTypeId))
            };

            //prepare model columns
            model.ColumnCollection = new List<ColumnProperty>
            {
                new ColumnProperty(nameof(ProductModel.Id))
                {
                    IsMasterCheckBox = true,
                    Render = new RenderCheckBox(nameof(AddProductToCategoryModel.SelectedProductIds)),
                    ClassName =  StyleColumn.CenterAll,
                    Width = "50",
                },
                new ColumnProperty(nameof(ProductModel.Name))
                {
                    Title = _localizationService.GetResource("Admin.Catalog.Products.Fields.Name")
                },
                new ColumnProperty(nameof(ProductModel.Published))
                {
                    Title = _localizationService.GetResource("Admin.Catalog.Products.Fields.Published"),
                    Width = "100",
                    Render = new RenderBoolean()
                }
            };

            return model;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare category search model
        /// </summary>
        /// <param name="searchModel">Category search model</param>
        /// <returns>Category search model</returns>
        public virtual CategorySearchModel PrepareCategorySearchModel(CategorySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available stores
            _baseAdminModelFactory.PrepareStores(searchModel.AvailableStores);

            searchModel.HideStoresList = _catalogSettings.IgnoreStoreLimitations || searchModel.AvailableStores.SelectionIsNotPossible();

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged category list model
        /// </summary>
        /// <param name="searchModel">Category search model</param>
        /// <returns>Category list model</returns>
        public virtual CategoryListModel PrepareCategoryListModel(CategorySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get categories
            var categories = _categoryService.GetAllCategories(categoryName: searchModel.SearchCategoryName,
                showHidden: true,
                storeId: searchModel.SearchStoreId,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = new CategoryListModel().PrepareToGrid(searchModel, categories, () =>
            {
                return categories.Select(category =>
                {
                    //fill in model values from the entity
                    var categoryModel = category.ToModel<CategoryModel>();

                    //fill in additional values (not existing in the entity)
                    categoryModel.Breadcrumb = _categoryService.GetFormattedBreadCrumb(category);
                    categoryModel.SeName = _urlRecordService.GetSeName(category, 0, true, false);

                    return categoryModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare category model
        /// </summary>
        /// <param name="model">Category model</param>
        /// <param name="category">Category</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Category model</returns>
        public virtual CategoryModel PrepareCategoryModel(CategoryModel model, Category category, bool excludeProperties = false)
        {
            Action<CategoryLocalizedModel, int> localizedModelConfiguration = null;

            if (category != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = category.ToModel<CategoryModel>();
                    model.SeName = _urlRecordService.GetSeName(category, 0, true, false);
                }

                //prepare nested search model
                PrepareCategoryProductSearchModel(model.CategoryProductSearchModel, category);

                //define localized model configuration action
                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.Name = _localizationService.GetLocalized(category, entity => entity.Name, languageId, false, false);
                    locale.Description = _localizationService.GetLocalized(category, entity => entity.Description, languageId, false, false);
                    locale.MetaKeywords = _localizationService.GetLocalized(category, entity => entity.MetaKeywords, languageId, false, false);
                    locale.MetaDescription = _localizationService.GetLocalized(category, entity => entity.MetaDescription, languageId, false, false);
                    locale.MetaTitle = _localizationService.GetLocalized(category, entity => entity.MetaTitle, languageId, false, false);
                    locale.SeName = _urlRecordService.GetSeName(category, languageId, false, false);
                };
            }

            //set default values for the new model
            if (category == null)
            {
                model.PageSize = _catalogSettings.DefaultCategoryPageSize;
                model.PageSizeOptions = _catalogSettings.DefaultCategoryPageSizeOptions;
                model.Published = true;
                model.IncludeInTopMenu = true;
                model.AllowCustomersToSelectPageSize = true;
            }

            //prepare localized models
            if (!excludeProperties)
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

            //prepare available category templates
            _baseAdminModelFactory.PrepareCategoryTemplates(model.AvailableCategoryTemplates, false);

            //prepare available parent categories
            _baseAdminModelFactory.PrepareCategories(model.AvailableCategories,
                defaultItemText: _localizationService.GetResource("Admin.Catalog.Categories.Fields.Parent.None"));

            //prepare model discounts
            var availableDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToCategories, showHidden: true);
            _discountSupportedModelFactory.PrepareModelDiscounts(model, category, availableDiscounts, excludeProperties);

            //prepare model customer roles
            _aclSupportedModelFactory.PrepareModelCustomerRoles(model, category, excludeProperties);

            //prepare model stores
            _storeMappingSupportedModelFactory.PrepareModelStores(model, category, excludeProperties);

            return model;
        }

        /// <summary>
        /// Prepare paged category product list model
        /// </summary>
        /// <param name="searchModel">Category product search model</param>
        /// <param name="category">Category</param>
        /// <returns>Category product list model</returns>
        public virtual CategoryProductListModel PrepareCategoryProductListModel(CategoryProductSearchModel searchModel, Category category)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (category == null)
                throw new ArgumentNullException(nameof(category));

            //get product categories
            var productCategories = _categoryService.GetProductCategoriesByCategoryId(category.Id,
                showHidden: true,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = new CategoryProductListModel().PrepareToGrid(searchModel, productCategories, () =>
            {
                return productCategories.Select(productCategory =>
                {
                    //fill in model values from the entity
                    var categoryProductModel = productCategory.ToModel<CategoryProductModel>();

                    //fill in additional values (not existing in the entity)
                    categoryProductModel.ProductName = _productService.GetProductById(productCategory.ProductId)?.Name;

                    return categoryProductModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare product search model to add to the category
        /// </summary>
        /// <param name="searchModel">Product search model to add to the category</param>
        /// <returns>Product search model to add to the category</returns>
        public virtual AddProductToCategorySearchModel PrepareAddProductToCategorySearchModel(AddProductToCategorySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available categories
            _baseAdminModelFactory.PrepareCategories(searchModel.AvailableCategories);

            //prepare available manufacturers
            _baseAdminModelFactory.PrepareManufacturers(searchModel.AvailableManufacturers);

            //prepare available stores
            _baseAdminModelFactory.PrepareStores(searchModel.AvailableStores);

            //prepare available vendors
            _baseAdminModelFactory.PrepareVendors(searchModel.AvailableVendors);

            //prepare available product types
            _baseAdminModelFactory.PrepareProductTypes(searchModel.AvailableProductTypes);

            //prepare page parameters
            searchModel.SetPopupGridPageSize();
            searchModel.Grid = PrepareAddProductGridModel(searchModel);

            return searchModel;
        }

        /// <summary>
        /// Prepare paged product list model to add to the category
        /// </summary>
        /// <param name="searchModel">Product search model to add to the category</param>
        /// <returns>Product list model to add to the category</returns>
        public virtual AddProductToCategoryListModel PrepareAddProductToCategoryListModel(AddProductToCategorySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get products
            var products = _productService.SearchProducts(showHidden: true,
                categoryIds: new List<int> { searchModel.SearchCategoryId },
                manufacturerId: searchModel.SearchManufacturerId,
                storeId: searchModel.SearchStoreId,
                vendorId: searchModel.SearchVendorId,
                productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = new AddProductToCategoryListModel().PrepareToGrid(searchModel, products, () =>
            {
                return products.Select(product =>
                {
                    var productModel = product.ToModel<ProductModel>();
                    productModel.SeName = _urlRecordService.GetSeName(product, 0, true, false);

                    return productModel;
                });
            });

            return model;
        }

        #endregion
    }
}