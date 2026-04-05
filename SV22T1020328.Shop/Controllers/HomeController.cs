using Microsoft.AspNetCore.Mvc;
using SV22T1020328.BusinessLayers;
using SV22T1020328.Models.Catalog;
using SV22T1020328.Models.Common;
using SV22T1020328.Shop.Models;
using System.Diagnostics;

namespace SV22T1020328.Shop.Controllers
{
    /// <summary>
    /// Trang chủ 
    /// </summary>
    public class HomeController : Controller
    {
        private const string HOMEPRODUCTSEARCHINPUT = "HomeProductSearchInput";
        /// <summary>
        /// Nhập đầu vào tìm kiếm
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            var input = ApplicationContext.GetSessionData<ProductSearchInput>(HOMEPRODUCTSEARCHINPUT);
            if (input == null)
            {
                input = new ProductSearchInput
                {
                    Page = 1,
                    PageSize = 8,
                    SearchValue = "",
                    CategoryID = 0,
                    SupplierID = 0,
                    MinPrice = 0,
                    MaxPrice = 0
                };
            }

            var categories = await CatalogDataService.ListCategoriesAsync(new PaginationSearchInput { Page = 1, PageSize = 0 });
            ViewBag.Categories = categories.DataItems;

            return View(input);
        }
        /// <summary>
        /// Tìm kiếm và hiển thị dữ liệu
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Search(ProductSearchInput input)
        {
            input.Page = input.Page < 1 ? 1 : input.Page;
            input.PageSize = 8; 
            ApplicationContext.SetSessionData(HOMEPRODUCTSEARCHINPUT, input);
            var data = await CatalogDataService.ListProductsAsync(input);
            return View(data);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
