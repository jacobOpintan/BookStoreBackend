📚 BookStore API - ASP.NET Core
A secure, modern RESTful API for a bookstore built using ASP.NET Core. It includes core features like JWT authentication, role-based authorization, email verification, password reset, logging, and robust querying (filtering, sorting, searching, pagination). Designed for learning how to build production-ready APIs.

🚀 Features
✅ User Registration & Login with JWT tokens

✅ Role-based Authorization (Admin, User)

✅ Email Confirmation (via Mailtrap)

✅ Password Reset with secure token link

✅ Filtering, Sorting, Search, and Pagination on data

✅ Centralized Logging (using built-in ILogger)

✅ Clean Architecture & DTOs

✅ Entity Framework Core + SQL Server

✅ Secure Credential Storage via Secret Manager

🔧 Technologies
ASP.NET Core 7.0

Entity Framework Core

SQL Server

MailKit (for email)

JWT Bearer Authentication


📁 Project Structure

BookStoreAPI/
├── Controllers/           # API Controllers
├── Dtos/                  # Data Transfer Objects
├── Helpers/               # Email, logging helpers, etc.
├── Models/                # ApplicationUser, domain models
├── Data/                  # DB Context, Seeding
├── Program.cs             # App startup logic
├── appsettings.json       # App configuration
└── README.md              # Project documentation
🔐 Authentication & Roles
New users must confirm their email to log in.

Only Admin users can assign roles.

JWT tokens are used for secure authentication and role validation.

✉️ Email Integration
Emails (for reset password and confirmation) are sent using Mailtrap.

To configure it:

Create an account at Mailtrap.io

Use the SMTP credentials in your Secret Manager like this:


dotnet user-secrets set "MailSettings:Username" "your-mailtrap-username"
dotnet user-secrets set "MailSettings:Password" "your-mailtrap-password"
🧪 Testing
You can test the API using:

Swagger UI (navigate to /swagger)

Postman or any REST client

Example flow:

Register → Confirm Email → Login → Use JWT for Auth → Reset Password if needed

✅ To Run Locally
Clone the repo

Update your DB connection string in appsettings.json

Run EF migrations:


dotnet ef database update
Run the app:


dotnet run
📌 Next Steps / Improvements
Add unit & integration tests

Deploy to cloud (e.g., Azure, Render)

Add shopping cart & order modules

Add rate-limiting & performance profiling

🙌 Author

Built with passion by Opintan Jacob , aspiring backend developer using ASP.NET Core.
This project is for learning purposes and may evolve with more features.
