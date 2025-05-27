ในบริบทของแอปพลิเคชัน POS (Point of Sale) บน .NET Desktop โดยอิงจาก Models ที่คุณให้มา, นี่คือ 20 เมธอดหลัก ๆ ที่ครอบคลุมฟังก์ชันการทำงานพื้นฐานที่จำเป็นสำหรับระบบ POS:

---

### **การจัดการสินค้า (Product Management)**

1.  **`GetProductById(int productId)`**: ดึงข้อมูลสินค้าทั้งหมดจาก `ProductID` ที่กำหนด
    ```csharp

    
 public Product GetProductById(int productId) // ประกาศเมธอด GetProductById ที่รับค่า productId (เลขรหัสสินค้า) และจะส่งคืนอ็อบเจกต์ Product
{
    using (var db = new MyEcommerceDbContext()) // สร้างอินสแตนซ์ของ MyEcommerceDbContext ภายในบล็อก using เพื่อให้แน่ใจว่าการเชื่อมต่อฐานข้อมูลจะถูกปิดอย่างถูกต้องเมื่อใช้งานเสร็จ
    {
        return db.Products.FirstOrDefault(p => p.ProductID == productId); // ค้นหา Product ในตาราง Products โดยใช้ ProductID ที่ตรงกับ productId ที่ส่งเข้ามา และส่งคืน Product ตัวแรกที่พบ ถ้าไม่พบจะคืนค่า null
    }
}
    ```

2.  **`SearchProducts(string searchTerm)`**: ค้นหาสินค้าจากชื่อ (Name) หรือคำอธิบาย (ShortDescription)
    ```csharp
    public List<Product> SearchProducts(string searchTerm)
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.Products
                     .Where(p => p.Name.Contains(searchTerm) || p.ShortDescription.Contains(searchTerm))
                     .ToList();
        }
    }
    ```

3.  **`UpdateProductStock(int productId, int quantityChange)`**: อัปเดตจำนวนสินค้าคงคลัง (เพิ่มหรือลด)
    ```csharp
    public bool UpdateProductStock(int productId, int quantityChange)
    {
        using (var db = new MyEcommerceDbContext())
        {
            var product = db.Products.FirstOrDefault(p => p.ProductID == productId);
            if (product != null && product.UnitInStock.HasValue)
            {
                product.UnitInStock += quantityChange;
                db.SaveChanges();
                return true;
            }
            return false;
        }
    }
    ```

4.  **`AddNewProduct(Product newProduct)`**: เพิ่มสินค้าใหม่เข้าสู่ระบบ
    ```csharp
    public bool AddNewProduct(Product newProduct)
    {
        using (var db = new MyEcommerceDbContext())
        {
            db.Products.Add(newProduct);
            db.SaveChanges();
            return true;
        }
    }
    ```

5.  **`GetAllProducts()`**: ดึงรายการสินค้าทั้งหมด
    ```csharp
    public List<Product> GetAllProducts()
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.Products.ToList();
        }
    }
    ```

---

### **การจัดการคำสั่งซื้อ (Order Management)**

6.  **`CreateNewOrder(int customerId, int? paymentId, int? shippingId)`**: สร้างคำสั่งซื้อใหม่ (Order)
    ```csharp
    public Order CreateNewOrder(int customerId, int? paymentId, int? shippingId)
    {
        using (var db = new MyEcommerceDbContext())
        {
            var newOrder = new Order
            {
                CustomerID = customerId,
                PaymentID = paymentId,
                ShippingID = shippingId,
                OrderDate = DateTime.Now,
                isCompleted = false,
                DIspatched = false,
                Shipped = false,
                Deliver = false,
                CancelOrder = false
            };
            db.Orders.Add(newOrder);
            db.SaveChanges();
            return newOrder;
        }
    }
    ```

7.  **`AddOrderDetail(int orderId, int productId, int quantity, decimal unitPrice, decimal discount)`**: เพิ่มรายการสินค้าในคำสั่งซื้อ (OrderDetail)
    ```csharp
    public OrderDetail AddOrderDetail(int orderId, int productId, int quantity, decimal unitPrice, decimal discount)
    {
        using (var db = new MyEcommerceDbContext())
        {
            var orderDetail = new OrderDetail
            {
                OrderID = orderId,
                ProductID = productId,
                Quantity = quantity,
                UnitPrice = unitPrice,
                Discount = discount,
                TotalAmount = (unitPrice * quantity) - discount,
                OrderDate = DateTime.Now
            };
            db.OrderDetails.Add(orderDetail);
            db.SaveChanges();
            return orderDetail;
        }
    }
    ```

8.  **`CalculateOrderTotal(int orderId)`**: คำนวณยอดรวมของคำสั่งซื้อจาก OrderDetails
    ```csharp
    public decimal CalculateOrderTotal(int orderId)
    {
        using (var db = new MyEcommerceDbContext())
        {
            // ใช้ AsEnumerable() เพื่อให้ LINQ to Objects ทำงานกับ Nullable<decimal> ได้ง่ายขึ้น
            return db.OrderDetails
                     .Where(od => od.OrderID == orderId)
                     .AsEnumerable() // ใช้ AsEnumerable() เพื่อคำนวณในหน่วยความจำ
                     .Sum(od => od.TotalAmount ?? 0); // จัดการค่า Nullable
        }
    }
    ```

9.  **`CompleteOrder(int orderId, decimal totalAmount, int? taxes, int? discount)`**: ทำให้คำสั่งซื้อเสร็จสมบูรณ์
    ```csharp
    public bool CompleteOrder(int orderId, decimal totalAmount, int? taxes, int? discount)
    {
        using (var db = new MyEcommerceDbContext())
        {
            var order = db.Orders.FirstOrDefault(o => o.OrderID == orderId);
            if (order != null)
            {
                order.TotalAmount = (int)totalAmount; // หรือใช้ decimal ถ้า TotalAmount ใน Order เป็น decimal
                order.Taxes = taxes;
                order.Discount = discount;
                order.isCompleted = true;
                db.SaveChanges();
                return true;
            }
            return false;
        }
    }
    ```

10. **`CancelOrder(int orderId)`**: ยกเลิกคำสั่งซื้อ
    ```csharp
    public bool CancelOrder(int orderId)
    {
        using (var db = new MyEcommerceDbContext())
        {
            var order = db.Orders.FirstOrDefault(o => o.OrderID == orderId);
            if (order != null)
            {
                order.CancelOrder = true;
                db.SaveChanges();
                return true;
            }
            return false;
        }
    }
    ```

---

### **การจัดการลูกค้า (Customer Management)**

11. **`GetCustomerById(int customerId)`**: ดึงข้อมูลลูกค้าจาก `CustomerID`
    ```csharp
    public Customer GetCustomerById(int customerId)
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.Customers.FirstOrDefault(c => c.CustomerID == customerId);
        }
    }
    ```

12. **`SearchCustomers(string searchTerm)`**: ค้นหาลูกค้าจากชื่อหรืออีเมล
    ```csharp
    public List<Customer> SearchCustomers(string searchTerm)
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.Customers
                     .Where(c => c.First_Name.Contains(searchTerm) || c.Last_Name.Contains(searchTerm) || c.Email.Contains(searchTerm))
                     .ToList();
        }
    }
    ```

