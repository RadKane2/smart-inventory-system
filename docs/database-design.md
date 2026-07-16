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