using Data;
using Microsoft.EntityFrameworkCore;
using Models;
using NUnit.Framework;
using Services;

namespace Tests;

[TestFixture]
public class DeviceServiceTests
{
    private static DbContextOptions<AppDbContext> CreateInMemoryOptions(string databaseName)
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;
    }

    private static async Task SeedProducerAndTypeAsync(AppDbContext dbContext, int producerId, int typeId)
    {
        var producer = new Producer
        {
            Id = producerId,
            Name = "Dell"
        };

        var deviceType = new DeviceType
        {
            Id = typeId,
            Name = "Laptop"
        };

        dbContext.Producers.Add(producer);
        dbContext.DeviceTypes.Add(deviceType);

        await dbContext.SaveChangesAsync();
    }

    [Test, Description("Create should return false when SerialNumber is null/empty/whitespace")]
    public async Task Create_ShouldReturnFalse_WhenSerialNumberIsInvalid()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        var dbContextOptions = CreateInMemoryOptions(databaseName);

        await using var dbContext = new AppDbContext(dbContextOptions);
        await SeedProducerAndTypeAsync(dbContext, producerId: 1, typeId: 1);

        var deviceService = new DeviceService(dbContext);

        var deviceToCreate = new Device
        {
            TypeId = 1,
            ProducerId = 1,
            Available = true,
            SerialNumber = "   "
        };

        // Act
        var created = await deviceService.Create(deviceToCreate);

        // Assert
        Assert.That(created, Is.False);

        var devicesInDatabase = await dbContext.Devices.ToListAsync();
        Assert.That(devicesInDatabase, Has.Count.EqualTo(0));
    }

    [Test, Description("Create should trim SerialNumber and prevent duplicates (case-insensitive)")]
    public async Task Create_ShouldReturnFalse_WhenDuplicateSerialNumberExists_IgnoringCaseAndTrim()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        var dbContextOptions = CreateInMemoryOptions(databaseName);

        await using var dbContext = new AppDbContext(dbContextOptions);
        await SeedProducerAndTypeAsync(dbContext, producerId: 1, typeId: 1);

        var existingDevice = new Device
        {
            Id = 100,
            TypeId = 1,
            ProducerId = 1,
            Available = true,
            SerialNumber = "ABC-123"
        };

        dbContext.Devices.Add(existingDevice);
        await dbContext.SaveChangesAsync();

        var deviceService = new DeviceService(dbContext);

        var deviceToCreate = new Device
        {
            TypeId = 1,
            ProducerId = 1,
            Available = false,
            SerialNumber = "  abc-123  "
        };

        // Act
        var created = await deviceService.Create(deviceToCreate);

        // Assert
        Assert.That(created, Is.False);

        var devicesInDatabase = await dbContext.Devices.ToListAsync();
        Assert.That(devicesInDatabase, Has.Count.EqualTo(1));
        Assert.That(devicesInDatabase.Single().SerialNumber, Is.EqualTo("ABC-123"));
    }

    [Test, Description("Create should persist a Device and return true for valid data")]
    public async Task Create_ShouldReturnTrue_AndPersistDevice_WhenDataIsValid()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        var dbContextOptions = CreateInMemoryOptions(databaseName);

        await using var dbContext = new AppDbContext(dbContextOptions);
        await SeedProducerAndTypeAsync(dbContext, producerId: 1, typeId: 1);

        var deviceService = new DeviceService(dbContext);

        var deviceToCreate = new Device
        {
            TypeId = 1,
            ProducerId = 1,
            Available = true,
            SerialNumber = "  NEW-001  "
        };

        // Act
        var created = await deviceService.Create(deviceToCreate);

        // Assert
        Assert.That(created, Is.True);

        var devicesInDatabase = await dbContext.Devices.ToListAsync();
        Assert.That(devicesInDatabase, Has.Count.EqualTo(1));
        Assert.That(devicesInDatabase.Single().SerialNumber, Is.EqualTo("NEW-001")); // trimmed
        Assert.That(devicesInDatabase.Single().Available, Is.True);
        Assert.That(devicesInDatabase.Single().TypeId, Is.EqualTo(1));
        Assert.That(devicesInDatabase.Single().ProducerId, Is.EqualTo(1));
    }

    [Test, Description("ReadOne should return null when Device does not exist")]
    public async Task ReadOne_ShouldReturnNull_WhenDeviceDoesNotExist()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        var dbContextOptions = CreateInMemoryOptions(databaseName);

        await using var dbContext = new AppDbContext(dbContextOptions);
        var deviceService = new DeviceService(dbContext);

        // Act
        var device = await deviceService.ReadOne(id: 999);

        // Assert
        Assert.That(device, Is.Null);
    }

    [Test, Description("ReadAll should return all devices")]
    public async Task ReadAll_ShouldReturnAllDevices()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        var dbContextOptions = CreateInMemoryOptions(databaseName);

        await using var dbContext = new AppDbContext(dbContextOptions);
        await SeedProducerAndTypeAsync(dbContext, producerId: 1, typeId: 1);

        dbContext.Devices.AddRange(
            new Device { Id = 1, TypeId = 1, ProducerId = 1, Available = true, SerialNumber = "A-1" },
            new Device { Id = 2, TypeId = 1, ProducerId = 1, Available = false, SerialNumber = "A-2" }
        );
        await dbContext.SaveChangesAsync();

        var deviceService = new DeviceService(dbContext);

        // Act
        var devices = await deviceService.ReadAll();

        // Assert
        Assert.That(devices, Has.Count.EqualTo(2));
        Assert.That(devices.Select(d => d.SerialNumber), Is.EquivalentTo(new[] { "A-1", "A-2" }));
    }

    [Test, Description("Delete should return false when Device does not exist")]
    public async Task Delete_ShouldReturnFalse_WhenDeviceDoesNotExist()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        var dbContextOptions = CreateInMemoryOptions(databaseName);

        await using var dbContext = new AppDbContext(dbContextOptions);
        var deviceService = new DeviceService(dbContext);

        // Act
        var deleted = await deviceService.Delete(id: 123);

        // Assert
        Assert.That(deleted, Is.False);
    }

    [Test, Description("Delete should remove Device and return true when Device exists")]
    public async Task Delete_ShouldReturnTrue_AndRemoveDevice_WhenDeviceExists()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        var dbContextOptions = CreateInMemoryOptions(databaseName);

        await using var dbContext = new AppDbContext(dbContextOptions);
        await SeedProducerAndTypeAsync(dbContext, producerId: 1, typeId: 1);

        dbContext.Devices.Add(new Device
        {
            Id = 10,
            TypeId = 1,
            ProducerId = 1,
            Available = true,
            SerialNumber = "DEL-10"
        });
        await dbContext.SaveChangesAsync();

        var deviceService = new DeviceService(dbContext);

        // Act
        var deleted = await deviceService.Delete(id: 10);

        // Assert
        Assert.That(deleted, Is.True);

        var deviceInDatabase = await dbContext.Devices.FindAsync(10);
        Assert.That(deviceInDatabase, Is.Null);
    }

    [Test, Description("Update should return false when SerialNumber is invalid")]
    public async Task Update_ShouldReturnFalse_WhenSerialNumberIsInvalid()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        var dbContextOptions = CreateInMemoryOptions(databaseName);

        await using var dbContext = new AppDbContext(dbContextOptions);
        await SeedProducerAndTypeAsync(dbContext, producerId: 1, typeId: 1);

        dbContext.Devices.Add(new Device
        {
            Id = 1,
            TypeId = 1,
            ProducerId = 1,
            Available = true,
            SerialNumber = "X-1"
        });
        await dbContext.SaveChangesAsync();

        var deviceService = new DeviceService(dbContext);

        var updatedDevice = new Device
        {
            TypeId = 1,
            ProducerId = 1,
            Available = false,
            SerialNumber = "   "
        };

        // Act
        var updated = await deviceService.Update(id: 1, updatedDevice: updatedDevice);

        // Assert
        Assert.That(updated, Is.False);

        var deviceInDatabase = await dbContext.Devices.FindAsync(1);
        Assert.That(deviceInDatabase!.Available, Is.True); // unchanged
        Assert.That(deviceInDatabase.SerialNumber, Is.EqualTo("X-1"));
    }

    [Test, Description("Update should return false when Device does not exist")]
    public async Task Update_ShouldReturnFalse_WhenDeviceDoesNotExist()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        var dbContextOptions = CreateInMemoryOptions(databaseName);

        await using var dbContext = new AppDbContext(dbContextOptions);
        var deviceService = new DeviceService(dbContext);

        var updatedDevice = new Device
        {
            TypeId = 1,
            ProducerId = 1,
            Available = true,
            SerialNumber = "NEW-999"
        };

        // Act
        var updated = await deviceService.Update(id: 999, updatedDevice: updatedDevice);

        // Assert
        Assert.That(updated, Is.False);
    }

    [Test, Description("Update should return false when SerialNumber duplicates another Device (case-insensitive)")]
    public async Task Update_ShouldReturnFalse_WhenSerialNumberDuplicatesAnotherDevice()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        var dbContextOptions = CreateInMemoryOptions(databaseName);

        await using var dbContext = new AppDbContext(dbContextOptions);
        await SeedProducerAndTypeAsync(dbContext, producerId: 1, typeId: 1);

        dbContext.Devices.AddRange(
            new Device { Id = 1, TypeId = 1, ProducerId = 1, Available = true, SerialNumber = "KEEP-001" },
            new Device { Id = 2, TypeId = 1, ProducerId = 1, Available = true, SerialNumber = "DUP-001" }
        );
        await dbContext.SaveChangesAsync();

        var deviceService = new DeviceService(dbContext);

        var updatedDevice = new Device
        {
            TypeId = 1,
            ProducerId = 1,
            Available = false,
            SerialNumber = " dup-001 " // duplicate of Id=2
        };

        // Act
        var updated = await deviceService.Update(id: 1, updatedDevice: updatedDevice);

        // Assert
        Assert.That(updated, Is.False);

        var deviceInDatabase = await dbContext.Devices.FindAsync(1);
        Assert.That(deviceInDatabase!.SerialNumber, Is.EqualTo("KEEP-001"));
        Assert.That(deviceInDatabase.Available, Is.True);
    }

    [Test, Description("Update should update fields and return true when data is valid")]
    public async Task Update_ShouldReturnTrue_AndUpdateFields_WhenDataIsValid()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        var dbContextOptions = CreateInMemoryOptions(databaseName);

        await using var dbContext = new AppDbContext(dbContextOptions);

        await SeedProducerAndTypeAsync(dbContext, producerId: 1, typeId: 1);
        await SeedProducerAndTypeAsync(dbContext, producerId: 2, typeId: 2);

        dbContext.Devices.Add(new Device
        {
            Id = 1,
            TypeId = 1,
            ProducerId = 1,
            Available = true,
            SerialNumber = "OLD-001"
        });
        await dbContext.SaveChangesAsync();

        var deviceService = new DeviceService(dbContext);

        var updatedDevice = new Device
        {
            TypeId = 2,
            ProducerId = 2,
            Available = false,
            SerialNumber = "  NEW-001  "
        };

        // Act
        var updated = await deviceService.Update(id: 1, updatedDevice: updatedDevice);

        // Assert
        Assert.That(updated, Is.True);

        var deviceInDatabase = await dbContext.Devices.FindAsync(1);
        Assert.That(deviceInDatabase, Is.Not.Null);
        Assert.That(deviceInDatabase!.TypeId, Is.EqualTo(2));
        Assert.That(deviceInDatabase.ProducerId, Is.EqualTo(2));
        Assert.That(deviceInDatabase.Available, Is.False);
        Assert.That(deviceInDatabase.SerialNumber, Is.EqualTo("NEW-001"));
    }

    [Test, Description("ReadAvailable should return only devices where Available == true")]
    public async Task ReadAvailable_ShouldReturnOnlyAvailableDevices()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        var dbContextOptions = CreateInMemoryOptions(databaseName);

        await using var dbContext = new AppDbContext(dbContextOptions);
        await SeedProducerAndTypeAsync(dbContext, producerId: 1, typeId: 1);

        dbContext.Devices.AddRange(
            new Device { Id = 1, TypeId = 1, ProducerId = 1, Available = true, SerialNumber = "AV-1" },
            new Device { Id = 2, TypeId = 1, ProducerId = 1, Available = false, SerialNumber = "NA-1" }
        );
        await dbContext.SaveChangesAsync();

        var deviceService = new DeviceService(dbContext);

        // Act
        var devices = await deviceService.ReadAvailable();

        // Assert
        Assert.That(devices, Has.Count.EqualTo(1));
        Assert.That(devices.Single().SerialNumber, Is.EqualTo("AV-1"));
    }

    [Test, Description("ReadByProducer should return all devices for a given ProducerId")]
    public async Task ReadByProducer_ShouldReturnDevicesForProducer()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        var dbContextOptions = CreateInMemoryOptions(databaseName);

        await using var dbContext = new AppDbContext(dbContextOptions);

        var producer1 = new Producer { Id = 1, Name = "Dell" };
        var producer2 = new Producer { Id = 2, Name = "HP" };
        var deviceType = new DeviceType { Id = 1, Name = "Laptop" };

        dbContext.Producers.AddRange(producer1, producer2);
        dbContext.DeviceTypes.Add(deviceType);

        dbContext.Devices.AddRange(
            new Device { Id = 1, TypeId = 1, ProducerId = 1, Available = true, SerialNumber = "P1-1" },
            new Device { Id = 2, TypeId = 1, ProducerId = 1, Available = false, SerialNumber = "P1-2" },
            new Device { Id = 3, TypeId = 1, ProducerId = 2, Available = true, SerialNumber = "P2-1" }
        );
        await dbContext.SaveChangesAsync();

        var deviceService = new DeviceService(dbContext);

        // Act
        var devices = await deviceService.ReadByProducer(producerId: 1);

        // Assert
        Assert.That(devices, Has.Count.EqualTo(2));
        Assert.That(devices.Select(d => d.SerialNumber), Is.EquivalentTo(new[] { "P1-1", "P1-2" }));
    }

    [Test, Description("ReadByType should return all devices for a given TypeId")]
    public async Task ReadByType_ShouldReturnDevicesForType()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        var dbContextOptions = CreateInMemoryOptions(databaseName);

        await using var dbContext = new AppDbContext(dbContextOptions);

        var producer = new Producer { Id = 1, Name = "Dell" };
        var type1 = new DeviceType { Id = 1, Name = "Laptop" };
        var type2 = new DeviceType { Id = 2, Name = "Monitor" };

        dbContext.Producers.Add(producer);
        dbContext.DeviceTypes.AddRange(type1, type2);

        dbContext.Devices.AddRange(
            new Device { Id = 1, TypeId = 1, ProducerId = 1, Available = true, SerialNumber = "T1-1" },
            new Device { Id = 2, TypeId = 1, ProducerId = 1, Available = false, SerialNumber = "T1-2" },
            new Device { Id = 3, TypeId = 2, ProducerId = 1, Available = true, SerialNumber = "T2-1" }
        );
        await dbContext.SaveChangesAsync();

        var deviceService = new DeviceService(dbContext);

        // Act
        var devices = await deviceService.ReadByType(typeId: 1);

        // Assert
        Assert.That(devices, Has.Count.EqualTo(2));
        Assert.That(devices.Select(d => d.SerialNumber), Is.EquivalentTo(new[] { "T1-1", "T1-2" }));
    }
}
