# Email Testing Guide

## Current Email Flow
1. User registers → EmailConfirmed = false
2. Verification email is sent automatically during registration
3. User clicks verification link → EmailConfirmed = true
4. User can now login and access protected endpoints

## To Test Email Functionality:

### Step 1: Update Gmail App Password
1. Follow the GMAIL_SETUP_GUIDE.md
2. Update the password in `Bikya.API/appsettings.json`

### Step 2: Test Registration
1. Build and run the API
2. Use Swagger or Postman to call: `POST /api/auth/register`
3. Check the API logs for email sending status
4. Check your email inbox for verification email

### Step 3: Test Email Verification
1. Click the verification link in the email
2. Or use the API endpoint: `POST /api/auth/verify-email`
   ```json
   {
     "token": "your-token-here",
     "email": "your-email@example.com"
   }
   ```

### Step 4: Test Login
1. After verification, try to login
2. User should be able to access protected endpoints

## Troubleshooting
- Check API logs for detailed error messages
- Make sure SMTP settings are correct
- Verify Gmail App Password is working
- Check spam folder for emails
- Ensure EmailConfirmed is set to false during registration

## Log Messages to Look For
- "Verification email sent to {Email} during registration"
- "Email sent successfully to {Email} via SMTP"
- "SMTP error sending email to {Email}: {Message}" 