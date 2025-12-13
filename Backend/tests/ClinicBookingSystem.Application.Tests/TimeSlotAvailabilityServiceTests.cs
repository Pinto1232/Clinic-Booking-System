namespace ClinicBookingSystem.Application.Tests;

using Xunit;
using Moq;
using ClinicBookingSystem.Domain.Entities;
using ClinicBookingSystem.Domain.Interfaces;
using ClinicBookingSystem.Application.Services;

/// <summary>
/// Unit tests for TimeSlotAvailabilityService.
/// </summary>
public class TimeSlotAvailabilityServiceTests
{
    private readonly Mock<ITimeSlotRepository> _mockTimeSlotRepository;
    private readonly Mock<IAppointmentRepository> _mockAppointmentRepository;
    private readonly TimeSlotAvailabilityService _availabilityService;

    public TimeSlotAvailabilityServiceTests()
    {
        _mockTimeSlotRepository = new Mock<ITimeSlotRepository>();
        _mockAppointmentRepository = new Mock<IAppointmentRepository>();

        _availabilityService = new TimeSlotAvailabilityService(
            _mockTimeSlotRepository.Object,
            _mockAppointmentRepository.Object);
    }

    [Fact]
    public async Task GetAvailableSlotsByDoctorAsync_WithValidDoctorId_ShouldReturnAvailableSlots()
    {
        // Arrange
        var doctorId = 1;
        var from = DateTime.UtcNow;
        var to = DateTime.UtcNow.AddDays(7);

        var timeSlots = new List<TimeSlot>
        {
            new() { Id = 1, DoctorId = doctorId, StartDateTime = from.AddHours(1), EndDateTime = from.AddHours(2), IsAvailable = true },
            new() { Id = 2, DoctorId = doctorId, StartDateTime = from.AddHours(3), EndDateTime = from.AddHours(4), IsAvailable = true }
        };

        _mockTimeSlotRepository
            .Setup(x => x.GetByDoctorAndDateRangeAsync(doctorId, from, to))
            .ReturnsAsync(timeSlots);

        _mockAppointmentRepository
            .Setup(x => x.GetAppointmentsByDateRangeAsync(from, to))
            .ReturnsAsync(new List<Appointment>());

        // Act
        var result = await _availabilityService.GetAvailableSlotsByDoctorAsync(doctorId, from, to);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task IsSlotAvailableAsync_WithConflictingAppointment_ShouldReturnFalse()
    {
        // Arrange
        var doctorId = 1;
        var startDateTime = DateTime.UtcNow.AddDays(1).AddHours(9);
        var endDateTime = startDateTime.AddHours(1);

        _mockTimeSlotRepository
            .Setup(x => x.GetByDoctorAndDateRangeAsync(doctorId, startDateTime, endDateTime))
            .ReturnsAsync(new List<TimeSlot>());

        _mockAppointmentRepository
            .Setup(x => x.GetAppointmentsByDateRangeAsync(startDateTime, endDateTime))
            .ReturnsAsync(new List<Appointment>());

        // Act
        var result = await _availabilityService.IsSlotAvailableAsync(doctorId, startDateTime, endDateTime);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ReserveSlotAsync_WithValidSlotId_ShouldMarkAsUnavailable()
    {
        // Arrange
        var slotId = 1;
        var timeSlot = new TimeSlot
        {
            Id = slotId,
            DoctorId = 1,
            StartDateTime = DateTime.UtcNow.AddDays(1),
            EndDateTime = DateTime.UtcNow.AddDays(1).AddHours(1),
            IsAvailable = true
        };

        _mockTimeSlotRepository
            .Setup(x => x.GetByIdAsync(slotId))
            .ReturnsAsync(timeSlot);

        _mockTimeSlotRepository
            .Setup(x => x.UpdateAsync(It.IsAny<TimeSlot>()))
            .ReturnsAsync(timeSlot);

        // Act
        var result = await _availabilityService.ReserveSlotAsync(slotId);

        // Assert
        Assert.True(result);
    }
}
