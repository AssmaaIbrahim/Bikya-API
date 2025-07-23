# حل مشكلة التحقق من كلمة المرور

## المشكلة:
كان هناك خطأ في validation لكلمة المرور `Admin@123` عند التسجيل كمدير.

## السبب:
Regex validation في `AdminRegisterDto.cs` لم يكن صحيحاً تماماً.

## الحل:
تم إصلاح regex من:
```
^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]
```

إلى:
```
^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$
```

## التغيير:
- إضافة `{8,}$` في النهاية لضمان الطول الصحيح (8 أحرف على الأقل)

## اختبار الحل:
كلمة المرور `Admin@123` الآن تمر بالتحقق بنجاح:
- `A` - حرف كبير ✅
- `d` - حرف صغير ✅
- `m` - حرف صغير ✅
- `i` - حرف صغير ✅
- `n` - حرف صغير ✅
- `@` - رمز خاص ✅
- `1` - رقم ✅
- `2` - رقم ✅
- `3` - رقم ✅
- الطول: 9 أحرف ✅

## الآن يمكنك استخدام:
```bash
curl -X 'POST' \
  'https://localhost:65162/api/Identity/Auth/register-admin' \
  -H 'accept: */*' \
  -H 'Content-Type: application/json' \
  -d '{
  "fullName": "مدير النظام",
    "email": "admin@example.com",
    "phoneNumber": "+201234567890",
    "password": "Admin@123",
    "confirmPassword": "Admin@123",
    "adminRegistrationCode": "ADMIN2024BIKYA",
    "registrationReason": "إدارة النظام والتحكم في المستخدمين"
}'
```

وستحصل على استجابة نجاح بدلاً من خطأ validation. 