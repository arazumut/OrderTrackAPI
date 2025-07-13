# 🍽️ OrderTrack: Smart Food Order Tracking API

## 🎯 Purpose
A RESTful API developed for local restaurants, allowing customers to place orders via mobile application, restaurants to manage orders and assign them to couriers, and track the entire order process.

## ✅ Features
- .NET Core Web API
- JWT Authentication
- Swagger documentation (Swashbuckle)
- Role-based authorization: Customer, Restaurant, Courier
- Restaurants can only manage their own products/orders

## 🔧 Technologies
- .NET 8 Web API
- Entity Framework Core
- Swashbuckle (Swagger UI)
- FluentValidation
- SQLite
- JWT Authentication

## 🚀 API Endpoints

### 🔐 Authentication System
- `POST /auth/register` - Register a new user
- `POST /auth/login` - Login and get JWT token
- `GET /auth/me` - Get current user information

### 🏪 Restaurant Management
- `GET /restaurants` - Get all restaurants
- `GET /restaurants/{id}` - Get restaurant by ID
- `POST /restaurants` - Create restaurant (only during registration)
- `PUT /restaurants/{id}` - Update restaurant information

### 🍔 Product Management
- `GET /products` - Get all products
- `GET /restaurants/{id}/products` - Get products for a specific restaurant
- `POST /products` - Add new product (restaurant only)
- `PUT /products/{id}` - Update product
- `DELETE /products/{id}` - Delete product

### 🛒 Order Management
- `POST /orders` - Create a new order
- `GET /orders` - Get customer's orders
- `GET /restaurant/orders` - Get restaurant's orders
- `GET /courier/orders` - Get courier's assigned orders
- `PATCH /orders/{id}/status` - Update order status
- `PATCH /orders/{id}/assign-courier` - Assign courier to an order

### 🛵 Courier Management
- `GET /couriers` - Get all couriers
- `POST /couriers` - Add new courier
- `PUT /couriers/{id}` - Update courier information
- `DELETE /couriers/{id}` - Delete courier

## 🌟 Optional Advanced Features
- Estimated delivery time calculation
- Order rating and comments (`POST /orders/{id}/rate`)
- Order history (audit trail)
- Mock courier location (with static data)

## 🧪 Swagger Requirements
- Each endpoint includes summary and description
- Clear display of enum definitions
- Authorized requests can be tested with JWT

## 📦 Getting Started

### Prerequisites
- .NET 8 SDK
- Visual Studio, VS Code, or another IDE with C# support

### Installation
1. Clone the repository
   ```
   git clone https://github.com/yourusername/OrderTrack.git
   cd OrderTrack
   ```

2. Restore packages
   ```
   dotnet restore
   ```

3. Run database migrations
   ```
   dotnet ef database update
   ```

4. Run the application
   ```
   dotnet run
   ```

5. Access Swagger UI at `https://localhost:5001/swagger`

## 📌 Notes
- Code comments and endpoint descriptions should be clear
- Each user can only access their own data
- Testability via Swagger is a priority
