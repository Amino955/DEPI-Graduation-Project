namespace TellaStore.Models.Entities;

// All enums for the project in one file

public enum Season
{
    Summer,     // صيفي
    Winter,     // شتوي
    AllSeason   // كل الأوقات
}

public enum SizeType
{
    Letters,    // XS, S, M, L, XL, XXL
    Numbers,    // 28, 30, 32, 34
    Kids,       // 2Y, 4Y, 6Y, 8Y
    FreeSize    // Free Size
}

public enum OrderStatus
{
    Pending,         // في انتظار التأكيد
    Confirmed,       // تم التأكيد
    Processing,      // جاري التجهيز
    Shipped,         // في الطريق
    Delivered,       // تم التسليم
    Cancelled,       // ملغي
    FailedDelivery   // فشل التسليم
}

public enum DeliveryAssignmentStatus
{
    Assigned,    // تم التعيين
    OnTheWay,    // في الطريق
    Delivered,   // تم التسليم
    Failed       // فشل التسليم
}

public enum DiscountTarget
{
    Product,     // خصم على منتج معين — Priority 0 (HIGHEST)
    Category,    // خصم على قسم كامل — Priority 1
    AllStore     // خصم على كل المتجر — Priority 2 (LOWEST)
}

public enum DiscountType
{
    Percentage,  // نسبة مئوية %
    FixedAmount  // مبلغ ثابت بالجنيه
}

public enum NotificationType
{
    OrderConfirmed,   // تم تأكيد الطلب
    OrderProcessing,  // جاري التجهيز
    OrderShipped,     // الطلب في الطريق
    OrderDelivered,   // تم التسليم
    OrderCancelled,   // تم الإلغاء
    LowStock,         // (للأدمن) مخزون منخفض
    NewOrder          // (للأدمن) طلب جديد
}
