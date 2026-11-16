# RUGENDO – Bus Ticket Booking and Management System

Desktop implementation (C# WinForms, .NET 8) of the RUGENDO SRS v1.0, backed by **MariaDB**
and structured with the **MVC** pattern.

## Setup

1. Install the .NET 8 SDK (Windows) and MariaDB 10.4+.
2. Create the database (also creates user `rugendo_user`):
   ```
   mariadb -u root -p < database/schema.sql
   ```
3. Edit `appsettings.json` if your MariaDB host, user or password differ.
4. Run:
   ```
   dotnet run
   ```
   NuGet packages restore automatically (Dapper, MySqlConnector, BCrypt.Net-Next).

On first launch the app seeds demo data:

| Role     | Email                | Password    |
|----------|----------------------|-------------|
| WORKER   | worker@rugendo.rw    | Worker123   |
| CUSTOMER | customer@rugendo.rw  | Customer123 |

New customers can also register from the login screen. Worker accounts are created by the
company/administrator in the database (public sign-up only creates CUSTOMER accounts).

## MVC structure

| Layer | Folder | Responsibility |
|-------|--------|----------------|
| Model | `Models/`, `Data/` | Entities/DTOs and repositories (all SQL, parameterised) |
| View | `Views/` | WinForms screens; no SQL, no business rules |
| Controller | `Controllers/` | Validation, role/company authorisation, calls repositories, returns `Result` objects |

Flow: **View → Controller → Repository → MariaDB**. Views never touch the database directly.

## SRS traceability

| Requirement | Implemented in |
|-------------|----------------|
| FR-01 Registration | `AuthController.Register`, `RegisterForm` |
| FR-02 Authentication | `AuthController.Login/Logout`, `LoginForm`, BCrypt hashing |
| FR-03 Roles | `Session.Worker/Customer`, separate `CustomerForm` / `WorkerForm` |
| FR-04 Company | `CompanyController`, Company tab (own company only) |
| FR-05 Routes | `RouteController`, Routes tab (remove = deactivate, history kept) |
| FR-06 Schedules | `ScheduleController`, Schedules tab |
| FR-07 Trip search | `ScheduleController.Search`, Search & Buy tab |
| FR-08 Ticket formats | `TicketFormatController`, Ticket formats tab |
| FR-09 Purchase | `TicketRepository.Purchase` (transaction + row lock, no overselling) |
| FR-10 Ticket management | `TicketController`, My Tickets / Company tickets tabs |
| FR-11 Booking history | My Tickets & History tab |
| FR-12 Receipts | `receipts` table, `ReceiptForm`, save as .txt |
| FR-13 Validation/errors | `Validator`, `Result`, `Safe.Run`, DB CHECK constraints |
| NFR-02 Security | BCrypt passwords, parameterised SQL, server-side ownership checks |