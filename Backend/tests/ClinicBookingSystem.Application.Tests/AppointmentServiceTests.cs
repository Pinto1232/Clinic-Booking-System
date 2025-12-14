using ClinicBookingSystem.Application.Services;
using ClinicBookingSystem.Domain.Entities;
using ClinicBookingSystem.Domain.Interfaces;
using Moq;

namespace ClinicBookingSystem.Application.Tests;

public class AppointmentServiceTests
{
    private readonly Mock<IAppointmentRepository> _mockAppointmentRepository;
    private readonly Mock<IPatientRepository> _mockPatientRepository;
    private readonly Mock<IDoctorRepository> _mockDoctorRepository;
    private readonly AppointmentService _appointmentService;

    public AppointmentServiceTests()
    {
        _mockAppointmentRepository = new Mock<IAppointmentRepository>();
        _mockPatientRepository = new Mock<IPatientRepository>();
        _mockDoctorRepository = new Mock<IDoctorRepository>();
        
        _appointmentService = new AppointmentService(
            _mockAppointmentRepository.Object,
            _mockPatientRepository.Object,
            _mockDoctorRepository.Object);
    }

    #region ScheduleAppointmentAsync Tests

    [Fact]
    public async Task ScheduleAppointmentAsync_WithValidData_ShouldCreateAppointment()
    {
        // Arrange
        var patientId = 1;
        var doctorId = 1;
        var appointmentDate = DateTime.UtcNow.AddDays(1).Date;
        var appointmentTime = new DateTime(1, 1, 1, 10, 0, 0); // 10:00 AM
        var durationInMinutes = 30;
        var reason = "Annual checkup";
        var notes = "First visit";

        var doctor = new Doctor
        {
            Id = doctorId,
            FirstName = "John",
            LastName = "Smith",
            Specialization = "General Practice",
            IsAvailable = true
        };

        _mockPatientRepository.Setup(x => x.ExistsAsync(patientId)).ReturnsAsync(true);
        _mockDoctorRepository.Setup(x => x.ExistsAsync(doctorId)).ReturnsAsync(true);
        _mockDoctorRepository.Setup(x => x.GetByIdAsync(doctorId)).ReturnsAsync(doctor);
        _mockAppointmentRepository
            .Setup(x => x.HasConflictAsync(doctorId, It.IsAny<DateTime>(), durationInMinutes, null))
            .ReturnsAsync(false);
        _mockAppointmentRepository
            .Setup(x => x.AddAsync(It.IsAny<Appointment>()))
            .ReturnsAsync((Appointment a) => { a.Id = 1; return a; });

        // Act
        var result = await _appointmentService.ScheduleAppointmentAsync(
            patientId, doctorId, appointmentDate, appointmentTime, durationInMinutes, reason, notes);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(patientId, result.PatientId);
        Assert.Equal(doctorId, result.DoctorId);
        Assert.Equal(AppointmentStatus.Scheduled, result.Status);
        Assert.Equal(reason, result.Reason);
        Assert.Equal(notes, result.Notes);
        _mockAppointmentRepository.Verify(x => x.AddAsync(It.IsAny<Appointment>()), Times.Once);
    }

    [Fact]
    public async Task ScheduleAppointmentAsync_WithInvalidPatientId_ShouldThrowArgumentException()
    {
        // Arrange
        var patientId = 0; // Invalid
        var doctorId = 1;
        var appointmentDate = DateTime.UtcNow.AddDays(1).Date;
        var appointmentTime = new DateTime(1, 1, 1, 10, 0, 0);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _appointmentService.ScheduleAppointmentAsync(
                patientId, doctorId, appointmentDate, appointmentTime, 30, null, null));
        
