using CleanArchitecture.Core.Domain.Abstractions;
using CleanArchitecture.Cmms.Domain.Technicians;
using CleanArchitecture.Cmms.Domain.Technicians.Enums;
using CleanArchitecture.Cmms.Domain.Technicians.ValueObjects;

namespace CleanArchitecture.Cmms.Domain.UnitTests.Technicians
{
    public class TechnicianTests
    {
        [Fact]
        public void Create_Should_Set_Defaults_And_Raise_CreatedEvent()
        {
            // Arrange
            var technicianName = "John Smith";
            var technicianSkillLevel = SkillLevel.Journeyman;

            // Act
            var technician = Technician.Create(technicianName, technicianSkillLevel);

            // Assert
            Assert.Equal(technicianName, technician.Name);
            Assert.Equal(technicianSkillLevel, technician.SkillLevel);
            Assert.Equal(TechnicianStatus.Available, technician.Status);
            Assert.Empty(technician.Certifications);
            Assert.Empty(technician.Assignments);
            Assert.Equal(3, technician.MaxConcurrentAssignments);
        }

        [Fact]
        public void AddCertification_Should_Add_New_Certification()
        {
            // Arrange
            var technician = Technician.Create("Tech One", SkillLevel.Apprentice);
            var hvacCertification = Certification.Create("HVAC-1", DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddYears(1));

            // Act
            technician.AddCertification(hvacCertification);

            // Assert
            Assert.Single(technician.Certifications);
            var added = technician.Certifications.First();
            Assert.Equal(hvacCertification, added);
        }

        [Fact]
        public void AddCertification_Should_Throw_When_Duplicate()
        {
            // Arrange
            var technician = Technician.Create("Tech One", SkillLevel.Apprentice);
            var hvacCertification = Certification.Create("HVAC-1", DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddYears(1));
            technician.AddCertification(hvacCertification);

            // Act
            void act() => technician.AddCertification(hvacCertification);

            // Assert
            Assert.Throws<DomainException>(act);
        }

        [Fact]
        public void AddAssignedOrder_Should_Add_When_Eligible()
        {
            // Arrange
            var technician = Technician.Create("Tech One", SkillLevel.Journeyman);
            var workOrderId = Guid.NewGuid();
            var assignedOn = DateTime.UtcNow;

            // Act
            technician.AddAssignedOrder(workOrderId, assignedOn);

            // Assert
            Assert.Single(technician.Assignments);
            var assignment = technician.Assignments.First();
            Assert.Equal(workOrderId, assignment.WorkOrderId);
            Assert.Equal(assignedOn, assignment.AssignedOn);
            Assert.False(assignment.IsCompleted);
        }

        [Fact]
        public void AddAssignedOrder_Should_Throw_When_Unavailable()
        {
            // Arrange
            var technician = Technician.Create("Tech One", SkillLevel.Journeyman);
            technician.SetUnavailable();
            var workOrderId = Guid.NewGuid();
            var assignedOn = DateTime.UtcNow;

            // Act
            void act() => technician.AddAssignedOrder(workOrderId, assignedOn);

            // Assert
            Assert.Throws<DomainException>(act);
        }

        [Fact]
        public void AddAssignedOrder_Should_Throw_When_Already_Assigned_And_Active()
        {
            // Arrange
            var technician = Technician.Create("Tech One", SkillLevel.Journeyman);
            var workOrderId = Guid.NewGuid();
            var firstAssignedOn = DateTime.UtcNow.AddMinutes(-5);
            technician.AddAssignedOrder(workOrderId, firstAssignedOn);

            // Act
            void act() => technician.AddAssignedOrder(workOrderId, DateTime.UtcNow);

            // Assert
            Assert.Throws<DomainException>(act);
        }