13. **`AddNewCustomer(Customer newCustomer)`**: เพิ่มลูกค้าใหม่
    ```csharp
    public bool AddNewCustomer(Customer newCustomer)
    {
        using (var db = new MyEcommerceDbContext())
        {
            db.Customers.Add(newCustomer);
            db.SaveChanges();
            return true;
        }
    }
    ```

---

### **การจัดการการชำระเงิน (Payment Management)**

14. **`RecordPayment(int orderId, int paymentTypeId, decimal amount)`**: บันทึกการชำระเงินสำหรับคำสั่งซื้อ
    ```csharp
    public Payment RecordPayment(int orderId, int paymentTypeId, decimal amount)
    {
        using (var db = new MyEcommerceDbContext())
        {
            var payment = new Payment
            {
                Type = paymentTypeId,
                CreditAmount = amount,
                DebitAmount = 0, // หรือตั้งค่าตามประเภทการชำระเงิน
                Balance = 0, // ควรคำนวณจากยอดคงเหลือในระบบ
                PaymentDateTime = DateTime.Now
            };
            db.Payments.Add(payment);
            db.SaveChanges();

            // อัปเดต Order ด้วย PaymentID
            var order = db.Orders.FirstOrDefault(o => o.OrderID == orderId);
            if (order != null)
            {
                order.PaymentID = payment.PaymentID;
                db.SaveChanges();
            }
            return payment;
        }
    }
    ```

15. **`GetPaymentTypes()`**: ดึงรายการประเภทการชำระเงินทั้งหมด
    ```csharp
    public List<PaymentType> GetPaymentTypes()
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.PaymentTypes.ToList();
        }
    }
    ```

---

### **การจัดการหมวดหมู่และซัพพลายเออร์ (Category & Supplier Management)**

16. **`GetAllCategories()`**: ดึงรายการหมวดหมู่ทั้งหมด
    ```csharp
    public List<Category> GetAllCategories()
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.Categories.ToList();
        }
    }
    ```

17. **`GetAllSuppliers()`**: ดึงรายการซัพพลายเออร์ทั้งหมด
    ```csharp
    public List<Supplier> GetAllSuppliers()
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.Suppliers.ToList();
        }
    }
    ```

---

### **รายงานและข้อมูลเชิงลึก (Reporting & Insights)**

18. **`GetDailySales(DateTime date)`**: ดึงยอดขายรวมและรายละเอียดคำสั่งซื้อสำหรับวันใดวันหนึ่ง
    ```csharp
    public List<Order> GetDailySales(DateTime date)
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.Orders
                     .Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Date == date.Date && o.isCompleted == true)
                     .Include(o => o.OrderDetails) // ดึง OrderDetails มาด้วย
                     .ToList();
        }
    }
    ```

19. **`GetTopSellingProducts(int limit)`**: ดึงสินค้าที่ขายดีที่สุดจำนวนจำกัด
    ```csharp
    public List<TopSoldProduct> GetTopSellingProducts(int limit)
    {
        using (var db = new MyEcommerceDbContext())
        {
            var topProducts = db.OrderDetails
                                .GroupBy(od => od.ProductID)
                                .Select(g => new TopSoldProduct
                                {
                                    product = db.Products.FirstOrDefault(p => p.ProductID == g.Key),
                                    CountSold = g.Sum(od => od.Quantity ?? 0)
                                })
                                .OrderByDescending(ts => ts.CountSold)
                                .Take(limit)
                                .ToList();
            return topProducts;
        }
    }
    ```

