# Bikya Authentication System Guide

## Overview

The Bikya API implements a comprehensive JWT-based authentication system with role-based authorization, following ASP.NET Core best practices and Clean Architecture principles.

## Features

### 🔐 Authentication Features
- **JWT Token Authentication** with secure token generation and validation
- **Role-based Authorization** (Admin, User)
- **Password Reset** functionality with email tokens
- **Account Lockout** after failed login attempts
- **Soft Delete** for user accounts
- **Refresh Token** support for extended sessions
- **Comprehensive Logging** for security auditing

### 🛡️ Security Features
- **Password Complexity** requirements
- **Email Validation** for registration
- **Account Verification** system
- **Rate Limiting** protection
- **Global Exception Handling** for consistent error responses
- **Input Validation** with data annotations
- **Secure Token Storage** and validation

## Architecture

### Clean Architecture Layers

```
┌─────────────────────────────────────┐
│           API Layer                 │
│  ┌─────────────────────────────┐    │
│  │     Controllers             │    │
│  │  - AuthController           │    │
│  │  - UsersController          │    │
│  │  - AdminUsersController     │    │
│  └─────────────────────────────┘    │
└─────────────────────────────────────┘
┌─────────────────────────────────────┐
│         Application Layer           │
│  ┌─────────────────────────────┐    │
│  │      Services               │    │
│  │  - AuthService              │    │
│  │  - UserService              │    │
│  │  - UserAdminService         │    │
│  │  - JwtService               │    │
│  └─────────────────────────────┘    │
└─────────────────────────────────────┘
┌─────────────────────────────────────┐
│         Domain Layer                │
│  ┌─────────────────────────────┐    │
│  │      Models                 │    │
│  │  - ApplicationUser          │    │
│  │  - ApplicationRole          │    │
│  └─────────────────────────────┘    │
└─────────────────────────────────────┘
┌─────────────────────────────────────┐
│      Infrastructure Layer           │
│  ┌─────────────────────────────┐    │
│  │    Repositories             │    │
│  │  - UserRepository           │    │
│  │  - GenericRepository        │    │
│  └─────────────────────────────┘    │
└─────────────────────────────────────┘
```

## API Endpoints

### Authentication Endpoints

#### 1. User Registration
```http
POST /api/Identity/Auth/register
Content-Type: application/json

{
  "fullName": "John Doe",
  "email": "john@example.com",
  "phoneNumber": "+1234567890",
  "password": "SecurePass123!",
  "confirmPassword": "SecurePass123!",
  "userType": "User"
}
```

#### 2. Admin Registration
```http
POST /api/Identity/Auth/register-admin
Content-Type: application/json

{
  "fullName": "Admin User",
  "email": "admin@example.com",
  "phoneNumber": "+1234567890",
  "password": "AdminPass123!",
  "confirmPassword": "AdminPass123!",
  "adminRegistrationCode": "ADMIN2024BIKYA",
  "registrationReason": "System administration"
}
```

#### 3. User Login
```http
POST /api/Identity/Auth/login
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "SecurePass123!",
  "rememberMe": false
}
```

#### 4. Refresh Token
```http
POST /api/Identity/Auth/refresh
Content-Type: application/json

{
  "token": "expired_jwt_token",
  "email": "john@example.com"
}
```

#### 5. Forgot Password
```http
POST /api/Identity/Auth/forgot-password
Content-Type: application/json

{
  "email": "john@example.com"
}
```

#### 6. Reset Password
```http
POST /api/Identity/Auth/reset-password
Content-Type: application/json

{
  "email": "john@example.com",
  "token": "reset_token",
  "newPassword": "NewSecurePass123!"
}
```

### User Management Endpoints

#### 1. Get User Profile
```http
GET /api/Identity/Users/me
Authorization: Bearer {jwt_token}
```

#### 2. Update Profile
```http
PUT /api/Identity/Users/profile
Authorization: Bearer {jwt_token}
Content-Type: application/json

{
  "fullName": "John Updated",
  "address": "123 Main St",
  "profileImageUrl": "https://example.com/image.jpg"
}
```

#### 3. Change Password
```http
PUT /api/Identity/Users/change-password
Authorization: Bearer {jwt_token}
Content-Type: application/json

{
  "currentPassword": "OldPass123!",
  "newPassword": "NewPass123!",
  "confirmNewPassword": "NewPass123!"
}
```

