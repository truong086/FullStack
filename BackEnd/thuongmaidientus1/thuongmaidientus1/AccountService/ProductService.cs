﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using thuongmaidientus1.Common;
using thuongmaidientus1.EmailConfig;
using thuongmaidientus1.Models;
using thuongmaidientus1.ViewModel;

namespace thuongmaidientus1.AccountService
{
    public class ProductService : IProductService
    {
        private readonly DBContexts _dbcontext;
        private readonly IMapper _mapper;
        private readonly EmailSetting _emaiSetting;
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly Jwt _jwt;
        private readonly IUserService _userService;
        public ProductService(DBContexts dbcontext, IMapper mapper, IOptions<EmailSetting> emailSetting, IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider, IServiceProvider serviceProvider, IOptionsMonitor<Jwt> jwt, IUserService userService)
        {
            _dbcontext = dbcontext;
            _mapper = mapper;
            _emaiSetting = emailSetting.Value;
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
            _jwt = jwt.CurrentValue;
            _userService = userService;
        }
        public async Task<PayLoad<ProductDTO>> AddProduct(ProductDTO productDTO)
        {
            try
            {
                var checkName = _dbcontext.products.Where(x => x.title == productDTO.title).FirstOrDefault();
                if (checkName != null)
                {
                    return await Task.FromResult(PayLoad<ProductDTO>.CreatedFail("Tên sản phẩm đã tồn tại"));
                }

                var MapProduct = _mapper.Map<Product>(productDTO);
                MapProduct.image = productDTO.image.Count() > 0 ? productDTO.image[0] : "Null";
                MapProduct.Deleted = false;

                var checkAccount = new Account();

                if(productDTO.account_name != null)
                {
                    checkAccount = _dbcontext.accounts.Include(r => r.role).Where(x => x.username == productDTO.account_name).FirstOrDefault();
                    if(checkAccount != null && checkAccount.role.name == "Admin")
                    {
                        MapProduct.Accounts = checkAccount;

                        var checckShop = _dbcontext.shops.Include(a => a.account).Where(x => x.account.id == productDTO.account_id).FirstOrDefault();
                        if (checckShop != null)
                        {
                            MapProduct.Shops = checckShop;
                            MapProduct.CretorEdit = "Admin đã tạo sản phẩm này, Admin tên là " + checkAccount.username;
                        }
                    }
                    else if(checkAccount != null && checkAccount.role.name == "Shop")
                    {
                        MapProduct.Accounts = checkAccount;
                        var checkShop = _dbcontext.shops.Include(a => a.account).Where(x => x.account.Equals(checkAccount)).FirstOrDefault();
                        MapProduct.Shops = checkShop;
                        MapProduct.CretorEdit = "Shop được tạo vào lúc " + DateTimeOffset.UtcNow;
                    }
                }
                //if (productDTO.account_id != 0 && productDTO.account_name == null)
                //{
                //    checkAccount = _dbcontext.accounts.Where(x => x.id == productDTO.account_id).FirstOrDefault();
                //    if (checkAccount == null)
                //    {
                //        return await Task.FromResult(PayLoad<ProductDTO>.CreatedFail("Account không tồn tại"));
                //    }
                //}
                //else if(productDTO.account_id == 0 && productDTO.account_name != null)
                //{
                //    checkAccount = _dbcontext.accounts.Where(x => x.username == productDTO.account_name).FirstOrDefault();
                //    if (checkAccount == null)
                //    {
                //        return await Task.FromResult(PayLoad<ProductDTO>.CreatedFail("Account không tồn tại"));
                //    }
                //}
                
                

                _dbcontext.products.Add(MapProduct);
                if(await _dbcontext.SaveChangesAsync() > 0)
                {
                    var productId = MapProduct;
                    var checkProductImage = _dbcontext.productimages.Include(p => p.product).Where(x => x.product == productId).ToList();
                    if(checkProductImage.Count > 0)
                    {
                        _dbcontext.productimages.RemoveRange(checkProductImage);
                        if(await _dbcontext.SaveChangesAsync() > 0)
                        {
                            for (var i = 0; i < productDTO.image.Count; i++)
                            {
                                var image = productDTO.image[i];
                                var productImage = new ProductImage()
                                {
                                    product = productId,
                                    image = image
                                };

                                _dbcontext.productimages.Add(productImage);
                                if (await _dbcontext.SaveChangesAsync() <= 0)
                                {
                                    return await Task.FromResult(PayLoad<ProductDTO>.CreatedFail("Faild Add"));
                                }
                            }
                        }
                    }
                    else if(checkProductImage.Count <= 0)
                    {
                        for(var i = 0; i < productDTO.image.Count; i++)
                        {
                            var image = productDTO.image[i];
                            var productImage = new ProductImage()
                            {
                                product = productId,
                                image = image
                            };

                            _dbcontext.productimages.Add(productImage);
                            if(await _dbcontext.SaveChangesAsync() <= 0)
                            {
                                return await Task.FromResult(PayLoad<ProductDTO>.CreatedFail("Faild Add"));
                            }
                        }
                    }

                    if (productDTO.category != null && productDTO.category.Any())
                    {
                        var checkCategoryProduct = _dbcontext.productCategories.Include(p => p.Product).Include(c => c.Category).Where(x => x.Product == productId).ToList();
                        var productCategory = new ProductCategory();
                        if (checkCategoryProduct.Count > 0)
                        {
                            _dbcontext.productCategories.RemoveRange(checkCategoryProduct);
                            if (await _dbcontext.SaveChangesAsync() > 0)
                            {
                                for (var i = 0; i < productDTO.category.Count(); i++)
                                {

                                    var data = productDTO.category[i];
                                    bool checkInt = Regex.IsMatch(data, @"^\d+$");
                                    if (int.TryParse(data, out int songuyen))
                                    {
                                        var CheckIntCategory = _dbcontext.categories.Where(x => x.id == songuyen).FirstOrDefault();
                                        if (CheckIntCategory != null)
                                        {
                                            if (MapProduct.Categorys == null)
                                            {
                                                MapProduct.Categorys = CheckIntCategory;
                                            }

                                            productCategory.Product = productId;
                                            productCategory.Category = CheckIntCategory;

                                            _dbcontext.productCategories.Add(productCategory);
                                            if(await _dbcontext.SaveChangesAsync() <= 0)
                                            {
                                                return await Task.FromResult(PayLoad<ProductDTO>.CreatedFail("Add Category Faild"));
                                            }
                                            
                                        }
                                    }
                                    if (!checkInt)
                                    {
                                        var checkNameCategory = _dbcontext.categories.Where(x => x.name == data).FirstOrDefault();
                                        if(checkNameCategory != null)
                                        {
                                            if(MapProduct.Categorys == null)
                                            {
                                                MapProduct.Categorys = checkNameCategory;
                                            }
                                            productCategory.Product = productId;
                                            productCategory.Category = checkNameCategory;

                                            _dbcontext.productCategories.Add(productCategory);
                                            if(await _dbcontext.SaveChangesAsync() <= 0)
                                            {
                                                return await Task.FromResult(PayLoad<ProductDTO>.CreatedFail("Add Category Faild"));
                                            }
                                        }
                                    }
                                }
                                

                            }
                        }
                        else
                        {
                            var checkData = await AddDatas(productDTO.category, productId);
                            if(checkData == null)
                            {
                                return await Task.FromResult(PayLoad<ProductDTO>.CreatedFail("Add Category Faild"));
                            }

                        }

                    }
                    return await Task.FromResult(PayLoad<ProductDTO>.Successfully(productDTO));
                }

                return await Task.FromResult(PayLoad<ProductDTO>.CreatedFail("Faild Add"));
            }
            catch(Exception ex)
            {
                return await Task.FromResult(PayLoad<ProductDTO>.CreatedFail(ex.Message));
            }
        }

