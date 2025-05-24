```csharp
// เนื่องจากข้อกำหนดคือ "เขียนโค้ด .net ตาม 50 ข้อด้านบน สั้นๆ เรียงตามข้อ"
// การสร้างโค้ด .NET ที่ทำงานได้จริงสำหรับทั้ง 50 ข้อในรูปแบบที่ "สั้นๆ" และ "เรียงตามข้อ"
// นั้นเป็นไปไม่ได้ในทางปฏิบัติ เนื่องจากแต่ละข้อต้องการตรรกะและโครงสร้างข้อมูลที่ซับซ้อน
// รวมถึงการเชื่อมต่อกับฐานข้อมูลและ UI ที่เป็นส่วนประกอบสำคัญของระบบ POS

// อย่างไรก็ตาม ผมจะพยายามนำเสนอโครงสร้างโค้ด C# (.NET) ที่เป็นไปได้
// สำหรับแต่ละข้อในลักษณะของ "Concept" หรือ "Snippet" เพื่อแสดงแนวคิด
// โดยจะเน้นไปที่ Business Logic และการคำนวณตามที่ระบุในโจทย์
// และละเว้นรายละเอียดปลีกย่อยของการใช้งาน UI, Database Context หรือ Error Handling ที่ครบถ้วน
// (ซึ่งในความเป็นจริงแล้วแต่ละข้ออาจต้องใช้โค้ดหลายสิบถึงร้อยบรรทัด)

// ข้อควรทราบ: นี่คือโค้ดตัวอย่างเชิงแนวคิด ไม่ใช่โค้ดที่สามารถรันได้ทันทีโดยไม่มีการปรับปรุง
// และเพิ่มเติมรายละเอียดที่จำเป็นสำหรับระบบ POS ที่สมบูรณ์

// ---

// สมมติฐานเบื้องต้นสำหรับโครงสร้างข้อมูล (Models)
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
    public string UnitOfMeasure { get; set; } // เช่น ชิ้น, แพ็ค, กิโลกรัม
    public DateTime? ExpiryDate { get; set; }
    public decimal VatRate { get; set; } = 0.07m; // Default VAT 7%
    public string Category { get; set; }
}

public class CartItem
{
    public Product Product { get; set; }
    public decimal Quantity { get; set; }
    public decimal ItemPrice { get; set; } // ราคาต่อหน่วย x จำนวน (ก่อนส่วนลด)
    public decimal ItemDiscountAmount { get; set; } // ส่วนลดของรายการนั้น
    public decimal ItemVatAmount { get; set; } // ภาษีของรายการนั้น
    public decimal NetPrice { get; set; } // ราคาสุทธิของรายการหลังหักส่วนลดและบวก VAT
}

public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public List<CartItem> Items { get; set; } = new List<CartItem>();
    public decimal SubTotal { get; set; } // ราคารวมก่อนหักส่วนลดและบวก VAT
    public decimal TotalDiscountAmount { get; set; }
    public decimal TotalVatAmount { get; set; }
    public decimal NetAmount { get; set; } // ยอดสุทธิที่ต้องจ่าย
    public string PaymentMethod { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal ChangeAmount { get; set; }
    public Customer Customer { get; set; }
    public User SalesPerson { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string Notes { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public decimal LoyaltyPoints { get; set; }
    public string Address { get; set; }
}

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public UserRole Role { get; set; }
}

public enum OrderStatus
{
    Pending,
    Completed,
    Refunded,
    Voided,
    OnHold,
    PreOrder
}

public enum UserRole
{
    Admin,
    Manager,
    Cashier
}

// ---

// คลาสหลักสำหรับ Logic ของ POS
public class PosSystem
{
    private List<Product> _products = new List<Product>(); // Mock Database for Products
    private List<Order> _orders = new List<Order>();     // Mock Database for Orders
    private List<Customer> _customers = new List<Customer>(); // Mock Database for Customers
    private List<User> _users = new List<User>();         // Mock Database for Users

    public Order CurrentOrder { get; set; } // ตะกร้าสินค้าปัจจุบัน

    public PosSystem()
    {
        // Sample Data
        _products.Add(new Product { Id = 1, Name = "Milk", Code = "P001", UnitPrice = 50.00m, StockQuantity = 100, UnitOfMeasure = "Litre", VatRate = 0.07m, Category = "Dairy" });
        _products.Add(new Product { Id = 2, Name = "Bread", Code = "P002", UnitPrice = 35.00m, StockQuantity = 50, UnitOfMeasure = "Loaf", VatRate = 0.07m, Category = "Bakery" });
        _products.Add(new Product { Id = 3, Name = "Apple", Code = "P003", UnitPrice = 10.00m, StockQuantity = 200, UnitOfMeasure = "Piece", VatRate = 0.07m, Category = "Fruit" });
        _products.Add(new Product { Id = 4, Name = "Medicine A", Code = "P004", UnitPrice = 120.00m, StockQuantity = 30, UnitOfMeasure = "Pack", ExpiryDate = DateTime.Now.AddMonths(3), VatRate = 0.00m, Category = "Medicine" }); // ยาส่วนใหญ่อาจไม่คิด VAT

        _customers.Add(new Customer { Id = 1, Name = "Alice", Phone = "0812345678", LoyaltyPoints = 100 });
        _customers.Add(new Customer { Id = 2, Name = "Bob", Phone = "0898765432", LoyaltyPoints = 50 });

        _users.Add(new User { Id = 1, Username = "admin", PasswordHash = "hashed_admin_pass", Role = UserRole.Admin });
        _users.Add(new User { Id = 2, Username = "cashier1", PasswordHash = "hashed_cashier_pass", Role = UserRole.Cashier });

        CurrentOrder = new Order { Items = new List<CartItem>() };
    }

    // 1. รับคำสั่งซื้อจากลูกค้า (รับรายการสินค้า, จำนวน)
    public void AddItemToOrder(string productIdentifier, decimal quantity)
    {
        // 18. ค้นหาสินค้าตามชื่อ, รหัสสินค้า หรือหมวดหมู่ (ใช้ในนี้)
        Product product = _products.FirstOrDefault(p =>
            p.Code == productIdentifier ||
            p.Name.Contains(productIdentifier, StringComparison.OrdinalIgnoreCase) ||
            p.Id.ToString() == productIdentifier);

        if (product == null)
        {
            Console.WriteLine($"Product '{productIdentifier}' not found.");
            return;
        }

        // 2. ตรวจสอบว่าสินค้ามีในสต็อกเพียงพอหรือไม่
        if (product.StockQuantity < quantity)
        {
            Console.WriteLine($"Insufficient stock for {product.Name}. Available: {product.StockQuantity}");
            return;
        }

        // 3. เพิ่มสินค้าลงในตะกร้าสินค้า (Shopping Cart)
        // 4. รวมรายการสินค้าซ้ำ (ถ้ามีสินค้าเดียวกันเพิ่มจำนวนแทนเพิ่มแถวใหม่)
        var existingItem = CurrentOrder.Items.FirstOrDefault(item => item.Product.Id == product.Id);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            CurrentOrder.Items.Add(new CartItem { Product = product, Quantity = quantity });
        }
        CalculateOrderTotals(); // คำนวณใหม่ทุกครั้งที่เพิ่ม/ลบ
        Console.WriteLine($"{quantity} x {product.Name} added to cart.");
    }

    // 5. คำนวณราคาสินค้าต่อรายการ (ราคาต่อหน่วย x จำนวน)
    // 6. คำนวณราคารวมของตะกร้าสินค้าทั้งหมด (รวมทุกรายการ)
    // 7. คำนวณส่วนลดสินค้าแต่ละรายการ (เช่น % หรือจำนวนเงินคงที่)
    // 8. คำนวณส่วนลดรวมทั้งหมด (รวมทุกรายการหลังหักส่วนลดสินค้า)
    // 9. คำนวณภาษีมูลค่าเพิ่ม (VAT) ตามอัตราที่กำหนด (เช่น 7%)
    // 10. คำนวณยอดเงินสุทธิที่ต้องจ่ายหลังหักส่วนลดและบวก VAT
    private void CalculateOrderTotals()
    {
        CurrentOrder.SubTotal = 0;
        CurrentOrder.TotalDiscountAmount = 0;
        CurrentOrder.TotalVatAmount = 0;
        CurrentOrder.NetAmount = 0;

        foreach (var item in CurrentOrder.Items)
        {
            item.ItemPrice = item.Product.UnitPrice * item.Quantity; // 5.
            item.ItemDiscountAmount = CalculateItemDiscount(item);   // 7. (ต้องมี logic ใน method นี้)

            decimal priceAfterItemDiscount = item.ItemPrice - item.ItemDiscountAmount;
            item.ItemVatAmount = priceAfterItemDiscount * item.Product.VatRate; // 9.
            item.NetPrice = priceAfterItemDiscount + item.ItemVatAmount;

            CurrentOrder.SubTotal += item.ItemPrice;
            CurrentOrder.TotalDiscountAmount += item.ItemDiscountAmount; // 8. (สำหรับส่วนลดรวมของแต่ละรายการ)
            CurrentOrder.TotalVatAmount += item.ItemVatAmount;
        }

        // Apply overall order discounts here if any (e.g., coupon code discount)
        decimal overallOrderDiscount = CalculateOverallOrderDiscount(CurrentOrder); // 8. (สำหรับส่วนลดรวมของบิล)
        CurrentOrder.TotalDiscountAmount += overallOrderDiscount;

        // 10. คำนวณยอดเงินสุทธิ
        CurrentOrder.NetAmount = CurrentOrder.SubTotal - CurrentOrder.TotalDiscountAmount + CurrentOrder.TotalVatAmount;

        Console.WriteLine($"SubTotal: {CurrentOrder.SubTotal:C}");
        Console.WriteLine($"Total Discount: {CurrentOrder.TotalDiscountAmount:C}");
        Console.WriteLine($"Total VAT: {CurrentOrder.TotalVatAmount:C}");
        Console.WriteLine($"Net Amount: {CurrentOrder.NetAmount:C}");
    }

    // 7. คำนวณส่วนลดสินค้าแต่ละรายการ (ตัวอย่าง)
    private decimal CalculateItemDiscount(CartItem item)
    {
        // Example: If product name contains "Special", give 10% discount
        if (item.Product.Name.Contains("Special", StringComparison.OrdinalIgnoreCase))
        {
            return item.ItemPrice * 0.10m;
        }
        // 20. คำนวณโปรโมชั่น เช่น ซื้อ 1 แถม 1 หรือ ซื้อครบตามจำนวนลดราคา
        // Example: Buy 2 Milk, get 1 free (discount on 1 unit)
        if (item.Product.Code == "P001" && item.Quantity >= 2)
        {
             // For every 2 milk, 1 is free. If 3 milk, 1 free. If 4 milk, 2 free.
             int freeUnits = (int)Math.Floor(item.Quantity / 2);
             return freeUnits * item.Product.UnitPrice;
        }
        return 0;
    }

    // 8. คำนวณส่วนลดรวมทั้งหมด (ตัวอย่างสำหรับการใช้คูปอง)
    private decimal CalculateOverallOrderDiscount(Order order)
    {
        // 23. ระบบคูปองส่วนลด (Coupon Code) และตรวจสอบความถูกต้อง
        // For simplicity, let's assume a hardcoded coupon
        if (order.Customer != null && order.Customer.Name == "Alice" && order.SubTotal > 100)
        {
            // Apply 5% discount if Alice and order > 100
            return order.SubTotal * 0.05m;
        }
        return 0;
    }

    public bool ProcessPayment(decimal amountPaid, string paymentMethod)
    {
        if (amountPaid < CurrentOrder.NetAmount)
        {
            Console.WriteLine("Amount paid is less than Net Amount.");
            return false;
        }

        // 11. รองรับการจ่ายเงินหลายช่องทาง
        CurrentOrder.PaymentMethod = paymentMethod;
        CurrentOrder.AmountPaid = amountPaid;
        CurrentOrder.ChangeAmount = amountPaid - CurrentOrder.NetAmount; // 24. แสดงยอดเงินทอน

        // 12. บันทึกข้อมูลการขาย (Order) ลงฐานข้อมูล
        CurrentOrder.Id = _orders.Count + 1; // Simple ID generation
        CurrentOrder.OrderDate = DateTime.Now;
        CurrentOrder.Status = OrderStatus.Completed;
        _orders.Add(CurrentOrder);

        // 13. อัพเดตสต็อกสินค้าหลังการขาย (ตัดสต็อก)
        foreach (var item in CurrentOrder.Items)
        {
            var productInStock = _products.FirstOrDefault(p => p.Id == item.Product.Id);
            if (productInStock != null)
            {
                productInStock.StockQuantity -= (int)item.Quantity;
            }
        }

        // 21. ระบบสมาชิก (ลูกค้า) เก็บข้อมูลและคำนวณแต้มสะสม
        if (CurrentOrder.Customer != null)
        {
            CurrentOrder.Customer.LoyaltyPoints += CurrentOrder.NetAmount / 10; // 10% of net amount as points
            Console.WriteLine($"Customer {CurrentOrder.Customer.Name} earned {CurrentOrder.NetAmount / 10} loyalty points.");
        }

        Console.WriteLine($"Payment successful via {paymentMethod}. Change: {CurrentOrder.ChangeAmount:C}");
        // 25. รองรับการพิมพ์ใบเสร็จรับเงิน (Receipt) - Logic for printing would go here
        PrintReceipt(CurrentOrder);

        CurrentOrder = new Order { Items = new List<CartItem>() }; // Reset for new order
        return true;
    }

    private void PrintReceipt(Order order)
    {
        Console.WriteLine("\n--- RECEIPT ---");
        Console.WriteLine($"Order ID: {order.Id}");
        Console.WriteLine($"Date: {order.OrderDate}");
        if (order.Customer != null) Console.WriteLine($"Customer: {order.Customer.Name}");
        Console.WriteLine("-----------------");
        foreach (var item in order.Items)
        {
            Console.WriteLine($"{item.Product.Name} x {item.Quantity} @ {item.Product.UnitPrice:C} = {item.ItemPrice:C}");
            if (item.ItemDiscountAmount > 0)
            {
                Console.WriteLine($"  Discount: -{item.ItemDiscountAmount:C}");
            }
            Console.WriteLine($"  Net Item Price: {item.NetPrice - item.ItemVatAmount:C} + VAT: {item.ItemVatAmount:C}");
        }
        Console.WriteLine("-----------------");
        Console.WriteLine($"SubTotal: {order.SubTotal:C}");
        Console.WriteLine($"Total Discount: {order.TotalDiscountAmount:C}");
        Console.WriteLine($"Total VAT: {order.TotalVatAmount:C}");
        Console.WriteLine($"Net Amount: {order.NetAmount:C}");
        Console.WriteLine($"Amount Paid: {order.AmountPaid:C}");
        Console.WriteLine($"Change: {order.ChangeAmount:C}");
        Console.WriteLine($"Payment Method: {order.PaymentMethod}");
        Console.WriteLine("-----------------\n");
    }

    // 14. รองรับการคืนสินค้า (Refund) พร้อมคืนสต็อก
    public void RefundOrder(int orderId)
    {
        var orderToRefund = _orders.FirstOrDefault(o => o.Id == orderId && o.Status == OrderStatus.Completed);
        if (orderToRefund == null)
        {
            Console.WriteLine($"Order {orderId} not found or cannot be refunded.");
            return;
        }

        // Logic to process refund amount and payment gateway interactions (if any)
        Console.WriteLine($"Refunding Order ID: {orderId}, Amount: {orderToRefund.NetAmount:C}");

        // Restore stock
        foreach (var item in orderToRefund.Items)
        {
            var productInStock = _products.FirstOrDefault(p => p.Id == item.Product.Id);
            if (productInStock != null)
            {
                productInStock.StockQuantity += (int)item.Quantity;
            }
        }
        orderToRefund.Status = OrderStatus.Refunded;
        Console.WriteLine($"Order {orderId} refunded and stock restored.");
    }

    // 15. รองรับการยกเลิกบิลขาย (Void)
    public void VoidOrder(int orderId)
    {
        var orderToVoid = _orders.FirstOrDefault(o => o.Id == orderId && (o.Status == OrderStatus.Pending || o.Status == OrderStatus.Completed));
        if (orderToVoid == null)
        {
            Console.WriteLine($"Order {orderId} not found or cannot be voided.");
            return;
        }

        if (orderToVoid.Status == OrderStatus.Completed)
        {
            // If already completed, need to revert stock and potentially payment
            foreach (var item in orderToVoid.Items)
            {
                var productInStock = _products.FirstOrDefault(p => p.Id == item.Product.Id);
                if (productInStock != null)
                {
                    productInStock.StockQuantity += (int)item.Quantity;
                }
            }
            // Logic to reverse payment if already processed
            Console.WriteLine($"Order {orderId} was completed, stock restored. Payment reversal may be needed.");
        }
        orderToVoid.Status = OrderStatus.Voided;
        Console.WriteLine($"Order {orderId} has been voided.");

        // 42. ระบบเก็บประวัติแก้ไขบิล (Audit Trail) - Record who voided it and when
    }

    // 16. แสดงประวัติการขายย้อนหลังของลูกค้า
    public List<Order> GetCustomerOrderHistory(int customerId)
    {
        return _orders.Where(o => o.Customer != null && o.Customer.Id == customerId).ToList();
    }

    // 17. รองรับการจัดการสินค้าหลายคลัง (สต็อกแยกตามสาขา)
    // (Requires a more complex ProductStock model with LocationId)
    // public class ProductStock
    // {
    //     public int ProductId { get; set; }
    //     public int LocationId { get; set; }
    //     public int Quantity { get; set; }
    // }
    // public void UpdateStockByLocation(int productId, int locationId, int quantityChange) { /* ... */ }


    // 19. รองรับการขายสินค้าตามหน่วย (เช่น ชิ้น, แพ็ค, กิโลกรัม)
    // (Already handled by `Product.UnitOfMeasure` and `CartItem.Quantity` can be decimal)

    // 21. ระบบสมาชิก (ลูกค้า) เก็บข้อมูลและคำนวณแต้มสะสม (ดูใน ProcessPayment)

    // 22. แสดงราคาสินค้าพิเศษสำหรับสมาชิก
    public decimal GetMemberPrice(Product product, Customer customer)
    {
        if (customer != null && customer.LoyaltyPoints > 500)
        {
            return product.UnitPrice * 0.95m; // 5% discount for high-point members
        }
        return product.UnitPrice;
    }

    // 23. ระบบคูปองส่วนลด (Coupon Code) และตรวจสอบความถูกต้อง (ดูใน CalculateOverallOrderDiscount)

    // 24. แสดงยอดเงินทอนเมื่อลูกค้าจ่ายเงินสด (ดูใน ProcessPayment)

    // 25. รองรับการพิมพ์ใบเสร็จรับเงิน (Receipt) (ดูใน PrintReceipt)

    // 26. ระบบเปิด/ปิดกะขาย (Shift Management)
    private DateTime _shiftStartTime;
    private User _currentCashier;

    public void StartShift(User cashier)
    {
        if (cashier.Role != UserRole.Cashier)
        {
            Console.WriteLine("Only cashiers can start a shift.");
            return;
        }
        _shiftStartTime = DateTime.Now;
        _currentCashier = cashier;
        Console.WriteLine($"Shift started by {cashier.Username} at {_shiftStartTime}");
    }

    public void EndShift()
    {
        if (_currentCashier == null)
        {
            Console.WriteLine("No active shift to end.");
            return;
        }
        DateTime shiftEndTime = DateTime.Now;
        Console.WriteLine($"Shift ended by {_currentCashier.Username} at {shiftEndTime}");
        // 27. บันทึกและสรุปยอดขายรายวัน (หรือรายกะ)
        SummarizeShiftSales(_shiftStartTime, shiftEndTime, _currentCashier.Id);
        _currentCashier = null;
    }

    // 27. บันทึกและสรุปยอดขายรายวัน (หรือรายกะ)
    public void SummarizeShiftSales(DateTime startTime, DateTime endTime, int? userId = null)
    {
        var salesInPeriod = _orders.Where(o => o.OrderDate >= startTime && o.OrderDate <= endTime && o.Status == OrderStatus.Completed);
        if (userId.HasValue)
        {
            salesInPeriod = salesInPeriod.Where(o => o.SalesPerson != null && o.SalesPerson.Id == userId.Value);
        }

        decimal totalSales = salesInPeriod.Sum(o => o.NetAmount);
        int numberOfTransactions = salesInPeriod.Count();
        Console.WriteLine($"\n--- Sales Summary ({startTime} to {endTime}) ---");
        if (userId.HasValue) Console.WriteLine($"For User ID: {userId.Value}");
        Console.WriteLine($"Total Sales: {totalSales:C}");
        Console.WriteLine($"Number of Transactions: {numberOfTransactions}");
        Console.WriteLine("---------------------------------\n");

        // Save this summary to a sales report table/file
    }

    // 28. ระบบรายงานสินค้าขายดี
    public List<Product> GetBestSellingProducts(int topN = 10)
    {
        return _orders
            .SelectMany(o => o.Items)
            .GroupBy(item => item.Product)
            .Select(group => new { Product = group.Key, TotalQuantitySold = group.Sum(item => item.Quantity) })
            .OrderByDescending(x => x.TotalQuantitySold)
            .Take(topN)
            .Select(x => x.Product)
            .ToList();
    }

    // 29. ระบบรายงานสินค้าคงเหลือต่ำ (Low Stock Alert)
    public List<Product> GetLowStockProducts(int threshold = 10)
    {
        return _products.Where(p => p.StockQuantity <= threshold).ToList();
    }

    // 30. ระบบแจ้งเตือนสินค้าหมดอายุ (สำหรับสินค้าประเภทอาหาร/ยา)
    public List<Product> GetExpiringProducts(int daysThreshold = 30)
    {
        return _products.Where(p => p.ExpiryDate.HasValue && p.ExpiryDate.Value <= DateTime.Now.AddDays(daysThreshold)).ToList();
    }

    // 31. บันทึกข้อมูลผู้ใช้งานระบบ (พนักงานขาย)
    public void RegisterUser(string username, string password, UserRole role)
    {
        if (_users.Any(u => u.Username == username))
        {
            Console.WriteLine($"Username '{username}' already exists.");
            return;
        }
        // In real app, hash password
        _users.Add(new User { Id = _users.Count + 1, Username = username, PasswordHash = $"hashed_{password}", Role = role });
        Console.WriteLine($"User '{username}' with role '{role}' registered.");
    }

    // 32. ระบบกำหนดสิทธิ์ผู้ใช้งาน (Admin, Cashier, Manager)
    public bool CheckUserPermission(User user, UserRole requiredRole)
    {
        return user.Role <= requiredRole; // Simple hierarchy: Admin > Manager > Cashier
    }

    // 33. ระบบล็อกอินด้วยรหัสผ่านหรือบาร์โค้ดพนักงาน
    public User Login(string username, string password) // Or barcode
    {
        // In real app, verify hashed password
        var user = _users.FirstOrDefault(u => u.Username == username && u.PasswordHash == $"hashed_{password}");
        if (user != null)
        {
            Console.WriteLine($"User '{username}' logged in successfully.");
            return user;
        }
        Console.WriteLine("Invalid username or password.");
        return null;
    }

    // 34. รองรับการสแกนบาร์โค้ดสินค้าด้วยเครื่องอ่านบาร์โค้ด
    public void ScanBarcode(string barcode)
    {
        // This method would call AddItemToOrder internally after lookup
        AddItemToOrder(barcode, 1);
        // This assumes the barcode is the product code
    }

    // 35. ระบบบันทึกและจัดการข้อมูลลูกค้า (CRM)
    public void AddOrUpdateCustomer(Customer customer)
    {
        var existingCustomer = _customers.FirstOrDefault(c => c.Id == customer.Id);
        if (existingCustomer != null)
        {
            existingCustomer.Name = customer.Name;
            existingCustomer.Phone = customer.Phone;
            existingCustomer.Address = customer.Address;
            Console.WriteLine($"Customer {customer.Name} updated.");
        }
        else
        {
            customer.Id = _customers.Count + 1;
            _customers.Add(customer);
            Console.WriteLine($"Customer {customer.Name} added.");
        }
    }

    // 36. ระบบจองสินค้า (Hold Items) ไว้ก่อนชำระเงิน
    public void HoldCurrentOrder()
    {
        if (CurrentOrder.Items.Any())
        {
            CurrentOrder.Status = OrderStatus.OnHold;
            CurrentOrder.Id = _orders.Count + 1; // Assign ID
            _orders.Add(CurrentOrder); // Save to held orders (or a separate list)
            Console.WriteLine($"Order held with ID: {CurrentOrder.Id}");
            CurrentOrder = new Order { Items = new List<CartItem>() }; // Start new order
        }
        else
        {
            Console.WriteLine("No items in current order to hold.");
        }
    }

    public void RetrieveHeldOrder(int orderId)
    {
        var heldOrder = _orders.FirstOrDefault(o => o.Id == orderId && o.Status == OrderStatus.OnHold);
        if (heldOrder != null)
        {
            CurrentOrder = heldOrder;
            CurrentOrder.Status = OrderStatus.Pending; // Change status back
            Console.WriteLine($"Held order {orderId} retrieved.");
            CalculateOrderTotals();
        }
        else
        {
            Console.WriteLine($"Held order {orderId} not found.");
        }
    }

    // 37. ระบบจัดการคืนเงินหรือแลกเปลี่ยนสินค้า (Exchange)
    public void ExchangeProduct(int originalOrderId, string productToExchangeCode, decimal returnQuantity, string newProductCode, decimal newQuantity)
    {
        // This is complex, typically involves a refund of original item and a new sale for new item
        // For simplicity, let's just refund the old item and add the new one.
        RefundProductFromOrder(originalOrderId, productToExchangeCode, returnQuantity);
        AddItemToOrder(newProductCode, newQuantity);
        Console.WriteLine($"Attempted exchange: Refunded {returnQuantity} of {productToExchangeCode} and added {newQuantity} of {newProductCode}.");
    }

    public void RefundProductFromOrder(int orderId, string productIdentifier, decimal quantity)
    {
        var order = _orders.FirstOrDefault(o => o.Id == orderId && o.Status == OrderStatus.Completed);
        if (order == null)
        {
            Console.WriteLine($"Order {orderId} not found or not completed for partial refund.");
            return;
        }

        var itemToRefund = order.Items.FirstOrDefault(item =>
            item.Product.Code == productIdentifier ||
            item.Product.Name.Contains(productIdentifier, StringComparison.OrdinalIgnoreCase));

        if (itemToRefund == null || itemToRefund.Quantity < quantity)
        {
            Console.WriteLine($"Product {productIdentifier} not found in order {orderId} or quantity exceeds purchased amount.");
            return;
        }

        // Reduce quantity in order item
        itemToRefund.Quantity -= quantity;

        // Restore stock
        var productInStock = _products.FirstOrDefault(p => p.Id == itemToRefund.Product.Id);
        if (productInStock != null)
        {
            productInStock.StockQuantity += (int)quantity;
        }

        // Recalculate order totals for this specific order and process partial refund
        // This logic is simplified and would need proper financial transaction handling.
        Console.WriteLine($"Refunded {quantity} of {itemToRefund.Product.Name} from order {orderId}. Stock restored.");
        // Re-calculate order total for `order` (not CurrentOrder) and process refund amount.
    }


    // 38. ระบบคำนวณค่าขนส่ง (ถ้ามี)
    public decimal CalculateShippingCost(string deliveryAddress, decimal orderWeight, decimal orderValue)
    {
        // Example: Flat rate + per kg
        decimal baseFee = 20.00m;
        decimal perKgRate = 5.00m;
        return baseFee + (orderWeight * perKgRate);
    }

    // 39. รองรับการสั่งซื้อสินค้าล่วงหน้า (Pre-order)
    public Order CreatePreOrder(Customer customer, List<CartItem> items)
    {
        var preOrder = new Order
        {
            Id = _orders.Count + 1,
            OrderDate = DateTime.Now,
            Customer = customer,
            Items = items,
            Status = OrderStatus.PreOrder,
            CreatedDate = DateTime.Now
        };
        // Do not deduct stock for pre-order, just record it
        _orders.Add(preOrder);
        Console.WriteLine($"Pre-order created for {customer.Name} with ID: {preOrder.Id}");
        return preOrder;
    }

    // 40. รองรับการทำงานแบบออฟไลน์ และซิงค์ข้อมูลเมื่อออนไลน์
    // (Requires a local database like SQLite and a synchronization service/API)
    // public void SaveOfflineData() { /* ... */ }
    // public void SyncOnlineData() { /* ... */ }

    // 41. ระบบจัดการราคาสินค้าตามช่วงเวลา (Time-based pricing)
    public decimal GetPriceAtTime(Product product, DateTime queryTime)
    {
        // Example: Product P001 is 40.00 from 10:00 to 12:00
        if (product.Code == "P001" && queryTime.Hour >= 10 && queryTime.Hour < 12)
        {
            return 40.00m;
        }
        return product.UnitPrice;
    }

    // 42. ระบบเก็บประวัติแก้ไขบิล (Audit Trail)
    public void RecordAudit(int orderId, string action, string changedBy, DateTime changeTime)
    {
        // Log to a separate AuditLog table/file
        Console.WriteLine($"Audit Log: Order {orderId}, Action: {action}, By: {changedBy}, At: {changeTime}");
    }

    // 43. รองรับการขายสินค้าหลายภาษา
    // (Requires localization resources, e.g., ResX files or database for product names/descriptions)
    // public string GetProductNameInLanguage(Product product, string languageCode) { /* ... */ return product.Name; }

    // 44. ระบบบันทึกหมายเหตุเพิ่มเติมในบิลขาย
    public void AddNoteToOrder(int orderId, string note)
    {
        var order = _orders.FirstOrDefault(o => o.Id == orderId);
        if (order != null)
        {
            order.Notes = note;
            order.LastModifiedDate = DateTime.Now;
            Console.WriteLine($"Note added to order {orderId}: {note}");
        }
    }

    // 45. ระบบพิมพ์ใบสั่งซื้อสินค้าให้ฝ่ายคลังสินค้า (Purchase Order)
    public void GeneratePurchaseOrder(List<Product> productsToOrder, int supplierId, DateTime requiredDate)
    {
        // Create a PurchaseOrder object and save it
        Console.WriteLine($"Generated Purchase Order for {productsToOrder.Count} items from supplier {supplierId}, required by {requiredDate.ToShortDateString()}");
        // Logic to print/send PO
    }

    // 46. ระบบเชื่อมต่อกับเครื่องชำระเงินภายนอก (POS Terminal)
    // (Requires integration with specific payment terminal SDKs/APIs, e.g., EMV, PCI DSS compliance)
    // public bool ProcessPaymentWithTerminal(decimal amount) { /* ... */ return true; }

    // 47. ระบบบันทึกและรายงานการขายตามพนักงาน
    // (ดูใน SummarizeShiftSales และ Order.SalesPerson)

    // 48. รองรับการปรับเปลี่ยนราคาสินค้าแบบทันที (Price Override)
    public void OverrideItemPriceInCurrentOrder(string productIdentifier, decimal newPrice)
    {
        var item = CurrentOrder.Items.FirstOrDefault(i =>
            i.Product.Code == productIdentifier ||
            i.Product.Name.Contains(productIdentifier, StringComparison.OrdinalIgnoreCase));

        if (item != null)
        {
            // Note: This changes the price for this specific order item, not the Product master price
            item.Product.UnitPrice = newPrice; // This is a simplistic override; usually you copy the product
            CalculateOrderTotals();
            Console.WriteLine($"Price for {item.Product.Name} overridden to {newPrice:C} for this order.");
            RecordAudit(CurrentOrder.Id, $"Price Override for {item.Product.Name}", _currentCashier?.Username ?? "System", DateTime.Now);
        }
        else
        {
            Console.WriteLine($"Product '{productIdentifier}' not found in current order.");
        }
    }

    // 49. ระบบจัดการภาษีในกรณีสินค้าหลายประเภท (VAT แยกตามสินค้า)
    // (Already handled by Product.VatRate and calculation in CalculateOrderTotals)

    // 50. ระบบจัดการและคืนเงินเครดิต (Credit Memo)
    public void IssueCreditMemo(Customer customer, decimal amount, string reason)
    {
        // Create a CreditMemo object and save it.
        // This amount can be used for future purchases by the customer.
        Console.WriteLine($"Issued Credit Memo for {customer.Name}, Amount: {amount:C}, Reason: {reason}");
        // Update customer's credit balance or issue a credit code.
    }
}

// --- Main Program (for demonstration) ---
public class Program
{
    public static void Main(string[] args)
    {
        PosSystem pos = new PosSystem();
        User currentUser = null;

        Console.WriteLine("--- POS System Simulation ---");

        // Simulate Login
        currentUser = pos.Login("cashier1", "cashier_pass");
        if (currentUser == null) return;

        pos.StartShift(currentUser);

        // 1. & 18. Add Items
        pos.AddItemToOrder("P001", 2); // Milk
        pos.AddItemToOrder("Bread", 1);
        pos.AddItemToOrder("P003", 5); // Apple
        pos.AddItemToOrder("P001", 1); // Add more Milk, should combine

        // Simulate adding a customer to the order
        pos.CurrentOrder.Customer = pos._customers.First(c => c.Id == 1); // Alice

        // 48. Price Override
        pos.OverrideItemPriceInCurrentOrder("Bread", 30.00m);

        // 36. Hold order
        pos.HoldCurrentOrder();

        // Start a new order
        pos.AddItemToOrder("P004", 1); // Medicine A (0% VAT example)
        pos.AddItemToOrder("P002", 3); // Bread

        // 22. Check member price
        var productForMember = pos._products.First(p => p.Id == 1);
        Console.WriteLine($"Member price for Milk: {pos.GetMemberPrice(productForMember, pos.CurrentOrder.Customer):C}");

        // 21. Loyalty points
        pos.CurrentOrder.Customer = pos._customers.First(c => c.Id == 2); // Bob

        // 11. Process Payment
        pos.ProcessPayment(200.00m, "Cash");

        // 36. Retrieve held order
        pos.RetrieveHeldOrder(1);
        pos.ProcessPayment(200.00m, "Credit Card");

        // 16. Customer history
        Console.WriteLine("\n--- Alice's Order History ---");
        var aliceOrders = pos.GetCustomerOrderHistory(1);
        foreach (var order in aliceOrders)
        {
            Console.WriteLine($"Order ID: {order.Id}, Total: {order.NetAmount:C}, Status: {order.Status}");
        }

        // 14. Refund
        pos.RefundOrder(1);

        // 15. Void (example if payment wasn't processed yet or just created)
        // Simulate creating a new order to void
        pos.AddItemToOrder("P003", 2);
        pos.CurrentOrder.Id = pos._orders.Count + 1; // Assign an ID to make it voidable
        pos._orders.Add(pos.CurrentOrder); // Add to mock DB to simulate saving it
        pos.VoidOrder(pos.CurrentOrder.Id);

        // 28. Best selling products
        Console.WriteLine("\n--- Best Selling Products ---");
        var bestSellers = pos.GetBestSellingProducts(2);
        foreach (var p in bestSellers)
        {
            Console.WriteLine($"- {p.Name}");
        }

        // 29. Low stock alert
        Console.WriteLine("\n--- Low Stock Products (Threshold 10) ---");
        var lowStock = pos.GetLowStockProducts(10);
        foreach (var p in lowStock)
        {
            Console.WriteLine($"- {p.Name} (Qty: {p.StockQuantity})");
        }

        // 30. Expiring products
        Console.WriteLine("\n--- Expiring Products (Next 30 days) ---");
        var expiring = pos.GetExpiringProducts(30);
        foreach (var p in expiring)
        {
            Console.WriteLine($"- {p.Name} (Expires: {p.ExpiryDate?.ToShortDateString()})");
        }

        // 31. Register new user
        pos.RegisterUser("manager1", "manager_pass", UserRole.Manager);

        // 35. Add/Update customer
        pos.AddOrUpdateCustomer(new Customer { Id = 3, Name = "Charlie", Phone = "0987654321", Address = "Some Street" });

        // 39. Pre-order
        var preOrderItems = new List<CartItem>
        {
            new CartItem { Product = pos._products.First(p => p.Id == 1), Quantity = 10 }
        };
        pos.CreatePreOrder(pos._customers.First(c => c.Id == 3), preOrderItems);

        // 44. Add note
        pos.AddNoteToOrder(1, "Customer requested no plastic bag.");

        // 50. Credit Memo
        pos.IssueCreditMemo(pos._customers.First(c => c.Id == 1), 50.00m, "Customer goodwill");

        pos.EndShift();
    }
}

Sure, here are another 50 logic points for a POS system, building on the previous list, with brief C# code concepts.

---

### Additional POS Logic and Features (51-100)

```csharp
// Continuing from the previous PosSystem class