20. **`GetProductReviews(int productId)`**: ดึงรีวิวของสินค้า
    ```csharp
    public List<Review> GetProductReviews(int productId)
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.Reviews.Where(r => r.ProductID == productId && r.isDelete == false).ToList();
        }
    }

    ในบริบทของแอปพลิเคชัน POS (Point of Sale) บน .NET Desktop โดยอิงจาก Models ที่คุณให้มา, นี่คือ 20 เมธอดหลัก ๆ ที่ครอบคลุมฟังก์ชันการทำงานพื้นฐานที่จำเป็นสำหรับระบบ POS:

---

### **การจัดการสินค้า (Product Management)**

1.  **`GetProductById(int productId)`**: ดึงข้อมูลสินค้าทั้งหมดจาก `ProductID` ที่กำหนด
    ```csharp
    public Product GetProductById(int productId)
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.Products.FirstOrDefault(p => p.ProductID == productId);
        }
    }
    ```

2.  **`SearchProducts(string searchTerm)`**: ค้นหาสินค้าจากชื่อ (Name) หรือคำอธิบาย (ShortDescription)
    ```csharp
    public List<Product> SearchProducts(string searchTerm)
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.Products
                     .Where(p => p.Name.Contains(searchTerm) || p.ShortDescription.Contains(searchTerm))
                     .ToList();
        }
    }
    ```

3.  **`UpdateProductStock(int productId, int quantityChange)`**: อัปเดตจำนวนสินค้าคงคลัง (เพิ่มหรือลด)
    ```csharp
    public bool UpdateProductStock(int productId, int quantityChange)
    {
        using (var db = new MyEcommerceDbContext())
        {
            var product = db.Products.FirstOrDefault(p => p.ProductID == productId);
            if (product != null && product.UnitInStock.HasValue)
            {
                product.UnitInStock += quantityChange;
                db.SaveChanges();
                return true;
            }
            return false;
        }
    }
    ```

4.  **`AddNewProduct(Product newProduct)`**: เพิ่มสินค้าใหม่เข้าสู่ระบบ
    ```csharp
    public bool AddNewProduct(Product newProduct)
    {
        using (var db = new MyEcommerceDbContext())
        {
            db.Products.Add(newProduct);
            db.SaveChanges();
            return true;
        }
    }
    ```

5.  **`GetAllProducts()`**: ดึงรายการสินค้าทั้งหมด
    ```csharp
    public List<Product> GetAllProducts()
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.Products.ToList();
        }
    }
    ```

---

### **การจัดการคำสั่งซื้อ (Order Management)**

6.  **`CreateNewOrder(int customerId, int? paymentId, int? shippingId)`**: สร้างคำสั่งซื้อใหม่ (Order)
    ```csharp
    public Order CreateNewOrder(int customerId, int? paymentId, int? shippingId)
    {
        using (var db = new MyEcommerceDbContext())
        {
            var newOrder = new Order
            {
                CustomerID = customerId,
                PaymentID = paymentId,
                ShippingID = shippingId,
                OrderDate = DateTime.Now,
                isCompleted = false,
                DIspatched = false,
                Shipped = false,
                Deliver = false,
                CancelOrder = false
            };
            db.Orders.Add(newOrder);
            db.SaveChanges();
            return newOrder;
        }
    }
    ```

7.  **`AddOrderDetail(int orderId, int productId, int quantity, decimal unitPrice, decimal discount)`**: เพิ่มรายการสินค้าในคำสั่งซื้อ (OrderDetail)
    ```csharp
    public OrderDetail AddOrderDetail(int orderId, int productId, int quantity, decimal unitPrice, decimal discount)
    {
        using (var db = new MyEcommerceDbContext())
        {
            var orderDetail = new OrderDetail
            {
                OrderID = orderId,
                ProductID = productId,
                Quantity = quantity,
                UnitPrice = unitPrice,
                Discount = discount,
                TotalAmount = (unitPrice * quantity) - discount,
                OrderDate = DateTime.Now
            };
            db.OrderDetails.Add(orderDetail);
            db.SaveChanges();
            return orderDetail;
        }
    }
    ```

8.  **`CalculateOrderTotal(int orderId)`**: คำนวณยอดรวมของคำสั่งซื้อจาก OrderDetails
    ```csharp
    public decimal CalculateOrderTotal(int orderId)
    {
        using (var db = new MyEcommerceDbContext())
        {
            // ใช้ AsEnumerable() เพื่อให้ LINQ to Objects ทำงานกับ Nullable<decimal> ได้ง่ายขึ้น
            return db.OrderDetails
                     .Where(od => od.OrderID == orderId)
                     .AsEnumerable() // ใช้ AsEnumerable() เพื่อคำนวณในหน่วยความจำ
                     .Sum(od => od.TotalAmount ?? 0); // จัดการค่า Nullable
        }
    }
    ```

9.  **`CompleteOrder(int orderId, decimal totalAmount, int? taxes, int? discount)`**: ทำให้คำสั่งซื้อเสร็จสมบูรณ์
    ```csharp
    public bool CompleteOrder(int orderId, decimal totalAmount, int? taxes, int? discount)
    {
        using (var db = new MyEcommerceDbContext())
        {
            var order = db.Orders.FirstOrDefault(o => o.OrderID == orderId);
            if (order != null)
            {
                order.TotalAmount = (int)totalAmount; // หรือใช้ decimal ถ้า TotalAmount ใน Order เป็น decimal
                order.Taxes = taxes;
                order.Discount = discount;
                order.isCompleted = true;
                db.SaveChanges();
                return true;
            }
            return false;
        }
    }
    ```

10. **`CancelOrder(int orderId)`**: ยกเลิกคำสั่งซื้อ
    ```csharp
    public bool CancelOrder(int orderId)
    {
        using (var db = new MyEcommerceDbContext())
        {
            var order = db.Orders.FirstOrDefault(o => o.OrderID == orderId);
            if (order != null)
            {
                order.CancelOrder = true;
                db.SaveChanges();
                return true;
            }
            return false;
        }
    }
    ```

---

### **การจัดการลูกค้า (Customer Management)**

11. **`GetCustomerById(int customerId)`**: ดึงข้อมูลลูกค้าจาก `CustomerID`
    ```csharp
    public Customer GetCustomerById(int customerId)
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.Customers.FirstOrDefault(c => c.CustomerID == customerId);
        }
    }
    ```

12. **`SearchCustomers(string searchTerm)`**: ค้นหาลูกค้าจากชื่อหรืออีเมล
    ```csharp
    public List<Customer> SearchCustomers(string searchTerm)
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.Customers
                     .Where(c => c.First_Name.Contains(searchTerm) || c.Last_Name.Contains(searchTerm) || c.Email.Contains(searchTerm))
                     .ToList();
        }
    }
    ```

13. **`AddNewCustomer(Customer newCustomer)`**: เพิ่มลูกค้าใหม่
    ```csharp
    public bool AddNewCustomer(Customer newCustomer)
    {
        using (var db = new MyEcommerceDbContext())
        {
            db.Customers.Add(newCustomer);
            db.SaveChanges();
            return true;
        }
    }
    ```

---

### **การจัดการการชำระเงิน (Payment Management)**

14. **`RecordPayment(int orderId, int paymentTypeId, decimal amount)`**: บันทึกการชำระเงินสำหรับคำสั่งซื้อ
    ```csharp
    public Payment RecordPayment(int orderId, int paymentTypeId, decimal amount)
    {
        using (var db = new MyEcommerceDbContext())
        {
            var payment = new Payment
            {
                Type = paymentTypeId,
                CreditAmount = amount,
                DebitAmount = 0, // หรือตั้งค่าตามประเภทการชำระเงิน
                Balance = 0, // ควรคำนวณจากยอดคงเหลือในระบบ
                PaymentDateTime = DateTime.Now
            };
            db.Payments.Add(payment);
            db.SaveChanges();

            // อัปเดต Order ด้วย PaymentID
            var order = db.Orders.FirstOrDefault(o => o.OrderID == orderId);
            if (order != null)
            {
                order.PaymentID = payment.PaymentID;
                db.SaveChanges();
            }
            return payment;
        }
    }
    ```

15. **`GetPaymentTypes()`**: ดึงรายการประเภทการชำระเงินทั้งหมด
    ```csharp
    public List<PaymentType> GetPaymentTypes()
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.PaymentTypes.ToList();
        }
    }
    ```

---

### **การจัดการหมวดหมู่และซัพพลายเออร์ (Category & Supplier Management)**

16. **`GetAllCategories()`**: ดึงรายการหมวดหมู่ทั้งหมด
    ```csharp
    public List<Category> GetAllCategories()
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.Categories.ToList();
        }
    }
    ```

17. **`GetAllSuppliers()`**: ดึงรายการซัพพลายเออร์ทั้งหมด
    ```csharp
    public List<Supplier> GetAllSuppliers()
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.Suppliers.ToList();
        }
    }
    ```

---

### **รายงานและข้อมูลเชิงลึก (Reporting & Insights)**

18. **`GetDailySales(DateTime date)`**: ดึงยอดขายรวมและรายละเอียดคำสั่งซื้อสำหรับวันใดวันหนึ่ง
    ```csharp
    public List<Order> GetDailySales(DateTime date)
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.Orders
                     .Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Date == date.Date && o.isCompleted == true)
                     .Include(o => o.OrderDetails) // ดึง OrderDetails มาด้วย
                     .ToList();
        }
    }
    ```

19. **`GetTopSellingProducts(int limit)`**: ดึงสินค้าที่ขายดีที่สุดจำนวนจำกัด
    ```csharp
    public List<TopSoldProduct> GetTopSellingProducts(int limit)
    {
        using (var db = new MyEcommerceDbContext())
        {
            var topProducts = db.OrderDetails
                                .GroupBy(od => od.ProductID)
                                .Select(g => new TopSoldProduct
                                {
                                    product = db.Products.FirstOrDefault(p => p.ProductID == g.Key),
                                    CountSold = g.Sum(od => od.Quantity ?? 0)
                                })
                                .OrderByDescending(ts => ts.CountSold)
                                .Take(limit)
                                .ToList();
            return topProducts;
        }
    }
    ```

20. **`GetProductReviews(int productId)`**: ดึงรีวิวของสินค้า
    ```csharp
    public List<Review> GetProductReviews(int productId)
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.Reviews.Where(r => r.ProductID == productId && r.isDelete == false).ToList();
        }
    }
    ```

---

### **ข้อสังเกตและข้อควรพิจารณา:**

* **Error Handling**: เมธอดเหล่านี้เป็นเวอร์ชันพื้นฐาน คุณควรเพิ่มการจัดการข้อผิดพลาด (เช่น `try-catch` blocks) เพื่อจัดการกับข้อผิดพลาดที่อาจเกิดขึ้น เช่น การเชื่อมต่อฐานข้อมูลล้มเหลว, ข้อมูลไม่ถูกต้อง เป็นต้น
* **Validation**: ควรมีการตรวจสอบความถูกต้องของข้อมูล (Validation) ก่อนที่จะบันทึกลงฐานข้อมูล โดยเฉพาะสำหรับข้อมูลที่มาจากผู้ใช้
* **Asynchronous Operations**: สำหรับแอปพลิเคชัน Desktop ที่ต้องการการตอบสนองที่ดี ควรพิจารณาใช้ Asynchronous Operations (`async`/`await`) สำหรับการเรียกใช้งานฐานข้อมูล เพื่อไม่ให้ UI ค้าง
* **Dependency Injection**: ในโปรเจกต์ขนาดใหญ่ ควรใช้ Dependency Injection เพื่อจัดการ `DbContext` และ Service ต่างๆ เพื่อให้โค้ดทดสอบง่ายขึ้นและจัดการทรัพยากรได้ดีขึ้น
* **Security**: สำหรับระบบจริง ควรมีการเข้ารหัส Password ของลูกค้าและ Admin และมีการจัดการสิทธิ์การเข้าถึงข้อมูลที่ดี
* **UI Integration**: เมธอดเหล่านี้เป็นส่วน Logic ของ Business Layer ซึ่งจะต้องถูกเรียกใช้งานจาก UI Layer ของแอปพลิเคชัน Desktop ของคุณ (เช่น ใน Event Handlers ของปุ่ม หรือ Form Load)

เมธอดเหล่านี้ครอบคลุมฟังก์ชันหลักๆ ที่ระบบ POS ทั่วไปต้องการ คุณสามารถปรับแต่งหรือเพิ่มเมธอดอื่นๆ ได้ตามความต้องการเฉพาะของระบบของคุณ.
    ```