        private async Task<ProductCategory> AddDatas (List<string> values, Product product)
        {
            for (var i = 0; i < values.Count(); i++)
            {

                var data = values[i];
                bool checkInt = Regex.IsMatch(data, @"^\d+$");
                if (int.TryParse(data, out int songuyen))
                {
                    var CheckIntCategory = _dbcontext.categories.Where(x => x.id == songuyen).FirstOrDefault();
                    if (CheckIntCategory != null)
                    {
                        if (product.Categorys == null)
                        {
                            product.Categorys = CheckIntCategory;
                        }
                        var productCategory = new ProductCategory();
                        productCategory.Product = product;
                        productCategory.Category = CheckIntCategory;

                        _dbcontext.productCategories.Add(productCategory);
                        //if (await _dbcontext.SaveChangesAsync() <= 0)
                        //{
                        //    return await Task.FromResult<ProductCategory>(null);
                        //}

                    }
                }
                if (!checkInt)
                {
                    var checkNameCategory = _dbcontext.categories.Where(x => x.name == data).FirstOrDefault();
                    if (checkNameCategory != null)
                    {
                        if (product.Categorys == null)
                        {
                            product.Categorys = checkNameCategory;
                        }
                        var productCategory = new ProductCategory();
                        productCategory.Product = product;
                        productCategory.Category = checkNameCategory;

                        _dbcontext.productCategories.Add(productCategory);
                        

                    }
                }
            }

            if (await _dbcontext.SaveChangesAsync() <= 0)
            {
                return await Task.FromResult<ProductCategory>(null);
            }

            return await Task.FromResult<ProductCategory>(new ProductCategory
            {
                Product = product
            });

        }