// 51. ระบบจัดการชุดสินค้า (Bundle Products) - เช่น ซื้อกาแฟ+ขนมปังได้ราคาพิเศษ
public class ProductBundle
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<int> ProductIdsInBundle { get; set; } // IDs of products in the bundle
    public decimal BundlePrice { get; set; }
}

public void AddBundleToOrder(int bundleId, decimal quantity = 1)
{
    var bundle = _productBundles.FirstOrDefault(b => b.Id == bundleId);
    if (bundle == null)
    {
        Console.WriteLine($"Bundle {bundleId} not found.");
        return;
    }

    // Check stock for all products in the bundle
    foreach (var productId in bundle.ProductIdsInBundle)
    {
        var product = _products.FirstOrDefault(p => p.Id == productId);
        if (product == null || product.StockQuantity < quantity)
        {
            Console.WriteLine($"Not enough stock for product in bundle: {product?.Name ?? "N/A"}");
            return;
        }
    }

    // Add a special "bundle" item to the cart, or add individual items with a discount applied
    // For simplicity, we'll add a single item representing the bundle.
    // In a real system, you might add the individual items and adjust their prices.
    var bundleProduct = new Product { Id = bundle.Id, Name = bundle.Name, UnitPrice = bundle.BundlePrice, StockQuantity = (int)quantity, UnitOfMeasure = "Bundle" };
    CurrentOrder.Items.Add(new CartItem { Product = bundleProduct, Quantity = quantity, ItemPrice = bundle.BundlePrice * quantity });

    // Deduct stock for individual items within the bundle
    foreach (var productId in bundle.ProductIdsInBundle)
    {
        var product = _products.FirstOrDefault(p => p.Id == productId);
        if (product != null)
        {
            product.StockQuantity -= (int)quantity;
        }
    }
    CalculateOrderTotals();
    Console.WriteLine($"Bundle '{bundle.Name}' added to cart.");
}