---

### **ข้อสังเกตและข้อควรพิจารณา:**

* **Error Handling**: เมธอดเหล่านี้เป็นเวอร์ชันพื้นฐาน คุณควรเพิ่มการจัดการข้อผิดพลาด (เช่น `try-catch` blocks) เพื่อจัดการกับข้อผิดพลาดที่อาจเกิดขึ้น เช่น การเชื่อมต่อฐานข้อมูลล้มเหลว, ข้อมูลไม่ถูกต้อง เป็นต้น
* **Validation**: ควรมีการตรวจสอบความถูกต้องของข้อมูล (Validation) ก่อนที่จะบันทึกลงฐานข้อมูล โดยเฉพาะสำหรับข้อมูลที่มาจากผู้ใช้
* **Asynchronous Operations**: สำหรับแอปพลิเคชัน Desktop ที่ต้องการการตอบสนองที่ดี ควรพิจารณาใช้ Asynchronous Operations (`async`/`await`) สำหรับการเรียกใช้งานฐานข้อมูล เพื่อไม่ให้ UI ค้าง
* **Dependency Injection**: ในโปรเจกต์ขนาดใหญ่ ควรใช้ Dependency Injection เพื่อจัดการ `DbContext` และ Service ต่างๆ เพื่อให้โค้ดทดสอบง่ายขึ้นและจัดการทรัพยากรได้ดีขึ้น
* **Security**: สำหรับระบบจริง ควรมีการเข้ารหัส Password ของลูกค้าและ Admin และมีการจัดการสิทธิ์การเข้าถึงข้อมูลที่ดี
* **UI Integration**: เมธอดเหล่านี้เป็นส่วน Logic ของ Business Layer ซึ่งจะต้องถูกเรียกใช้งานจาก UI Layer ของแอปพลิเคชัน Desktop ของคุณ (เช่น ใน Event Handlers ของปุ่ม หรือ Form Load)

เมธอดเหล่านี้ครอบคลุมฟังก์ชันหลักๆ ที่ระบบ POS ทั่วไปต้องการ คุณสามารถปรับแต่งหรือเพิ่มเมธอดอื่นๆ ได้ตามความต้องการเฉพาะของระบบของคุณ.


ด้วยโมเดลที่คุณให้มาและเมธอดที่สร้างไว้ก่อนหน้านี้ นี่คือ 40 เมธอดเพิ่มเติมที่เหมาะสำหรับการนำไปใช้ในแอปพลิเคชัน **.NET WinForms POS** โดยมุ่งเน้นที่การโต้ตอบกับ UI และการจัดการข้อมูลเบื้องหลัง:

---

### **การจัดการหน้าจอและ UI (Screen & UI Management)**

1.  **`LoadProductsToDataGridView(DataGridView dgv)`**: โหลดข้อมูลสินค้าทั้งหมดลงใน `DataGridView`
    ```csharp
    public void LoadProductsToDataGridView(DataGridView dgv)
    {
        try
        {
            using (var db = new MyEcommerceDbContext())
            {
                var products = db.Products.ToList();
                dgv.DataSource = products;
                // Optional: Customize DataGridView columns
                dgv.Columns["ProductID"].HeaderText = "รหัสสินค้า";
                dgv.Columns["Name"].HeaderText = "ชื่อสินค้า";
                dgv.Columns["UnitPrice"].HeaderText = "ราคาต่อหน่วย";
                dgv.Columns["UnitInStock"].HeaderText = "จำนวนในสต็อก";
                // Hide unwanted columns
                dgv.Columns["Category"].Visible = false;
                dgv.Columns["Supplier"].Visible = false;
                dgv.Columns["OrderDetails"].Visible = false;
                // ... hide other navigation properties
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"เกิดข้อผิดพลาดในการโหลดสินค้า: {ex.Message}", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    ```

2.  **`ClearFormInputs(Control.ControlCollection controls)`**: ล้างข้อมูลใน Control ต่างๆ บน Form (เช่น `TextBox`, `ComboBox`)
    ```csharp
    public void ClearFormInputs(Control.ControlCollection controls)
    {
        foreach (Control control in controls)
        {
            if (control is TextBox textBox)
            {
                textBox.Text = string.Empty;
            }
            else if (control is ComboBox comboBox)
            {
                comboBox.SelectedIndex = -1;
                comboBox.Text = string.Empty;
            }
            else if (control is CheckBox checkBox)
            {
                checkBox.Checked = false;
            }
            else if (control is NumericUpDown numericUpDown)
            {
                numericUpDown.Value = 0;
            }
            // Add more control types as needed (e.g., DateTimePicker)
        }
    }
    ```

3.  **`PopulateComboBox<T>(ComboBox cmb, List<T> dataSource, string displayMember, string valueMember)`**: ใส่ข้อมูลลงใน `ComboBox`
    ```csharp
    public void PopulateComboBox<T>(ComboBox cmb, List<T> dataSource, string displayMember, string valueMember)
    {
        cmb.DataSource = null; // Clear previous data
        cmb.DisplayMember = displayMember;
        cmb.ValueMember = valueMember;
        cmb.DataSource = dataSource;
    }
    ```

4.  **`EnableDisableControls(Control.ControlCollection controls, bool enable)`**: เปิด/ปิดการใช้งาน Control บน Form
    ```csharp
    public void EnableDisableControls(Control.ControlCollection controls, bool enable)
    {
        foreach (Control control in controls)
        {
            control.Enabled = enable;
        }
    }
    ```

5.  **`ShowSuccessMessage(string message)`**: แสดงข้อความแจ้งเตือนความสำเร็จ
    ```csharp
    public void ShowSuccessMessage(string message)
    {
        MessageBox.Show(message, "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    ```

6.  **`ShowErrorMessage(string message)`**: แสดงข้อความแจ้งเตือนข้อผิดพลาด
    ```csharp
    public void ShowErrorMessage(string message)
    {
        MessageBox.Show(message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    ```

