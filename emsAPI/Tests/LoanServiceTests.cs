using Data;
using Microsoft.EntityFrameworkCore;
using Models;
using NUnit.Framework;
using Services;

namespace Tests;

[TestFixture]
public class LoanServiceTests
{
    private static DbContextOptions<AppDbContext> CreateInMemoryOptions(string databaseName)
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;
    }

    private static async Task SeedMinimalAsync(AppDbContext dbContext)
    {
        var branch = new Branch { Id = 1, Name = "HQ" };
        dbContext.Branches.Add(branch);

        dbContext.Producers.Add(new Producer { Id = 1, Name = "Dell" });
        dbContext.DeviceTypes.Add(new DeviceType { Id = 1, Name = "Laptop" });

        dbContext.Employees.Add(new Employee
        {
            Id = 1,
            Name = "Jan",
            LastName = "Kowalski",
            Email = "jan.kowalski@ems.local",
            BranchId = 1,
            Branch = branch
        });

        dbContext.Employees.Add(new Employee
        {
            Id = 2,
            Name = "Anna",
            LastName = "Nowak",
            Email = "anna.nowak@ems.local",
            BranchId = 1,
            Branch = branch
        });

        dbContext.Devices.Add(new Device
        {
            Id = 1,
            TypeId = 1,
            ProducerId = 1,
            Available = true,
            SerialNumber = "DEV-001"
        });

        dbContext.Devices.Add(new Device
        {
            Id = 2,
            TypeId = 1,
            ProducerId = 1,
            Available = false,
            SerialNumber = "DEV-002"
        });

        await dbContext.SaveChangesAsync();
    }


    [Test, Description("Create should return false when Device does not exist")]
    public async Task Create_ShouldReturnFalse_WhenDeviceDoesNotExist()
    {
        // Arrange
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        await using var db = new AppDbContext(options);
        await SeedMinimalAsync(db);

        var service = new LoanService(db);

        var loanToCreate = new Loan
        {
            Id = 1,
            DeviceId = 999,
            EmployeeId = 1,
            LoanDate = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            Returned = false
        };

        // Act
        var created = await service.Create(loanToCreate);

        // Assert
        Assert.That(created, Is.False);
        Assert.That(await db.Loans.CountAsync(), Is.EqualTo(0));
    }

    [Test, Description("Create should return false when Device is not available")]
    public async Task Create_ShouldReturnFalse_WhenDeviceIsNotAvailable()
    {
        // Arrange
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        await using var db = new AppDbContext(options);
        await SeedMinimalAsync(db);

        var service = new LoanService(db);

        var loanToCreate = new Loan
        {
            Id = 1,
            DeviceId = 2, // Available=false
            EmployeeId = 1,
            LoanDate = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            Returned = false
        };

        // Act
        var created = await service.Create(loanToCreate);

        // Assert
        Assert.That(created, Is.False);
        Assert.That(await db.Loans.CountAsync(), Is.EqualTo(0));
    }

    [Test, Description("Create should return false when Employee does not exist")]
    public async Task Create_ShouldReturnFalse_WhenEmployeeDoesNotExist()
    {
        // Arrange
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        await using var db = new AppDbContext(options);
        await SeedMinimalAsync(db);

        var service = new LoanService(db);

        var loanToCreate = new Loan
        {
            Id = 1,
            DeviceId = 1,
            EmployeeId = 999,
            LoanDate = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            Returned = false
        };

        // Act
        var created = await service.Create(loanToCreate);

        // Assert
        Assert.That(created, Is.False);
        Assert.That(await db.Loans.CountAsync(), Is.EqualTo(0));

        var device = await db.Devices.FindAsync(1);
        Assert.That(device!.Available, Is.True);
    }

    [Test, Description("Create should create loan and set Device.Available=false")]
    public async Task Create_ShouldReturnTrue_AndSetDeviceUnavailable_WhenDataIsValid()
    {
        // Arrange
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        await using var db = new AppDbContext(options);
        await SeedMinimalAsync(db);

        var service = new LoanService(db);

        var loanToCreate = new Loan
        {
            Id = 10,
            DeviceId = 1,
            EmployeeId = 1,
            LoanDate = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            Returned = false
        };

        // Act
        var created = await service.Create(loanToCreate);

        // Assert
        Assert.That(created, Is.True);

        var device = await db.Devices.FindAsync(1);
        Assert.That(device!.Available, Is.False);

        var loan = await db.Loans.SingleAsync();
        Assert.That(loan.DeviceId, Is.EqualTo(1));
        Assert.That(loan.EmployeeId, Is.EqualTo(1));
        Assert.That(loan.Returned, Is.False);
        Assert.That(loan.LoanDate, Is.EqualTo(new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc)));
    }

    [Test, Description("Return should return false when Loan does not exist")]
    public async Task Return_ShouldReturnFalse_WhenLoanDoesNotExist()
    {
        // Arrange
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        await using var db = new AppDbContext(options);
        await SeedMinimalAsync(db);

        var service = new LoanService(db);

        // Act
        var returned = await service.Return(id: 999);

        // Assert
        Assert.That(returned, Is.False);
    }

    [Test, Description("Return should return false when Loan is already returned")]
    public async Task Return_ShouldReturnFalse_WhenLoanAlreadyReturned()
    {
        // Arrange
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        await using var db = new AppDbContext(options);
        await SeedMinimalAsync(db);

        db.Loans.Add(new Loan
        {
            Id = 1,
            DeviceId = 1,
            EmployeeId = 1,
            LoanDate = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            Returned = true,
            ReturnDate = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc)
        });
        await db.SaveChangesAsync();

        var service = new LoanService(db);

        // Act
        var returned = await service.Return(id: 1);

        // Assert
        Assert.That(returned, Is.False);
    }

    [Test, Description("Return should set Returned=true, set ReturnDate, and set Device.Available=true")]
    public async Task Return_ShouldReturnTrue_AndSetDeviceAvailable_WhenLoanIsActive()
    {
        // Arrange
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        await using var db = new AppDbContext(options);
        await SeedMinimalAsync(db);

        var device = await db.Devices.FindAsync(1);
        device!.Available = false;

        db.Loans.Add(new Loan
        {
            Id = 1,
            DeviceId = 1,
            EmployeeId = 1,
            LoanDate = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            Returned = false,
            ReturnDate = null
        });

        await db.SaveChangesAsync();

        var service = new LoanService(db);

        // Act
        var returned = await service.Return(id: 1);

        // Assert
        Assert.That(returned, Is.True);

        var loan = await db.Loans.FindAsync(1);
        Assert.That(loan!.Returned, Is.True);
        Assert.That(loan.ReturnDate, Is.Not.Null);

        var updatedDevice = await db.Devices.FindAsync(1);
        Assert.That(updatedDevice!.Available, Is.True);
    }

    [Test, Description("ShowUserActiveLoans should return null when user does not exist")]
    public async Task ShowUserActiveLoans_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        await using var db = new AppDbContext(options);
        await SeedMinimalAsync(db);

        var service = new LoanService(db);

        // Act
        var loans = await service.ShowUserActiveLoans(userId: 999);

        // Assert
        Assert.That(loans, Is.Null);
    }

    [Test, Description("ShowUserHistory should return null when user does not exist")]
    public async Task ShowUserHistory_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        await using var db = new AppDbContext(options);
        await SeedMinimalAsync(db);

        var service = new LoanService(db);

        // Act
        var loans = await service.ShowUserHistory(userId: 999);

        // Assert
        Assert.That(loans, Is.Null);
    }

    [Test, Description("ShowUserActiveLoans should return only not returned loans for user")]
    public async Task ShowUserActiveLoans_ShouldReturnOnlyActiveLoans_ForUser()
    {
        // Arrange
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        await using var db = new AppDbContext(options);
        await SeedMinimalAsync(db);

        db.Loans.AddRange(
            new Loan { Id = 1, DeviceId = 1, EmployeeId = 1, LoanDate = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc), Returned = false },
            new Loan { Id = 2, DeviceId = 2, EmployeeId = 1, LoanDate = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc), Returned = true, ReturnDate = new DateTime(2026, 1, 3, 10, 0, 0, DateTimeKind.Utc) },
            new Loan { Id = 3, DeviceId = 2, EmployeeId = 2, LoanDate = new DateTime(2026, 1, 4, 10, 0, 0, DateTimeKind.Utc), Returned = false }
        );
        await db.SaveChangesAsync();

        var service = new LoanService(db);

        // Act
        var loans = await service.ShowUserActiveLoans(userId: 1);

        // Assert
        Assert.That(loans, Is.Not.Null);
        Assert.That(loans!, Has.Count.EqualTo(1));
        Assert.That(loans.Single().Id, Is.EqualTo(1));
        Assert.That(loans.Single().Returned, Is.False);
    }

    [Test, Description("ShowUserHistory should return only returned loans for user")]
    public async Task ShowUserHistory_ShouldReturnOnlyReturnedLoans_ForUser()
    {
        // Arrange
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        await using var db = new AppDbContext(options);
        await SeedMinimalAsync(db);

        db.Loans.AddRange(
            new Loan { Id = 1, DeviceId = 1, EmployeeId = 1, LoanDate = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc), Returned = false },
            new Loan { Id = 2, DeviceId = 2, EmployeeId = 1, LoanDate = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc), Returned = true, ReturnDate = new DateTime(2026, 1, 3, 10, 0, 0, DateTimeKind.Utc) },
            new Loan { Id = 3, DeviceId = 2, EmployeeId = 2, LoanDate = new DateTime(2026, 1, 4, 10, 0, 0, DateTimeKind.Utc), Returned = true, ReturnDate = new DateTime(2026, 1, 5, 10, 0, 0, DateTimeKind.Utc) }
        );
        await db.SaveChangesAsync();

        var service = new LoanService(db);

        // Act
        var loans = await service.ShowUserHistory(userId: 1);

        // Assert
        Assert.That(loans, Is.Not.Null);
        Assert.That(loans!, Has.Count.EqualTo(1));
        Assert.That(loans.Single().Id, Is.EqualTo(2));
        Assert.That(loans.Single().Returned, Is.True);
    }

    [Test, Description("ShowActiveLoans should return only loans where Returned == false")]
    public async Task ShowActiveLoans_ShouldReturnOnlyNotReturnedLoans()
    {
        // Arrange
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        await using var db = new AppDbContext(options);
        await SeedMinimalAsync(db);

        db.Loans.AddRange(
            new Loan { Id = 1, DeviceId = 1, EmployeeId = 1, LoanDate = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc), Returned = false },
            new Loan { Id = 2, DeviceId = 2, EmployeeId = 1, LoanDate = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc), Returned = true, ReturnDate = new DateTime(2026, 1, 3, 10, 0, 0, DateTimeKind.Utc) },
            new Loan { Id = 3, DeviceId = 2, EmployeeId = 2, LoanDate = new DateTime(2026, 1, 4, 10, 0, 0, DateTimeKind.Utc), Returned = false }
        );
        await db.SaveChangesAsync();

        var service = new LoanService(db);

        // Act
        var activeLoans = await service.ShowActiveLoans();

        // Assert
        Assert.That(activeLoans, Has.Count.EqualTo(2));
        Assert.That(activeLoans.Select(l => l.Id), Is.EquivalentTo(new[] { 1, 3 }));
        Assert.That(activeLoans.All(l => l.Returned == false), Is.True);
    }

    [Test, Description("Delete should set Device.Available=true when deleting an active loan")]
    public async Task Delete_ShouldReturnTrue_AndSetDeviceAvailable_WhenDeletingActiveLoan()
    {
        // Arrange
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        await using var db = new AppDbContext(options);
        await SeedMinimalAsync(db);

        var device = await db.Devices.FindAsync(1);
        device!.Available = false;

        db.Loans.Add(new Loan
        {
            Id = 1,
            DeviceId = 1,
            EmployeeId = 1,
            LoanDate = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            Returned = false
        });

        await db.SaveChangesAsync();

        var service = new LoanService(db);

        // Act
        var deleted = await service.Delete(id: 1);

        // Assert
        Assert.That(deleted, Is.True);
        Assert.That(await db.Loans.CountAsync(), Is.EqualTo(0));

        var updatedDevice = await db.Devices.FindAsync(1);
        Assert.That(updatedDevice!.Available, Is.True);
    }

    [Test, Description("Update should return false when loan does not exist")]
    public async Task Update_ShouldReturnFalse_WhenLoanDoesNotExist()
    {
        // Arrange
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        await using var db = new AppDbContext(options);
        await SeedMinimalAsync(db);

        var service = new LoanService(db);

        var updatedLoan = new Loan
        {
            Returned = true,
            ReturnDate = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var updated = await service.Update(id: 999, updatedLoan: updatedLoan);

        // Assert
        Assert.That(updated, Is.False);
    }

    [Test, Description("Update should update ReturnDate and Returned fields")]
    public async Task Update_ShouldReturnTrue_AndUpdateFields()
    {
        // Arrange
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        await using var db = new AppDbContext(options);
        await SeedMinimalAsync(db);

        db.Loans.Add(new Loan
        {
            Id = 1,
            DeviceId = 1,
            EmployeeId = 1,
            LoanDate = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            Returned = false,
            ReturnDate = null
        });

        await db.SaveChangesAsync();

        var service = new LoanService(db);

        var updatedLoan = new Loan
        {
            Returned = true,
            ReturnDate = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var updated = await service.Update(id: 1, updatedLoan: updatedLoan);

        // Assert
        Assert.That(updated, Is.True);

        var loan = await db.Loans.FindAsync(1);
        Assert.That(loan!.Returned, Is.True);
        Assert.That(loan.ReturnDate, Is.EqualTo(new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc)));
    }
}