// 52. รองรับการใช้งานเครื่องชั่งน้ำหนัก (Weight Scale Integration)
// This requires hardware integration, often via serial port or network.
public decimal GetWeightFromScale()
{
    // Mock implementation
    Console.WriteLine("Please place item on scale and press enter (simulating weight reading).");
    // In real-world, this would involve reading from a serial port or SDK
    // return _weightScaleService.ReadWeight();
    return 1.5m; // Example weight in KG
}

// 53. ระบบจัดการค่าธรรมเนียมบัตรเครดิต (Credit Card Surcharge)
public decimal CalculateCreditCardSurcharge(decimal amount)
{
    const decimal SurchargeRate = 0.03m; // 3% surcharge
    return amount * SurchargeRate;
}

// 54. ระบบสำรองข้อมูลและกู้คืน (Backup & Restore)
// Typically handled by database management system (e.g., SQL Server backup, SQLite file copy)
public void PerformDatabaseBackup(string path)
{
    // Example: copy SQLite file or call SQL Server backup command
    Console.WriteLine($"Database backup initiated to {path}");
}

// 55. ระบบจัดการผู้จัดจำหน่าย (Supplier Management)
public class Supplier
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string ContactPerson { get; set; }
    public string Phone { get; set; }
}
private List<Supplier> _suppliers = new List<Supplier>();
public void AddSupplier(Supplier supplier) { _suppliers.Add(supplier); }
public void UpdateSupplier(Supplier supplier) { /* Find and update */ }

