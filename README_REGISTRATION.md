# نظام التسجيل المحسن - Bikya

## الميزات الجديدة

✅ **تسجيل المستخدمين العاديين** - بدون رمز خاص  
✅ **تسجيل المديرين** - مع رمز أمان خاص  
✅ **التحقق من صحة البيانات** - رسائل خطأ باللغة العربية  
✅ **تعيين الأدوار تلقائياً** - User أو Admin  
✅ **كلمات مرور قوية للمديرين** - متطلبات أمان أعلى  

## نقاط النهاية (Endpoints)

### 1. تسجيل مستخدم عادي
```
POST /api/Identity/Auth/register
```

**مثال:**
```json
{
    "fullName": "أحمد محمد",
    "email": "ahmed@example.com",
    "password": "password123",
    "confirmPassword": "password123",
    "userType": "User"
}
```

### 2. تسجيل مدير
```
POST /api/Identity/Auth/register-admin
```

**مثال:**
```json
{
    "fullName": "مدير النظام",
    "email": "admin@example.com",
    "password": "Admin@123",
    "confirmPassword": "Admin@123",
    "adminRegistrationCode": "ADMIN2024BIKYA",
    "registrationReason": "إدارة النظام"
}
```

## رمز المدير

يتم تخزين رمز التسجيل كمدير في `appsettings.json`:

```json
{
    "AdminRegistration": {
        "Code": "ADMIN2024BIKYA"
    }
}
```

**⚠️ مهم:** غيّر هذا الرمز في بيئة الإنتاج!

## متطلبات كلمة المرور

| نوع المستخدم | الحد الأدنى | المتطلبات |
|--------------|-------------|-----------|
| مستخدم عادي | 6 أحرف | أي حروف |
| مدير | 8 أحرف | حرف كبير + صغير + رقم + رمز |

## الاستجابة

### نجح التسجيل:
```json
{
    "success": true,
    "message": "تم التسجيل بنجاح",
    "data": {
        "token": "jwt_token_here",
        "fullName": "أحمد محمد",
        "email": "ahmed@example.com",
        "userId": 1,
        "roles": ["User"]
    }
}
```

### خطأ في الرمز:
```json
{
    "success": false,
    "message": "رمز التسجيل كمدير غير صحيح",
    "statusCode": 400
}
```

## الميزات الأمنية

🔒 **التحقق من رمز المدير**  
🔒 **كلمات مرور قوية للمديرين**  
🔒 **التحقق من صحة البريد الإلكتروني**  
🔒 **التحقق من تطابق كلمة المرور**  
🔒 **تعيين الأدوار تلقائياً**  

## التشغيل

```bash
# بناء المشروع
dotnet build

# تشغيل API
dotnet run --project Bikya.API
```

## ملاحظات

- جميع الرسائل باللغة العربية
- يتم إنشاء الأدوار تلقائياً
- التوكن صالح لمدة 24 ساعة
- يمكن تسجيل مديرين متعددين 