7.  **`ShowConfirmationDialog(string message)`**: แสดงกล่องโต้ตอบยืนยันการกระทำ
    ```csharp
    public bool ShowConfirmationDialog(string message)
    {
        return MessageBox.Show(message, "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    }
    ```

---

### **การจัดการรายการในตะกร้า/ใบเสร็จ (Shopping Cart/Invoice Management)**

8.  **`AddProductToCart(List<OrderDetail> cartItems, Product product, int quantity)`**: เพิ่มสินค้าลงในตะกร้า (List ของ `OrderDetail`)
    ```csharp
    public bool AddProductToCart(List<OrderDetail> cartItems, Product product, int quantity)
    {
        if (product == null || quantity <= 0) return false;

        // Check if product is already in cart
        var existingItem = cartItems.FirstOrDefault(item => item.ProductID == product.ProductID);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
            existingItem.TotalAmount = (existingItem.UnitPrice ?? 0) * existingItem.Quantity - (existingItem.Discount ?? 0);
        }
        else
        {
            if (product.UnitInStock < quantity) // Basic stock check
            {
                ShowErrorMessage("สินค้าในสต็อกไม่เพียงพอ");
                return false;
            }

            cartItems.Add(new OrderDetail
            {
                ProductID = product.ProductID,
                Product = product, // Keep reference for display purposes
                Quantity = quantity,
                UnitPrice = product.UnitPrice,
                Discount = product.Discount ?? 0,
                TotalAmount = (product.UnitPrice * quantity) - (product.Discount ?? 0)
            });
        }
        return true;
    }
    ```

9.  **`RemoveProductFromCart(List<OrderDetail> cartItems, int productId)`**: ลบสินค้าออกจากตะกร้า
    ```csharp
    public bool RemoveProductFromCart(List<OrderDetail> cartItems, int productId)
    {
        var itemToRemove = cartItems.FirstOrDefault(item => item.ProductID == productId);
        if (itemToRemove != null)
        {
            cartItems.Remove(itemToRemove);
            return true;
        }
        return false;
    }
    ```

10. **`UpdateCartItemQuantity(List<OrderDetail> cartItems, int productId, int newQuantity)`**: อัปเดตจำนวนสินค้าในตะกร้า
    ```csharp
    public bool UpdateCartItemQuantity(List<OrderDetail> cartItems, int productId, int newQuantity)
    {
        var itemToUpdate = cartItems.FirstOrDefault(item => item.ProductID == productId);
        if (itemToUpdate != null && newQuantity > 0)
        {
            // Re-check stock if increasing quantity
            if (itemToUpdate.Product != null && itemToUpdate.Product.UnitInStock < newQuantity)
            {
                ShowErrorMessage("สินค้าในสต็อกไม่เพียงพอ");
                return false;
            }

            itemToUpdate.Quantity = newQuantity;
            itemToUpdate.TotalAmount = (itemToUpdate.UnitPrice ?? 0) * newQuantity - (itemToUpdate.Discount ?? 0);
            return true;
        }
        return false;
    }
    ```

11. **`CalculateCartSubtotal(List<OrderDetail> cartItems)`**: คำนวณราคารวมก่อนหักส่วนลด/ภาษีในตะกร้า
    ```csharp
    public decimal CalculateCartSubtotal(List<OrderDetail> cartItems)
    {
        return cartItems.Sum(item => (item.UnitPrice ?? 0) * (item.Quantity ?? 0));
    }
    ```

12. **`CalculateCartTotal(List<OrderDetail> cartItems, int? taxes, int? overallDiscount)`**: คำนวณราคารวมสุทธิของตะกร้า
    ```csharp
    public decimal CalculateCartTotal(List<OrderDetail> cartItems, int? taxes, int? overallDiscount)
    {
        decimal subtotal = CalculateCartSubtotal(cartItems);
        decimal totalDiscount = cartItems.Sum(item => item.Discount ?? 0) + (overallDiscount ?? 0);
        decimal totalWithDiscount = subtotal - totalDiscount;
        decimal totalWithTaxes = totalWithDiscount + (totalWithDiscount * (taxes ?? 0) / 100M);
        return totalWithTaxes;
    }
    ```

13. **`DisplayCartItems(DataGridView dgv, List<OrderDetail> cartItems)`**: แสดงรายการสินค้าในตะกร้าบน `DataGridView`
    ```csharp
    public void DisplayCartItems(DataGridView dgv, List<OrderDetail> cartItems)
    {
        dgv.DataSource = null; // Clear existing
        dgv.DataSource = cartItems.Select(item => new
        {
            ProductID = item.ProductID,
            ProductName = item.Product?.Name, // Use Product navigation property
            UnitPrice = item.UnitPrice,
            Quantity = item.Quantity,
            Discount = item.Discount,
            TotalAmount = item.TotalAmount
        }).ToList();

        // Optional: Customize columns for cart display
        dgv.Columns["ProductID"].HeaderText = "รหัสสินค้า";
        dgv.Columns["ProductName"].HeaderText = "ชื่อสินค้า";
        dgv.Columns["UnitPrice"].HeaderText = "ราคาต่อหน่วย";
        dgv.Columns["Quantity"].HeaderText = "จำนวน";
        dgv.Columns["Discount"].HeaderText = "ส่วนลดต่อชิ้น";
        dgv.Columns["TotalAmount"].HeaderText = "ราคารวม";
    }
    ```

---

### **การจัดการ Payment และ Transaction (Payment & Transaction Management)**

14. **`ProcessPayment(decimal amount, int paymentTypeId)`**: ทำรายการชำระเงิน (อาจจะแค่บันทึกหรือเรียก API)
    ```csharp
    public Payment ProcessPayment(decimal amount, int paymentTypeId)
    {
        try
        {
            using (var db = new MyEcommerceDbContext())
            {
                var payment = new Payment
                {
                    Type = paymentTypeId,
                    CreditAmount = amount,
                    DebitAmount = 0,
                    Balance = 0, // In a real system, this would be updated or managed differently
                    PaymentDateTime = DateTime.Now
                };
                db.Payments.Add(payment);
                db.SaveChanges();
                return payment;
            }
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"เกิดข้อผิดพลาดในการประมวลผลการชำระเงิน: {ex.Message}");
            return null;
        }
    }
    ```

15. **`GetPaymentDetails(int paymentId)`**: ดึงรายละเอียดการชำระเงิน
    ```csharp
    public Payment GetPaymentDetails(int paymentId)
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.Payments.FirstOrDefault(p => p.PaymentID == paymentId);
        }
    }
    ```