### Admin Management Endpoints

#### 1. Get All Users (Admin Only)
```http
GET /api/Identity/AdminUsers?search=john&status=active&page=1&pageSize=10
Authorization: Bearer {admin_jwt_token}
```

#### 2. Update User (Admin Only)
```http
PUT /api/Identity/AdminUsers/{userId}
Authorization: Bearer {admin_jwt_token}
Content-Type: application/json

{
  "fullName": "Updated Name",
  "phoneNumber": "+1234567890",
  "role": "User"
}
```

#### 3. Lock/Unlock User (Admin Only)
```http
POST /api/Identity/AdminUsers/{userId}/lock
Authorization: Bearer {admin_jwt_token}

POST /api/Identity/AdminUsers/{userId}/unlock
Authorization: Bearer {admin_jwt_token}
```

## Configuration

### JWT Settings (appsettings.json)
```json
{
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-here-minimum-32-characters",
    "Issuer": "http://localhost:65162/",
    "Audience": "http://localhost:4200/",
    "ExpirationInMinutes": "1440"
  },
  "AdminRegistration": {
    "Code": "ADMIN2024BIKYA",
    "RequireApproval": false,
    "MaxAdmins": 10
  }
}
```

### Identity Configuration
```csharp
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.RequireUniqueEmail = true;
})
```

## Security Best Practices

### 1. Password Requirements
- Minimum 6 characters
- Must contain uppercase, lowercase, digit, and special character
- Validated using regex pattern

### 2. Token Security
- JWT tokens with 60-minute expiration
- Secure key generation using HMAC-SHA256
- Token validation with issuer and audience verification

### 3. Account Protection
- Account lockout after 5 failed attempts
- Soft delete for user accounts
- Email verification system

### 4. Input Validation
- Comprehensive data annotations
- Server-side validation
- SQL injection protection through Entity Framework

## Error Handling

The system uses a unified `ApiResponse<T>` format for all responses:

### Success Response
```json
{
  "success": true,
  "statusCode": 200,
  "message": "تم تسجيل الدخول بنجاح",
  "data": {
    "token": "jwt_token_here",
    "email": "user@example.com",
    "fullName": "John Doe",
    "userId": 1,
    "roles": ["User"]
  },
  "timestamp": "2024-01-01T12:00:00Z"
}
```

### Error Response
```json
{
  "success": false,
  "statusCode": 400,
  "message": "كلمة المرور مطلوبة",
  "errors": ["Password is required"],
  "timestamp": "2024-01-01T12:00:00Z"
}
```

## Usage Examples

### Frontend Integration (JavaScript)
```javascript
// Login
const loginResponse = await fetch('/api/Identity/Auth/login', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    email: 'user@example.com',
    password: 'Password123!'
  })
});

const loginData = await loginResponse.json();
if (loginData.success) {
  localStorage.setItem('token', loginData.data.token);
  localStorage.setItem('user', JSON.stringify(loginData.data));
}

// Authenticated Request
const response = await fetch('/api/Identity/Users/me', {
  headers: {
    'Authorization': `Bearer ${localStorage.getItem('token')}`
  }
});
```

### Role-based Access Control
```csharp
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    // Only admins can access
}

[Authorize(Roles = "User,Admin")]
public class UserController : ControllerBase
{
    // Both users and admins can access
}
```

## Testing

### Swagger Documentation
Access the interactive API documentation at:
```
http://localhost:65162/swagger
```

### Test Users
You can create test users using the registration endpoints or seed them in the database.

## Deployment Considerations

1. **Change JWT Secret Key** in production
2. **Use HTTPS** in production
3. **Configure proper CORS** for your frontend domain
4. **Set up email service** for password reset functionality
5. **Configure logging** for production monitoring
6. **Use secure database connection** strings

## Troubleshooting

### Common Issues

1. **JWT Token Expired**: Use refresh token endpoint
2. **Invalid Credentials**: Check email/password combination
3. **Account Locked**: Wait for lockout period or contact admin
4. **CORS Issues**: Verify CORS configuration matches frontend domain

### Logs
Check application logs for detailed error information and security events.

## Support

For issues or questions about the authentication system, please refer to the application logs or contact the development team. 