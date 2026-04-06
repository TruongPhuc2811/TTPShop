using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020328.BusinessLayers;
using SV22T1020328.Models.Partner;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SV22T1020328.Shop.Controllers
{
    /// <summary>
    /// Xử lý các chức năng liên quan đến tài khoản người dùng như đăng nhập, đăng ký, xem và cập nhật thông tin cá nhân, đổi mật khẩu.
    /// </summary>
    public class AccountController : Controller
    {
        /// <summary>
        /// Liên quan đến tính hợp lệ của email và số điện thoại, 
        ///dùng regex để kiểm tra định dạng đầu vào nhằm đảm bảo tính hợp lệ của dữ liệu người dùng nhập vào.
        /// </summary>
        private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex PhoneRegex = new(@"^(?:\+84|0)\d{9}$", RegexOptions.Compiled);

        /// <summary>
        /// Hiển thị giao diện đăng nhập.
        /// Nếu người dùng đã đăng nhập, chuyển hướng về trang chủ.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            return View();
        }

        /// <summary>
        /// Xử lý đăng nhập
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username, string password)
        {
            ViewBag.Username = username;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("Error", "Vui lòng nhập đầy đủ email và mật khẩu.");
                return View();
            }

            string hashedPassword = CryptHelper.HashMD5(password);
            var userAccount = await SecurityDataService.CustomerAuthorizeAsync(username, hashedPassword);
            if (userAccount == null)
            {
                ModelState.AddModelError("Error", "Email hoặc mật khẩu không đúng.");
                return View();
            }

            var userData = new WebUserData()
            {
                UserId = userAccount.UserId,
                UserName = userAccount.UserName,
                DisplayName = userAccount.DisplayName,
                Email = userAccount.Email,
                Photo = userAccount.Photo
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                userData.CreatePrincipal());

            return RedirectToAction("Index", "Home");
        }
        /// <summary>
        /// Đăng xuất người dùng bằng cách xóa session và cookie xác thực,
        /// sau đó chuyển hướng về trang đăng nhập.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }

        /// <summary>
        /// Giao diện đăng ký
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            return View();
        }
        /// <summary>
        /// Xử lý đăng ký
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="confirmPassword"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(
            string email, 
            string password, string confirmPassword)
            
        {
            ViewBag.Email = email;
            if ( string.IsNullOrWhiteSpace(email)
                || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("Error", "Vui lòng điền đầy đủ các trường bắt buộc.");
                return View();
            }

            if (password != confirmPassword)
            {
                ModelState.AddModelError("Error", "Mật khẩu xác nhận không khớp.");
                return View();
            }

            bool emailExists = !(await PartnerDataService.ValidatelCustomerEmailAsync(email, 0));
            if (emailExists)
            {
                ModelState.AddModelError("Error", "Email này đã được sử dụng. Vui lòng dùng email khác.");
                return View();
            }
            var defaultName = email.Split('@')[0];
            var customer = new Customer
            {
                CustomerName = defaultName,
                ContactName = defaultName,
                Email = email,
                Phone = "",
                Address = "",
                Province = null,
                IsLocked = false
            };

            int newId = await PartnerDataService.AddCustomerAsync(customer);
            if (newId <= 0)
            {
                ModelState.AddModelError("Error", "Đăng ký thất bại. Vui lòng thử lại.");
                return View();
            }

            string hashedPassword = CryptHelper.HashMD5(password);
            await SecurityDataService.ChangeCustomerPasswordAsync(email, hashedPassword);

            TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }
        /// <summary>
        /// Hiển thị thông tin profile
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userData = User.GetUserData();
            if (userData == null || !int.TryParse(userData.UserId, out int customerId))
                return RedirectToAction("Login");

            var customer = await PartnerDataService.GetCustomerAsync(customerId);
            if (customer == null)
                return RedirectToAction("Login");

            return View(customer);
        }
        /// <summary>
        /// Xử lý thông tin profile
        /// </summary>
        /// <param name="customerID"></param>
        /// <param name="customerName"></param>
        /// <param name="contactName"></param>
        /// <param name="email"></param>
        /// <param name="phone"></param>
        /// <param name="address"></param>
        /// <param name="province"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Profile(int customerID, string customerName, string contactName,
            string email, string phone, string address, string province)
        {
            var customer = await PartnerDataService.GetCustomerAsync(customerID);
            
            //Sử dụng ModelState để kiểm soát thông báo lỗi và gửi thông báo lỗi cho view
            if (string.IsNullOrWhiteSpace(customerName))
                ModelState.AddModelError(nameof(customerName), "Vui lòng nhập tên của bạn");
            if (string.IsNullOrWhiteSpace(email))
                ModelState.AddModelError(nameof(email), "Vui lòng cho biết email của bạn");
            else if (!EmailRegex.IsMatch(email))
                ModelState.AddModelError(nameof(email), "Email không đúng định dạng");
            else if (!(await PartnerDataService.ValidatelCustomerEmailAsync(email, customerID)))
                ModelState.AddModelError(nameof(email), "Email này đã có người sử dụng");
            if (string.IsNullOrWhiteSpace(phone))
                ModelState.AddModelError(nameof(phone), "Vui lòng nhập số điện thoại của bạn");
            else if (!PhoneRegex.IsMatch(phone))
                ModelState.AddModelError(nameof(phone), "Số điện thoại không đúng định dạng (VD: 0xxxxxxxxx hoặc +84xxxxxxxxx)");

            if (string.IsNullOrWhiteSpace(province))
                ModelState.AddModelError(nameof(province), "Vui lòng chọn tỉnh/thành");
            if (!ModelState.IsValid)
                return View(customer);
            //Điều chỉnh lại các giá trị dữ liệu khác theo qui định/ qui ước của APP
            if (string.IsNullOrWhiteSpace(contactName)) contactName = "";
            if (string.IsNullOrWhiteSpace(phone)) phone = "";
            if (string.IsNullOrWhiteSpace(address)) address = "";
            
            if (customer == null)
                return RedirectToAction("Login");

            customer.CustomerName = customerName;
            customer.ContactName = contactName;
            customer.Phone = phone;
            customer.Address = address;
            customer.Province = province;

            bool result = await PartnerDataService.UpdateCustomerAsync(customer);
            if (result)
                ViewBag.SuccessMessage = "Cập nhật thông tin thành công.";
            else
                ModelState.AddModelError("Error", "Cập nhật thất bại. Vui lòng thử lại.");

            return View(customer);
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            var userData = User.GetUserData();
            if (userData == null || string.IsNullOrWhiteSpace(userData.UserName))
                return RedirectToAction("Login");

            if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword)
                || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ModelState.AddModelError("Error", "Vui lòng nhập đầy đủ thông tin.");
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("Error", "Mật khẩu xác nhận không khớp.");
                return View();
            }

            string oldHash = CryptHelper.HashMD5(oldPassword);
            var account = await SecurityDataService.CustomerAuthorizeAsync(userData.UserName, oldHash);
            if (account == null)
            {
                ModelState.AddModelError("Error", "Mật khẩu cũ không đúng.");
                return View();
            }

            string newHash = CryptHelper.HashMD5(newPassword);
            bool success = await SecurityDataService.ChangeCustomerPasswordAsync(userData.UserName, newHash);
            if (!success)
            {
                ModelState.AddModelError("Error", "Đổi mật khẩu thất bại. Vui lòng thử lại.");
                return View();
            }

            ViewBag.SuccessMessage = "Đổi mật khẩu thành công.";
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
