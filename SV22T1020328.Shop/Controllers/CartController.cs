using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020328.BusinessLayers;
using SV22T1020328.Models.Sales;

namespace SV22T1020328.Shop.Controllers
{
    public class CartController : Controller
    {
        /// <summary>
        /// Giao diện xem giỏ hàng hiện tại. 
        /// Người dùng có thể cập nhật số lượng hoặc xóa sản phẩm khỏi giỏ hàng từ đây.
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var cart = ShoppingCartHelper.GetShoppingCart();
            return View(cart);
        }
        /// <summary>
        /// Thêm sản phẩm vào giỏ hàng. 
        /// Nếu sản phẩm đã tồn tại trong giỏ, sẽ cập nhật số lượng.
        /// </summary>
        /// <param name="productID"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productID, int quantity = 1)
        {
            var product = await CatalogDataService.GetProductAsync(productID);
            if (product == null)
                return Json(new { success = false, message = "Sản phẩm không tồn tại." });

            if (quantity <= 0) quantity = 1;

            var item = new OrderDetailViewInfo
            {
                ProductID = productID,
                ProductName = product.ProductName,
                Unit = product.Unit,
                Photo = product.Photo ?? "nophoto.png",
                Quantity = quantity,
                SalePrice = product.Price
            };

            ShoppingCartHelper.AddItemToCart(item);
            int cartCount = ShoppingCartHelper.GetCartItemCount();

            return Json(new { success = true, cartCount, message = "Đã thêm vào giỏ hàng." });
        }
        /// <summary>
        /// Cập nhật số lượng hoặc giá bán của một sản phẩm trong giỏ hàng.
        /// </summary>
        /// <param name="productID"></param>
        /// <param name="quantity"></param>
        /// <param name="salePrice"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult UpdateCart(int productID, int quantity, decimal salePrice)
        {
            if (quantity <= 0)
                ShoppingCartHelper.RemoveItemFromCart(productID);
            else
                ShoppingCartHelper.UpdateItemInCart(productID, quantity, salePrice);

            return RedirectToAction("Index");
        }
        /// <summary>
        /// Xóa một sản phẩm khỏi giỏ hàng dựa trên productID.
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult RemoveFromCart(int productID)
        {
            ShoppingCartHelper.RemoveItemFromCart(productID);
            return RedirectToAction("Index");
        }
        /// <summary>
        /// Xóa toàn bộ giỏ hàng, thường được gọi khi người dùng muốn bắt đầu lại hoặc sau khi đặt hàng thành công.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ClearCart()
        {
            ShoppingCartHelper.ClearCart();
            return RedirectToAction("Index");
        }
        /// <summary>
        /// Giao diện xác nhận đơn hàng bao gồm thông tin giao hàng, 
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cart = ShoppingCartHelper.GetShoppingCart();
            if (!cart.Any())
                return RedirectToAction("Index");

            var userData = User.GetUserData();
            if (userData == null || !int.TryParse(userData.UserId, out int customerId))
                return RedirectToAction("Login", "Account");

            var customer = await PartnerDataService.GetCustomerAsync(customerId);
            ViewBag.Customer = customer;
            ViewBag.Cart = cart;

            return View(cart);
        }
        /// <summary>
        /// Xử lý đặt hàng
        /// </summary>
        /// <param name="customerName"></param>
        /// <param name="phone"></param>
        /// <param name="deliveryProvince"></param>
        /// <param name="deliveryAddress"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Checkout(string customerName, string phone, string deliveryProvince, string deliveryAddress)
        {
            var cart = ShoppingCartHelper.GetShoppingCart();
            if (!cart.Any())
                return RedirectToAction("Index");

            var userData = User.GetUserData();
            if (userData == null || !int.TryParse(userData.UserId, out int customerId))
                return RedirectToAction("Login", "Account");

            var customer = await PartnerDataService.GetCustomerAsync(customerId);
            if (customer == null)
                return RedirectToAction("Login", "Account");

            customerName = customerName?.Trim() ?? "";
            phone = phone?.Trim() ?? "";
            deliveryProvince = deliveryProvince?.Trim() ?? "";
            deliveryAddress = deliveryAddress?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(customerName))
                ModelState.AddModelError(nameof(customerName), "Vui lòng nhập tên người nhận.");
            if (string.IsNullOrWhiteSpace(phone))
                ModelState.AddModelError(nameof(phone), "Vui lòng nhập số điện thoại.");
            if (string.IsNullOrWhiteSpace(deliveryProvince))
                ModelState.AddModelError(nameof(deliveryProvince), "Vui lòng chọn tỉnh/thành.");
            if (string.IsNullOrWhiteSpace(deliveryAddress))
                ModelState.AddModelError(nameof(deliveryAddress), "Vui lòng nhập địa chỉ giao hàng.");

            if (!ModelState.IsValid)
            {
                ViewBag.Customer = customer;
                ViewBag.Cart = cart;
                ViewBag.CustomerName = customerName;
                ViewBag.Phone = phone;
                ViewBag.DeliveryProvinces = deliveryProvince;
                ViewBag.DeliveryAddress = deliveryAddress;
                return View(cart);
            }

            // Cập nhật hồ sơ khách hàng nếu mới tạo tài khoản chưa có thông tin
            customer.CustomerName = customerName;
            customer.ContactName = string.IsNullOrWhiteSpace(customer.ContactName) ? customerName : customer.ContactName;
            customer.Phone = phone;
            customer.Province = deliveryProvince;
            customer.Address = deliveryAddress;

            bool updated = await PartnerDataService.UpdateCustomerAsync(customer);
            if (!updated)
            {
                ViewBag.Customer = customer;
                ViewBag.Cart = cart;
                ModelState.AddModelError("Error", "Không thể cập nhật thông tin khách hàng.");
                return View(cart);
            }

            int orderID = await SalesDataService.AddOrderAsync(customerId, deliveryProvince, deliveryAddress);
            if (orderID <= 0)
            {
                ViewBag.Customer = customer;
                ViewBag.Cart = cart;
                ModelState.AddModelError("Error", "Đặt hàng thất bại. Vui lòng thử lại.");
                return View(cart);
            }

            foreach (var item in cart)
            {
                var detail = new OrderDetail
                {
                    OrderID = orderID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    SalePrice = item.SalePrice
                };
                await SalesDataService.AddDetailAsync(detail);
            }

            ShoppingCartHelper.ClearCart();
            TempData["SuccessMessage"] = "Đặt hàng thành công! Đơn hàng của bạn đang được xử lý.";
            return RedirectToAction("Details", "Order", new { id = orderID });
        }
    }
}
