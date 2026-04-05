# Tạo Solution
Tạo solution có tên SV.<Mã Sinh Viên>
BỔ sung cho solution này các project sau:
- <Tên solution>.Admin: project có dạng ASP.NET Core MVC
- <Tên solution>.Shop: project có dạng ASP.NET Core MVC
- <Tên solution>.Models: project có dạng Class Library
- <Tên solution>.DomainModels: project có dạng Class Library
- <Tên solution>.BusinessLayers: project có dạng Class Library
# Thiết kế Layout cho SV.Admin
- Sử dụng theme AdminLTE4
- THiết kế file _Layout.cshtml
	+ Thiết kế file _Header.cshtml,_SideBar.cshtml,_Footer.cshtml
	+ Sử dụng @RenderBody() để hiển thị nội dung chính có thể thay đổi
- Liên kết các chức năng dự kiến trên Layout(Header,SideBar)
# Các controller, Action dự kiến cho chứ năng
## Account : Các chức năng liên quan đến tài khoản (Cá nhân)
- Account/Login
- Account/Logout
- Account/ChangePassword
## Supplier: Các chức năng liên quan đến quản lý nhà cung cấp
- Supplier/Index: trang
	- Hiển thị danh sách nhà cung cấp dưới dạng bảng, có phân trang
	- Tìm kiếm nhà cung cấp theo tên
	- Điều hướng đến các chức năng bổ sung, cập nhật, xóa nhà cung cấp
- Supplier/Create:
- Supplier/Edit/{id}
- Supplier/Delete/{id}

## Customer: Các chức năng liên quan đến quản lý khách hàng
- Customer/Index
- Customer/Create
- Customer/Edit/{id}
- Customer/Delete/{id}
- Customer/ChangePassword/{id}

## Shipper: Các chức năng liên quan đến quản lý người giao hàng
- Shipper/Index
- Shipper/Create
- Shipper/Edit/{id}
- Shipper/Delete/{id}
## Employee: Các chức năng liên quan đến Nhân viên
- Employee/Index
- Employee/Create
- Employee/Edit/{id}
- Employee/Delete/{id}
- Employee/ChangePassword/{id}
- Employee/ChangeRole/{id}
## Category: Các chức năng liên quan đến Loại hàng
- Category/Index
- Category/Create
- Category/Edit/{id}
- Category/Delete/{id}
## Product: Các chức năng liên quan đến Mặt hàng
- Product/Index
- Product/Detail/{id}
- Product/Create
- Product/Edit/{id}
- Product/Delete/{id}
- Product/ListAttribute/{id}
- Product/AddAttribute/{id}
- Product/EditAttribute/{id}?attributeId={attributeId}
- Product/DeleteAttribute/{id}?attributeId={attributeId}
- Product/ListPhotos/{id}
- Product/AddPhoto/{id}
- Product/EditPhoto/{id}?photoId={photoId}
- Product/DeletePhoto/{id}?photoId={photoId}
## Order: Các chức năng liên quan đến Đơn hàng 
## Xây dựng Models
- Data Dictionary - Province
- Partner 
	+ Supplier
 	+ Shipper
	+ Customers
- HR + Employee
- catalog 
 + Category
	+ Product
		- Attributev
		- Photo
- Sales
	+ Order
		- OrderDetail
		- OrderStatus -> Enum
- Security
	+ UserAccount
- Common

- Tạo 2 thư mục DataLayers:
 - Interfaces: Khai báo các interface định nghĩa các phép xử lý dữ liệu cần cài đặt.
	- SQL Server: Cài đặt các phép xử lý dữ liệu dựa trên các định nghĩa từ interface.
Cần làm: Xác định xem cần cài đặt những chức năng gì ở tầng dữ liệu để đáp ứng được yêu cầu của bài toán?
	- với các dữ liệu như supplier, customer, Shipper ,employee, Category:
		+ Tìm kiếm, lấy dữ liệu phân sang:
			Đầu vào như nhau: Page? Page Size? SearchValue?
			Đầu ra khác nhau kiểu dữ liệu
		+ Lấy một bản ghi dựa vào id
		+ Bổ sung 1 bản ghi
		+ Cập nhật 1 bản ghi
		+ Xóa 1 bản ghi
		+ Kiểm tra xem một bản ghi nào đó có dữ liệu liên quan hay không ?