// 56. ระบบบันทึกการรับสินค้าเข้าสต็อก (Goods Receipt)
public void RecordGoodsReceipt(int productId, int quantity, int supplierId)
{
    var product = _products.FirstOrDefault(p => p.Id == productId);
    var supplier = _suppliers.FirstOrDefault(s => s.Id == supplierId);
    if (product != null && supplier != null)
    {
        product.StockQuantity += quantity;
        Console.WriteLine($"Received {quantity} of {product.Name} from {supplier.Name}. New stock: {product.StockQuantity}");
        // Log this transaction
    }
}

// 57. ระบบจัดการการคืนสินค้าให้ผู้จัดจำหน่าย (Return to Supplier)
public void ReturnToSupplier(int productId, int quantity, int supplierId, string reason)
{
    var product = _products.FirstOrDefault(p => p.Id == productId);
    var supplier = _suppliers.FirstOrDefault(s => s.Id == supplierId);
    if (product != null && supplier != null && product.StockQuantity >= quantity)
    {
        product.StockQuantity -= quantity;
        Console.WriteLine($"Returned {quantity} of {product.Name} to {supplier.Name} due to {reason}. Remaining stock: {product.StockQuantity}");
        // Log this transaction
    }
}

// 58. ระบบจัดการภาษีอื่น ๆ (Other Tax Handling) - เช่น ภาษีสรรพสามิต
// Requires a more flexible tax model, e.g., TaxRule per product/category
public decimal CalculateExciseTax(Product product, decimal quantity)
{
    // Example: Alcohol may have excise tax
    if (product.Category == "Alcohol")
    {
        return product.UnitPrice * quantity * 0.15m; // 15% excise tax
    }
    return 0;
}

