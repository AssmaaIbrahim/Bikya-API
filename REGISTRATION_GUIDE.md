# دليل التسجيل - نظام Bikya

## نظرة عامة

تم تحسين نظام التسجيل ليدعم تسجيل نوعين من المستخدمين:
1. **المستخدم العادي (User)** - للمستخدمين العاديين
2. **المدير (Admin)** - للمديرين مع صلاحيات إضافية

## نقاط النهاية (Endpoints)

### 1. التسجيل كالمستخدم العادي

**POST** `/api/Identity/Auth/register`

```json
{
    "fullName": "أحمد محمد",
    "email": "ahmed@example.com",
    "phoneNumber": "+201234567890",
    "password": "password123",
    "confirmPassword": "password123",
    "userType": "User"
}
```

### 2. التسجيل كمدير

**POST** `/api/Identity/Auth/register-admin`

```json
{
    "fullName": "مدير النظام",
    "email": "admin@example.com",
    "phoneNumber": "+201234567890",
    "password": "Admin@123",
    "confirmPassword": "Admin@123",
    "adminRegistrationCode": "ADMIN2024BIKYA",
    "registrationReason": "إدارة النظام والتحكم في المستخدمين"
}
```

### 3. التسجيل المختلط (يدعم كلا النوعين)

**POST** `/api/Identity/Auth/register`

```json
{
    "fullName": "مدير النظام",
    "email": "admin@example.com",
    "phoneNumber": "+201234567890",
    "password": "Admin@123",
    "confirmPassword": "Admin@123",
    "userType": "Admin",
    "adminRegistrationCode": "ADMIN2024BIKYA"
}
```

## متطلبات كلمة المرور

### للمستخدم العادي:
- الحد الأدنى: 6 أحرف

### للمدير:
- الحد الأدنى: 8 أحرف
- يجب أن تحتوي على:
  - حرف كبير (A-Z)
  - حرف صغير (a-z)
  - رقم (0-9)
  - رمز خاص (@$!%*?&)

## رمز التسجيل كمدير

يتم تخزين رمز التسجيل كمدير في ملف `appsettings.json`:

```json
{
    "AdminRegistration": {
        "Code": "ADMIN2024BIKYA",
        "RequireApproval": false,
        "MaxAdmins": 10
    }
}
```

**ملاحظة أمنية:** يجب تغيير هذا الرمز في بيئة الإنتاج.

## الاستجابة

### نجح التسجيل كالمستخدم العادي:
```json
{
    "success": true,
    "message": "تم التسجيل بنجاح",
    "data": {
        "token": "jwt_token_here",
        "fullName": "أحمد محمد",
        "email": "ahmed@example.com",
        "userId": 1,
        "userName": "ahmed@example.com",
        "roles": ["User"],
        "expiration": "2024-01-01T12:00:00Z"
    }
}
```

### نجح التسجيل كمدير:
```json
{
    "success": true,
    "message": "تم التسجيل كمدير بنجاح",
    "data": {
        "token": "jwt_token_here",
        "fullName": "مدير النظام",
        "email": "admin@example.com",
        "userId": 2,
        "userName": "admin@example.com",
        "roles": ["Admin"],
        "expiration": "2024-01-01T12:00:00Z"
    }
}
```

## رسائل الخطأ

### خطأ في رمز المدير:
```json
{
    "success": false,
    "message": "رمز التسجيل كمدير غير صحيح",
    "statusCode": 400
}
```

### خطأ في كلمة المرور:
```json
{
    "success": false,
    "message": "كلمة المرور يجب أن تكون 8 أحرف على الأقل",
    "statusCode": 400
}
```

### المستخدم موجود:
```json
{
    "success": false,
    "message": "المستخدم موجود بالفعل",
    "statusCode": 400
}
```

## الميزات الأمنية

1. **التحقق من رمز المدير:** لا يمكن التسجيل كمدير بدون الرمز الصحيح
2. **كلمات مرور قوية للمديرين:** متطلبات أعلى لكلمات المرور
3. **التحقق من صحة البريد الإلكتروني:** التحقق من صيغة البريد الإلكتروني
4. **التحقق من تطابق كلمة المرور:** التأكد من تطابق كلمة المرور وتأكيدها
5. **تعيين الأدوار تلقائياً:** يتم تعيين الدور المناسب تلقائياً

## إعدادات التكوين

### في appsettings.json:
```json
{
    "AdminRegistration": {
        "Code": "ADMIN2024BIKYA",        // رمز التسجيل كمدير
        "RequireApproval": false,        // هل يتطلب موافقة
        "MaxAdmins": 10                  // الحد الأقصى للمديرين
    }
}
```

## أمثلة الاستخدام

### مثال 1: تسجيل مستخدم عادي
```bash
curl -X POST "https://localhost:7001/api/Identity/Auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "أحمد محمد",
    "email": "ahmed@example.com",
    "password": "password123",
    "confirmPassword": "password123",
    "userType": "User"
  }'
```

### مثال 2: تسجيل مدير
```bash
curl -X POST "https://localhost:7001/api/Identity/Auth/register-admin" \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "مدير النظام",
    "email": "admin@example.com",
    "password": "Admin@123",
    "confirmPassword": "Admin@123",
    "adminRegistrationCode": "ADMIN2024BIKYA",
    "registrationReason": "إدارة النظام"
  }'
```

## ملاحظات مهمة

1. **الأمان:** يجب تغيير رمز المدير في بيئة الإنتاج
2. **التحقق:** يتم التحقق من صحة جميع البيانات المدخلة
3. **الأدوار:** يتم إنشاء الأدوار تلقائياً إذا لم تكن موجودة
4. **التوكن:** يتم إرجاع JWT token مع معلومات المستخدم
5. **الرسائل:** جميع الرسائل باللغة العربية للواجهة العربية 