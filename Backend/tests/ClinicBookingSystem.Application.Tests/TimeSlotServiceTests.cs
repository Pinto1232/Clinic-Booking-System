using ClinicBookingSystem.Application.Services;
using ClinicBookingSystem.Domain.Entities;
using ClinicBookingSystem.Domain.Interfaces;
using Moq;

namespace ClinicBookingSystem.Application.Tests;

public class TimeSlotServiceTests
{
    private readonly Mock<ITimeSlotRepository> _mockTimeSlotRepository;
    private readonly Mock<IDoctorRepository> _mockDoctorRepository;
    private readonly Mock<IAppointmentRepository> _mockAppointmentRepository;
    private readonly TimeSlotService _timeSlotService;

    public TimeSlotServiceTests()
    {
        _mockTimeSlotRepository = new Mock<ITimeSlotRepository>();
        _mockDoctorRepository = new Mock<IDoctorRepository>();
        _mockAppointmentRepository = new Mock<IAppointmentRepository>();
        
        _timeSlotService = new TimeSlotService(
            _mockTimeSlotRepository.Object,
            _mockDoctorRepository.Object,
            _mockAppointmentRepository.Object);
    }

    #region GetAvailableSlotsByDoctorAndDateAsync Tests

    [Fact]
    public async Task GetAvailableSlotsByDoctorAndDateAsync_WithValidData_ShouldReturnAvailableSlots()
    {
        // Arrange
        var doctorId = 1;
        var date = DateTime.UtcNow.AddDays(1).Date;
        
        _mockDoctorRepository.Setup(x => x.ExistsAsync(doctorId)).ReturnsAsync(true);
        _mockAppointmentRepository
            .Setup(x => x.GetByDoctorAndDateRangeAsync(doctorId, date, date.AddDays(1)))
            .ReturnsAsync(new List<Appointment>());

        // Act
        var result = await _timeSlotService.GetAvailableSlotsByDoctorAndDateAsync(doctorId, date);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.All(result, slot =>
        {
            Assert.Equal(doctorId, slot.DoctorId);
            Assert.True(slot.IsAvailable);
            Assert.False(slot.IsBlocked);
        });
    }

    [Fact]
    public async Task GetAvailableSlotsByDoctorAndDateAsync_ShouldExcludeBookedSlots()
    {
        // Arrange
        var doctorId = 1;
        var date = DateTime.UtcNow.AddDays(1).Date;
        var bookedTime = date.AddHours(10); // 10:00 AM is booked
        
        var existingAppointment = new Appointment
        {
            Id = 1,
            DoctorId = doctorId,
            AppointmentDate = date,
            AppointmentTime = bookedTime,
            Status = AppointmentStatus.Scheduled
        };

        _mockDoctorRepository.Setup(x => x.ExistsAsync(doctorId)).ReturnsAsync(true);
        _mockAppointmentRepository
            .Setup(x => x.GetByDoctorAndDateRangeAsync(doctorId, date, date.AddDays(1)))
            .ReturnsAsync(new List<Appointment> { existingAppointment });

        // Act
        var result = await _timeSlotService.GetAvailableSlotsByDoctorAndDateAsync(doctorId, date);

        // Assert
        Assert.NotNull(result);
        // Verify the booked slot (10:00 AM) is not in the available slots
        Assert.DoesNotContain(result, slot => 
            slot.StartTime.Hour == 10 && slot.StartTime.Minute == 0);
    }

    [Fact]
    public async Task GetAvailableSlotsByDoctorAndDateAsync_ShouldIncludeCancelledAppointmentSlots()
    {
        // Arrange
        var doctorId = 1;
        var date = DateTime.UtcNow.AddDays(1).Date;
        var bookedTime = date.AddHours(10); // 10:00 AM was cancelled
        
        var cancelledAppointment = new Appointment
        {
            Id = 1,
            DoctorId = doctorId,
            AppointmentDate = date,
            AppointmentTime = bookedTime,
            Status = AppointmentStatus.Cancelled // Cancelled - should be available
        };

        _mockDoctorRepository.Setup(x => x.ExistsAsync(doctorId)).ReturnsAsync(true);
        _mockAppointmentRepository
            .Setup(x => x.GetByDoctorAndDateRangeAsync(doctorId, date, date.AddDays(1)))
            .ReturnsAsync(new List<Appointment> { cancelledAppointment });

        // Act
        var result = await _timeSlotService.GetAvailableSlotsByDoctorAndDateAsync(doctorId, date);

        // Assert
        Assert.NotNull(result);
        // The 10:00 AM slot should be available since appointment was cancelled
        Assert.Contains(result, slot => 
            slot.StartTime.Hour == 10 && slot.StartTime.Minute == 0);
    }

    [Fact]
    public async Task GetAvailableSlotsByDoctorAndDateAsync_WithInvalidDoctorId_ShouldThrowArgumentException()
    {
        // Arrange
        var doctorId = 0; // Invalid
        var date = DateTime.UtcNow.AddDays(1).Date;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _timeSlotService.GetAvailableSlotsByDoctorAndDateAsync(doctorId, date));
        