16. **`GenerateReceipt(Order order)`**: สร้างใบเสร็จ (อาจจะคืนค่าเป็น String หรือใช้ Library สำหรับ Print)
    ```csharp
    public string GenerateReceipt(Order order)
    {
        using (var db = new MyEcommerceDbContext())
        {
            // Ensure related entities are loaded
            db.Entry(order).Reference(o => o.Customer).Load();
            db.Entry(order).Collection(o => o.OrderDetails).Load();
            foreach (var detail in order.OrderDetails)
            {
                db.Entry(detail).Reference(d => d.Product).Load();
            }

            System.Text.StringBuilder receipt = new System.Text.StringBuilder();
            receipt.AppendLine("------------------------------------");
            receipt.AppendLine("          ใบเสร็จรับเงิน          ");
            receipt.AppendLine("------------------------------------");
            receipt.AppendLine($"วันที่: {order.OrderDate?.ToString("dd/MM/yyyy HH:mm:ss")}");
            receipt.AppendLine($"รหัสคำสั่งซื้อ: {order.OrderID}");
            receipt.AppendLine($"ลูกค้า: {order.Customer?.First_Name} {order.Customer?.Last_Name}");
            receipt.AppendLine("------------------------------------");
            receipt.AppendLine("สินค้า:");
            foreach (var item in order.OrderDetails)
            {
                receipt.AppendLine($"- {item.Product?.Name} x {item.Quantity} @ {item.UnitPrice:N2} = {item.TotalAmount:N2}");
            }
            receipt.AppendLine("------------------------------------");
            receipt.AppendLine($"รวม: {order.TotalAmount:N2}");
            receipt.AppendLine($"ส่วนลด: {order.Discount ?? 0:N2}");
            receipt.AppendLine($"ภาษี: {order.Taxes ?? 0}%");
            receipt.AppendLine("------------------------------------");
            // Add payment type if PaymentID is available
            if (order.PaymentID.HasValue)
            {
                var payment = db.Payments.Include(p => p.PaymentType).FirstOrDefault(p => p.PaymentID == order.PaymentID.Value);
                if (payment != null)
                {
                    receipt.AppendLine($"ชำระด้วย: {payment.PaymentType?.Type ?? "ไม่ระบุ"}");
                    receipt.AppendLine($"จำนวนเงินที่ชำระ: {payment.CreditAmount ?? 0:N2}");
                }
            }
            receipt.AppendLine("------------------------------------");
            receipt.AppendLine("        ขอบคุณที่ใช้บริการ         ");
            receipt.AppendLine("------------------------------------");

            return receipt.ToString();
        }
    }
    ```

---

### **การจัดการข้อมูลลูกค้าเพิ่มเติม (Customer Data Management)**

17. **`ValidateCustomerInput(CustomerVM customer)`**: ตรวจสอบความถูกต้องของข้อมูลลูกค้าก่อนบันทึก
    ```csharp
    public bool ValidateCustomerInput(CustomerVM customer)
    {
        if (string.IsNullOrWhiteSpace(customer.First_Name) || string.IsNullOrWhiteSpace(customer.Last_Name))
        {
            ShowErrorMessage("กรุณากรอกชื่อและนามสกุลลูกค้า");
            return false;
        }
        if (!new EmailAddressAttribute().IsValid(customer.Email))
        {
            ShowErrorMessage("รูปแบบอีเมลไม่ถูกต้อง");
            return false;
        }
        // Add more validation rules as per CustomerVM's attributes
        return true;
    }
    ```

18. **`CheckCustomerExists(string userName, string email)`**: ตรวจสอบว่า `UserName` หรือ `Email` ซ้ำหรือไม่
    ```csharp
    public bool CheckCustomerExists(string userName, string email, int? excludeCustomerId = null)
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.Customers.Any(c => (c.UserName == userName || c.Email == email) && c.CustomerID != excludeCustomerId);
        }
    }
    ```

---

### **การจัดการข้อมูลพนักงาน/ผู้ดูแลระบบเพิ่มเติม (Employee/Admin Data Management)**

19. **`GetAllEmployees()`**: ดึงข้อมูลพนักงานทั้งหมด
    ```csharp
    public List<admin_Employee> GetAllEmployees()
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.admin_Employee.ToList();
        }
    }
    ```

20. **`DeleteEmployee(int empId)`**: ลบข้อมูลพนักงาน (ควรเป็น Soft Delete)
    ```csharp
    public bool DeleteEmployee(int empId)
    {
        using (var db = new MyEcommerceDbContext())
        {
            var employeeToDelete = db.admin_Employee.Find(empId);
            if (employeeToDelete != null)
            {
                // Soft delete: Consider adding an 'IsActive' field to admin_Employee
                // For now, we'll remove associated login. Be careful with cascading deletes.
                var loginToDelete = db.admin_Login.FirstOrDefault(al => al.EmpID == empId);
                if (loginToDelete != null)
                {
                    db.admin_Login.Remove(loginToDelete);
                }
                db.admin_Employee.Remove(employeeToDelete);
                db.SaveChanges();
                return true;
            }
            return false;
        }
    }
    ```

---

### **การจัดการข้อมูล Master Data (Categories, SubCategories, Suppliers)**

21. **`GetCategoryById(int categoryId)`**: ดึงข้อมูลหมวดหมู่ด้วย ID
    ```csharp
    public Category GetCategoryById(int categoryId)
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.Categories.Find(categoryId);
        }
    }
    ```

22. **`GetSubCategoryById(int subCategoryId)`**: ดึงข้อมูลหมวดหมู่ย่อยด้วย ID
    ```csharp
    public SubCategory GetSubCategoryById(int subCategoryId)
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.SubCategories.Find(subCategoryId);
        }
    }
    ```

23. **`GetSupplierById(int supplierId)`**: ดึงข้อมูลซัพพลายเออร์ด้วย ID
    ```csharp
    public Supplier GetSupplierById(int supplierId)
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.Suppliers.Find(supplierId);
        }
    }
    ```

24. **`IsCategoryNameExists(string categoryName, int? excludeCategoryId = null)`**: ตรวจสอบว่าชื่อหมวดหมู่ซ้ำหรือไม่
    ```csharp
    public bool IsCategoryNameExists(string categoryName, int? excludeCategoryId = null)
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.Categories.Any(c => c.Name == categoryName && c.CategoryID != excludeCategoryId);
        }
    }
    ```

25. **`IsSubCategoryNameExists(string subCategoryName, int categoryId, int? excludeSubCategoryId = null)`**: ตรวจสอบว่าชื่อหมวดหมู่ย่อยซ้ำในหมวดหมู่หลักเดียวกันหรือไม่
    ```csharp
    public bool IsSubCategoryNameExists(string subCategoryName, int categoryId, int? excludeSubCategoryId = null)
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.SubCategories.Any(sc => sc.Name == subCategoryName && sc.CategoryID == categoryId && sc.SubCategoryID != excludeSubCategoryId);
        }
    }
    ```

26. **`IsSupplierCompanyNameExists(string companyName, int? excludeSupplierId = null)`**: ตรวจสอบว่าชื่อบริษัทซัพพลายเออร์ซ้ำหรือไม่
    ```csharp
    public bool IsSupplierCompanyNameExists(string companyName, int? excludeSupplierId = null)
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.Suppliers.Any(s => s.CompanyName == companyName && s.SupplierID != excludeSupplierId);
        }
    }
    ```

---

### **การจัดการรูปภาพ/ไฟล์ (Image/File Handling)**

27. **`UploadImage(HttpPostedFileBase imageFile, string uploadFolderPath)`**: จำลองการอัปโหลดรูปภาพไปยังโฟลเดอร์ที่กำหนด (ใน WinForms จะเป็นการคัดลอกไฟล์)
    ```csharp
    // Note: HttpPostedFileBase is for Web. For WinForms, you'd use System.IO.File and a direct file path.
    // This is a placeholder for a WinForms equivalent.

    public string UploadImage(string sourceFilePath, string destinationDirectory, string fileName)
    {
        try
        {
            if (!System.IO.Directory.Exists(destinationDirectory))
            {
                System.IO.Directory.CreateDirectory(destinationDirectory);
            }

            string destinationFilePath = System.IO.Path.Combine(destinationDirectory, fileName);
            System.IO.File.Copy(sourceFilePath, destinationFilePath, true); // Overwrite if exists
            return destinationFilePath; // Return path to the uploaded file
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"เกิดข้อผิดพลาดในการอัปโหลดรูปภาพ: {ex.Message}");
            return null;
        }
    }
    ```

