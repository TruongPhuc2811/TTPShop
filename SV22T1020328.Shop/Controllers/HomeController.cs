using Microsoft.AspNetCore.Mvc;
using SV22T1020328.BusinessLayers;
using SV22T1020328.Models.Sales;
using SV22T1020328.Shop.Models;
using System.Diagnostics;

namespace SV22T1020328.Shop.Controllers
{
    /// <summary>
    /// Trang chủ 
    /// </summary>
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var topProducts = new List<OrderDetailViewInfo>();

            var completedOrders = await SalesDataService.ListOrdersAsync(new OrderSearchInput
            {
                Page = 1,
                PageSize = 0,
                SearchValue = "",
                Status = OrderStatusEnum.Completed
            });

            if (completedOrders.DataItems.Any())
            {
                var detailTasks = completedOrders.DataItems.Select(o => SalesDataService.ListDetailsAsync(o.OrderID));
                var allDetails = (await Task.WhenAll(detailTasks)).SelectMany(x => x);

                topProducts = allDetails
                    .GroupBy(d => new { d.ProductID, d.ProductName, d.Photo })
                    .Select(g => new OrderDetailViewInfo
                    {
                        ProductID = g.Key.ProductID,
                        ProductName = g.Key.ProductName,
                        Photo = g.Key.Photo,
                        Quantity = g.Sum(x => x.Quantity),
                        SalePrice = 0 // sẽ gán lại theo giá hiện tại
                    })
                    .OrderByDescending(x => x.Quantity)
                    .Take(8)
                    .ToList();

                // Gán giá hiện tại từ bảng Products
                var productTasks = topProducts.Select(async p => new
                {
                    p.ProductID,
                    Product = await CatalogDataService.GetProductAsync(p.ProductID)
                });
                var currentProducts = await Task.WhenAll(productTasks);

                foreach (var item in topProducts)
                {
                    var current = currentProducts.FirstOrDefault(x => x.ProductID == item.ProductID)?.Product;
                    if (current != null)
                    {
                        item.SalePrice = current.Price;
                        if (string.IsNullOrWhiteSpace(item.Photo))
                            item.Photo = current.Photo ?? "nophoto.png";
                    }
                }
            }

            ViewBag.TopProducts = topProducts;
            return View();
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
