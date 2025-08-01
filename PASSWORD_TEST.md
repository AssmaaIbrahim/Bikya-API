# اختبار كلمة المرور

## كلمة المرور المثال: `Admin@123`

### تحليل الكلمة:
- `A` - حرف كبير ✅
- `d` - حرف صغير ✅  
- `m` - حرف صغير ✅
- `i` - حرف صغير ✅
- `n` - حرف صغير ✅
- `@` - رمز خاص ✅
- `1` - رقم ✅
- `2` - رقم ✅
- `3` - رقم ✅

### الطول: 9 أحرف ✅ (أكثر من 8)

## Regex المستخدم:
```
^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$
```

### شرح Regex:
- `^` - بداية السلسلة
- `(?=.*[a-z])` - يجب أن يحتوي على حرف صغير واحد على الأقل
- `(?=.*[A-Z])` - يجب أن يحتوي على حرف كبير واحد على الأقل  
- `(?=.*\d)` - يجب أن يحتوي على رقم واحد على الأقل
- `(?=.*[@$!%*?&])` - يجب أن يحتوي على رمز خاص واحد على الأقل
- `[A-Za-z\d@$!%*?&]{8,}` - يجب أن يكون الطول 8 أحرف على الأقل
- `$` - نهاية السلسلة

## النتيجة:
كلمة المرور `Admin@123` يجب أن تمر بالتحقق بنجاح ✅

## أمثلة أخرى صحيحة:
- `Admin@123` ✅
- `MyPass@1` ✅
- `Secure#2024` ✅
- `Test@123` ✅

## أمثلة خاطئة:
- `admin@123` ❌ (لا يوجد حرف كبير)
- `Admin123` ❌ (لا يوجد رمز خاص)
- `Admin@` ❌ (لا يوجد رقم)
- `ADMIN@123` ❌ (لا يوجد حرف صغير) 