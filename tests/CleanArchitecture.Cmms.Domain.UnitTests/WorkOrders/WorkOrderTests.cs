using CleanArchitecture.Core.Domain.Abstractions;
using CleanArchitecture.Cmms.Domain.WorkOrders;
using CleanArchitecture.Cmms.Domain.WorkOrders.Enums;
using CleanArchitecture.Cmms.Domain.WorkOrders.ValueObjects;

namespace CleanArchitecture.Cmms.Domain.UnitTests.WorkOrders
{
    public class WorkOrderTests
    {
        private static Location Location() => Domain.WorkOrders.ValueObjects.Location.Create("B1", "F2", "R3");

        [Fact]
        public void Create_Should_Set_Properties_And_Open_Status()
        {
            // Arrange
            var assetId = Guid.NewGuid();
            var title = "Fix AC";
            var location = Location();

            // Act
            var workOrder = WorkOrder.Create(assetId, title, location);

            // Assert
            Assert.Equal(assetId, workOrder.AssetId);
            Assert.Equal(title, workOrder.Title);
            Assert.Equal(location, workOrder.Location);
            Assert.Equal(WorkOrderStatus.Open, workOrder.Status);
            Assert.Null(workOrder.TechnicianId);
            Assert.Empty(workOrder.Steps);
            Assert.Empty(workOrder.Comments);
        }

        [Fact]
        public void Create_Should_Throw_When_Title_Empty()
        {
            // Arrange
            var assetId = Guid.NewGuid();
            var location = Location();

            // Act
            void act() => WorkOrder.Create(assetId, " ", location);

            // Assert
            Assert.Throws<DomainException>(act);
        }

        [Fact]
        public void Create_Should_Throw_When_AssetId_Empty()
        {
            // Arrange
            var location = Location();

            // Act
            void act() => WorkOrder.Create(Guid.Empty, "Title", location);

            // Assert
            Assert.Throws<DomainException>(act);
        }

        [Fact]
        public void AssignTechnician_From_Open_Should_Set_Assigned_And_TechId()
        {
            // Arrange
            var workOrder = WorkOrder.Create(Guid.NewGuid(), "Title", Location());
            var techId = Guid.NewGuid();

            // Act
            workOrder.AssignTechnician(techId);

            // Assert
            Assert.Equal(WorkOrderStatus.Assigned, workOrder.Status);
            Assert.Equal(techId, workOrder.TechnicianId);
        }

        [Fact]
        public void AssignTechnician_From_InProgress_Should_Update_TechId_Keep_Status()
        {
            // Arrange
            var workOrder = WorkOrder.Create(Guid.NewGuid(), "Title", Location());
            var tech1 = Guid.NewGuid();
            workOrder.AssignTechnician(tech1);
            workOrder.Start();
            var tech2 = Guid.NewGuid();

            // Act
            workOrder.AssignTechnician(tech2);

            // Assert
            Assert.Equal(WorkOrderStatus.InProgress, workOrder.Status);
            Assert.Equal(tech2, workOrder.TechnicianId);
        }

        [Fact]
        public void AssignTechnician_Should_Throw_When_Cancelled()
        {
            // Arrange
            var workOrder = WorkOrder.Create(Guid.NewGuid(), "Title", Location());
            workOrder.Cancel();

            // Act
            void act() => workOrder.AssignTechnician(Guid.NewGuid());

            // Assert
            Assert.Throws<DomainException>(act);
        }

        [Fact]
        public void AssignTechnician_Should_Throw_When_Completed()
        {
            // Arrange
            var workOrder = WorkOrder.Create(Guid.NewGuid(), "Title", Location());
            workOrder.AssignTechnician(Guid.NewGuid());
            workOrder.Start();
            workOrder.Complete(); // No steps -> allowed

            // Act
            void act() => workOrder.AssignTechnician(Guid.NewGuid());

            // Assert
            Assert.Throws<DomainException>(act);
        }

        [Fact]
        public void AddStep_Should_Add_TaskStep()
        {
            // Arrange
            var workOrder = WorkOrder.Create(Guid.NewGuid(), "Title", Location());

            // Act
            workOrder.AddStep("Tighten bolts");

            // Assert
            Assert.Single(workOrder.Steps);
            var step = workOrder.Steps.First();
            Assert.Equal("Tighten bolts", step.Description);
            Assert.False(step.Completed);
        }

