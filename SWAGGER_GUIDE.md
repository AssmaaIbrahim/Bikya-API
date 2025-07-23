# دليل التعامل مع Swagger في مشروع Bikya

## نظرة عامة
Swagger هو أداة قوية لتوثيق واختبار APIs. في مشروع Bikya، تم تكوين Swagger ليعمل مع JWT Authentication ويوفر واجهة تفاعلية لاختبار جميع endpoints.

## 📋 المحتويات
1. [الوصول إلى Swagger](#الوصول-إلى-swagger)
2. [التعامل مع JWT Authentication](#التعامل-مع-jwt-authentication)
3. [اختبار Endpoints](#اختبار-endpoints)
4. [أمثلة عملية](#أمثلة-عملية)
5. [استكشاف الأخطاء](#استكشاف-الأخطاء)
6. [أفضل الممارسات](#أفضل-الممارسات)

---

## 🚀 الوصول إلى Swagger

### الخطوة 1: تشغيل المشروع
```bash
# من مجلد المشروع الرئيسي
dotnet run --project Bikya.API
```

### الخطوة 2: فتح Swagger UI
- افتح المتصفح واذهب إلى: `https://localhost:7001/swagger`
- أو: `http://localhost:5000/swagger`

### الخطوة 3: استكشاف الواجهة
ستجد في Swagger UI:
- **قائمة Endpoints**: منظمة حسب Areas (Identity, Products, Orders, etc.)
- **تفاصيل كل Endpoint**: Method, Path, Parameters, Request Body, Response
- **أزرار الاختبار**: Try it out لكل endpoint

---

## 🔐 التعامل مع JWT Authentication

### الخطوة 1: الحصول على Token
1. ابحث عن endpoint التسجيل أو تسجيل الدخول:
   ```
   POST /api/Identity/Auth/Login
   POST /api/Identity/Auth/Register
   ```

2. اضغط على "Try it out"

3. أدخل بيانات المستخدم:
   ```json
   {
     "email": "user@example.com",
     "password": "Password123!"
   }
   ```

4. اضغط "Execute"

5. انسخ الـ `token` من الـ response

### الخطوة 2: تفعيل Authentication
1. اضغط على زر "Authorize" في أعلى الصفحة (🔒)
2. أدخل الـ token بالشكل التالي:
   ```
   Bearer YOUR_TOKEN_HERE
   ```
3. اضغط "Authorize"
4. الآن يمكنك الوصول إلى endpoints المحمية

---

## 🧪 اختبار Endpoints

### مثال 1: تسجيل مستخدم جديد
```
POST /api/Identity/Auth/Register
```

**Request Body:**
```json
{
  "fullName": "أحمد محمد",
  "email": "ahmed@example.com",
  "userName": "ahmed123",
  "password": "Password123!",
  "confirmPassword": "Password123!",
  "phoneNumber": "+201234567890",
  "address": "القاهرة، مصر"
}
```

### مثال 2: تسجيل دخول
```
POST /api/Identity/Auth/Login
```

**Request Body:**
```json
{
  "email": "ahmed@example.com",
  "password": "Password123!"
}
```

### مثال 3: إنشاء منتج جديد (يتطلب Authentication)
```
POST /api/Products/Product/Create
```

**Request Body:**
```json
{
  "title": "هاتف ذكي جديد",
  "description": "هاتف ذكي بمواصفات عالية",
  "price": 1500.00,
  "condition": "New",
  "categoryId": 1
}
```

---

## 📝 أمثلة عملية

### 1. تسجيل Admin جديد
```
POST /api/Identity/Auth/AdminRegister
```

**Request Body:**
```json
{
  "fullName": "مدير النظام",
  "email": "admin@bikya.com",
  "userName": "admin",
  "password": "AdminPass123!",
  "confirmPassword": "AdminPass123!",
  "phoneNumber": "+201234567890",
  "address": "القاهرة، مصر",
  "adminRegistrationCode": "ADMIN2024",
  "adminRegistrationReason": "إدارة النظام"
}
```

### 2. إنشاء محفظة
```
POST /api/Wallet/Wallet/Create
```

**Headers:**
```
Authorization: Bearer YOUR_TOKEN
```

### 3. إيداع أموال في المحفظة
```
POST /api/Wallet/Wallet/Deposit
```

**Request Body:**
```json
{
  "amount": 1000.00,
  "description": "إيداع أولي"
}
```

### 4. البحث عن المنتجات
```
GET /api/Products/Product/Search?query=هاتف&categoryId=1&minPrice=100&maxPrice=2000
```

### 5. إنشاء طلب
```
POST /api/Order/Order/Create
```

**Request Body:**
```json
{
  "productId": 1,
  "buyerId": 2,
  "shippingInfo": {
    "recipientName": "أحمد محمد",
    "address": "شارع النيل، القاهرة",
    "city": "القاهرة",
    "postalCode": "12345",
    "phoneNumber": "+201234567890"
  }
}
```

---

## 🔍 استكشاف الأخطاء

### مشاكل شائعة وحلولها:

#### 1. خطأ 401 Unauthorized
**السبب:** عدم وجود token صحيح
**الحل:**
- تأكد من تسجيل الدخول أولاً
- انسخ الـ token بشكل صحيح
- تأكد من إضافة "Bearer " قبل الـ token

#### 2. خطأ 400 Bad Request
**السبب:** بيانات غير صحيحة في الـ request
**الحل:**
- تحقق من صحة الـ JSON format
- تأكد من إرسال جميع الحقول المطلوبة
- تحقق من قواعد التحقق (validation rules)

#### 3. خطأ 404 Not Found
**السبب:** endpoint غير موجود أو مسار خاطئ
**الحل:**
- تحقق من المسار الصحيح
- تأكد من تشغيل المشروع
- تحقق من تكوين Areas في المشروع

#### 4. خطأ 500 Internal Server Error
**السبب:** خطأ في الخادم
**الحل:**
- تحقق من سجلات الخطأ (logs)
- تأكد من صحة قاعدة البيانات
- تحقق من تكوين المشروع

---

## 💡 أفضل الممارسات

### 1. تنظيم الاختبارات
- ابدأ دائماً بتسجيل الدخول للحصول على token
- اختبر endpoints البسيطة أولاً
- استخدم بيانات اختبار واقعية

### 2. إدارة الـ Tokens
- احفظ الـ token في مكان آمن
- لا تشارك الـ token مع الآخرين
- استخدم token جديد لكل جلسة اختبار

### 3. اختبار الـ Responses
- تحقق من status codes
- راجع الـ response body
- اختبر حالات الخطأ المختلفة

### 4. توثيق الاختبارات
- سجل الخطوات المهمة
- احفظ أمثلة الـ requests والـ responses
- وثق أي مشاكل تواجهها

---

## 🛠️ تكوين Swagger المتقدم

### إضافة وصف مخصص للـ API
في ملف `Program.cs`:

```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Bikya API",
        Description = "API لتطبيق Bikya - منصة تبادل المنتجات",
        Contact = new OpenApiContact
        {
            Name = "فريق Bikya",
            Email = "support@bikya.com"
        }
    });
});
```

### إضافة أمثلة للـ Requests
```csharp
c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
    Name = "Authorization",
    In = ParameterLocation.Header,
    Type = SecuritySchemeType.ApiKey,
    Scheme = "Bearer"
});
```

---

## 📚 موارد إضافية

### روابط مفيدة:
- [Swagger Documentation](https://swagger.io/docs/)
- [OpenAPI Specification](https://swagger.io/specification/)
- [JWT Authentication](https://jwt.io/)

### أدوات مساعدة:
- [Postman](https://www.postman.com/) - لاختبار APIs
- [Insomnia](https://insomnia.rest/) - بديل لـ Postman
- [JWT Debugger](https://jwt.io/) - لفك تشفير JWT tokens

---

## 🎯 خاتمة

Swagger يوفر طريقة سهلة ومفيدة لاختبار وتوثيق APIs. باتباع هذا الدليل، ستتمكن من:

1. ✅ الوصول إلى Swagger UI
2. ✅ التعامل مع JWT Authentication
3. ✅ اختبار جميع endpoints
4. ✅ استكشاف وحل المشاكل
5. ✅ تطبيق أفضل الممارسات

**نصيحة:** استخدم هذا الدليل كمرجع سريع أثناء تطوير واختبار مشروع Bikya. 