# أمثلة عملية لاستخدام Swagger في Bikya

## 🎯 سيناريوهات الاختبار العملية

### السيناريو الأول: تسجيل مستخدم جديد وإنشاء محفظة

#### الخطوة 1: تسجيل مستخدم عادي
```
POST /api/Identity/Auth/Register
```

**Request Body:**
```json
{
  "fullName": "سارة أحمد",
  "email": "sara@example.com",
  "userName": "sara123",
  "password": "SaraPass123!",
  "confirmPassword": "SaraPass123!",
  "phoneNumber": "+201234567891",
  "address": "الإسكندرية، مصر"
}
```

**Response المتوقع:**
```json
{
  "success": true,
  "message": "تم التسجيل بنجاح",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "email": "sara@example.com",
    "fullName": "سارة أحمد",
    "userName": "sara123"
  }
}
```

#### الخطوة 2: إنشاء محفظة للمستخدم
```
POST /api/Wallet/Wallet/Create
```

**Headers:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response المتوقع:**
```json
{
  "success": true,
  "message": "تم إنشاء المحفظة بنجاح",
  "data": {
    "id": 1,
    "userId": 1,
    "balance": 0.00,
    "linkedPaymentMethod": null,
    "createdAt": "2024-01-15T10:30:00Z"
  }
}
```

---

### السيناريو الثاني: إضافة منتج جديد

#### الخطوة 1: إنشاء فئة جديدة (Admin فقط)
```
POST /api/Category/Category/Create
```

**Request Body:**
```json
{
  "name": "الإلكترونيات",
  "description": "جميع الأجهزة الإلكترونية والهواتف"
}
```

#### الخطوة 2: إنشاء منتج جديد
```
POST /api/Products/Product/Create
```

**Request Body:**
```json
{
  "title": "iPhone 14 Pro",
  "description": "هاتف ذكي من Apple بمواصفات عالية",
  "price": 25000.00,
  "condition": "New",
  "categoryId": 1
}
```

**Response المتوقع:**
```json
{
  "success": true,
  "message": "تم إنشاء المنتج بنجاح",
  "data": {
    "id": 1,
    "title": "iPhone 14 Pro",
    "description": "هاتف ذكي من Apple بمواصفات عالية",
    "price": 25000.00,
    "condition": "New",
    "categoryId": 1,
    "userId": 1,
    "status": "Active",
    "createdAt": "2024-01-15T10:35:00Z"
  }
}
```

#### الخطوة 3: إضافة صور للمنتج
```
POST /api/Products/ProductImage/Upload
```

**Request Body (Form Data):**
```
productId: 1
image: [ملف الصورة]
isMain: true
```

---

### السيناريو الثالث: إنشاء طلب شراء

#### الخطوة 1: إيداع أموال في المحفظة
```
POST /api/Wallet/Wallet/Deposit
```

**Request Body:**
```json
{
  "amount": 30000.00,
  "description": "إيداع لشراء منتج"
}
```

#### الخطوة 2: إنشاء طلب شراء
```
POST /api/Order/Order/Create
```

**Request Body:**
```json
{
  "productId": 1,
  "buyerId": 1,
  "shippingInfo": {
    "recipientName": "سارة أحمد",
    "address": "شارع سعد زغلول، الإسكندرية",
    "city": "الإسكندرية",
    "postalCode": "21500",
    "phoneNumber": "+201234567891"
  }
}
```

**Response المتوقع:**
```json
{
  "success": true,
  "message": "تم إنشاء الطلب بنجاح",
  "data": {
    "id": 1,
    "productId": 1,
    "productTitle": "iPhone 14 Pro",
    "buyerId": 1,
    "buyerName": "sara123",
    "sellerId": 1,
    "sellerName": "sara123",
    "totalAmount": 25000.00,
    "platformFee": 1250.00,
    "sellerAmount": 23750.00,
    "status": "Pending",
    "createdAt": "2024-01-15T10:40:00Z",
    "shippingInfo": {
      "recipientName": "سارة أحمد",
      "address": "شارع سعد زغلول، الإسكندرية",
      "city": "الإسكندرية",
      "postalCode": "21500",
      "phoneNumber": "+201234567891"
    }
  }
}
```

---

### السيناريو الرابع: إدارة الطلبات

#### الخطوة 1: عرض جميع الطلبات
```
GET /api/Order/Order/GetAll
```

**Response المتوقع:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "productTitle": "iPhone 14 Pro",
      "buyerName": "sara123",
      "sellerName": "sara123",
      "totalAmount": 25000.00,
      "status": "Pending",
      "createdAt": "2024-01-15T10:40:00Z"
    }
  ]
}
```

#### الخطوة 2: تحديث حالة الطلب
```
PUT /api/Order/Order/UpdateStatus
```

**Request Body:**
```json
{
  "orderId": 1,
  "newStatus": "Paid"
}
```

---

### السيناريو الخامس: نظام التقييمات

#### الخطوة 1: إضافة تقييم للمنتج
```
POST /api/Review/Review/Create
```

**Request Body:**
```json
{
  "productId": 1,
  "orderId": 1,
  "rating": 5,
  "comment": "منتج ممتاز وحالة جديدة تماماً"
}
```

#### الخطوة 2: عرض تقييمات المنتج
```
GET /api/Review/Review/GetByProduct/1
```

**Response المتوقع:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "productId": 1,
      "orderId": 1,
      "rating": 5,
      "comment": "منتج ممتاز وحالة جديدة تماماً",
      "userName": "sara123",
      "createdAt": "2024-01-15T11:00:00Z"
    }
  ]
}
```

