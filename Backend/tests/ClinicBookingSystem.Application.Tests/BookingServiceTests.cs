namespace ClinicBookingSystem.Application.Tests;

using Xunit;
using Moq;
using ClinicBookingSystem.Domain.Entities;
using ClinicBookingSystem.Domain.Interfaces;
using ClinicBookingSystem.Application.Services;

/// <summary>
/// Unit tests for BookingService following AAA (Arrange, Act, Assert) pattern.
/// </summary>
public class BookingServiceTests
{
    private readonly Mock<IAppointmentRepository> _mockAppointmentRepository;
    private readonly Mock<ITimeSlotAvailabilityService> _mockTimeSlotAvailability;
    private readonly Mock<IAppointmentConfirmationService> _mockConfirmationService;
    private readonly BookingService _bookingService;

    public BookingServiceTests()
    {
        _mockAppointmentRepository = new Mock<IAppointmentRepository>();
        _mockTimeSlotAvailability = new Mock<ITimeSlotAvailabilityService>();
        _mockConfirmationService = new Mock<IAppointmentConfirmationService>();

        _bookingService = new BookingService(
            _mockAppointmentRepository.Object,
            _mockTimeSlotAvailability.Object,
            _mockConfirmationService.Object);
    }

    [Fact]
    public async Task BookAppointmentAsync_WithValidData_ShouldReturnSuccessResult()
    {
        // Arrange
        var patientId = 1;
        var doctorId = 1;
        var appointmentDateTime = DateTime.UtcNow.AddDays(1);
        var durationInMinutes = 30;

        var appointment = new Appointment
        {
            Id = 1,
            PatientId = patientId,
            DoctorId = doctorId,
            AppointmentDateTime = appointmentDateTime,
            DurationInMinutes = durationInMinutes,
            Status = AppointmentStatus.Pending
        };

        _mockTimeSlotAvailability
            .Setup(x => x.IsSlotAvailableAsync(doctorId, appointmentDateTime, It.IsAny<DateTime>()))
            .ReturnsAsync(true);

        _mockAppointmentRepository
            .Setup(x => x.AddAsync(It.IsAny<Appointment>()))
            .ReturnsAsync(appointment);

        _mockAppointmentRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Appointment>()))
            .ReturnsAsync(appointment);

        _mockConfirmationService
            .Setup(x => x.SendConfirmationAsync(It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        var result = await _bookingService.BookAppointmentAsync(patientId, doctorId, appointmentDateTime, durationInMinutes);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Appointment booked successfully", result.Message);
        Assert.Equal(1, result.AppointmentId);
    }

    [Fact]
    public async Task BookAppointmentAsync_WithInvalidPatientId_ShouldReturnFailure()
    {
        // Arrange
        var invalidPatientId = -1;
        var doctorId = 1;
        var appointmentDateTime = DateTime.UtcNow.AddDays(1);
        var durationInMinutes = 30;

        // Act
        var result = await _bookingService.BookAppointmentAsync(invalidPatientId, doctorId, appointmentDateTime, durationInMinutes);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid patient ID", result.Message);
    }

    [Fact]
    public async Task BookAppointmentAsync_WithPastDateTime_ShouldReturnFailure()
    {
        // Arrange
        var patientId = 1;
        var doctorId = 1;
        var pastDateTime = DateTime.UtcNow.AddDays(-1);
        var durationInMinutes = 30;

        // Act
        var result = await _bookingService.BookAppointmentAsync(patientId, doctorId, pastDateTime, durationInMinutes);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Appointment date must be in the future", result.Message);
    }

    [Fact]
    public async Task BookAppointmentAsync_WithUnavailableSlot_ShouldReturnFailure()
    {
        // Arrange
        var patientId = 1;
        var doctorId = 1;
        var appointmentDateTime = DateTime.UtcNow.AddDays(1);
        var durationInMinutes = 30;

        _mockTimeSlotAvailability
            .Setup(x => x.IsSlotAvailableAsync(doctorId, appointmentDateTime, It.IsAny<DateTime>()))
            .ReturnsAsync(false);

        // Act
        var result = await _bookingService.BookAppointmentAsync(patientId, doctorId, appointmentDateTime, durationInMinutes);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Time slot is not available", result.Message);
    }

    [Fact]
    public async Task CancelAppointmentAsync_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        var appointmentId = 1;
        var appointment = new Appointment
        {
            Id = appointmentId,
            PatientId = 1,
            DoctorId = 1,
            AppointmentDateTime = DateTime.UtcNow.AddDays(1),
            DurationInMinutes = 30,
            Status = AppointmentStatus.Confirmed
        };

        _mockAppointmentRepository
            .Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync(appointment);

        _mockAppointmentRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Appointment>()))
            .ReturnsAsync(appointment);

        _mockConfirmationService
            .Setup(x => x.SendCancellationAsync(It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        var result = await _bookingService.CancelAppointmentAsync(appointmentId);

        // Assert
        Assert.True(result);
    }
}
