# LitBot Backend (.NET)

A .NET 9 Web API backend for LitBot - an AI-powered research paper assistant that provides authentication services via Supabase.

## Prerequisites

- **.NET 9 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Supabase Account** - You'll need Supabase URL and API key (will be provided separately)

## Project Setup

### 1. Clone the Repository

```bash
git clone <repository-url>
cd LitBot-Backend-Dotnet
```

### 2. Configure User Secrets

The application requires Supabase credentials to be configured via .NET User Secrets. Choose your preferred IDE method below:

#### Visual Studio

1. Right-click on the `LitBot.API` project in Solution Explorer
2. Select **"Manage User Secrets"**
3. Add the following JSON structure:

```json
{
  "SUPABASE_URL": "your-supabase-url-here",
  "SUPABASE_KEY": "your-supabase-anon-key-here"
}
```

#### Visual Studio Code / Command Line

1. Navigate to the API project directory:
   ```bash
   cd src/LitBot.API
   ```

2. Initialize user secrets:
   ```bash
   dotnet user-secrets init
   ```

3. Set the Supabase credentials:
   ```bash
   dotnet user-secrets set "SUPABASE_URL" "your-supabase-url-here"
   dotnet user-secrets set "SUPABASE_KEY" "your-supabase-anon-key-here"
   ```

#### JetBrains Rider

1. Right-click on the `LitBot.API` project
2. Go to **Tools** → **User Secrets**
3. Add the secrets in the JSON format shown above

### 3. Run the Application

From the solution root directory:

```bash
dotnet run --project src/LitBot.API
```

Or if you prefer to run from the API project directory:

```bash
cd src/LitBot.API
dotnet run
```

The API will start on:
- **HTTP**: `http://localhost:5170`
- **HTTPS**: `https://localhost:7226` (if HTTPS is enabled)

### 4. Access Swagger Documentation

Once the application is running, navigate to the base URL in your browser. It will automatically redirect you to the Swagger documentation where you can explore and test all available endpoints.

## Important Authentication Behaviors

### Password Management

The API provides two distinct password management workflows:

#### 1. Change Password (Authenticated Users)
- **Endpoint**: `POST /api/auth/change-password`
- **Requirements**: User must be logged in
- **Payload**: `currentPassword` and `newPassword`
- **Use Case**: Users who know their current password and want to change it

#### 2. Forgot/Reset Password (Email-based Recovery)
This is a two-step process:

**Step 1 - Request Reset:**
- **Endpoint**: `POST /api/auth/forgot-password`
- **Payload**: `email`
- **Result**: User receives a password reset email with a link

**Step 2 - Reset Password:**
- **Endpoint**: `POST /api/auth/reset-password`
- **Payload**: `token` (from email link) and `newPassword`
- **Use Case**: Users who forgot their password

### ⚠️ Important Configuration Note

**For Frontend Developers**: The password reset email callback URL must be configured in Supabase to point to your frontend application. This URL should lead to a page where:

1. The user can enter their new password
2. Your frontend can extract the `token` from the URL parameters
3. Both the `token` and `newPassword` are sent to the `reset-password` endpoint

Please ensure this callback URL is properly configured in your Supabase project settings under Authentication → URL Configuration.

## Project Structure

```
src/
├── LitBot.API/              # Web API layer (controllers, authentication, middleware)
├── LitBot.Application/      # Infrastructure layer (services, Supabase integration)
├── LitBot.Contract/         # DTOs and contracts
```

## Authentication & Security

- **Authentication**: Cookie-based authentication using Supabase
- **Authorization**: JWT tokens stored in HTTP-only cookies
- **CORS**: Configured for local development (ports 3000, 5079) - YOU MAY NEED TO ADD ADDITIONAL!!
- **Security Headers**: Automatically applied (X-Content-Type-Options, X-Frame-Options, etc.)

## API Documentation

All endpoints are fully documented with OpenAPI/Swagger. When running the application, visit the base URL to access the interactive documentation where you can:

- View all available endpoints
- See request/response schemas
- Test endpoints directly from the browser
- View authentication requirements

## Development Notes

- The application uses Serilog for logging to console
- Health checks are available at `/health`
- Global exception handling is configured
- Response compression is enabled
- The project follows Clean Architecture principles

## Troubleshooting

### Common Issues

1. **"Supabase URL not configured" error**
    - Ensure user secrets are properly set up with valid Supabase credentials

2. **CORS errors from frontend**
    - Verify your frontend URL is included in the CORS policy in `ApiServiceRegistration.cs`

3. **Authentication failures**
    - Check that your Supabase project is properly configured
    - Verify the Supabase anon key has the correct permissions

### Getting Help

- Check the Swagger documentation for endpoint details
- Review the console logs for detailed error information
- Ensure all prerequisites are installed and configured correctly