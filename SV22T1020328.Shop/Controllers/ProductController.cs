using Microsoft.AspNetCore.Mvc;
using SV22T1020328.BusinessLayers;
using SV22T1020328.Models.Catalog;
using SV22T1020328.Models.Common;

namespace SV22T1020328.Shop.Controllers
{
    public class ProductController : Controller
    {
        private const string PRODUCTSEARCHINPUT = "ProductSearchInput";
        /// <summary>
        /// Nhập đâu vào tìm kiếm sản phẩm
        /// </summary>
        /// <returns></returns>
        public IActionResult  Index()
        {
            var input = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCTSEARCHINPUT);
            if(input ==null) input = new ProductSearchInput
            {
                Page = 1,
                PageSize = ApplicationContext.PageSize,
                SearchValue = "",
                CategoryID = 0,
                MinPrice = 0,
                MaxPrice = 0
            };
            return View(input);
        }
        /// <summary>
        /// Tìm kiếm sản phẩm và hiển thị danh sách sản phẩm
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Search(ProductSearchInput input)
        {

            var result = await CatalogDataService.ListProductsAsync(input);
            ApplicationContext.SetSessionData(PRODUCTSEARCHINPUT, input);
            return View(result);
        }
        /// <summary>
        /// Chi tiết sản phẩm
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Details(int id)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            if (product == null)
                return RedirectToAction("Index");

            var photos = await CatalogDataService.ListPhotosAsync(id);
            var attributes = await CatalogDataService.ListAttributesAsync(id);

            ViewBag.Photos = photos;
            ViewBag.Attributes = attributes;

            return View(product);
        }
    }
}