// 59. ระบบจำกัดสิทธิ์การเข้าถึงข้อมูล (Data Access Control)
// (Implemented at a higher level, e.g., in service layer using UserRole)
// public bool CanViewSalesReports(User user) { return user.Role == UserRole.Admin || user.Role == UserRole.Manager; }

// 60. ระบบจัดการราคาแบบหลายระดับ (Tiered Pricing) - เช่น ราคาส่ง, ราคาปลีก
public decimal GetTieredPrice(Product product, decimal quantity)
{
    if (quantity >= 100) return product.UnitPrice * 0.80m; // 20% discount for 100+
    if (quantity >= 50) return product.UnitPrice * 0.90m;  // 10% discount for 50+
    return product.UnitPrice;
}

// 61. ระบบแจ้งเตือนเมื่อยอดขายถึงเป้า (Sales Target Alert)
public void CheckSalesTarget(decimal dailyTarget)
{
    var todaySales = _orders.Where(o => o.OrderDate.Date == DateTime.Today && o.Status == OrderStatus.Completed).Sum(o => o.NetAmount);
    if (todaySales >= dailyTarget)
    {
        Console.WriteLine($"Daily sales target of {dailyTarget:C} reached! Current sales: {todaySales:C}");
    }
}

// 62. ระบบแสดงผลบนจอแสดงผลลูกค้า (Customer Facing Display)
// (Requires integration with a secondary display, often a separate thread/process updating the display)
public void UpdateCustomerDisplay(string message)
{
    Console.WriteLine($"[Customer Display]: {message}");
}