        [Fact]
        public void AddAssignedOrder_Should_Allow_Reassign_After_Completion()
        {
            // Arrange
            var technician = Technician.Create("Tech One", SkillLevel.Journeyman);
            var workOrderId = Guid.NewGuid();
            var assignedOn = DateTime.UtcNow.AddMinutes(-30);
            technician.AddAssignedOrder(workOrderId, assignedOn);
            technician.CompleteAssignment(workOrderId, DateTime.UtcNow.AddMinutes(-10));

            // Act
            technician.AddAssignedOrder(workOrderId, DateTime.UtcNow);

            // Assert
            Assert.Equal(2, technician.Assignments.Count);
            var latest = technician.Assignments.OrderByDescending(a => a.AssignedOn).First();
            Assert.Equal(workOrderId, latest.WorkOrderId);
            Assert.False(latest.IsCompleted);
        }

        [Fact]
        public void AddAssignedOrder_Should_Throw_When_Exceeding_MaxConcurrent()
        {
            // Arrange
            var technician = Technician.Create("Tech One", SkillLevel.Master);
            var workOrderId1 = Guid.NewGuid();
            var workOrderId2 = Guid.NewGuid();
            var workOrderId3 = Guid.NewGuid();
            var workOrderId4 = Guid.NewGuid();
            var now = DateTime.UtcNow;
            technician.AddAssignedOrder(workOrderId1, now.AddMinutes(-3));
            technician.AddAssignedOrder(workOrderId2, now.AddMinutes(-2));
            technician.AddAssignedOrder(workOrderId3, now.AddMinutes(-1));

            // Act
            void act() => technician.AddAssignedOrder(workOrderId4, now);

            // Assert
            Assert.Throws<DomainException>(act);
        }

        [Fact]
        public void CompleteAssignment_Should_Set_CompletedOn()
        {
            // Arrange
            var technician = Technician.Create("Tech One", SkillLevel.Master);
            var workOrderId = Guid.NewGuid();
            var assignedOn = DateTime.UtcNow.AddMinutes(-2);
            technician.AddAssignedOrder(workOrderId, assignedOn);
            var completedOn = DateTime.UtcNow;

            // Act
            technician.CompleteAssignment(workOrderId, completedOn);

            // Assert
            var assignment = technician.Assignments.First();
            Assert.True(assignment.IsCompleted);
            Assert.Equal(completedOn, assignment.CompletedOn);
        }

        [Fact]
        public void CompleteAssignment_Should_Throw_When_Assignment_Not_Found()
        {
            // Arrange
            var technician = Technician.Create("Tech One", SkillLevel.Master);
            var missingWorkOrderId = Guid.NewGuid();
            var completedOn = DateTime.UtcNow;

            // Act
            void act() => technician.CompleteAssignment(missingWorkOrderId, completedOn);

            // Assert
            Assert.Throws<DomainException>(act);
        }

        [Fact]
        public void SetUnavailable_Should_Set_Status_And_Be_Idempotent()
        {
            // Arrange
            var technician = Technician.Create("Tech One", SkillLevel.Journeyman);

            // Act
            technician.SetUnavailable();
            technician.SetUnavailable(); // second call should not change anything

            // Assert
            Assert.Equal(TechnicianStatus.Unavailable, technician.Status);
        }

        [Fact]
        public void SetAvailable_Should_Set_Status_And_Be_Idempotent()
        {
            // Arrange
            var technician = Technician.Create("Tech One", SkillLevel.Journeyman);
            technician.SetUnavailable();

            // Act
            technician.SetAvailable();
            technician.SetAvailable(); // idempotent

            // Assert
            Assert.Equal(TechnicianStatus.Available, technician.Status);
        }

        [Fact]
        public void IsAvailable_Should_Reflect_Status()
        {
            // Arrange
            var technician = Technician.Create("Tech One", SkillLevel.Journeyman);

            // Act
            var initiallyAvailable = technician.IsAvailable();
            technician.SetUnavailable();
            var afterSettingUnavailable = technician.IsAvailable();

            // Assert
            Assert.True(initiallyAvailable);
            Assert.False(afterSettingUnavailable);
        }
    }
}
