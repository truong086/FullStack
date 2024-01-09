﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using thuongmaidientus1.AccountService;
using thuongmaidientus1.Common;
using thuongmaidientus1.Migrations;
using thuongmaidientus1.Models;

namespace thuongmaidientus1.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _category;
        public CategoryController(ICategoryService category)
        {
            _category = category;
        }

        [HttpGet]
        [Route(nameof(FindAll))]
        public async Task<PayLoad<object>> FindAll(string? name, int page = 1, int pageSize = 20)
        {
            return await _category.FindAll(name, page, pageSize);
        }

        [HttpGet]
        [Route(nameof(FindOne))]
        public async Task<PayLoad<Category>> FindOne(int id)
        {
            return await _category.FindOneId(id);
        }

        [HttpPost]
        [Route(nameof(AddCategory))]
        public async Task<PayLoad<Category>> AddCategory(Category category)
        {
            return await _category.AddCategory(category);
        }

        [HttpPut]
        [Route(nameof(UpdateCategory))]
        public async Task<PayLoad<Category>> UpdateCategory(int id, Category category, string? name)
        {
            return await _category.UpdateCategory(id, category, name);
        }

        [HttpDelete]
        [Route(nameof(DeleteCategory))]
        public async Task<PayLoad<string>> DeleteCategory(IList<string> id)
        {
            return await _category.DeleteCategory(id);
        }
    }
}