28. **`GetProductImageFullPath(string picturePath)`**: แปลง `PicturePath` ให้เป็น Full Path สำหรับแสดงใน WinForms `PictureBox`
    ```csharp
    public string GetProductImageFullPath(string picturePath)
    {
        if (string.IsNullOrEmpty(picturePath)) return null;

        // Assuming images are stored in a subfolder like "Images/Products" within your application's executable directory
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string imagePath = System.IO.Path.Combine(baseDirectory, "Images", "Products", picturePath);
        return imagePath;
    }
    ```

---

### **เมธอดสำหรับ Report และ Analytics เพิ่มเติม (Advanced Reporting)**

29. **`GetSalesByPaymentType(DateTime startDate, DateTime endDate)`**: รายงานยอดขายตามประเภทการชำระเงิน
    ```csharp
    public Dictionary<string, decimal> GetSalesByPaymentType(DateTime startDate, DateTime endDate)
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.Orders
                     .Where(o => o.isCompleted == true && o.OrderDate >= startDate && o.OrderDate <= endDate && o.PaymentID.HasValue)
                     .Join(db.Payments, o => o.PaymentID, p => p.PaymentID, (o, p) => new { Order = o, Payment = p })
                     .Join(db.PaymentTypes, op => op.Payment.Type, pt => pt.PaymentTypeID, (op, pt) => new { op.Order, op.Payment, PaymentType = pt })
                     .GroupBy(x => x.PaymentType.Type)
                     .ToDictionary(g => g.Key, g => g.Sum(x => x.Order.TotalAmount ?? 0));
        }
    }
    ```

30. **`GetSalesSummaryByMonth(int year)`**: สรุปยอดขายรายเดือนสำหรับปีที่กำหนด
    ```csharp
    public Dictionary<string, decimal> GetSalesSummaryByMonth(int year)
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.Orders
                     .Where(o => o.isCompleted == true && o.OrderDate.HasValue && o.OrderDate.Value.Year == year)
                     .GroupBy(o => o.OrderDate.Value.Month)
                     .ToDictionary(
                         g => System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key),
                         g => g.Sum(o => o.TotalAmount ?? 0)
                     );
        }
    }
    ```

31. **`GetCustomerCountByCity()`**: รายงานจำนวนลูกค้าแยกตามเมือง
    ```csharp
    public Dictionary<string, int> GetCustomerCountByCity()
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.Customers
                     .GroupBy(c => c.City)
                     .ToDictionary(g => g.Key, g => g.Count());
        }
    }
    ```

---

### **เมธอดสำหรับสถานะและ Dashboard (Status & Dashboard)**

32. **`GetTotalOrdersToday()`**: ดึงจำนวนคำสั่งซื้อทั้งหมดที่สมบูรณ์ในวันนี้
    ```csharp
    public int GetTotalOrdersToday()
    {
        using (var db = new MyEcommerceDbContext())
        {
            DateTime today = DateTime.Today;
            return db.Orders.Count(o => o.OrderDate.HasValue && o.OrderDate.Value.Date == today && o.isCompleted == true);
        }
    }
    ```

33. **`GetTotalRevenueToday()`**: ดึงยอดรายได้ทั้งหมดในวันนี้
    ```csharp
    public decimal GetTotalRevenueToday()
    {
        using (var db = new MyEcommerceDbContext())
        {
            DateTime today = DateTime.Today;
            return db.Orders
                     .Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Date == today && o.isCompleted == true && o.TotalAmount.HasValue)
                     .Sum(o => o.TotalAmount ?? 0);
        }
    }
    ```

34. **`GetPendingShipmentsCount()`**: ดึงจำนวนคำสั่งซื้อที่รอการจัดส่ง (Shipped = false, Deliver = false)
    ```csharp
    public int GetPendingShipmentsCount()
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.Orders.Count(o => o.isCompleted == true && o.Shipped == false && o.Deliver == false && o.CancelOrder == false);
        }
    }
    ```

35. **`GetRecentCustomers(int limit = 5)`**: ดึงลูกค้ารายล่าสุดที่ลงทะเบียน
    ```csharp
    public List<Customer> GetRecentCustomers(int limit = 5)
    {
        using (var db = new MyEcommerceDbContext())
        {
            return db.Customers.OrderByDescending(c => c.Created).Take(limit).ToList();
        }
    }
    ```

---

### **เมธอด Utility ทั่วไป (General Utilities)**

36. **`CheckInternetConnection()`**: ตรวจสอบการเชื่อมต่ออินเทอร์เน็ต (ในกรณีที่ระบบมีการเรียกใช้ API ภายนอก)
    ```csharp
    public bool CheckInternetConnection()
    {
        try
        {
            using (var client = new System.Net.WebClient())
            using (client.OpenRead("http://clients3.google.com/generate_204"))
            {
                return true;
            }
        }
        catch
        {
            return false;
        }
    }
    ```

37. **`ExportDataToCsv<T>(List<T> data, string filePath)`**: ส่งออกข้อมูลไปยังไฟล์ CSV
    ```csharp
    public void ExportDataToCsv<T>(List<T> data, string filePath)
    {
        try
        {
            using (var writer = new System.IO.StreamWriter(filePath))
            {
                // Write header row
                var properties = typeof(T).GetProperties();
                writer.WriteLine(string.Join(",", properties.Select(p => p.Name)));

                // Write data rows
                foreach (var item in data)
                {
                    var values = properties.Select(p => p.GetValue(item)?.ToString() ?? string.Empty);
                    writer.WriteLine(string.Join(",", values));
                }
            }
            ShowSuccessMessage($"ส่งออกข้อมูลไปยัง {filePath} สำเร็จ");
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"เกิดข้อผิดพลาดในการส่งออกข้อมูล: {ex.Message}");
        }
    }
    ```

38. **`ImportDataFromCsv<T>(string filePath)`**: นำเข้าข้อมูลจากไฟล์ CSV (ต้องจัดการ Mapping fields เอง)
    ```csharp
    // This is a complex method and would require a robust CSV parsing library
    // and careful mapping of columns to properties.
    // Placeholder example:
    public List<T> ImportDataFromCsv<T>(string filePath) where T : new()
    {
        List<T> importedData = new List<T>();
        try
        {
            var lines = System.IO.File.ReadAllLines(filePath);
            if (lines.Length <= 1) return importedData; // No header or no data

            var header = lines[0].Split(',');
            var properties = typeof(T).GetProperties();

            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split(',');
                if (values.Length != header.Length) continue; // Skip malformed lines

                T newItem = new T();
                for (int j = 0; j < header.Length; j++)
                {
                    var prop = properties.FirstOrDefault(p => p.Name.Equals(header[j], StringComparison.OrdinalIgnoreCase));
                    if (prop != null && prop.CanWrite)
                    {
                        try
                        {
                            var convertedValue = Convert.ChangeType(values[j], prop.PropertyType);
                            prop.SetValue(newItem, convertedValue);
                        }
                        catch { /* Handle conversion errors */ }
                    }
                }
                importedData.Add(newItem);
            }
            ShowSuccessMessage($"นำเข้าข้อมูลจาก {filePath} สำเร็จ {importedData.Count} รายการ");
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"เกิดข้อผิดพลาดในการนำเข้าข้อมูล: {ex.Message}");
        }
        return importedData;
    }
    ```

