# Architecture

Frontend (React + TypeScript + TailwindCSS)
        ↓
REST API (ASP.NET Core)
        ↓
Application Services
        ↓
Repositories
        ↓
SQL Server Database

## Roles

### Administrator

- User Management
- Product Management
- Inventory Management
- Reports
- Audit Logs

### Employee

- Inventory Operations
- Stock Updates
- Notifications

### Customer

- Browse Products
- Place Orders
- View Order History

## Modules

### Authentication

- Login
- Register
- JWT
- Refresh Tokens

### Product Management

- Create Product
- Update Product
- Delete Product

### Inventory Management

- Stock Entries
- Stock Exits
- Inventory Adjustments

### Orders

- Shopping Cart
- Checkout
- Order Tracking

### Notifications

- Low Stock Alerts
- Order Notifications

### Audit Logs

- User Actions
- Inventory Changes
- Product Changes