        [Fact]
        public void AddStep_Should_Throw_When_Description_Empty()
        {
            // Arrange
            var workOrder = WorkOrder.Create(Guid.NewGuid(), "Title", Location());

            // Act
            void act() => workOrder.AddStep(" ");

            // Assert
            Assert.Throws<DomainException>(act);
        }

        [Fact]
        public void AddComment_Should_Add_Comment()
        {
            // Arrange
            var workOrder = WorkOrder.Create(Guid.NewGuid(), "Title", Location());
            var author = Guid.NewGuid();

            // Act
            workOrder.AddComment("Looks good", author);

            // Assert
            Assert.Single(workOrder.Comments);
            var c = workOrder.Comments.First();
            Assert.Equal("Looks good", c.Text);
            Assert.Equal(author, c.AuthorId);
        }

        [Fact]
        public void AddComment_Should_Throw_When_Text_Empty()
        {
            // Arrange
            var workOrder = WorkOrder.Create(Guid.NewGuid(), "Title", Location());

            // Act
            void act() => workOrder.AddComment(" ", Guid.NewGuid());

            // Assert
            Assert.Throws<DomainException>(act);
        }

        [Fact]
        public void Start_From_Assigned_Should_Set_InProgress()
        {
            // Arrange
            var workOrder = WorkOrder.Create(Guid.NewGuid(), "Title", Location());
            workOrder.AssignTechnician(Guid.NewGuid());

            // Act
            workOrder.Start();

            // Assert
            Assert.Equal(WorkOrderStatus.InProgress, workOrder.Status);
        }

        [Fact]
        public void Start_From_NotAssigned_Should_Throw()
        {
            // Arrange
            var workOrder = WorkOrder.Create(Guid.NewGuid(), "Title", Location());

            // Act
            void act() => workOrder.Start();

            // Assert
            Assert.Throws<DomainException>(act);
        }

        [Fact]
        public void Complete_From_InProgress_With_All_Steps_Done_Should_Set_Completed()
        {
            // Arrange
            var workOrder = WorkOrder.Create(Guid.NewGuid(), "Title", Location());
            workOrder.AddStep("Step 1");
            var step = workOrder.Steps.First();
            step.MarkCompleted();
            workOrder.AssignTechnician(Guid.NewGuid());
            workOrder.Start();

            // Act
            workOrder.Complete();

            // Assert
            Assert.Equal(WorkOrderStatus.Completed, workOrder.Status);
        }

        [Fact]
        public void Complete_Should_Throw_When_Not_InProgress()
        {
            // Arrange
            var workOrder = WorkOrder.Create(Guid.NewGuid(), "Title", Location());

            // Act
            void act() => workOrder.Complete();

            // Assert
            Assert.Throws<DomainException>(act);
        }

        [Fact]
        public void Complete_Should_Throw_When_Steps_Not_All_Completed()
        {
            // Arrange
            var workOrder = WorkOrder.Create(Guid.NewGuid(), "Title", Location());
            workOrder.AddStep("Incomplete step");
            workOrder.AssignTechnician(Guid.NewGuid());
            workOrder.Start();

            // Act
            void act() => workOrder.Complete();

            // Assert
            Assert.Throws<DomainException>(act);
        }

        [Fact]
        public void Cancel_From_Open_Should_Set_Cancelled()
        {
            // Arrange
            var workOrder = WorkOrder.Create(Guid.NewGuid(), "Title", Location());

            // Act
            workOrder.Cancel();

            // Assert
            Assert.Equal(WorkOrderStatus.Cancelled, workOrder.Status);
        }

        [Fact]
        public void Cancel_From_InProgress_Should_Set_Cancelled()
        {
            // Arrange
            var workOrder = WorkOrder.Create(Guid.NewGuid(), "Title", Location());
            workOrder.AssignTechnician(Guid.NewGuid());
            workOrder.Start();

            // Act
            workOrder.Cancel();

            // Assert
            Assert.Equal(WorkOrderStatus.Cancelled, workOrder.Status);
        }

        [Fact]
        public void Cancel_From_Completed_Should_Throw()
        {
            // Arrange
            var workOrder = WorkOrder.Create(Guid.NewGuid(), "Title", Location());
            workOrder.AssignTechnician(Guid.NewGuid());
            workOrder.Start();
            workOrder.Complete();

            // Act
            void act() => workOrder.Cancel();

            // Assert
            Assert.Throws<DomainException>(act);
        }
    }
}