// 63. ระบบควบคุมลิ้นชักเก็บเงิน (Cash Drawer Control)
// (Requires hardware integration, e.g., sending a pulse signal via serial/USB printer)
public void OpenCashDrawer()
{
    Console.WriteLine("Cash drawer opened.");
    // Example: _cashDrawerService.Open();
}

// 64. ระบบจัดการสถานะการชำระเงิน (Payment Status Management) - Pending, Approved, Failed
// (Already partially covered by OrderStatus.Completed, but can be more granular for payment gateway interactions)
// public void UpdatePaymentStatus(int orderId, string status) { /* ... */ }

// 65. ระบบพิมพ์ป้ายราคา/บาร์โค้ดสินค้า (Label/Barcode Printing)
// (Requires integration with label printer SDKs or generating print-ready files)
public void PrintProductLabel(Product product, int quantity)
{
    Console.WriteLine($"Printing {quantity} labels for Product: {product.Name} (Barcode: {product.Code})");
}

// 66. ระบบบันทึกการเคลื่อนไหวสต็อก (Stock Movement Log) - เช่น รับเข้า, ขายออก, โอนย้าย, ปรับปรุง
public class StockMovement
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public int ProductId { get; set; }
    public string MovementType { get; set; } // e.g., "Sale", "Receipt", "Adjustment", "Transfer"
    public int QuantityChange { get; set; }
    public string Reference { get; set; } // Order ID, PO ID etc.
    public int UserId { get; set; }
}
private List<StockMovement> _stockMovements = new List<StockMovement>();
public void LogStockMovement(int productId, string type, int quantityChange, string reference, int userId)
{
    _stockMovements.Add(new StockMovement
    {
        Id = _stockMovements.Count + 1,
        Timestamp = DateTime.Now,
        ProductId = productId,
        MovementType = type,
        QuantityChange = quantityChange,
        Reference = reference,
        UserId = userId
    });
}

// 67. ระบบจัดการค่าใช้จ่ายหน้าร้าน (Petty Cash/Expense Management)
public class PettyCashTransaction
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } // "Expense", "Income"
    public int UserId { get; set; }
}
private List<PettyCashTransaction> _pettyCashTransactions = new List<PettyCashTransaction>();
public void AddPettyCashExpense(string description, decimal amount, int userId)
{
    _pettyCashTransactions.Add(new PettyCashTransaction
    {
        Id = _pettyCashTransactions.Count + 1,
        Date = DateTime.Now,
        Description = description,
        Amount = amount,
        Type = "Expense",
        UserId = userId
    });
}

// 68. ระบบอนุมัติการทำรายการพิเศษ (Override/Approval Workflow) - เช่น การลดราคามากเกินไป, คืนเงิน
public bool RequestOverrideApproval(User requestingUser, User manager, string action, decimal amount)
{
    // In a real system, this would involve sending a notification to a manager,
    // who would then approve/deny via their interface.
    Console.WriteLine($"User {requestingUser.Username} requesting approval for '{action}' of {amount:C}. Awaiting manager approval...");
    // For simulation, assume auto-approved by Admin
    return manager.Role == UserRole.Admin || manager.Role == UserRole.Manager;
}

// 69. ระบบเชื่อมต่อกับเครื่องรูดบัตร (EDC Terminal Integration)
// (Similar to 46, specific to various EDC providers and their APIs/SDKs)
// public bool InitiateEDCPayment(decimal amount) { /* ... */ return true; }

// 70. ระบบรายงานวิเคราะห์ข้อมูลการขาย (Sales Analytics Dashboard)
// (Requires aggregation of sales data, often using a reporting tool or custom UI)
public Dictionary<string, decimal> GetSalesByDayOfWeek()
{
    return _orders.Where(o => o.Status == OrderStatus.Completed)
                  .GroupBy(o => o.OrderDate.DayOfWeek.ToString())
                  .ToDictionary(g => g.Key, g => g.Sum(o => o.NetAmount));
}