---

## 🔧 اختبار حالات الخطأ

### اختبار 1: تسجيل مستخدم ببيانات غير صحيحة
```
POST /api/Identity/Auth/Register
```

**Request Body (بيانات غير صحيحة):**
```json
{
  "fullName": "",
  "email": "invalid-email",
  "userName": "sa",
  "password": "123",
  "confirmPassword": "456"
}
```

**Response المتوقع:**
```json
{
  "success": false,
  "message": "بيانات غير صحيحة",
  "errors": [
    "الاسم مطلوب",
    "البريد الإلكتروني غير صحيح",
    "اسم المستخدم يجب أن يكون 3 أحرف على الأقل",
    "كلمة المرور يجب أن تكون 6 أحرف على الأقل",
    "كلمة المرور غير متطابقة"
  ]
}
```

### اختبار 2: الوصول لـ endpoint محمي بدون token
```
GET /api/Wallet/Wallet/GetBalance
```

**Response المتوقع:**
```json
{
  "success": false,
  "message": "غير مصرح",
  "statusCode": 401
}
```

### اختبار 3: إنشاء منتج بفئة غير موجودة
```
POST /api/Products/Product/Create
```

**Request Body:**
```json
{
  "title": "منتج تجريبي",
  "description": "وصف المنتج",
  "price": 100.00,
  "condition": "New",
  "categoryId": 999
}
```

**Response المتوقع:**
```json
{
  "success": false,
  "message": "الفئة غير موجودة",
  "statusCode": 404
}
```

---

## 📊 اختبار الأداء

### اختبار 1: البحث في المنتجات
```
GET /api/Products/Product/Search?query=هاتف&categoryId=1&minPrice=1000&maxPrice=50000&page=1&pageSize=10
```

### اختبار 2: عرض المنتجات مع التقييمات
```
GET /api/Products/Product/GetById/1?includeReviews=true
```

### اختبار 3: عرض محفظة مع المعاملات
```
GET /api/Wallet/Wallet/GetWithTransactions
```

---

## 🎨 نصائح للاختبار الفعال

### 1. استخدام بيانات واقعية
- استخدم أسماء عربية حقيقية
- استخدم أسعار منطقية للمنتجات
- استخدم عناوين مصرية حقيقية

### 2. اختبار جميع الحالات
- الحالات الناجحة
- حالات الخطأ
- البيانات الحدية (boundary values)
- البيانات الفارغة

### 3. تتبع المعاملات
- احفظ IDs للمنتجات والطلبات
- استخدم نفس المستخدم للاختبارات المتسلسلة
- تحقق من تحديث البيانات

### 4. اختبار الأمان
- اختبر endpoints المحمية
- اختبر صلاحيات المستخدمين
- اختبر validation rules

---

## 📝 قائمة مراجعة الاختبار

### ✅ اختبارات المستخدم
- [ ] تسجيل مستخدم جديد
- [ ] تسجيل دخول
- [ ] تحديث الملف الشخصي
- [ ] تغيير كلمة المرور
- [ ] تسجيل Admin جديد

### ✅ اختبارات المنتجات
- [ ] إنشاء فئة جديدة
- [ ] إنشاء منتج جديد
- [ ] رفع صور المنتج
- [ ] البحث في المنتجات
- [ ] عرض تفاصيل المنتج

### ✅ اختبارات المحفظة
- [ ] إنشاء محفظة
- [ ] إيداع أموال
- [ ] سحب أموال
- [ ] عرض الرصيد
- [ ] عرض المعاملات

### ✅ اختبارات الطلبات
- [ ] إنشاء طلب جديد
- [ ] عرض الطلبات
- [ ] تحديث حالة الطلب
- [ ] إلغاء الطلب

### ✅ اختبارات التقييمات
- [ ] إضافة تقييم
- [ ] عرض تقييمات المنتج
- [ ] تحديث التقييم
- [ ] حذف التقييم

---

## 🚀 نصائح سريعة

1. **احفظ الـ Token**: انسخ الـ token فور الحصول عليه
2. **استخدم Authorize**: اضغط على زر Authorize لإضافة الـ token
3. **اختبر بالترتيب**: اتبع السيناريوهات بالترتيب المنطقي
4. **تحقق من الـ Response**: راجع الـ status code والـ message
5. **استخدم أمثلة واقعية**: استخدم بيانات حقيقية للاختبار

---

**ملاحظة:** هذه الأمثلة مبنية على التكوين الحالي لـ API. قد تحتاج لتعديل البيانات حسب إعدادات قاعدة البيانات الخاصة بك. 