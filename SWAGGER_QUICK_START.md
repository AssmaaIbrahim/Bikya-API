# دليل البدء السريع مع Swagger في Bikya

## ⚡ البدء في 5 دقائق

### 1️⃣ تشغيل المشروع
```bash
dotnet run --project Bikya.API
```

### 2️⃣ فتح Swagger
اذهب إلى: `https://localhost:7001/swagger`

### 3️⃣ تسجيل مستخدم جديد
```
POST /api/Identity/Auth/Register
```
```json
{
  "fullName": "أحمد محمد",
  "email": "ahmed@test.com",
  "userName": "ahmed123",
  "password": "Test123!",
  "confirmPassword": "Test123!",
  "phoneNumber": "+201234567890",
  "address": "القاهرة، مصر"
}
```

### 4️⃣ الحصول على Token
انسخ الـ `token` من الـ response

### 5️⃣ تفعيل Authentication
- اضغط على زر 🔒 (Authorize)
- أدخل: `Bearer YOUR_TOKEN_HERE`
- اضغط Authorize

---

## 🎯 الاختبارات الأساسية

### ✅ تسجيل دخول
```
POST /api/Identity/Auth/Login
```
```json
{
  "email": "ahmed@test.com",
  "password": "Test123!"
}
```

### ✅ إنشاء محفظة
```
POST /api/Wallet/Wallet/Create
```

### ✅ إيداع أموال
```
POST /api/Wallet/Wallet/Deposit
```
```json
{
  "amount": 1000.00,
  "description": "إيداع تجريبي"
}
```

### ✅ عرض الرصيد
```
GET /api/Wallet/Wallet/GetBalance
```

---

## 🔧 حل المشاكل الشائعة

| المشكلة | الحل |
|---------|------|
| خطأ 401 | تأكد من إضافة Token في Authorize |
| خطأ 400 | تحقق من صحة البيانات المرسلة |
| خطأ 404 | تأكد من صحة المسار |
| لا يفتح Swagger | تأكد من تشغيل المشروع |

---

## 📞 المساعدة السريعة

### بيانات اختبار جاهزة:
```json
{
  "email": "test@bikya.com",
  "password": "Test123!",
  "fullName": "مستخدم تجريبي",
  "userName": "testuser",
  "phoneNumber": "+201234567890"
}
```

### Token مثال:
```
Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
```

---

## 🚀 نصائح سريعة

1. **احفظ الـ Token** فور الحصول عليه
2. **استخدم Authorize** للوصول للـ endpoints المحمية
3. **اختبر بالترتيب** المنطقي
4. **تحقق من الـ Response** دائماً

---

**🎯 الهدف:** اختبار جميع وظائف API بسرعة وفعالية 