        public async Task<PayLoad<string>> DeleteProduct(IList<string> id)
        {
            string? message = null;
            try
            {
                if (id.Count <= 0 || !id.Any() || id == null)
                    return await Task.FromResult(PayLoad<string>.CreatedFail("Dữ liệu chuyền vào null"));

                var list = new List<Product>();
                foreach(var item in id)
                {
                    bool checkInt = Regex.IsMatch(item, @"^\d+$");
                    if(int.TryParse(item, out int songuyen))
                    {
                        var checkId = _dbcontext.products.Where(x => x.id == songuyen).FirstOrDefault();
                        if(checkId != null)
                        {
                            checkId.Deleted = true;
                            _dbcontext.products.Update(checkId);
                        }
                    }

                    if (!checkInt)
                    {
                        var checkName = _dbcontext.products.Where(x => x.title == item).FirstOrDefault();
                        if(checkName != null)
                        {
                            checkName.Deleted = true;
                            _dbcontext.products.UpdateRange(checkName);
                        }
                    }
                }

                if(await _dbcontext.SaveChangesAsync() <= 0) {
                    message = "Delete Faild";
                    return await Task.FromResult(PayLoad<string>.CreatedFail(message));
                }

                message = "Success";
                return await Task.FromResult(PayLoad<string>.Successfully(message));

            }
            catch(Exception ex)
            {
                message = ex.Message;
                return await Task.FromResult(PayLoad<string>.CreatedFail(message));
            }
        }

