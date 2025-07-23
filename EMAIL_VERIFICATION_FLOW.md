# Email Verification Flow - Complete Guide

## New Registration Flow

### 1. User Registration
- **Endpoint**: `POST /api/identity/auth/register`
- **Behavior**: 
  - Creates user with `EmailConfirmed = false`
  - Sends verification email automatically
  - **NO TOKEN RETURNED** - User must verify email first
  - If email sending fails, user is deleted and error returned

### 2. Email Verification
- **Endpoint**: `POST /api/identity/auth/verify-email?token={token}&email={email}`
- **Behavior**:
  - Verifies email and sets `EmailConfirmed = true`
  - **RETURNS TOKEN** - User can now login
  - If already verified, returns token immediately

### 3. Login
- **Endpoint**: `POST /api/identity/auth/login`
- **Behavior**:
  - **REQUIRES** `EmailConfirmed = true`
  - Returns error if email not verified
  - Only allows login after email verification

## API Response Examples

### Registration Response (No Token)
```json
{
  "success": true,
  "message": "Registration initiated. Please check your email and verify your account before logging in.",
  "data": null,
  "statusCode": 200
}
```

### Email Verification Response (With Token)
```json
{
  "success": true,
  "message": "Email verified successfully. Welcome to Bikya!",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "fullName": "John Doe",
    "email": "john@example.com",
    "userId": 1,
    "userName": "john@example.com",
    "roles": ["User"],
    "expiration": "2024-01-01T12:00:00Z"
  },
  "statusCode": 200
}
```

### Login Response (After Verification)
```json
{
  "success": true,
  "message": "Login successful.",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "fullName": "John Doe",
    "email": "john@example.com",
    "userId": 1,
    "userName": "john@example.com",
    "roles": ["User"],
    "expiration": "2024-01-01T12:00:00Z"
  },
  "statusCode": 200
}
```

### Login Error (Unverified Email)
```json
{
  "success": false,
  "message": "Please verify your email address before logging in. Check your inbox for the verification email.",
  "data": null,
  "statusCode": 401
}
```

## Testing Steps

### Step 1: Setup Gmail
1. Follow `GMAIL_SETUP_GUIDE.md`
2. Update `appsettings.json` with App Password

### Step 2: Test Registration
```bash
POST /api/identity/auth/register
{
  "email": "test@example.com",
  "password": "Test123!",
  "confirmPassword": "Test123!",
  "fullName": "Test User",
  "phoneNumber": "1234567890",
  "userType": "User"
}
```

### Step 3: Check Email
- Look for verification email in inbox
- Copy the verification URL or token

### Step 4: Verify Email
```bash
POST /api/identity/auth/verify-email?token=YOUR_TOKEN&email=test@example.com
```

### Step 5: Test Login
```bash
POST /api/identity/auth/login
{
  "email": "test@example.com",
  "password": "Test123!"
}
```

## Security Features

### ✅ Email Verification Required
- No login without email verification
- Automatic email sending during registration
- User deletion if email sending fails

### ✅ Token Generation After Verification
- Token only generated after email confirmation
- Immediate token return for already verified users
- Secure token generation with proper expiration

### ✅ Error Handling
- Clear error messages for unverified emails
- Proper logging for debugging
- Graceful handling of email sending failures

## Frontend Integration

### Registration Flow
1. User fills registration form
2. Submit to `/api/identity/auth/register`
3. Show message: "Check your email for verification"
4. **Don't redirect to dashboard** - user needs to verify first

### Email Verification Flow
1. User clicks email link or uses verification endpoint
2. Call `/api/identity/auth/verify-email`
3. **Get token from response**
4. Store token and redirect to dashboard

### Login Flow
1. User tries to login
2. If email not verified, show error message
3. Provide option to resend verification email
4. Only allow login after verification

## Error Scenarios

### Email Sending Fails
- User is deleted from database
- Clear error message returned
- User must try registration again

### Invalid Verification Token
- Clear error message
- Option to resend verification email
- Token expiration handling

### Already Verified User
- Returns token immediately
- No duplicate verification needed
- Smooth user experience

## Benefits

1. **Security**: Prevents fake email registrations
2. **Data Quality**: Ensures valid email addresses
3. **User Experience**: Clear flow with proper feedback
4. **Compliance**: Meets email verification requirements
5. **Scalability**: Handles edge cases gracefully 