# Clinic Booking System

A modern, scalable clinic appointment booking system built with ASP.NET Core Web API (Backend) and Blazor WebAssembly (Frontend). The project follows SOLID principles and clean architecture patterns.

## 📋 Table of Contents

- [Project Structure](#project-structure)
- [Architecture](#architecture)
- [SOLID Principles Implementation](#solid-principles-implementation)
- [Features](#features)
- [API Endpoints](#api-endpoints)
- [Getting Started](#getting-started)
- [Running Tests](#running-tests)

## 🏗️ Project Structure

```
Clinic-Booking-System/
├── Backend/
│   ├── src/
│   │   ├── ClinicBookingSystem.Domain/          # Domain layer (entities, interfaces)
│   │   │   ├── Entities/
│   │   │   │   ├── Patient.cs
│   │   │   │   ├── Doctor.cs
│   │   │   │   ├── Appointment.cs
│   │   │   │   └── TimeSlot.cs
│   │   │   ├── Interfaces/
│   │   │   │   ├── IBookingService.cs
│   │   │   │   ├── ITimeSlotAvailabilityService.cs
│   │   │   │   ├── IAppointmentConfirmationService.cs
│   │   │   │   ├── IRepository.cs
│   │   │   │   ├── IAppointmentRepository.cs
│   │   │   │   └── ITimeSlotRepository.cs
│   │   │   └── GlobalUsings.cs
│   │   │
│   │   ├── ClinicBookingSystem.Application/      # Application layer (business logic)
│   │   │   ├── Services/
│   │   │   │   ├── BookingService.cs
│   │   │   │   ├── TimeSlotAvailabilityService.cs
│   │   │   │   └── EmailConfirmationService.cs
│   │   │   └── GlobalUsings.cs
│   │   │
│   │   ├── ClinicBookingSystem.Infrastructure/   # Infrastructure layer (data access)
│   │   │   ├── Repositories/
│   │   │   ├── Data/
│   │   │   └── Services/
│   │   │
│   │   └── ClinicBookingSystem.API/              # API layer (controllers, DTOs)
│   │       ├── Controllers/
│   │       │   ├── AppointmentsController.cs
│   │       │   └── TimeSlotsController.cs
│   │       ├── DTOs/
│   │       │   └── AppointmentDTOs.cs
│   │       └── GlobalUsings.cs
│   │
│   └── tests/
│       ├── ClinicBookingSystem.Application.Tests/  # Unit tests for services
│       │   ├── BookingServiceTests.cs
│       │   ├── TimeSlotAvailabilityServiceTests.cs
│       │   └── GlobalUsings.cs
│       └── ClinicBookingSystem.Infrastructure.Tests/
│
├── Frontend/
│   ├── src/
│   │   ├── Models/
│   │   │   └── AppointmentModels.cs
│   │   ├── Services/
│   │   │   ├── AppointmentService.cs
│   │   │   └── TimeSlotService.cs
│   │   ├── Pages/
│   │   │   ├── BookAppointment.razor
│   │   │   └── MyAppointments.razor
│   │   └── Components/
│   │
│   └── tests/
│
└── README.md
```

## 🏛️ Architecture

The system follows a **Layered Clean Architecture** pattern:

### Domain Layer
- Contains core business entities and abstractions
- **No external dependencies**
- Defines interfaces for repositories and services

### Application Layer
- Implements business logic and use cases
- Depends on Domain layer abstractions
- Contains service implementations

### Infrastructure Layer
- Implements data access (repositories)
- Contains EF Core DbContext and configurations
- Implements external service integrations

### API Layer (Presentation)
- ASP.NET Core controllers
- Data Transfer Objects (DTOs)
- REST endpoints
- Depends on Application and Domain layers

## 🎯 SOLID Principles Implementation

### 1. **Single Responsibility Principle (SRP)**
Each class has one reason to change:
- `BookingService` - Handles only booking logic
- `TimeSlotAvailabilityService` - Handles only availability checks
- `EmailConfirmationService` - Handles only confirmation notifications
- `AppointmentsController` - Handles only appointment HTTP requests

**Example:**
```csharp
// SRP: Service focuses only on availability
public class TimeSlotAvailabilityService : ITimeSlotAvailabilityService
{
    public async Task<IEnumerable<TimeSlot>> GetAvailableSlotsByDoctorAsync(...)
    public async Task<bool> IsSlotAvailableAsync(...)
    public async Task<bool> ReserveSlotAsync(...)
}
```

### 2. **Open/Closed Principle (OCP)**
Classes are open for extension, closed for modification:
- Services use dependency injection for extensions
- Strategy pattern for confirmation services (email, SMS, etc.)
- Repositories can be extended without modifying existing code

**Example:**
```csharp
// Extendable: Can add SmsConfirmationService without modifying EmailConfirmationService
public interface IAppointmentConfirmationService
{
    Task<bool> SendConfirmationAsync(int appointmentId);
    Task<bool> SendCancellationAsync(int appointmentId);
}
```

### 3. **Liskov Substitution Principle (LSP)**
Derived classes can be substituted for their base classes:
- All `IRepository<T>` implementations are interchangeable
- All `IAppointmentConfirmationService` implementations can replace each other
- Appointment status transitions follow proper inheritance

### 4. **Interface Segregation Principle (ISP)**
Clients depend on specific, narrowly-focused interfaces:
- Separate interfaces for different responsibilities
- `IAppointmentRepository` extends `IRepository<Appointment>` with specific methods
- `ITimeSlotRepository` extends `IRepository<TimeSlot>` with specific methods

**Example:**
```csharp
// Segregated: Services only depend on what they need
public interface IAppointmentRepository : IRepository<Appointment>
{
    Task<IEnumerable<Appointment>> GetByPatientIdAsync(int patientId);
    Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId);
}
```

### 5. **Dependency Inversion Principle (DIP)**
High-level modules depend on abstractions, not concrete implementations:
- Services depend on interfaces, not concrete classes
- Constructor injection for all dependencies
- Easy to swap implementations for testing

**Example:**
```csharp
// DIP: Depends on abstractions (interfaces)
public class BookingService : IBookingService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ITimeSlotAvailabilityService _timeSlotAvailabilityService;
    
    public BookingService(
        IAppointmentRepository appointmentRepository,
        ITimeSlotAvailabilityService timeSlotAvailabilityService,
        IAppointmentConfirmationService confirmationService)
    {
        // All injected as interfaces!
    }
}
```

## ✨ Features

### Booking Logic
- **Book Appointments**: Create new appointments with validation
- **Automatic Confirmation**: Appointments are auto-confirmed upon booking
- **Conflict Detection**: Prevents double-booking by checking availability
- **Duration-based Booking**: Support for customizable appointment durations

### Time Slot Availability
- **Check Availability**: Query available slots for a doctor
- **Date Range Filtering**: Get available slots within specific periods
- **Conflict Prevention**: Automatically prevents overlapping appointments
- **Slot Management**: Reserve and manage time slots

### Confirmation Messages
- **Email Notifications**: Send confirmation emails to patients
- **Cancellation Notifications**: Notify patients of cancellations
- **Extensible Design**: Easy to add SMS, push notifications, etc.

## 📡 API Endpoints

### Appointments

#### Book Appointment
```http
POST /api/appointments/book
Content-Type: application/json

{
  "patientId": 1,
  "doctorId": 1,
  "appointmentDateTime": "2024-12-20T10:00:00Z",
  "durationInMinutes": 30,
  "notes": "Regular checkup"
}

Response: 201 Created
{
  "success": true,
  "message": "Appointment booked successfully",
  "appointmentId": 1
}
```

#### Get Appointment
```http
GET /api/appointments/{id}

Response: 200 OK
{
  "id": 1,
  "patientId": 1,
  "doctorId": 1,
  "appointmentDateTime": "2024-12-20T10:00:00Z",
  "durationInMinutes": 30,
  "status": "Confirmed",
  "notes": "Regular checkup",
  "createdAt": "2024-12-13T10:00:00Z",
  "confirmedAt": "2024-12-13T10:00:00Z"
}
```

#### Get Patient Appointments
```http
GET /api/appointments/patient/{patientId}

Response: 200 OK
[{ ... appointment details ... }]
```

#### Get Doctor Appointments
```http
GET /api/appointments/doctor/{doctorId}

Response: 200 OK
[{ ... appointment details ... }]
```

#### Cancel Appointment
```http
DELETE /api/appointments/{id}

Response: 204 No Content
```

### Time Slots

#### Get Available Slots
```http
GET /api/timeslots/available/{doctorId}?from=2024-12-20T00:00:00Z&to=2024-12-27T00:00:00Z

Response: 200 OK
[
  {
    "id": 1,
    "doctorId": 1,
    "startDateTime": "2024-12-20T09:00:00Z",
    "endDateTime": "2024-12-20T09:30:00Z",
    "durationInMinutes": 30,
    "isAvailable": true
  }
]
```

#### Check Availability
```http
GET /api/timeslots/check-availability?doctorId=1&startDateTime=2024-12-20T09:00:00Z&endDateTime=2024-12-20T09:30:00Z

Response: 200 OK
{
  "available": true
}
```

## 🚀 Getting Started

### Prerequisites
- .NET 9.0 SDK or later
- A modern web browser

### Quick Start - Running the Application

#### 1. Run the Backend API

Open a terminal and navigate to the Backend API directory:

```bash
cd Backend/src/ClinicBookingSystem.API
```

Then run the API:

```bash
dotnet run
```

The API will start on `http://localhost:5220` (HTTP) and `https://localhost:7220` (HTTPS).

#### 2. Run the Frontend (Blazor WASM)

Open a **new terminal** and navigate to the Frontend directory:

```bash
cd Frontend
```

Then run the frontend:

```bash
dotnet run
```

The frontend will start on `http://localhost:5173` (or similar port). Open this URL in your browser.

> **Note**: Make sure the Backend API is running before using the Frontend.

### Backend Setup (Development)

1. **Create ASP.NET Core Web API project**
   ```bash
   dotnet new webapi -n ClinicBookingSystem.API
   ```

2. **Create Class Libraries**
   ```bash
   dotnet new classlib -n ClinicBookingSystem.Domain
   dotnet new classlib -n ClinicBookingSystem.Application
   dotnet new classlib -n ClinicBookingSystem.Infrastructure
   ```

3. **Add Project References**
   ```bash
   cd ClinicBookingSystem.API
   dotnet add reference ../ClinicBookingSystem.Application
   dotnet add reference ../ClinicBookingSystem.Domain
   ```

4. **Install NuGet Packages**
   ```bash
   dotnet add package Microsoft.EntityFrameworkCore
   dotnet add package Microsoft.EntityFrameworkCore.SqlServer
   ```

5. **Configure Dependency Injection in Program.cs**
   ```csharp
   builder.Services.AddScoped<IBookingService, BookingService>();
   builder.Services.AddScoped<ITimeSlotAvailabilityService, TimeSlotAvailabilityService>();
   builder.Services.AddScoped<IAppointmentConfirmationService, EmailConfirmationService>();
   ```

6. **Run the API**
   ```bash
   dotnet run
   ```

### Frontend Setup

1. **Create Blazor WebAssembly App**
   ```bash
   dotnet new blazorwasm -n ClinicBookingSystem.Frontend
   ```

2. **Add Services to Program.cs**
   ```csharp
   builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7001") });
   builder.Services.AddScoped<AppointmentService>();
   builder.Services.AddScoped<TimeSlotService>();
   ```

3. **Run the Frontend**
   ```bash
   dotnet run
   ```

## 🧪 Running Tests

The project uses **xUnit** as the testing framework and **Moq** for mocking dependencies.

### Run All Tests

Navigate to the Backend directory and run:

```bash
cd Backend
dotnet test
```

### Run Specific Test Projects

**Application Tests (Unit Tests for Services):**

```bash
cd Backend/tests/ClinicBookingSystem.Application.Tests
dotnet test
```

**Infrastructure Tests:**

```bash
cd Backend/tests/ClinicBookingSystem.Infrastructure.Tests
dotnet test
```

### Run Tests with Verbose Output

```bash
cd Backend
dotnet test --verbosity normal
```

### Run Tests with Coverage

```bash
cd Backend
dotnet test /p:CollectCoverage=true
```

### Test Summary

The project includes **35 unit tests** covering critical booking logic:

| Test Category | Tests | Description |
|--------------|-------|-------------|
| Appointment Booking | 10 | Valid booking, validation errors, double-booking prevention |
| Time Slot Availability | 12 | Available slots, slot generation, booking conflicts |
| Appointment Management | 13 | Get, cancel, update appointments |

### Key Test: Double Booking Prevention

```csharp
[Fact]
public async Task ScheduleAppointmentAsync_WhenDoubleBooking_ShouldThrowInvalidOperationException()
{
    // This critical test ensures no double bookings occur
    // It verifies the HasConflictAsync() validation
}
```

### Test Structure

The project includes comprehensive unit tests:

- **BookingServiceTests**: Tests for appointment booking logic
  - ✅ Valid booking returns success
  - ✅ Invalid patient ID returns failure
  - ✅ Past datetime returns failure
  - ✅ Unavailable slot returns failure
  - ✅ Cancellation works correctly

- **TimeSlotAvailabilityServiceTests**: Tests for availability checking
  - ✅ Get available slots by doctor
  - ✅ Check slot availability
  - ✅ Reserve slot functionality

## 📝 Naming Conventions

The project follows these naming conventions:

- **Classes**: PascalCase (e.g., `BookingService`, `AppointmentModel`)
- **Interfaces**: `IPascalCase` (e.g., `IBookingService`)
- **Methods**: PascalCase (e.g., `BookAppointmentAsync`)
- **Properties**: PascalCase (e.g., `PatientId`)
- **Private Fields**: `_camelCase` (e.g., `_appointmentRepository`)
- **Local Variables**: camelCase (e.g., `appointmentDateTime`)
- **Constants**: UPPER_CASE (e.g., `MAX_DURATION`)

## 🔒 Security Considerations

- ✅ Input validation on all endpoints
- ✅ DateTime validation (past appointments prevention)
- ✅ Entity-level business logic validation
- ⚠️ TODO: Add authentication/authorization
- ⚠️ TODO: Add rate limiting
- ⚠️ TODO: Add HTTPS enforcement
- ⚠️ TODO: Add CORS configuration

## 📚 Design Patterns Used

1. **Repository Pattern**: Data access abstraction
2. **Dependency Injection**: Loose coupling
3. **Service Pattern**: Business logic encapsulation
4. **Strategy Pattern**: Pluggable confirmation services
5. **Factory Pattern**: Could be added for entity creation
6. **DTO Pattern**: Separation of API contracts from domain models

## 🎓 Learning Resources

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Blazor Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/)

## 📄 License

This project is licensed under the MIT License.

## 👥 Contributing

Contributions are welcome! Please follow the established patterns and conventions.

## 📞 Support

For issues or questions, please create an issue in the repository.

---

**Last Updated**: December 15, 2025  
**Version**: 1.0.0