        public async Task<PayLoad<ProductDTO>> EditProduct(int id, ProductDTO productDTO, string? name)
        {
            try
            {
                var checkId = _dbcontext.products.Where(x => x.id == id).FirstOrDefault();
                if(checkId == null)
                {
                    return await Task.FromResult(PayLoad<ProductDTO>.CreatedFail("không tồn tại"));
                }

                var checkAccount = _dbcontext.accounts.Where(x => x.username == name).FirstOrDefault();
                if (checkAccount == null)
                {
                    return await Task.FromResult(PayLoad<ProductDTO>.CreatedFail(" Account không tồn tại"));
                }
                if(productDTO.account_name != null)
                {
                    var checkAdmin = _dbcontext.accounts.Include(r => r.role).Where(x => x.username == productDTO.account_name).FirstOrDefault();

                    if (checkAccount != null && checkAccount.role.name == "Admin")
                    {
                        if (productDTO.account_id != 0)
                        {

                            var checkShop = _dbcontext.shops.Where(x => x.id == productDTO.account_id).FirstOrDefault();
                            if (checkShop != null)
                            {
                                checkId.Shops = checkShop;
                                checkId.CretorEdit = "Admin" + checkAccount.username + " sửa shop này vào lúc " + DateTimeOffset.UtcNow;
                            }

                        }
                        
                    }
                    
                    else if(productDTO.account_id == 0 && checkAdmin.role.name == "Shop")
                    {
                        var checkShop = _dbcontext.shops.Include(a => a.account).Where(x => x.account.Equals(checkAdmin)).FirstOrDefault();
                        if(checkShop != null)
                        {
                            checkId.Shops = checkId.Shops == checkShop ? checkShop : checkId.Shops;
                            checkId.CretorEdit = checkAdmin.username + " là người đã sửa sản phẩm này vào lúc " + DateTimeOffset.UtcNow;
                        }
                    }
                }
                

                checkId.price = productDTO.price;
                checkId.image = productDTO.image.Count > 0 ? productDTO.image[0] : null;
                checkId.title = productDTO.title;
                checkId.description = productDTO.description;

                var checkDataList = new List<object>();
                if(productDTO.category.Any() || productDTO.category.Count > 0)
                {
                    foreach(var item in productDTO.category)
                    {
                        if(int.TryParse(item, out int songuyen))
                        {
                            var checkInt = _dbcontext.categories.Where(x => x.id ==  songuyen).FirstOrDefault();
                            if(checkInt != null)
                            {
                                if(checkDataList == null || checkDataList.Count <= 0)
                                {
                                    checkId.Categorys = checkInt;
                                    checkDataList.Add(checkId.Categorys);
                                }
                            }
                        }
                        else
                        {
                            var checkName = _dbcontext.categories.Where(x => x.name == item).FirstOrDefault();
                            if (checkName != null)
                            {
                                if (checkDataList == null || checkDataList.Count <= 0)
                                {
                                    checkId.Categorys = checkName;
                                    checkDataList.Add(checkId.Categorys);
                                }
                            }
                        }
                    }
                }

                _dbcontext.products.Update(checkId);
                var listImage = new List<object>();
                if (await _dbcontext.SaveChangesAsync() > 0)
                {
                    
                    var idProduct = checkId;
                    var deleteImage = _dbcontext.productimages.Include(p => p.product).Where(x => x.product == idProduct).ToList();
                    if(deleteImage.Count > 0 || deleteImage.Any())
                    {
                        _dbcontext.productimages.RemoveRange(deleteImage);
                        if (await _dbcontext.SaveChangesAsync() > 0)
                        {
                            for (var i = 0; i < productDTO.image.Count; i++)
                            {
                                var image = productDTO.image[i];

                                if (!string.IsNullOrEmpty(image))
                                {
                                    var ImageProduct = new ProductImage()
                                    {
                                        product = checkId,
                                        image = image,
                                    };

                                    _dbcontext.productimages.Add(ImageProduct);
                                    listImage.Add(ImageProduct);
                                }
                            }
                        }
                    }


                    else if(deleteImage.Count <= 0 || !deleteImage.Any())
                    {
                        for (var i = 0; i < productDTO.image.Count; i++)
                        {
                            var image = productDTO.image[i];
                            var ImageProduct = new ProductImage()
                            {
                                product = checkId,
                                image = image,
                            };

                            _dbcontext.productimages.Add(ImageProduct);
                            listImage.Add(ImageProduct);
                        }
                    }


                    if (productDTO.category.Count >= 0 && productDTO.category.Any())
                    {
                        var checkCategoryProduct = _dbcontext.productCategories.Include(p => p.Product).Include(c => c.Category).Where(x => x.Product == idProduct).ToList();
                        
                        if (checkCategoryProduct.Count > 0)
                        {
                            _dbcontext.productCategories.RemoveRange(checkCategoryProduct);
                            if (await _dbcontext.SaveChangesAsync() > 0)
                            {
                                for (var i = 0; i < productDTO.category.Count(); i++)
                                {
                                    var productCategory = new ProductCategory();

                                    var data = productDTO.category[i];
                                    bool checkInt = Regex.IsMatch(data, @"^\d+$");
                                    if (int.TryParse(data, out int songuyen))
                                    {
                                        var CheckIntCategory = _dbcontext.categories.Where(x => x.id == songuyen).FirstOrDefault();
                                        if (CheckIntCategory != null)
                                        {
                                            productCategory.Product = idProduct;
                                            productCategory.Category = CheckIntCategory;

                                            _dbcontext.productCategories.Add(productCategory);
                                            if (await _dbcontext.SaveChangesAsync() <= 0)
                                            {
                                                return await Task.FromResult(PayLoad<ProductDTO>.CreatedFail("Add Category Faild"));
                                            }

                                        }
                                    }
                                    if (!checkInt)
                                    {
                                        var checkNameCategory = _dbcontext.categories.Where(x => x.name == data).FirstOrDefault();
                                        if (checkNameCategory != null)
                                        {

                                            productCategory.Product = idProduct;
                                            productCategory.Category = checkNameCategory;

                                            _dbcontext.productCategories.Add(productCategory);
                                            //if (await _dbcontext.SaveChangesAsync() <= 0)
                                            //{
                                            //    return await Task.FromResult(PayLoad<ProductDTO>.CreatedFail("Add Category Faild"));
                                            //}
                                        }
                                    }
                                }


                            }
                        }
                        else
                        {
                            var checkData = await AddDatas(productDTO.category, idProduct);
                            if (checkData == null)
                            {
                                return await Task.FromResult(PayLoad<ProductDTO>.CreatedFail("Add Category Faild"));
                            }

                        }

                    }



                }

                if(listImage.Count > 0 || listImage.Any())
                {
                    if (await _dbcontext.SaveChangesAsync() > 0)
                    {
                        return await Task.FromResult(PayLoad<ProductDTO>.Successfully(productDTO));
                    }
                }


                return await Task.FromResult(PayLoad<ProductDTO>.CreatedFail("Update Faild"));


            }
            catch(Exception ex)
            {
                return await Task.FromResult(PayLoad<ProductDTO>.CreatedFail(ex.Message));
            }
        }

