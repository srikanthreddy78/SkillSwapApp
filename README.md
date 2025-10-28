# SkillSwap API

A .NET Core Web API for managing video sessions and payments in the SkillSwap platform. This API integrates with Agora for video calling functionality and Stripe for payment processing.

## Prerequisites

Before you begin, ensure you have the following installed:
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)
- A [Stripe Account](https://stripe.com/) for payment processing
- An [Agora Account](https://www.agora.io/) for video functionality
- A [Firebase/Firestore Project](https://firebase.google.com/) for data storage

## Installation

### 1. Clone the Repository
```bash
git clone <repository-url>
cd SkillSwapAPI
```

### 2. Install Dependencies

Navigate to the project directory and restore NuGet packages:
```bash
dotnet restore
```

### 3. Install Required NuGet Packages

If packages are not automatically restored, install them manually:
```bash
# Stripe SDK
dotnet add package Stripe.net

# Google Cloud Firestore
dotnet add package Google.Cloud.Firestore

# ASP.NET Core packages (usually included by default)
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.Extensions.Configuration
```

### 4. Configure Application Settings

The application is already configured with all necessary settings. You just need to add your secret keys from the provided secret key file.

**Locate the secret key file** (it should be included in the project or sent separately) and add the values to `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Stripe": {
    "SecretKey": "ADD_FROM_SECRET_KEY_FILE",
    "PublishableKey": "ADD_FROM_SECRET_KEY_FILE",
    "WebhookSecret": "ADD_FROM_SECRET_KEY_FILE"
  },
  "Agora": {
    "AppId": "ADD_FROM_SECRET_KEY_FILE",
    "AppCertificate": "ADD_FROM_SECRET_KEY_FILE"
  },
  "Firebase": {
    "ProjectId": "ADD_FROM_SECRET_KEY_FILE"
  }
}
```

**Note:** Copy the exact values from the secret key file into the corresponding fields in `appsettings.json`.


## Running the Application

### Step 1: Find Your IP Address

First, you need to find your local IP address to make the API accessible on your network.

**Linux/Mac:**
```bash
ifconfig | grep "inet " | grep -v 127.0.0.1
```

**Windows (PowerShell):**
```powershell
Get-NetIPAddress -AddressFamily IPv4 | Where-Object {$_.InterfaceAlias -notlike "*Loopback*"} | Select-Object IPAddress
```

**Windows (Command Prompt):**
```cmd
ipconfig
```

Look for your IPv4 Address (e.g., `192.168.4.60`)

### Step 2: Run the Application with Your IP

Replace `192.168.4.60` with your actual IP address from Step 1:
```bash
dotnet run --urls "http://192.168.4.60:5205"
```

**Example:**
```bash
# If your IP is 192.168.1.100
dotnet run --urls "http://192.168.1.100:5205"
```

The API will be available at:
- Your Network: `http://YOUR_IP_ADDRESS:5205`
- Example: `http://192.168.4.60:5205`


## Verify Installation

Once the application is running, navigate to:
```
http://YOUR_IP_ADDRESS:5205/swagger
```

Example:
```
http://192.168.4.60:5205/swagger
```

You should see the Swagger UI with all available API endpoints.

## Testing the API

### Test Video Token Generation

Replace `YOUR_IP_ADDRESS` with your actual IP:
```bash
curl -X POST http://YOUR_IP_ADDRESS:5205/api/video/generate-token \
  -H "Content-Type: application/json" \
  -d '{
    "channelName": "test-channel",
    "userId": 12345,
    "expirationSeconds": 3600
  }'
```

### Test Payment Intent Creation
```bash
curl -X POST http://YOUR_IP_ADDRESS:5205/api/payment/create-intent \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 50.00,
    "currency": "usd",
    "description": "Test payment"
  }'
```


## Common Issues

### Issue: Cannot Access API from Other Devices
**Solution:**
- Ensure you're using your network IP address (not 127.0.0.1 or localhost)
- Check your firewall settings to allow incoming connections on port 5205
- Make sure both devices are on the same network

### Issue: Firebase Authentication Error
**Solution:** Ensure `GOOGLE_APPLICATION_CREDENTIALS` environment variable is set correctly and the JSON file path is valid.

### Issue: Stripe API Key Invalid
**Solution:** Verify your Stripe keys in `appsettings.json`. Use test keys (starting with `sk_test_`) for development.

### Issue: Port Already in Use
**Solution:** Change the port number in the command or kill the process using the port:
```bash
# Linux/Mac
lsof -ti:5205 | xargs kill -9

# Windows
netstat -ano | findstr :5205
taskkill /PID <process_id> /F
```

### Issue: Address Already in Use
**Solution:** Use a different port number:
```bash
dotnet run --urls "http://YOUR_IP_ADDRESS:5206"
```

### Issue: CORS Errors from Frontend
**Solution:** Ensure CORS is properly configured in `Program.cs` to allow requests from your frontend's origin.

## API Documentation

Once running, access the interactive API documentation at:
- Swagger UI: `http://YOUR_IP_ADDRESS:5205/swagger`
- API JSON: `http://YOUR_IP_ADDRESS:5205/swagger/v1/swagger.json`

