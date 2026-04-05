using SV22T1020328.BusinessLayers;
using SV22T1020328.Models.Sales;

namespace SV22T1020328.Admin
{
    public static class OrderItemHelper
    {
        /// <summary>
        /// Cập nhật thông tin chi tiết đơn hàng (số lượng, giá bán) cho một sản phẩm trong đơn hàng.
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="productId"></param>
        /// <param name="quantity"></param>
        /// <param name="salePrice"></param>
        /// <returns></returns>
        public static async Task<bool> UpdateOrderItemAsync(int orderId, int productId, int quantity, decimal salePrice)
        {
            var data = new OrderDetail
            {
                OrderID = orderId,
                ProductID = productId,
                Quantity = quantity,
                SalePrice = salePrice
            };

            return await SalesDataService.UpdateDetailAsync(data);
        }
        /// <summary>
        /// Xóa thông tin chi tiết đơn hàng cho một sản phẩm trong đơn hàng. Điều này sẽ xóa sản phẩm khỏi đơn hàng.
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static async Task<bool> DeleteOrderItemAsync(int orderId, int productId)
        {
            return await SalesDataService.DeleteDetailAsync(orderId, productId);
        }
        
    }
}