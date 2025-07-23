# Gmail SMTP Setup Guide

## Step 1: Enable 2-Factor Authentication
1. Go to your Google Account settings: https://myaccount.google.com/
2. Navigate to "Security"
3. Enable "2-Step Verification" if not already enabled

## Step 2: Generate App Password
1. In the same Security section, find "App passwords"
2. Click on "App passwords"
3. Select "Mail" as the app
4. Select "Other (Custom name)" as device
5. Enter a name like "Bikya API"
6. Click "Generate"
7. Copy the 16-character password (it will look like: xxxx xxxx xxxx xxxx)

## Step 3: Update appsettings.json
Replace the password in `Bikya.API/appsettings.json`:

```json
"Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "EnableSsl": true,
    "Username": "your-email@gmail.com",
    "Password": "your-16-character-app-password"
}
```

## Step 4: Test Email Sending
1. Build and run the API
2. Register a new user
3. Check the logs for email sending status
4. Check your email inbox for the verification email

## Troubleshooting
- If you get authentication errors, make sure you're using the App Password, not your regular Gmail password
- If emails are not received, check spam folder
- Check API logs for detailed error messages
- Make sure your Gmail account allows "less secure app access" or use App Passwords

## Security Notes
- Never commit your actual App Password to source control
- Use environment variables or Azure Key Vault in production
- App Passwords are more secure than regular passwords for API access 