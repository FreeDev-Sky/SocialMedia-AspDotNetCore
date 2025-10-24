# Social Media ASP.NET Core Application

A modern social media application built with ASP.NET Core 8.0 featuring JWT authentication, MVC views, and SQLite database.

## 🚀 Features

- **JWT Authentication** - Secure token-based authentication
- **User Registration & Login** - Complete user management system
- **MVC Views** - Beautiful web interface with Bootstrap
- **API Endpoints** - RESTful API for mobile/frontend integration
- **SQLite Database** - Lightweight, file-based database
- **Entity Framework Core** - Code-first database approach
- **ASP.NET Core Identity** - Built-in user management
- **Role-based Authorization** - Admin and user roles

## 🛠️ Technology Stack

- **Backend**: ASP.NET Core 8.0
- **Database**: SQLite
- **ORM**: Entity Framework Core
- **Authentication**: JWT Bearer Tokens
- **Frontend**: Bootstrap 5, JavaScript
- **IDE**: Visual Studio / VS Code

## 📋 Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Entity Framework Core Tools](https://docs.microsoft.com/en-us/ef/core/cli/dotnet)
- [Git](https://git-scm.com/)

## 🚀 Getting Started

### 1. Clone the Repository
```bash
git clone <repository-url>
cd SocialMedia-AspDotNetCore
```

### 2. Restore Dependencies
```bash
dotnet restore
```

### 3. Install Entity Framework Tools (if not already installed)
```bash
dotnet tool install --global dotnet-ef
```

### 4. Create Database
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 5. Run the Application
```bash
dotnet run
```

The application will be available at:
- **Web Interface**: http://localhost:5000
- **API**: http://localhost:5000/api

## 🌐 Application Endpoints

### Web Views (MVC)
- **Home**: `GET /` - Welcome page
- **Login**: `GET /Home/Login` - User login form
- **Register**: `GET /Home/Register` - User registration form

### API Endpoints
- **Register**: `POST /api/auth/register`
- **Login**: `POST /api/auth/login`
- **Logout**: `POST /api/auth/logout`
- **Profile**: `GET /api/protected/profile` (requires authentication)
- **Admin**: `GET /api/protected/admin` (requires Admin role)

## 📊 Database Schema

The application uses SQLite with the following main tables:
- `AspNetUsers` - User accounts
- `AspNetRoles` - User roles
- `AspNetUserClaims` - User claims
- `AspNetUserLogins` - External logins
- `AspNetUserTokens` - User tokens

## 🔐 Authentication

### JWT Configuration
The application uses JWT tokens for authentication. Configuration is in `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "SocialMediaApp",
    "Audience": "SocialMediaApp",
    "ExpiryMinutes": 60
  }
}
```

### Password Requirements
- Minimum 6 characters
- Must contain uppercase letter
- Must contain lowercase letter
- Must contain digit
- No special characters required

## 🧪 Testing the API

### Register a New User
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "password": "Password123",
    "confirmPassword": "Password123"
  }'
```

### Login
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "Password123"
  }'
```

### Access Protected Endpoint
```bash
curl -X GET http://localhost:5000/api/protected/profile \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## 📁 Project Structure

```
SocialMedia-AspDotNetCore/
├── Controllers/
│   ├── AuthController.cs          # Authentication API
│   ├── HomeController.cs          # MVC Views
│   └── ProtectedController.cs     # Protected API endpoints
├── Models/
│   ├── User.cs                    # User model
│   └── ApplicationDbContext.cs    # Database context
├── DTOs/
│   ├── LoginDto.cs               # Login request model
│   ├── RegisterDto.cs            # Registration request model
│   └── AuthResponseDto.cs        # Authentication response model
├── Services/
│   └── JwtService.cs             # JWT token service
├── Views/
│   ├── Home/
│   │   ├── Index.cshtml          # Home page
│   │   ├── Login.cshtml          # Login page
│   │   └── Register.cshtml       # Registration page
│   └── Shared/
│       └── _Layout.cshtml        # Main layout
├── wwwroot/
│   ├── css/site.css              # Custom styles
│   └── js/site.js                # JavaScript
├── Properties/
│   └── launchSettings.json       # Launch configuration
├── appsettings.json              # Application settings
├── Program.cs                    # Application entry point
└── AspNetCoreEmpty.csproj        # Project file
```

## 🔧 Configuration

### Database Connection
The application uses SQLite by default. The connection string is in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=SocialMediaDb.db"
  }
}
```

### Environment-Specific Settings
- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development settings
- `appsettings.Production.json` - Production settings (ignored by git)

## 🚀 Deployment

### Local Development
```bash
dotnet run
```

### Production Build
```bash
dotnet publish -c Release -o ./publish
```

### Docker (Optional)
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY . /app
WORKDIR /app
EXPOSE 80
ENTRYPOINT ["dotnet", "AspNetCoreEmpty.dll"]
```

## 🛡️ Security Features

- **JWT Authentication** - Secure token-based auth
- **Password Hashing** - ASP.NET Core Identity hashing
- **CORS Configuration** - Cross-origin resource sharing
- **HTTPS Support** - Secure communication
- **Input Validation** - Model validation and sanitization

## 📝 Development Notes

### Adding New Features
1. Create models in `Models/` folder
2. Add controllers in `Controllers/` folder
3. Create views in `Views/` folder
4. Update database with migrations

### Database Changes
```bash
# Add new migration
dotnet ef migrations add FeatureName

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

If you encounter any issues:

1. Check the [Issues](https://github.com/your-repo/issues) page
2. Create a new issue with detailed description
3. Include error logs and steps to reproduce

## 🔄 Version History

- **v1.0.0** - Initial release with authentication and basic features
- **v1.1.0** - Added MVC views and improved UI
- **v1.2.0** - Enhanced security and error handling

---

**Happy Coding! 🚀**
