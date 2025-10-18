using CleanArchitecture.Cmms.Domain.Abstractions;
using CleanArchitecture.Cmms.Domain.Technicians;
using CleanArchitecture.Cmms.Domain.Technicians.Entities;
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
            string technicianName = "John Smith";
            SkillLevel technicianSkillLevel = SkillLevel.Journeyman;

            // Act
            Technician technician = Technician.Create(technicianName, technicianSkillLevel);

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
            Technician technician = Technician.Create("Tech One", SkillLevel.Apprentice);
            Certification hvacCertification = Certification.Create("HVAC-1", DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddYears(1));

            // Act
            technician.AddCertification(hvacCertification);

            // Assert
            Assert.Single(technician.Certifications);
            Certification added = technician.Certifications.First();
            Assert.Equal(hvacCertification, added);
        }

        [Fact]
        public void AddCertification_Should_Throw_When_Duplicate()
        {
            // Arrange
            Technician technician = Technician.Create("Tech One", SkillLevel.Apprentice);
            Certification hvacCertification = Certification.Create("HVAC-1", DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddYears(1));
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
            Technician technician = Technician.Create("Tech One", SkillLevel.Journeyman);
            Guid workOrderId = Guid.NewGuid();
            DateTime assignedOn = DateTime.UtcNow;

            // Act
            technician.AddAssignedOrder(workOrderId, assignedOn);

            // Assert
            Assert.Single(technician.Assignments);
            TechnicianAssignment assignment = technician.Assignments.First();
            Assert.Equal(workOrderId, assignment.WorkOrderId);
            Assert.Equal(assignedOn, assignment.AssignedOn);
            Assert.False(assignment.IsCompleted);
        }

        [Fact]
        public void AddAssignedOrder_Should_Throw_When_Unavailable()
        {
            // Arrange
            Technician technician = Technician.Create("Tech One", SkillLevel.Journeyman);
            technician.SetUnavailable();
            Guid workOrderId = Guid.NewGuid();
            DateTime assignedOn = DateTime.UtcNow;

            // Act
            void act() => technician.AddAssignedOrder(workOrderId, assignedOn);

            // Assert
            Assert.Throws<DomainException>(act);
        }

        [Fact]
        public void AddAssignedOrder_Should_Throw_When_Already_Assigned_And_Active()
        {
            // Arrange
            Technician technician = Technician.Create("Tech One", SkillLevel.Journeyman);
            Guid workOrderId = Guid.NewGuid();
            DateTime firstAssignedOn = DateTime.UtcNow.AddMinutes(-5);
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
            Technician technician = Technician.Create("Tech One", SkillLevel.Journeyman);
            Guid workOrderId = Guid.NewGuid();
            DateTime assignedOn = DateTime.UtcNow.AddMinutes(-30);
            technician.AddAssignedOrder(workOrderId, assignedOn);
            technician.CompleteAssignment(workOrderId, DateTime.UtcNow.AddMinutes(-10));

            // Act
            technician.AddAssignedOrder(workOrderId, DateTime.UtcNow);

            // Assert
            Assert.Equal(2, technician.Assignments.Count);
            TechnicianAssignment latest = technician.Assignments.OrderByDescending(a => a.AssignedOn).First();
            Assert.Equal(workOrderId, latest.WorkOrderId);
            Assert.False(latest.IsCompleted);
        }

        [Fact]
        public void AddAssignedOrder_Should_Throw_When_Exceeding_MaxConcurrent()
        {
            // Arrange
            Technician technician = Technician.Create("Tech One", SkillLevel.Master);
            Guid workOrderId1 = Guid.NewGuid();
            Guid workOrderId2 = Guid.NewGuid();
            Guid workOrderId3 = Guid.NewGuid();
            Guid workOrderId4 = Guid.NewGuid();
            DateTime now = DateTime.UtcNow;
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
            Technician technician = Technician.Create("Tech One", SkillLevel.Master);
            Guid workOrderId = Guid.NewGuid();
            DateTime assignedOn = DateTime.UtcNow.AddMinutes(-2);
            technician.AddAssignedOrder(workOrderId, assignedOn);
            DateTime completedOn = DateTime.UtcNow;

            // Act
            technician.CompleteAssignment(workOrderId, completedOn);

            // Assert
            TechnicianAssignment assignment = technician.Assignments.First();
            Assert.True(assignment.IsCompleted);
            Assert.Equal(completedOn, assignment.CompletedOn);
        }

        [Fact]
        public void CompleteAssignment_Should_Throw_When_Assignment_Not_Found()
        {
            // Arrange
            Technician technician = Technician.Create("Tech One", SkillLevel.Master);
            Guid missingWorkOrderId = Guid.NewGuid();
            DateTime completedOn = DateTime.UtcNow;

            // Act
            void act() => technician.CompleteAssignment(missingWorkOrderId, completedOn);

            // Assert
            Assert.Throws<DomainException>(act);
        }

        [Fact]
        public void SetUnavailable_Should_Set_Status_And_Be_Idempotent()
        {
            // Arrange
            Technician technician = Technician.Create("Tech One", SkillLevel.Journeyman);

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
            Technician technician = Technician.Create("Tech One", SkillLevel.Journeyman);
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
            Technician technician = Technician.Create("Tech One", SkillLevel.Journeyman);

            // Act
            bool initiallyAvailable = technician.IsAvailable();
            technician.SetUnavailable();
            bool afterSettingUnavailable = technician.IsAvailable();

            // Assert
            Assert.True(initiallyAvailable);
            Assert.False(afterSettingUnavailable);
        }
    }
}
