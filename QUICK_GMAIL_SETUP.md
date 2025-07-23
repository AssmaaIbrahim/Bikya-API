# Quick Gmail Setup - Fix Email Sending Error

## The Problem
You're getting this error:
```json
{
  "success": false,
  "statusCode": 500,
  "message": "Failed to send verification email. Please try again."
}
```

## Quick Fix Steps

### Step 1: Enable 2-Factor Authentication
1. Go to https://myaccount.google.com/security
2. Click "2-Step Verification" and enable it

### Step 2: Generate App Password
1. Go to https://myaccount.google.com/apppasswords
2. Select "Mail" from dropdown
3. Select "Other (Custom name)"
4. Type "Bikya API"
5. Click "Generate"
6. Copy the 16-character password (looks like: `abcd efgh ijkl mnop`)

### Step 3: Update appsettings.json
Replace the password in `Bikya.API/appsettings.json`:

```json
"Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "EnableSsl": true,
    "Username": "hnouredein@gmail.com",
    "Password": "YOUR-16-CHARACTER-APP-PASSWORD"
}
```

### Step 4: Test
1. Build and run the API
2. Try registration again
3. Check logs for email sending status

## Alternative: Use Development Mode
If you don't want to set up Gmail right now, the system will log emails instead of sending them. Check the console logs for:
```
=== EMAIL WOULD BE SENT ===
To: your-email@example.com
Subject: Welcome to Bikya - Verify Your Email
Body: <html>...
=== END EMAIL ===
```

## Common Issues
- **Authentication failed**: Use App Password, not regular password
- **Less secure apps**: Gmail no longer supports this - use App Password
- **Wrong email**: Make sure username matches your Gmail address

## Need Help?
1. Check the console logs for detailed error messages
2. Make sure 2FA is enabled on your Gmail account
3. Use the exact App Password format (16 characters with spaces) 