--> Viết interface để mô tả các chức năng cần làm
		-Định nghĩa các phép xử lý dữ liệu cần cài đặt cho 1 entity T nào đó
		+TÌm kiếm và lấy dữ liệu dưới dạng phân trang Task<PageResult> <T> ListAsync(PaginationSearchInput input)
		+Lấy một bản ghi dữ liệu dựa vào ID  Tast<T?> GetAsync(int id)
		+ Bổ sung một bản ghi vào một bảng trong CSDL Task<int> AddAsync (T data)

		+ Cập nhật một bản ghi trong CSDL Task<bool> UpdateAsync (T data)
		+ Xóa một bảng ghi trong CSDL Task<book> DeleteAsync(int id)
		+ Kiểm tra một bản ghi nào đó có dữ liệu hay không Task<bool> IsUsed (int id);
--> Chưa đủ cho customers, Employee vì thiếu chức năng kiểm tra email có bị trùng hay không
		+ Tạo thêm interface IcustomerRepository : IgenericRepository<Customer>
			+ Task<bool> ValidateEmail(String email, int id =0) Kiểm tra email của 1 khách hàng có hợp lệ hay không?, nếu id =0 kiểm tra email của Kh mới, ngược lại kiểm tra email của khách hàng có mã là ID
				- Định nghĩa cho mặt hàng:
					+ Tìm kiếm và lấy dữ liệu dưới dạng phân trang ProductSearchInput : PaginationSearchInput
						+ Viết interface các phép xử lý dữ liệu liên quan tới mặt hàng IProductRepository; Task<PageResult<Product>> ListAsync(ProductSearchInput input)>
							+ Có chức năng tìm kiếm và lấy danh sách mặt hàng dưới dạng phân trang
					+ Bổ sung mặt hàng mới, trả về id của mặt hàng được bổ sung Task<int> AddAsync(Product data)
					+	Cập nhật 1 mặt hàng Task<bool> UpdateAsync(Product data)

					+ Xóa 1 mặt hàng dựa vào mã mặt hàng Task<bool> DeleteAsync(int ProductID)
					+ Kiêm tra một mặt hàng đã bán hay chưa Task<bool> IsUsed(int ProductID)
					+ Lấy danh sách các thuộc tính của mặt hàng Task<List<ProductAttribute>> ListAttributesAsync(int ProductID)>
					+ Lấy thông tin một thuộc tính Task<ProductAttribute> GetAttributeAsync(long AttributeID)
					+ Bổ sung một thuộc tính cho mặt hàng Task<long> AddAttributeAsync(ProductAttribute attribute)
					+ Cập nhật thông tin một thuộc tính Task<bool> UpdateAttributeAsync(ProductAttribute attribute)
					+ Xóa một thuộc tính Task<bool> DeleteAttributeAsync(long AttributeID)
					+ Lấy danh sách các hình ảnh của mặt hàng Task<List<ProductPhoto>> ListPhotosAsync(int ProductID)
					+ Lấy một ảnh của mặt hàng Task<ProductPhoto> GetPhotoAsync(long PhotoID)
					+ Bổ sung ảnh cho một mặt hàng Task<long> AddPhotoAsync(ProductPhoto photo)
					+ Cập nhật ảnh cho một mặt hàng Task<bool> UpdatePhotoAsync(ProductPhoto photo)
					+ Xóa một ảnh của mặt hàng Task<bool> DeletePhotoAsync(long PhotoID)
				- Định nghĩa cho đơn hàng (Đơn hàng không thể chứa hết dữ liệu khách hàng(Họ tên, sdt), nhân viên(tên nhân viên) -> Lớp OrderSearchInfo : Order --> Gọi là DTO (Data Transfer Object) để chứa thông tin cần thiết của đơn hàng khi hiển thị trên giao diện)
					+ Tìm kiếm và lấy dữ liệu dưới dạng phân trang OrderSearchInput : PaginationSearchInput
							+ Viết interface các phép xử lý dữ liệu liên quan tới đơn hàng Có chức năng tìm kiếm và lấy danh sách đơn hàng
						dưới dạng phân trang IOrderRepository; Task<PageResult<Order>> ListAsync(OrderSearchInput input)>
					+ Lấy thông tin một đơn hàng dựa vào mã đơn hàng Task<Order> GetAsync(int OrderID)
					+ Bổ sung một đơn hàng mới, trả về id của đơn hàng được bổ sung Task<int> AddAsync(Order data)
					+	Cập nhật 1 đơn hàng Task<bool> UpdateAsync(Order data)
					+ Xóa 1 đơn hàng dựa vào mã đơn hàng Task<bool> DeleteAsync(int OrderID)
					+ Kiêm tra một đơn hàng đã bán hay chưa Task<bool> IsUsed(int OrderID)
					+ CÒn nữa
				- Định nghĩa các phép xử lý dữ liệu trên từ điển dữ liệu có kiểu là T interface IDataDictionaryRepository <T> where : T:class
					+ Lấy danh sách dữ liệu từ điển Task<List<T>> ListAsync()
				- Định nghĩa các phép dữ liệu liên quan đến tài khoản  IUserAccountRepository 
					+ Tack<UserAccount> GetAsync(string username,string password);
					+ Đổi tài khoản Task<bool> ChangePassword(string username, string newPassword);)

	- Cài đặt packpage Newtonsoft.Json để hỗ trợ chuyển đổi dữ liệu giữa JSON và C# Object
	- Tạo thư mục AppCodes trong Admin
	- Copy file ApplicationDbContext.cs vào thư mục AppCodes trong Admin 
	- Sửa code của file Program.cs theo code mẫu
	- BỔ sung cấu hình Connectionstrings vào file appsettings.json
	- BỔ sung lớp Configuration cho BusinessLayers
	- Bổ sung lớp DictionaryDataService cho BusinessLayers để cung cấp dữ liệu cho các dữ liệu từ điển như Province, Country, Unit, ...
	- Bổ sung lớp PartnerDataService cho BusinessLayers để cung cấp dữ liệu cho các đối tác như Supplier, Shipper, Customer
	- Bổ sung các lớp Service khác cho BusinessLayers để cung cấp dữ liệu cho các chức năng khác như Employee, Category, Product, Order, ...
	- 

		- Lưu ý: Khi Action truyền dữ liệu về cho View:
          + Phải biết được kiểu của dữ liệu mà Action truyền cho view là gi
			+ Trong view phải báo kiểu dữ liệu nhận được từ Action bằng cách sử dụng cú pháp @model KiểuDữLiệu
			+ Trong view, dữ liệu do Action truyền ra được lưu trong thuộc tính có tên là Model, (Thông qua thuộc tính Model để lấy được dữu liệu cho Action truyền ra)
			+  asp-for="SearchValue": Gán cho thuộc tính SearchValue của Model có giá trị được nhập vào trong ô tìm kiếm, khi người dùng nhập vào ô tìm kiếm thì giá trị đó sẽ được gán cho thuộc tính SearchValue của Model, và khi người dùng nhấn nút tìm kiếm thì giá trị của thuộc tính SearchValue sẽ được gửi về Action để thực hiện tìm kiếm.
                hoặc có thể viết  name="searchValue" value="@Model.SearchValue"
			+ Dùng Viewbag để truyền dữ liệu đơn giản từ Action sang View, nhưng cách này không được khuyến khích vì không có kiểu dữ liệu
		- Session là gì?
			+ Khi thực hiện tìm kiếm (Search): Lưu lại điều kiện tìm kiếm trong Session
			+ Khi vào trang danh sách khách hàng (Index): Kiểm tra nếu trong session có điều kiện tìm kiếm đang lưu lại thì sử dụng, không có thì tạo mới.
		- Không lưu pagesize trong controler -> Hard code -> khó cho chỉnh sửa
				-> Giải pháp: Định nghĩa trong appsetting.json -> viết hàm trả về PageSize trong ApplicationContext
		Sử dụng SelectListHelper để đọc dữ liệu từ db hiển thị lên combobox
		- Thiết kế thêm controller SaveData để bổ sung hoặc cập nhật dữ liệu



	- Cơ chế chung (Đăng nhập, kiểm tra quyền)
		+ 1.Người sử dụng cung cấp thông tin để kiểm tra xem có được phép truy cập không ? (username+password/Vân tay/Khuôn mặt)
		+ 2.Hệ thống kiểm tra xem có hợp lệ không? Nếu hợp lệ thì cấp cho client một chứng nhật (principal). Client lưu giữ dưới dạng cookie (Chuỗi mã hóa principal)
		+ 3.Client "Xuất trình" chứng nhận (Cookie) cho Server mỗi khi thực hiện request. (Nằm trong header của lời gọi)
		+ 4.Server dựa vào cookie đê kiểm tra xem người dùng có được phép sử dụng chức năng không
	- 2 Thao tác:
		+ Authentication: 2
		+ Authorization: 4
	- Làm chức năng đăng nhập, đăng ký
	- Thêm Authorize vào các controller để kiểm tra quyền truy cập
	- Trong View hoặc trong các Action (Cửa controller), thông qua thuộc tính User để lấy được principal (Giấy chứng nhận) của người dùng đã đăng nhập, kiểm tra xem đã đăng nhập hay chưa
	- Về nhà: Hoàn thiện log in lấy từ database, Chức năng đổi mật khẩu của người dùng, của người dùng có thể là khách hàng hoặc nhân viên, nhưng chỉ có nhân viên mới có thể đổi quyền của nhân viên khác, phân quyền


	- Lập đơn hàng :+Cart được lưu ở Session, làm cho Shop thì Cart nên lưu vào database => ShoppingCartDataService để lưu database
					+ Thiết kế lớp ShoppingCartHelper để xử lý giỏ hàng 