        Assert.Contains("Doctor ID", exception.Message);
    }

    [Fact]
    public async Task GetAvailableSlotsByDoctorAndDateAsync_WhenDoctorNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var doctorId = 999;
        var date = DateTime.UtcNow.AddDays(1).Date;

        _mockDoctorRepository.Setup(x => x.ExistsAsync(doctorId)).ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _timeSlotService.GetAvailableSlotsByDoctorAndDateAsync(doctorId, date));
        
        Assert.Contains("Doctor", exception.Message);
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task GetAvailableSlotsByDoctorAndDateAsync_WithPastDate_ShouldThrowArgumentException()
    {
        // Arrange
        var doctorId = 1;
        var date = DateTime.UtcNow.AddDays(-1).Date; // Past date

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _timeSlotService.GetAvailableSlotsByDoctorAndDateAsync(doctorId, date));
        
        Assert.Contains("past", exception.Message);
    }

    #endregion

    #region GetTimeSlotsByDoctorAsync Tests

    [Fact]
    public async Task GetTimeSlotsByDoctorAsync_WithValidDoctorId_ShouldReturnSlots()
    {
        // Arrange
        var doctorId = 1;
        var timeSlots = new List<TimeSlot>
        {
            new TimeSlot { Id = 1, DoctorId = doctorId },
            new TimeSlot { Id = 2, DoctorId = doctorId }
        };

        _mockDoctorRepository.Setup(x => x.ExistsAsync(doctorId)).ReturnsAsync(true);
        _mockTimeSlotRepository.Setup(x => x.GetByDoctorIdAsync(doctorId)).ReturnsAsync(timeSlots);

        // Act
        var result = await _timeSlotService.GetTimeSlotsByDoctorAsync(doctorId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetTimeSlotsByDoctorAsync_WithInvalidDoctorId_ShouldThrowArgumentException()
    {
        // Arrange
        var doctorId = -1;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _timeSlotService.GetTimeSlotsByDoctorAsync(doctorId));
    }

    #endregion

    #region GetTimeSlotByIdAsync Tests

    [Fact]
    public async Task GetTimeSlotByIdAsync_WithValidId_ShouldReturnTimeSlot()
    {
        // Arrange
        var slotId = 1;
        var expectedSlot = new TimeSlot
        {
            Id = slotId,
            DoctorId = 1,
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddHours(2).AddMinutes(30),
            IsAvailable = true
        };

        _mockTimeSlotRepository.Setup(x => x.GetByIdAsync(slotId)).ReturnsAsync(expectedSlot);

        // Act
        var result = await _timeSlotService.GetTimeSlotByIdAsync(slotId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(slotId, result.Id);
    }

    [Fact]
    public async Task GetTimeSlotByIdAsync_WithInvalidId_ShouldThrowArgumentException()
    {
        // Arrange
        var slotId = 0;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _timeSlotService.GetTimeSlotByIdAsync(slotId));
    }

    #endregion

    #region GetTimeSlotsByDateRangeAsync Tests

    [Fact]
    public async Task GetTimeSlotsByDateRangeAsync_WithValidRange_ShouldReturnSlots()
    {
        // Arrange
        var startDate = DateTime.UtcNow.Date;
        var endDate = DateTime.UtcNow.AddDays(7).Date;
        var slots = new List<TimeSlot>
        {
            new TimeSlot { Id = 1, StartTime = startDate.AddDays(1) },
            new TimeSlot { Id = 2, StartTime = startDate.AddDays(3) }
        };

        _mockTimeSlotRepository.Setup(x => x.GetByDateRangeAsync(startDate, endDate)).ReturnsAsync(slots);

        // Act
        var result = await _timeSlotService.GetTimeSlotsByDateRangeAsync(startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetTimeSlotsByDateRangeAsync_WithInvalidRange_ShouldThrowArgumentException()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(7).Date;
        var endDate = DateTime.UtcNow.Date; // End before start

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _timeSlotService.GetTimeSlotsByDateRangeAsync(startDate, endDate));
    }

    #endregion

    #region Time Slot Generation Tests

    [Fact]
    public async Task GetAvailableSlotsByDoctorAndDateAsync_ShouldGenerateSlotsWithin9AMTo5PM()
    {
        // Arrange
        var doctorId = 1;
        var date = DateTime.UtcNow.AddDays(1).Date;
        
        _mockDoctorRepository.Setup(x => x.ExistsAsync(doctorId)).ReturnsAsync(true);
        _mockAppointmentRepository
            .Setup(x => x.GetByDoctorAndDateRangeAsync(doctorId, date, date.AddDays(1)))
            .ReturnsAsync(new List<Appointment>());

        // Act
        var result = (await _timeSlotService.GetAvailableSlotsByDoctorAndDateAsync(doctorId, date)).ToList();

        // Assert
        Assert.All(result, slot =>
        {
            Assert.True(slot.StartTime.Hour >= 9, "Slot should not start before 9 AM");
            Assert.True(slot.StartTime.Hour < 17, "Slot should not start at or after 5 PM");
        });
    }

    [Fact]
    public async Task GetAvailableSlotsByDoctorAndDateAsync_ShouldGenerate30MinuteSlots()
    {
        // Arrange
        var doctorId = 1;
        var date = DateTime.UtcNow.AddDays(1).Date;
        
        _mockDoctorRepository.Setup(x => x.ExistsAsync(doctorId)).ReturnsAsync(true);
        _mockAppointmentRepository
            .Setup(x => x.GetByDoctorAndDateRangeAsync(doctorId, date, date.AddDays(1)))
            .ReturnsAsync(new List<Appointment>());

        // Act
        var result = (await _timeSlotService.GetAvailableSlotsByDoctorAndDateAsync(doctorId, date)).ToList();

        // Assert
        Assert.All(result, slot =>
        {
            var duration = (slot.EndTime - slot.StartTime).TotalMinutes;
            Assert.Equal(30, duration);
        });
    }

    #endregion
}