        public async Task<PayLoad<object>> FindAll(string? name, string? account_name, string? type, int page = 1, int pageSize = 10)
        {
            try
            {
                //var listData = _dbcontext.products
                //    .Where(x => !x.Deleted)
                //    .Include(p => p.ProductImages.Where(pi => !string.IsNullOrEmpty(pi.image)))
                //    .Include(p => p.ProductCategories.Select(pc => pc.Category))
                //    .Where(x => string.IsNullOrEmpty(name) || x.title.Contains(name))
                //    .Select(item => new ProductDTO
                //    {
                //        price = item.price,
                //        title = item.title,
                //        description = item.description,
                //        click = item.click,
                //        account_id = item.Shop.id > 0 ? item.Shop.id : 0,
                //        imageShop = !string.IsNullOrEmpty(item.Shop.image) ? item.Shop.image : "Shop này chưa có ảnh",
                //        nameShop = !string.IsNullOrEmpty(item.Shop.Name) ? item.Shop.Name : "Sản phẩm này không thuộc quyền sở hữu của shop nào hết",
                //        image = item.ProductImages.Select(pi => pi.image).ToList(),
                //        category = item.ProductCategories.Select(pc => pc.Category.name).ToList()
                //    })
                //    .ToList();

                if(account_name == null)
                {
                    return await Task.FromResult(PayLoad<object>.CreatedFail("Data Null"));
                }

                var listData = new List<object>();
                page = page <= 0 ? 1 : page;
                pageSize = pageSize <= 0 ? 10 : pageSize;

                var data = _dbcontext.products.Include(c => c.Categorys).Include(s => s.Shops).ThenInclude(a => a.account).Where(x => !x.Deleted).ToList();
                if (type == "donhang")
                {
                    var listInt = new List<int>();
                    var orderDetails = _dbcontext.OrderDetails.
                        Include(o => o.Orders)
                        .Include(p => p.Products)
                        .GroupBy(x => new { x.Products.id })
                        .Select(l => new
                        {
                            product_id = l.Key.id
                        }).ToList();
                    for(var i = 0; i < orderDetails.Count; i++)
                    {
                        var order = orderDetails[i];
                        listInt.Add(order.product_id);
                    }
                    data = data.Where(x => listInt.Contains(x.id)).ToList();
                }

                else if(type == "click")
                {
                    data = data.OrderByDescending(x => x.click).ToList();
                }
                else if(type == "sapxep")
                {
                    data = data.OrderByDescending(x => x.id).ToList();
                }
                

                if (!string.IsNullOrEmpty(name))
                    data = data.Where(x => x.title.Contains(name)).ToList();


                var checkAccount = _dbcontext.accounts.Include(r => r.role).Where(x => x.username == account_name).FirstOrDefault();
                if(checkAccount == null)
                {
                    return await Task.FromResult(PayLoad<object>.CreatedFail("Data Null"));
                }

                if(checkAccount.role.name == "Shop")
                {
                    data = data.Where(x => x.Shops != null && x.Shops.account != null && x.Shops.account.Equals(checkAccount)).ToList();
                }else if(checkAccount.role.name == "User")
                {
                    return await Task.FromResult(PayLoad<object>.CreatedFail("Bạn không có quyền"));
                }
                foreach(var item in data)
                {
                    var listImage = new List<string>();
                    var listCategory = new List<string>();
                    var productDTO = new ProductDTO();
                    var checkImageProduct = _dbcontext.productimages.Include(p => p.product).Where(x => x.product == item).ToList();
                    if(checkImageProduct != null)
                    {
                        for (var i = 0; i < checkImageProduct.Count; i++)
                        {
                            var image = checkImageProduct[i];
                            if (!string.IsNullOrEmpty(image.image))
                            {
                                var images = image.image.ToString();
                                listImage.Add(images);
                                
                            }
                            

                        }
                        productDTO.image = listImage;
                    }

                    var checkCategory = _dbcontext.productCategories.Include(p => p.Product).Include(c => c.Category).Where(x => x.Product == item).ToList();
                    for(var i = 0; i < checkCategory.Count; i++)
                    {
                        var dataCategory = checkCategory[i];
                        var checkIdCategory = _dbcontext.categories.Where(x => x.Equals(dataCategory.Category)).FirstOrDefault();
                        if(checkIdCategory != null)
                        {
                            if (!string.IsNullOrEmpty(checkIdCategory.name))
                            {
                                listCategory.Add(checkIdCategory.name);
                                
                            }
                            
                        }
                    }
                    productDTO.category = listCategory;

                    var checkShop = _dbcontext.shops.Where(x => x.Equals(item.Shops)).FirstOrDefault();

                    productDTO.Id = item.id;
                    productDTO.price = item.price;
                    productDTO.title = item.title;
                    productDTO.description = item.description;
                    productDTO.click = item.click;
                    productDTO.account_id = checkShop == null ? 0 : checkShop.id ;
                    productDTO.imageShop = checkShop != null && !string.IsNullOrEmpty(checkShop.image) ? checkShop.image : "Shop này chưa có ảnh";
                    productDTO.nameShop = checkShop != null && !string.IsNullOrEmpty(checkShop.Name) ? checkShop.Name : "Sản phẩm này không thuộc quyền sở hữu của shop nào hết";

                    listData.Add(productDTO);
                }

                var pageList = new PageList<object>(listData, page - 1, pageSize);

                return await Task.FromResult(PayLoad<object>.Successfully(new
                {
                    data = pageList,
                    page,
                    pageList.pageSize,
                    pageList.totalCounts,
                    pageList.totalPages
                }));
            }
            catch (Exception ex)
            {
                return await Task.FromResult(PayLoad<object>.CreatedFail(ex.Message));
            }
            
        }

