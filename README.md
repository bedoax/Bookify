# 📚 Bookify - Audio & PDF Book Platform

Bookify is a scalable backend system for managing books (PDF & Audio), users, subscriptions, payments, and analytics.
It is designed using clean architecture principles and built to support real-world SaaS features.

---

## 🚀 Features

### 👤 Authentication & User Management

* User Registration & Login (JWT-based)
* Role-based Authorization
* Profile Management (Update user data)
* Secure Password Handling (Hashing & Verification)
* Password Recovery:

  * Change Password
  * Forgot Password (Email with reset code)
  * Reset Password

---

### 📚 Books System

* Create / Update / Delete Books
* Search & Filter Books
* Get Books by Author
* Support for:

  * 📄 PDF Books
  * 🎧 Audio Books
* Monetization Control (Free / Paid Books)

---

### ✍️ Authors

* Create, Update, Delete Authors
* Pagination Support
* Author-based Book Filtering

---

### 🏷️ Tags

* Create, Update, Delete Tags
* Assign Tags to Books (Many-to-Many)
* Tag-based filtering

---

### 📁 Collections

* Users can create collections
* Add books to collections
* Manage personal libraries

---

### ⭐ Favorites & Engagement

* Add/Remove favorite books
* Track user engagement
* Metrics used in analytics

---

### 🔔 Notifications

* Get user notifications (Paginated)
* Mark notifications as read
* Delete notifications
* Designed for future real-time support (SignalR)

---

### 💳 Payments (Stripe Integration)

* Stripe PaymentIntent integration
* Secure payment handling
* Webhook support for payment confirmation
* Metadata-based linking (userId + planId)

---

### 📦 Subscription System

* Subscribe / Unsubscribe
* Check access to books
* Subscription history tracking
* Payment tracking

#### 🧠 Rank-Based Subscription Design

The system uses a **rank-based approach** for subscription plans:

* Each plan has a **rank level**
* Higher rank → more privileges
* Access control is determined by comparing ranks instead of hardcoded rules

✅ Benefits:

* Scalable plan hierarchy
* Easy upgrades/downgrades
* Flexible feature control
* Clean authorization logic

Example:

| Plan    | Rank |
| ------- | ---- |
| Free    | 10    |
| Premium | 20    |
| Gold    | 30    |

---

### 📊 Analytics

* Top Rated Books
* Most Favorited Books
* Most Read Books
* Most Listened Audio Books
* Most Reviewed Books
* Most Active Users
* Subscription Metrics
* Dashboard Aggregation Endpoint

---

### ⚡ Performance & Optimization

* Pagination across multiple endpoints
* Efficient queries with LINQ & projections
* Optimistic Concurrency (RowVersion)
* Transaction handling for critical operations
* ExecuteUpdate for bulk updates

---

### 🛡️ Security

* JWT Authentication
* Role-based access control
* Input validation
* Secure password hashing
* Safe password reset flow (code-based with expiration)

---

### ☁️ Storage

* Integration with Supabase Storage
* Public URLs for:

  * PDF files
  * Audio files

---

## 🏗️ Architecture

* Clean Architecture (Controllers → Services → Data Layer)
* DTO-based communication
* ServiceResponse wrapper for consistent responses
* Dependency Injection across services

---

## 🛠️ Tech Stack

* ASP.NET Core Web API
* Entity Framework Core
* SQL Server
* Stripe API
* Supabase Storage
* JWT Authentication

---

## 🔮 Future Improvements

* Real-time notifications using SignalR
* Full Stripe Subscription API (auto-renew)
* Caching (Redis)
* Background Jobs (Hangfire)
* Rate Limiting
* Admin Dashboard
* Recommendation System

---

## 📌 Getting Started

1. Clone the repository
2. Configure appsettings.json:

   * Database connection
   * JWT settings
   * Stripe keys
3. Run migrations:

   ```bash
   dotnet ef database update
   ```
4. Run the project:

   ```bash
   dotnet run
   ```

---

## 👨💻 Author

Developed by Abdelrahman
Passionate about building scalable backend systems and clean architectures 🚀