// 71. ระบบจัดการลูกค้าองค์กร/ลูกค้าเครดิต (Corporate/Credit Customers)
public class CorporateCustomer : Customer
{
    public decimal CreditLimit { get; set; }
    public decimal CurrentCreditBalance { get; set; }
}
public void ProcessCreditPayment(CorporateCustomer customer, decimal amount)
{
    if (customer.CurrentCreditBalance + amount <= customer.CreditLimit)
    {
        customer.CurrentCreditBalance += amount;
        Console.WriteLine($"Processed credit payment for {amount:C}. New balance: {customer.CurrentCreditBalance:C}");
    }
    else
    {
        Console.WriteLine("Credit limit exceeded.");
    }
}

// 72. ระบบจัดการการนับสต็อก (Stock Counting / Inventory Audit)
public void StartStockCount(string warehouseId)
{
    Console.WriteLine($"Initiating stock count for warehouse {warehouseId}.");
    // Generate a count sheet or enable counting mode
}
public void RecordStockCount(int productId, int actualQuantity)
{
    var product = _products.FirstOrDefault(p => p.Id == productId);
    if (product != null)
    {
        int difference = actualQuantity - product.StockQuantity;
        if (difference != 0)
        {
            Console.WriteLine($"Stock adjustment for {product.Name}: {difference} units.");
            product.StockQuantity = actualQuantity;
            LogStockMovement(productId, "Adjustment", difference, "Stock Count", _users.First().Id); // Log adjustment
        }
    }
}

// 73. ระบบจัดการราคาตามหน่วยย่อย/หน่วยใหญ่ (Unit Conversion Pricing) - เช่น ซื้อเป็นโหลถูกกว่าซื้อเป็นชิ้น
// (Requires a unit conversion table for products and logic to select price based on purchased unit)
public decimal GetConvertedPrice(Product product, string desiredUnit)
{
    // Example: if product is "Egg" (piece) and desiredUnit is "Dozen"
    // return product.UnitPrice * 12 * 0.95m; // Discounted dozen price
    return product.UnitPrice;
}

// 74. ระบบจัดการรายการโปรด/สินค้าขายเร็ว (Favorite/Quick Sale Items)
// (UI feature: often a configurable grid of popular items for fast selection)
public List<Product> GetQuickSaleItems()
{
    // Return a predefined list or frequently sold items
    return _products.Where(p => p.Id == 1 || p.Id == 2).ToList();
}

// 75. ระบบแจ้งเตือนข้อผิดพลาด (Error Logging and Alerts)
public void LogError(string errorMessage, string source)
{
    Console.Error.WriteLine($"[ERROR] {DateTime.Now}: {errorMessage} (Source: {source})");
    // In a real system, log to file, database, or monitoring service
}

// 76. ระบบจัดการผู้รับผิดชอบ (Responsible Person for Transactions)
// (Already covered by associating User with Order, Shift Management)

// 77. ระบบจัดการการออกใบกำกับภาษีเต็มรูป (Full Tax Invoice Issuance)
// (Requires more comprehensive customer and company tax details, and specific format)
public void PrintFullTaxInvoice(Order order)
{
    Console.WriteLine($"\n--- Full Tax Invoice for Order ID: {order.Id} ---");
    // Include all required tax invoice details (customer tax ID, company address etc.)
    PrintReceipt(order); // Re-use basic receipt printing, add more details
    Console.WriteLine("--- End Full Tax Invoice ---\n");
}

// 78. ระบบจัดการการบันทึกข้อมูลภาษีซื้อ/ภาษีขาย (Input/Output Tax Recording)
// (Part of comprehensive accounting integration, often handled by exporting data)
// public void RecordInputTax(decimal amount, string supplierTaxId) { /* ... */ }
// public void RecordOutputTax(decimal amount, string customerTaxId) { /* ... */ }

// 79. ระบบจัดการรูปภาพสินค้า (Product Image Management)
public string GetProductImageUrl(int productId)
{
    // Return URL/path from database
    return $"./images/product_{productId}.jpg";
}

// 80. ระบบจัดการหมวดหมู่สินค้า (Product Category Management)
// (Product model already has Category, this is for CRUD on categories)
public class ProductCategory
{
    public int Id { get; set; }
    public string Name { get; set; }
}
private List<ProductCategory> _categories = new List<ProductCategory>();
public void AddCategory(string name) { _categories.Add(new ProductCategory { Id = _categories.Count + 1, Name = name }); }

// 81. ระบบจัดการหน่วยนับสินค้า (Unit of Measure Management)
// (Product model has UnitOfMeasure, this is for master list)
public List<string> GetAvailableUnitsOfMeasure()
{
    return new List<string> { "Piece", "Pack", "Kg", "Litre", "Dozen" };
}

// 82. ระบบแจ้งเตือนการอัปเดตซอฟต์แวร์ (Software Update Notification)
public bool IsSoftwareUpdateAvailable(string currentVersion)
{
    // Check external server for latest version
    // return _updateService.CheckForUpdates(currentVersion);
    return true; // Simulating an update is available
}

// 83. ระบบล็อกเอาต์อัตโนมัติเมื่อไม่มีการใช้งาน (Auto Logout on Inactivity)
// (Requires monitoring user activity / input events in UI)
public void SetAutoLogoutTimer(int minutes)
{
    Console.WriteLine($"Auto-logout set for {minutes} minutes of inactivity.");
    // Implement a timer that triggers logout if no input is detected
}

// 84. ระบบจัดการการโอนสต็อกระหว่างคลัง (Stock Transfer between Warehouses)
public void TransferStock(int productId, int fromWarehouseId, int toWarehouseId, int quantity)
{
    // Deduct from source warehouse, add to destination warehouse
    Console.WriteLine($"Transferred {quantity} of {productId} from warehouse {fromWarehouseId} to {toWarehouseId}.");
    LogStockMovement(productId, "Transfer Out", -quantity, $"To WH {toWarehouseId}", _users.First().Id);
    LogStockMovement(productId, "Transfer In", quantity, $"From WH {fromWarehouseId}", _users.First().Id);
}

// 85. ระบบจัดการหมายเลขซีเรียลสินค้า (Serial Number Tracking)
// (Requires each product item to have a unique serial number)
public class SerializedProductInstance : CartItem
{
    public string SerialNumber { get; set; }
}
// When adding, track serial numbers:
// public void AddSerializedItem(Product product, string serialNumber) { /* ... */ }