        public async Task<PayLoad<object>> FindOne(int id, string? name)
        {
            try
            {
                
                var productDTOs = new ProductDTO();
                var checkId = _dbcontext.products.Include(c => c.Categorys).Include(s => s.Shops).Where(x => x.id == id).FirstOrDefault();
                if(checkId == null)
                {
                    return await Task.FromResult(PayLoad<object>.CreatedFail("Null"));
                }
                if (!string.IsNullOrEmpty(name))
                {
                    checkId.click += 1;
                    _dbcontext.products.Update(checkId);
                    await _dbcontext.SaveChangesAsync();
                }
                var checkIdImage = _dbcontext.productimages.Include(p => p.product).Where(x => x.product == checkId).ToList();
                if(checkIdImage != null)
                {
                    var listImage = new List<string>();
                    for (var i = 0; i < checkIdImage.Count; i++)
                    {
                        
                        var data = checkIdImage[i];   
                        if (!string.IsNullOrEmpty(data.image))
                        {
                            listImage.Add(data.image);
                            
                        }
                    }
                    productDTOs.image = listImage;
                }

                var checkCategory = _dbcontext.productCategories.Include(p => p.Product).Include(c => c.Category).Where(x => x.Product == checkId).ToList();
                if(checkCategory != null)
                {
                    var listCategory = new List<string>();
                    for (var i = 0; i < checkCategory.Count; i++)
                    {
                        var data = checkCategory[i];
                        var checkCategoryName = _dbcontext.categories.Where(x => x.Equals(data.Category)).FirstOrDefault();
                        if(checkCategoryName != null)
                        {
                            if (!string.IsNullOrEmpty(checkCategoryName.name))
                            {
                                listCategory.Add(checkCategoryName.name);
                                
                            }
                        }

                    }
                    productDTOs.category = listCategory;
                }
                var checkShop = _dbcontext.shops.Where(x => x.Equals(checkId.Shops)).FirstOrDefault();
                //var MapProductDTO = _mapper.Map<ProductDTO>(checkId);
                //MapProductDTO.image = productDTOs.image;
                //MapProductDTO.category = productDTOs.category;
                productDTOs.price = checkId.price;
                productDTOs.description = checkId.description;
                productDTOs.title = checkId.title;
                productDTOs.Id = checkId.id;
                productDTOs.nameShop = checkShop != null &&!string.IsNullOrEmpty(checkShop.Name) ? checkShop.Name : "Sản phẩm chưa thuộc shop nào hết";
                productDTOs.account_id = checkShop != null&& checkShop.id > 0 ? checkShop.id : 0;
                productDTOs.imageShop = checkShop != null && !string.IsNullOrEmpty(checkShop.image) ? checkShop.image : "Chữa có ảnh";
                productDTOs.click = checkId.click;

                return await Task.FromResult(PayLoad<object>.Successfully(productDTOs));
            }
            catch (Exception ex)
            {
                return await Task.FromResult(PayLoad<object>.CreatedFail(ex.Message));
            }
        }