        Assert.Contains("Patient ID", exception.Message);
    }

    [Fact]
    public async Task ScheduleAppointmentAsync_WithInvalidDoctorId_ShouldThrowArgumentException()
    {
        // Arrange
        var patientId = 1;
        var doctorId = -1; // Invalid
        var appointmentDate = DateTime.UtcNow.AddDays(1).Date;
        var appointmentTime = new DateTime(1, 1, 1, 10, 0, 0);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _appointmentService.ScheduleAppointmentAsync(
                patientId, doctorId, appointmentDate, appointmentTime, 30, null, null));
        
        Assert.Contains("Doctor ID", exception.Message);
    }

    [Fact]
    public async Task ScheduleAppointmentAsync_WhenPatientNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var patientId = 999;
        var doctorId = 1;
        var appointmentDate = DateTime.UtcNow.AddDays(1).Date;
        var appointmentTime = new DateTime(1, 1, 1, 10, 0, 0);

        _mockPatientRepository.Setup(x => x.ExistsAsync(patientId)).ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _appointmentService.ScheduleAppointmentAsync(
                patientId, doctorId, appointmentDate, appointmentTime, 30, null, null));
        
        Assert.Contains("Patient", exception.Message);
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task ScheduleAppointmentAsync_WhenDoctorNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var patientId = 1;
        var doctorId = 999;
        var appointmentDate = DateTime.UtcNow.AddDays(1).Date;
        var appointmentTime = new DateTime(1, 1, 1, 10, 0, 0);

        _mockPatientRepository.Setup(x => x.ExistsAsync(patientId)).ReturnsAsync(true);
        _mockDoctorRepository.Setup(x => x.ExistsAsync(doctorId)).ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _appointmentService.ScheduleAppointmentAsync(
                patientId, doctorId, appointmentDate, appointmentTime, 30, null, null));
        
        Assert.Contains("Doctor", exception.Message);
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task ScheduleAppointmentAsync_WhenDoctorNotAvailable_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var patientId = 1;
        var doctorId = 1;
        var appointmentDate = DateTime.UtcNow.AddDays(1).Date;
        var appointmentTime = new DateTime(1, 1, 1, 10, 0, 0);

        var doctor = new Doctor
        {
            Id = doctorId,
            FirstName = "John",
            LastName = "Smith",
            IsAvailable = false // Doctor not available
        };

        _mockPatientRepository.Setup(x => x.ExistsAsync(patientId)).ReturnsAsync(true);
        _mockDoctorRepository.Setup(x => x.ExistsAsync(doctorId)).ReturnsAsync(true);
        _mockDoctorRepository.Setup(x => x.GetByIdAsync(doctorId)).ReturnsAsync(doctor);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _appointmentService.ScheduleAppointmentAsync(
                patientId, doctorId, appointmentDate, appointmentTime, 30, null, null));
        
        Assert.Contains("not available", exception.Message);
    }

    [Fact]
    public async Task ScheduleAppointmentAsync_WhenDoubleBooking_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var patientId = 1;
        var doctorId = 1;
        var appointmentDate = DateTime.UtcNow.AddDays(1).Date;
        var appointmentTime = new DateTime(1, 1, 1, 10, 0, 0);

        var doctor = new Doctor
        {
            Id = doctorId,
            FirstName = "John",
            LastName = "Smith",
            IsAvailable = true
        };

        _mockPatientRepository.Setup(x => x.ExistsAsync(patientId)).ReturnsAsync(true);
        _mockDoctorRepository.Setup(x => x.ExistsAsync(doctorId)).ReturnsAsync(true);
        _mockDoctorRepository.Setup(x => x.GetByIdAsync(doctorId)).ReturnsAsync(doctor);
        _mockAppointmentRepository
            .Setup(x => x.HasConflictAsync(doctorId, It.IsAny<DateTime>(), 30, null))
            .ReturnsAsync(true); // Conflict exists - double booking

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _appointmentService.ScheduleAppointmentAsync(
                patientId, doctorId, appointmentDate, appointmentTime, 30, null, null));
        
        Assert.Contains("scheduling conflict", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(481)]
    public async Task ScheduleAppointmentAsync_WithInvalidDuration_ShouldThrowArgumentException(int duration)
    {
        // Arrange
        var patientId = 1;
        var doctorId = 1;
        var appointmentDate = DateTime.UtcNow.AddDays(1).Date;
        var appointmentTime = new DateTime(1, 1, 1, 10, 0, 0);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _appointmentService.ScheduleAppointmentAsync(
                patientId, doctorId, appointmentDate, appointmentTime, duration, null, null));
        
        Assert.Contains("Duration", exception.Message);
    }

    #endregion

    #region GetAppointmentByIdAsync Tests

    [Fact]
    public async Task GetAppointmentByIdAsync_WithValidId_ShouldReturnAppointment()
    {
        // Arrange
        var appointmentId = 1;
        var expectedAppointment = new Appointment
        {
            Id = appointmentId,
            PatientId = 1,
            DoctorId = 1,
            AppointmentDate = DateTime.UtcNow.AddDays(1).Date,
            Status = AppointmentStatus.Scheduled
        };

        _mockAppointmentRepository
            .Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync(expectedAppointment);

        // Act
        var result = await _appointmentService.GetAppointmentByIdAsync(appointmentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(appointmentId, result.Id);
    }

    [Fact]
    public async Task GetAppointmentByIdAsync_WithInvalidId_ShouldThrowArgumentException()
    {
        // Arrange
        var appointmentId = 0;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _appointmentService.GetAppointmentByIdAsync(appointmentId));
    }

    [Fact]
    public async Task GetAppointmentByIdAsync_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        var appointmentId = 999;
        _mockAppointmentRepository
            .Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync((Appointment?)null);

        // Act
        var result = await _appointmentService.GetAppointmentByIdAsync(appointmentId);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region CancelAppointmentAsync Tests

    [Fact]
    public async Task CancelAppointmentAsync_WithValidAppointment_ShouldCancelSuccessfully()
    {
        // Arrange
        var appointmentId = 1;
        var cancellationReason = "Patient requested";
        var appointment = new Appointment
        {
            Id = appointmentId,
            PatientId = 1,
            DoctorId = 1,
            AppointmentDate = DateTime.UtcNow.AddDays(1).Date,
            Status = AppointmentStatus.Scheduled
        };

        _mockAppointmentRepository.Setup(x => x.GetByIdAsync(appointmentId)).ReturnsAsync(appointment);
        _mockAppointmentRepository.Setup(x => x.UpdateAsync(It.IsAny<Appointment>()))
            .ReturnsAsync((Appointment a) => a);

        // Act
        var result = await _appointmentService.CancelAppointmentAsync(appointmentId, cancellationReason);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(AppointmentStatus.Cancelled, result.Status);
        Assert.Equal(cancellationReason, result.CancellationReason);
        Assert.NotNull(result.CancelledAt);
    }

    [Fact]
    public async Task CancelAppointmentAsync_WhenAlreadyCancelled_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var appointmentId = 1;
        var appointment = new Appointment
        {
            Id = appointmentId,
            Status = AppointmentStatus.Cancelled
        };

        _mockAppointmentRepository.Setup(x => x.GetByIdAsync(appointmentId)).ReturnsAsync(appointment);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _appointmentService.CancelAppointmentAsync(appointmentId, "Reason"));
        
        Assert.Contains("already cancelled", exception.Message);
    }

    #endregion

    #region GetAppointmentsByPatientAsync Tests

    [Fact]
    public async Task GetAppointmentsByPatientAsync_WithValidPatientId_ShouldReturnAppointments()
    {
        // Arrange
        var patientId = 1;
        var appointments = new List<Appointment>
        {
            new Appointment { Id = 1, PatientId = patientId },
            new Appointment { Id = 2, PatientId = patientId }
        };

        _mockPatientRepository.Setup(x => x.ExistsAsync(patientId)).ReturnsAsync(true);
        _mockAppointmentRepository.Setup(x => x.GetByPatientIdAsync(patientId)).ReturnsAsync(appointments);

        // Act
        var result = await _appointmentService.GetAppointmentsByPatientAsync(patientId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAppointmentsByPatientAsync_WhenPatientNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var patientId = 999;
        _mockPatientRepository.Setup(x => x.ExistsAsync(patientId)).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _appointmentService.GetAppointmentsByPatientAsync(patientId));
    }

    #endregion

    #region GetAppointmentsByDateRangeAsync Tests

    [Fact]
    public async Task GetAppointmentsByDateRangeAsync_WithValidRange_ShouldReturnAppointments()
    {
        // Arrange
        var startDate = DateTime.UtcNow.Date;
        var endDate = DateTime.UtcNow.AddDays(7).Date;
        var appointments = new List<Appointment>
        {
            new Appointment { Id = 1, AppointmentDate = startDate.AddDays(1) },
            new Appointment { Id = 2, AppointmentDate = startDate.AddDays(3) }
        };

        _mockAppointmentRepository.Setup(x => x.GetByDateRangeAsync(startDate, endDate)).ReturnsAsync(appointments);

        // Act
        var result = await _appointmentService.GetAppointmentsByDateRangeAsync(startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAppointmentsByDateRangeAsync_WithInvalidRange_ShouldThrowArgumentException()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(7).Date;
        var endDate = DateTime.UtcNow.Date; // End before start

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _appointmentService.GetAppointmentsByDateRangeAsync(startDate, endDate));
    }

    #endregion
}