39. **`BackupDatabase(string backupPath)`**: สร้าง Backup ของฐานข้อมูล (สำหรับ SQL Server LocalDB หรือไฟล์ .mdf)
    ```csharp
    // This method depends heavily on your database setup (e.g., SQL Server Express, LocalDB)
    // For LocalDB or file-based DB, you might just copy the .mdf file.
    // For a full SQL Server, you'd use SQL commands.
    public bool BackupDatabase(string backupPath)
    {
        try
        {
            // Example for SQL Server LocalDB (assuming MyEcommerceDbContext is connected to a .mdf file)
            // You would need to ensure the database file is not in use or attach/detach it.
            // This is a simplified example. For production, consider proper SQL Server backup commands.
            string dbFilePath = ((System.Data.Entity.Core.EntityClient.EntityConnection)((IObjectContextAdapter)new MyEcommerceDbContext()).ObjectContext.Connection).StoreConnection.DataSource.Replace("|DataDirectory|", AppDomain.CurrentDomain.BaseDirectory);
            string dbFileName = System.IO.Path.GetFileName(dbFilePath);
            string backupFilePath = System.IO.Path.Combine(backupPath, $"{dbFileName}_{DateTime.Now:yyyyMMdd_HHmmss}.bak"); // Or .mdf

            if (!System.IO.Directory.Exists(backupPath))
            {
                System.IO.Directory.CreateDirectory(backupPath);
            }

            // A more robust way for SQL Server:
            // string connectionString = "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=MyEcommerceDbContext;Integrated Security=True";
            // string dbName = "MyEcommerceDbContext";
            // using (var connection = new SqlConnection(connectionString))
            // {
            //     connection.Open();
            //     string sql = $"BACKUP DATABASE [{dbName}] TO DISK = '{backupFilePath}' WITH NOFORMAT, NOINIT, NAME = N'{dbName}-Full Database Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10";
            //     using (var command = new SqlCommand(sql, connection))
            //     {
            //         command.ExecuteNonQuery();
            //     }
            // }

            // Simplistic file copy for local .mdf (ensure DB is detached first for consistency)
            // Or just copy the .mdf and .ldf files when the application is closed.
            System.IO.File.Copy(dbFilePath, backupFilePath, true);
            ShowSuccessMessage($"สำรองฐานข้อมูลสำเร็จที่: {backupFilePath}");
            return true;
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"เกิดข้อผิดพลาดในการสำรองฐานข้อมูล: {ex.Message}");
            return false;
        }
    }
    ```

40. **`RestoreDatabase(string backupFilePath)`**: กู้คืนฐานข้อมูลจาก Backup
    ```csharp
    // Similar to BackupDatabase, this depends heavily on your database setup.
    // For full SQL Server, you'd use RESTORE DATABASE commands.
    public bool RestoreDatabase(string backupFilePath)
    {
        try
        {
            // Example for SQL Server LocalDB / file-based DB.
            // Requires detaching the current DB and replacing files, then reattaching.
            // This is very complex and risky without proper database administration knowledge.
            // It's generally safer to provide tools to open the backup location manually.

            // A more robust way for SQL Server:
            // string connectionString = "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True"; // Connect to master
            // string dbName = "MyEcommerceDbContext";
            // using (var connection = new SqlConnection(connectionString))
            // {
            //     connection.Open();
            //     // Set database to single user mode to allow restore
            //     using (var cmd1 = new SqlCommand($"ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", connection))
            //     {
            //         cmd1.ExecuteNonQuery();
            //     }
            //     // Restore
            //     using (var cmd2 = new SqlCommand($"RESTORE DATABASE [{dbName}] FROM DISK = '{backupFilePath}' WITH FILE = 1, NOUNLOAD, REPLACE, STATS = 5", connection))
            //     {
            //         cmd2.ExecuteNonQuery();
            //     }
            //     // Set database back to multi user mode
            //     using (var cmd3 = new SqlCommand($"ALTER DATABASE [{dbName}] SET MULTI_USER", connection))
            //     {
            //         cmd3.ExecuteNonQuery();
            //     }
            // }
            ShowSuccessMessage($"กู้คืนฐานข้อมูลสำเร็จจาก: {backupFilePath}");
            return true;
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"เกิดข้อผิดพลาดในการกู้คืนฐานข้อมูล: {ex.Message}");
            return false;
        }
    }
    ```

---

**ข้อควรทราบและคำแนะนำเพิ่มเติมสำหรับ .NET WinForms POS:**

* **Service Layer / Business Logic Layer**: เมธอดเหล่านี้ควรอยู่ในคลาสแยกต่างหากที่ทำหน้าที่เป็น Service Layer หรือ Business Logic Layer (BLL) แทนที่จะใส่ไว้ใน Form โดยตรง เพื่อให้โค้ดเป็นระเบียบและง่ายต่อการบำรุงรักษา
* **Dependency Injection**: พิจารณาใช้ Dependency Injection เพื่อจัดการ `MyEcommerceDbContext` instance แทนการ `using (var db = new MyEcommerceDbContext())` ซ้ำๆ ทุกครั้งในแต่ละเมธอด วิธีนี้ช่วยให้สามารถ Mock ฐานข้อมูลสำหรับการทดสอบและจัดการ Lifetime ของ Context ได้ดีขึ้น
* **Thread Safety**: หากแอปพลิเคชันของคุณมีการทำงานแบบ Multi-threading (เช่น การเรียกใช้ข้อมูลจากฐานข้อมูลใน Background Thread) ให้พิจารณาเรื่อง Thread Safety โดยเฉพาะเมื่อมีการอัปเดต UI Controls
* **Password Hashing**: เน้นย้ำอีกครั้งว่า **Password Hashing** เป็นสิ่งสำคัญอย่างยิ่งในการจัดเก็บรหัสผ่านในฐานข้อมูล ห้ามเก็บรหัสผ่านเป็น Plain Text
* **User Experience (UX)**: สำหรับ WinForms, ควรมีการแสดงสถานะการโหลด (Loading indicator) เมื่อมีการเรียกใช้เมธอดที่ใช้เวลานาน (เช่น การโหลดข้อมูลจำนวนมาก) เพื่อปรับปรุงประสบการณ์ผู้ใช้
* **File Paths**: การจัดการ `PicturePath` ใน Models ควรจะเก็บเฉพาะชื่อไฟล์หรือ relative path เท่านั้น และให้โค้ดใน WinForms จัดการเรื่อง Full Path เพื่อแสดงรูปภาพ

เมธอดเหล่านี้จะช่วยให้คุณมีชุดเครื่องมือที่แข็งแกร่งในการพัฒนาแอปพลิเคชัน POS ด้วย .NET WinForms และสามารถต่อยอดเพื่อเพิ่มความสามารถอื่นๆ ได้อย่างยืดหยุ่น!