        public Task<PayLoad<object>> UpdateClick(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<PayLoad<object>> FindAllProductCategory(IList<string> id, int page = 1, int pageSize = 20)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : pageSize;
            try
            {
                if (!id.Any() || id.Count <= 0)
                    return await Task.FromResult(PayLoad<object>.CreatedFail("Null"));
                var listProduct = new List<ProductDTO>();
                var listCheck = new List<string>();
                for (var i = 0; i < id.Count; i++)
                {
                    
                    var data = id[i];
                    bool checkInt = Regex.IsMatch(data, @"^\d+$");
                    var checkCategory = _dbcontext.categories.Where(x => !x.Deleted).ToList();
                    if(int.TryParse(data, out int songuyen))
                    {
                        checkCategory = checkCategory.Where(x => x.id == songuyen).ToList();
                    }
                    if (!checkInt)
                    {
                        checkCategory = checkCategory.Where(x => x.name == data).ToList();
                    }
                    //checkCategory.FirstOrDefault();
                    for(var categoryFindOne = 0; categoryFindOne < checkCategory.Count; categoryFindOne++)
                    {
                        
                        var dataItemCaytegoru = checkCategory[categoryFindOne];
                        var checkProductItem = _dbcontext.products.Include(c => c.Categorys).Include(s => s.Shops).Where(x => x.Categorys.Equals(dataItemCaytegoru)).ToList();
                        if (checkProductItem.Count > 0)
                        {
                            foreach (var item in checkProductItem)
                            {
                               
                                if (!listCheck.Contains(item.title))
                                {
                                    var listImage = new List<string>();
                                    var listCategory = new List<string>();
                                    var productDTO = new ProductDTO();

                                    var checkImage = _dbcontext.productimages.Include(p => p.product).Where(x => x.product.Equals(item)).ToList();
                                    if (checkImage.Count > 0)
                                    {
                                        for (var image = 0; image < checkImage.Count; image++)
                                        {
                                            var dataImage = checkImage[image];
                                            if (!string.IsNullOrEmpty(dataImage.image))
                                            {
                                                listImage.Add(dataImage.image);
                                            }
                                        }
                                        productDTO.image = listImage;
                                    }

                                    var checkCategoryItem = _dbcontext.productCategories.Include(c => c.Category).Include(p => p.Product).Where(x => x.Product.Equals(item)).ToList();
                                    if (checkCategoryItem.Count > 0)
                                    {
                                        for (var itemCategory = 0; itemCategory < checkCategoryItem.Count; itemCategory++)
                                        {
                                            var dataCategory = checkCategoryItem[itemCategory];
                                            var categoryItem = _dbcontext.categories.Where(x => x.Equals(dataCategory.Category)).FirstOrDefault();
                                            if (categoryItem != null)
                                            {
                                                if (!string.IsNullOrEmpty(categoryItem.name))
                                                {
                                                    listCategory.Add(categoryItem.name);
                                                }
                                            }
                                        }
                                        productDTO.category = listCategory;
                                    }

                                    var checkShop = _dbcontext.shops.Where(x => x.Equals(item.Shops)).FirstOrDefault();

                                    productDTO.Id = item.id;
                                    productDTO.price = item.price;
                                    productDTO.description = item.description;
                                    productDTO.title = item.title;
                                    productDTO.Id = item.id;
                                    productDTO.nameShop = checkShop != null && !string.IsNullOrEmpty(checkShop.Name) ? checkShop.Name : "Sản phẩm chưa thuộc shop nào hết";
                                    productDTO.account_id = checkShop != null && checkShop.id > 0 ? checkShop.id : 0;
                                    productDTO.imageShop = checkShop != null && !string.IsNullOrEmpty(checkShop.image) ? checkShop.image : "Chữa có ảnh";

                                    listProduct.Add(productDTO);
                                    listCheck.Add(item.title);
                                }
                            }
                        }
                    }
                    
                    
                }
                var pageList = new PageList<object>(listProduct, page - 1, pageSize);
                return await Task.FromResult(PayLoad<object>.Successfully(new
                {
                    data = pageList,
                    page,
                    pageList.pageSize,
                    pageList.totalCounts,
                    pageList.totalPages
                }));
            }
            catch(Exception ex)
            {
                return await Task.FromResult(PayLoad<object>.CreatedFail(ex.Message));
            }
        }
    }
}
