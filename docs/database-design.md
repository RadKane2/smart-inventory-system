# Database Design

## Users

- UserId
- Name
- Email
- PasswordHash
- RoleId
- Status

## Roles

- RoleId
- RoleName

## Products

- ProductId
- Name
- Description
- Price
- Stock
- MinimumStock

## Orders

- OrderId
- UserId
- Total
- Status

## AuditLogs

- AuditLogId
- UserId
- Action
- Date

## Categories

- CategoryId
- Name
- Description

## InventoryMovements

- MovementId
- ProductId
- Quantity
- Type
- UserId
- Date

## OrderDetails

- OrderDetailId
- OrderId
- ProductId
- Quantity
- UnitPrice

## Notifications

- NotificationId
- Title
- Message
- IsRead
- CreatedAt

## Relationships

Roles (1) -> (N) Users

Users (1) -> (N) Orders

Orders (1) -> (N) OrderDetails

Products (1) -> (N) OrderDetails

Products (1) -> (N) InventoryMovements

Users (1) -> (N) AuditLogs

Users (1) -> (N) Notifications

```mermaid
erDiagram

    Roles {
        int RoleId PK
        string RoleName
    }

    Users {
        int UserId PK
        string Name
        string Email
        string PasswordHash
        int RoleId FK
        string Status
        DATETIME CreatedAt
        DATETIME LastLogin
    }

    Products {
        int ProductId PK
        string Name
        string Description
        decimal Price
        int Stock
        int MinimumStock
        VARCHAR(500) ImageUrl
        string Status
        DATETIME CreatedAt
    }

    Orders {
        int OrderId PK
        int UserId FK
        decimal Total
        string Status
        DATETIME CreatedAt
        int OrderNumber
    }

    OrderDetails {
        int OrderDetailId PK
        int OrderId FK
        int ProductId FK
        int Quantity
        decimal UnitPrice
    }

    InventoryMovements {
        int MovementId PK
        int ProductId FK
        int UserId FK
        int Quantity
        string Type
    }

    Notifications {
        int NotificationId PK
        int UserId FK
        string Title
        string Message
    }

    AuditLogs {
        int AuditLogId PK
        int UserId FK
        string Action
        string Module
        CURRENT_DATE Date
        string Description
    }

    Roles ||--o{ Users : has
    Users ||--o{ Orders : places
    Orders ||--o{ OrderDetails : contains
    Products ||--o{ OrderDetails : included_in
    Products ||--o{ InventoryMovements : tracked_by
    Users ||--o{ InventoryMovements : performs
    Users ||--o{ Notifications : receives
    Users ||--o{ AuditLogs : generates
```