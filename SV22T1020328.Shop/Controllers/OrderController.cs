using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020328.BusinessLayers;
using SV22T1020328.Models.Common;
using SV22T1020328.Models.Sales;
using System.Text.RegularExpressions;

namespace SV22T1020328.Shop.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        /// <summary>
        /// Nhập đầu vào trạng thái đơn hàng để lọc trạng thái đơn hàng
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public async Task<IActionResult> Index(OrderStatusEnum status = OrderStatusEnum.All)
        {
            var orders = await GetCustomerOrdersAsync(status);
            ViewBag.Status = status;
            return View(orders);
        }
        /// <summary>
        /// Hiển thị danh sách đơn hàng theo trạng thái được
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Search(OrderStatusEnum status = OrderStatusEnum.All)
        {
            var orders = await GetCustomerOrdersAsync(status);
            return PartialView("OrderSearchResult", orders);
        }
        /// <summary>
        /// Lấy list đơn hàng của khách hàng hiện tại theo trạng thái được chọn
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        private async Task<List<OrderViewInfo>> GetCustomerOrdersAsync(OrderStatusEnum status)
        {
            var userData = User.GetUserData();
            if (userData == null || !int.TryParse(userData.UserId, out int customerId))
                return new List<OrderViewInfo>();

            var input = new OrderSearchInput
            {
                Page = 1,
                PageSize = 0, // lấy tất cả để lọc theo customer chính xác
                Status = status
            };
            var allOrders = await SalesDataService.ListOrdersAsync(input);
            return allOrders.DataItems
                .Where(o => o.CustomerID == customerId)
                .OrderByDescending(o => o.OrderTime)
                .ToList();
        }
        /// <summary>
        /// Giao diện xem chi tiết đơn hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Details(int id)
        {
            var userData = User.GetUserData();
            if (userData == null || !int.TryParse(userData.UserId, out int customerId))
                return RedirectToAction("Login", "Account");

            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null || order.CustomerID != customerId)
                return RedirectToAction("Index");

            var details = await SalesDataService.ListDetailsAsync(id);
            ViewBag.Details = details;

            return View(order);
        }
        /// <summary>
        /// Hủy đơn hàng nếu đơn hàng đang ở trạng thái New hoặc Accepted
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userData = User.GetUserData();
            if (userData == null || !int.TryParse(userData.UserId, out int customerId))
                return RedirectToAction("Login", "Account");

            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null || order.CustomerID != customerId ||
                (order.Status != OrderStatusEnum.New && order.Status != OrderStatusEnum.Accepted))
            {
                TempData["ErrorMessage"] = "Đơn hàng không hợp lệ hoặc không thể hủy.";
                return RedirectToAction("Index");
            }

            bool result = await SalesDataService.CancelOrderByCustomerAsync(id);
            TempData[result ? "SuccessMessage" : "ErrorMessage"] =
                result ? "Hủy đơn hàng thành công." : "Hủy đơn hàng thất bại. Vui lòng thử lại.";

            return RedirectToAction("Index");
        }
    }
}