// 86. ระบบจัดการบัตรกำนัล/บัตรของขวัญ (Gift Card Management)
public class GiftCard
{
    public string Code { get; set; }
    public decimal Balance { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsActive { get; set; }
}
private List<GiftCard> _giftCards = new List<GiftCard>();
public decimal RedeemGiftCard(string code, decimal amountToRedeem)
{
    var card = _giftCards.FirstOrDefault(c => c.Code == code && c.IsActive && c.ExpiryDate > DateTime.Now);
    if (card != null && card.Balance >= amountToRedeem)
    {
        card.Balance -= amountToRedeem;
        Console.WriteLine($"Redeemed {amountToRedeem:C} from gift card {code}. Remaining balance: {card.Balance:C}");
        return amountToRedeem;
    }
    Console.WriteLine("Invalid or insufficient gift card.");
    return 0;
}

// 87. ระบบจัดการการหัก ณ ที่จ่าย (Withholding Tax)
// (Complex accounting feature, usually for B2B or specific services)
public decimal CalculateWithholdingTax(decimal amount, decimal rate)
{
    return amount * rate;
}

// 88. ระบบจัดการการรับชำระค่าสาธารณูปโภค (Utility Bill Payment Service)
// (Requires integration with specific bill payment APIs or service providers)
public bool ProcessUtilityBillPayment(string biller, string accountNumber, decimal amount)
{
    Console.WriteLine($"Processing {biller} bill for account {accountNumber} for {amount:C}.");
    // Call external API
    return true;
}

// 89. ระบบจัดการค่าบริการ/ค่าใช้จ่ายเพิ่มเติม (Service Charges/Additional Fees)
public void AddServiceCharge(decimal amount, string description)
{
    // Add a special item to the CurrentOrder for service charge
    CurrentOrder.Items.Add(new CartItem
    {
        Product = new Product { Name = description, UnitPrice = amount, StockQuantity = 1 },
        Quantity = 1,
        ItemPrice = amount
    });
    CalculateOrderTotals();
    Console.WriteLine($"Added service charge: {description} - {amount:C}");
}

// 90. ระบบจัดการการชำระเงินมัดจำ (Deposit Payment)
public void RecordDepositPayment(int orderId, decimal amount)
{
    var order = _orders.FirstOrDefault(o => o.Id == orderId);
    if (order != null)
    {
        // Add to order's payment details, possibly a separate field for deposits
        Console.WriteLine($"Recorded deposit of {amount:C} for order {orderId}.");
    }
}

// 91. ระบบจัดการการรับชำระด้วยแต้มสะสม/เครดิต (Loyalty Points/Credit Payment)
public decimal PayWithLoyaltyPoints(Customer customer, decimal pointsToRedeem)
{
    if (customer.LoyaltyPoints >= pointsToRedeem)
    {
        customer.LoyaltyPoints -= pointsToRedeem;
        decimal cashEquivalent = pointsToRedeem / 10; // e.g., 10 points = 1 THB
        Console.WriteLine($"Redeemed {pointsToRedeem} points for {cashEquivalent:C}. New points: {customer.LoyaltyPoints}");
        return cashEquivalent;
    }
    Console.WriteLine("Insufficient loyalty points.");
    return 0;
}

// 92. ระบบจัดการการยกเลิกรายการสินค้าในบิล (Cancel Item in Order)
public void RemoveItemFromCurrentOrder(string productIdentifier)
{
    var itemToRemove = CurrentOrder.Items.FirstOrDefault(item =>
        item.Product.Code == productIdentifier ||
        item.Product.Name.Contains(productIdentifier, StringComparison.OrdinalIgnoreCase));

    if (itemToRemove != null)
    {
        CurrentOrder.Items.Remove(itemToRemove);
        CalculateOrderTotals();
        Console.WriteLine($"Removed {itemToRemove.Product.Name} from cart.");
        RecordAudit(CurrentOrder.Id, $"Removed item {itemToRemove.Product.Name}", _currentCashier?.Username ?? "System", DateTime.Now);
    }
    else
    {
        Console.WriteLine($"Product '{productIdentifier}' not found in current order.");
    }
}

// 93. ระบบจัดการการขอใบเสนอราคา (Quotation Generation)
public Order GenerateQuotation(Customer customer, List<CartItem> items)
{
    var quotation = new Order
    {
        Id = _orders.Count + 1, // Assign temporary ID
        OrderDate = DateTime.Now,
        Customer = customer,
        Items = items,
        Status = OrderStatus.Pending, // Or a specific 'Quotation' status
        CreatedDate = DateTime.Now
    };
    // Calculate totals for quotation
    // Save quotation, but don't affect stock
    Console.WriteLine($"Quotation generated for {customer.Name} with ID: {quotation.Id}");
    return quotation;
}

// 94. ระบบจัดการการชำระเงินด้วย Wallet/E-Wallet (E-Wallet Payment)
// (Requires integration with specific E-Wallet providers like PromptPay QR, TrueMoney, etc.)
public bool ProcessEWalletPayment(decimal amount, string walletType)
{
    Console.WriteLine($"Initiating {walletType} payment for {amount:C}...");
    // Call E-Wallet API
    return true; // Assume success
}

// 95. ระบบจัดการส่วนลดตามกลุ่มลูกค้า (Customer Group Discounts)
public decimal GetCustomerGroupDiscount(Customer customer, decimal subTotal)
{
    // Example: VIP customers get 10% off if order > 500
    if (customer.LoyaltyPoints > 1000 && subTotal > 500)
    {
        return subTotal * 0.10m;
    }
    return 0;
}

// 96. ระบบจัดการการคืนเงินแบบเต็มบิล/บางส่วน (Full/Partial Refund)
// (RefundOrder already handles full, RefundProductFromOrder handles partial)

// 97. ระบบจัดการสินค้าคงคลังติดลบ (Negative Stock Handling)
// (Configuration option: allow/disallow negative stock. If allowed, issue alert.)
public bool AllowNegativeStock { get; set; } = false; // Default: disallow

// Inside AddItemToOrder or when deducting stock:
// if (!AllowNegativeStock && product.StockQuantity < quantity) { // Block sale }
// else if (product.StockQuantity < quantity) { Console.WriteLine("Warning: Negative stock will occur!"); }

// 98. ระบบจัดการการปรับปรุงสต็อก (Stock Adjustment) - เพิ่ม/ลดสต็อกโดยไม่มีการซื้อขาย
public void AdjustStock(int productId, int quantityChange, string reason, int userId)
{
    var product = _products.FirstOrDefault(p => p.Id == productId);
    if (product != null)
    {
        product.StockQuantity += quantityChange;
        Console.WriteLine($"Stock for {product.Name} adjusted by {quantityChange}. New stock: {product.StockQuantity}. Reason: {reason}");
        LogStockMovement(productId, "Adjustment", quantityChange, reason, userId);
    }
}

// 99. ระบบจัดการผู้ดูแลระบบ (Admin Panel/User Interface for Admin)
// (This refers to the UI/backend for managing users, products, reports, etc.)
// No direct code snippet for this, as it's a UI/Architecture concern.

// 100. ระบบจัดการการจัดส่งสินค้า (Delivery Management) - Tracking, Assigning Driver
public class Delivery
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string Status { get; set; } // "Pending", "Out for Delivery", "Delivered"
    public string DriverName { get; set; }
    public DateTime? DeliveryTime { get; set; }
    public string DeliveryAddress { get; set; }
}
private List<Delivery> _deliveries = new List<Delivery>();
public void AssignDelivery(int orderId, string driverName)
{
    var delivery = new Delivery { Id = _deliveries.Count + 1, OrderId = orderId, Status = "Pending", DriverName = driverName };
    _deliveries.Add(delivery);
    Console.WriteLine($"Order {orderId} assigned to {driverName} for delivery.");
}
public void UpdateDeliveryStatus(int deliveryId, string newStatus)
{
    var delivery = _deliveries.FirstOrDefault(d => d.Id == deliveryId);
    if (delivery != null)
    {
        delivery.Status = newStatus;
        if (newStatus == "Delivered") delivery.DeliveryTime = DateTime.Now;
        Console.WriteLine($"Delivery {deliveryId} status updated to {newStatus}.");
    }
}

// End of PosSystem class
```

---

**Explanation of the additional points:**

These 50 points expand the POS system's capabilities into more advanced areas such as:

* **Inventory Management:** Bundles, goods receipt, returns to supplier, stock movement logs, stock counting, serial number tracking, negative stock handling, and stock adjustments.
* **Pricing & Promotions:** Tiered pricing, customer group discounts.
* **Payment & Financials:** Credit card surcharge, other tax handling (excise tax), credit/corporate customer management, gift card redemption, withholding tax, utility bill payments, service charges, deposit payments, loyalty points/credit payment, and E-wallet integration.
* **Customer & Sales Operations:** Customer facing display, quick sale items, full tax invoice, quotation generation, and delivery management.
* **System Administration & Maintenance:** Supplier management, backup & restore, data access control, error logging, software update notifications, and auto-logout.
* **Hardware Integration:** Weight scale and EDC terminal (conceptual).

As before, the C# code snippets are conceptual and aim to illustrate the logic. A full implementation would require robust error handling, database interactions (ORM like Entity Framework), a well-structured UI framework (like WPF, ASP.NET Core, or WinForms), and proper architectural patterns (e.g., MVVM, Clean Architecture).

This comprehensive list of features, spanning 100 points, covers a very broad range of functionalities expected from a modern POS system, making it suitable for various retail environments.
```