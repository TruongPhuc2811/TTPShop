using SV22T1020328.Models.Sales;

namespace SV22T1020328.Shop
{
    public static class ShoppingCartHelper
    {
        private const string CART = "ShoppingCart";
        /// <summary>
        /// Lấy danh sách các mặt hàng trong giỏ hàng từ session. 
        /// Nếu chưa có thì tạo mới một giỏ hàng rỗng và lưu vào session trước khi trả về.
        /// </summary>
        /// <returns></returns>
        public static List<OrderDetailViewInfo> GetShoppingCart()
        {
            var cart = ApplicationContext.GetSessionData<List<OrderDetailViewInfo>>(CART);
            if (cart == null)
            {
                cart = new List<OrderDetailViewInfo>();
                ApplicationContext.SetSessionData(CART, cart);
            }
            return cart;
        }
        /// <summary>
        /// Lấy tổng số lượng mặt hàng trong giỏ hàng,
        /// bằng cách cộng dồn số lượng của từng mặt hàng trong giỏ
        /// </summary>
        /// <returns></returns>
        public static int GetCartItemCount()
        {
            return GetShoppingCart().Sum(i => i.Quantity);
        }
        /// <summary>
        /// Lấy thông tin mặt hàng trong giỏ hàng theo mã sản phẩm.
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static OrderDetailViewInfo? GetCartItem(int productId)
        {
            return GetShoppingCart().Find(m => m.ProductID == productId);
        }
        /// <summary>
        /// Thêm mặt hàng vào giỏ hàng
        /// </summary>
        /// <param name="item"></param>
        public static void AddItemToCart(OrderDetailViewInfo item)
        {
            var cart = GetShoppingCart();
            var existingItem = cart.Find(m => m.ProductID == item.ProductID);
            if (existingItem == null)
            {
                cart.Add(item);
            }
            else
            {
                existingItem.Quantity += item.Quantity;
                existingItem.SalePrice = item.SalePrice;
            }
            ApplicationContext.SetSessionData(CART, cart);
        }
        /// <summary>
        /// Cập nhật thông tin mặt hàng trong giỏ hàng, bao gồm số lượng và giá bán.
        /// </summary>
        /// <param name="productID"></param>
        /// <param name="quantity"></param>
        /// <param name="salePrice"></param>
        public static void UpdateItemInCart(int productID, int quantity, decimal salePrice)
        {
            var cart = GetShoppingCart();
            var item = cart.Find(m => m.ProductID == productID);
            if (item != null)
            {
                item.Quantity = quantity;
                item.SalePrice = salePrice;
                ApplicationContext.SetSessionData(CART, cart);
            }
        }
        /// <summary>
        /// Xóa mặt hàng khỏi giỏ hàng theo mã mặt hàng.
        /// </summary>
        /// <param name="productID">Mã mặt hàng</param>
        public static void RemoveItemFromCart(int productID)
        {
            var cart = GetShoppingCart();
            int index = cart.FindIndex(m => m.ProductID == productID);
            if (index >= 0)
            {
                cart.RemoveAt(index);
                ApplicationContext.SetSessionData(CART, cart);
            }
        }
        /// <summary>
        /// Xóa toàn bộ giỏ hàng
        /// </summary>
        public static void ClearCart()
        {
            ApplicationContext.SetSessionData(CART, new List<OrderDetailViewInfo>());
        }
    }
}
