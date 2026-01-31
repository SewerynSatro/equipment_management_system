using Data;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Dtos.Device;
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

    [Test, Description("Create should return null when SerialNumber is null/empty/whitespace")]
    public async Task Create_ShouldReturnNull_WhenSerialNumberIsInvalid()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        var dbContextOptions = CreateInMemoryOptions(databaseName);

        await using var dbContext = new AppDbContext(dbContextOptions);
        await SeedProducerAndTypeAsync(dbContext, producerId: 1, typeId: 1);

        var deviceService = new DeviceService(dbContext);

        var deviceToCreate = new DeviceCreateDto
        {
            TypeId = 1,
            ProducerId = 1,
            Available = true,
            SerialNumber = "   "
        };

        // Act
        var result = await deviceService.Create(deviceToCreate);

        // Assert
        Assert.That(result, Is.Null);

        var devicesInDatabase = await dbContext.Devices.ToListAsync();
        Assert.That(devicesInDatabase, Has.Count.EqualTo(0));
    }

    [Test, Description("Create should trim SerialNumber and prevent duplicates (case-insensitive)")]
    public async Task Create_ShouldReturnNull_WhenDuplicateSerialNumberExists_IgnoringCaseAndTrim()
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

        var deviceToCreate = new DeviceCreateDto
        {
            TypeId = 1,
            ProducerId = 1,
            Available = false,
            SerialNumber = "  abc-123  "
        };

        // Act
        var result = await deviceService.Create(deviceToCreate);

        // Assert
        Assert.That(result, Is.Null);

        var devicesInDatabase = await dbContext.Devices.ToListAsync();
        Assert.That(devicesInDatabase, Has.Count.EqualTo(1));
        Assert.That(devicesInDatabase.Single().SerialNumber, Is.EqualTo("ABC-123"));
    }

    [Test, Description("Create should persist a Device and return DeviceReadDto for valid data")]
    public async Task Create_ShouldReturnDeviceReadDto_AndPersistDevice_WhenDataIsValid()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        var dbContextOptions = CreateInMemoryOptions(databaseName);

        await using var dbContext = new AppDbContext(dbContextOptions);
        await SeedProducerAndTypeAsync(dbContext, producerId: 1, typeId: 1);

        var deviceService = new DeviceService(dbContext);

        var deviceToCreate = new DeviceCreateDto
        {
            TypeId = 1,
            ProducerId = 1,
            Available = true,
            SerialNumber = "  NEW-001  "
        };

        // Act
        var result = await deviceService.Create(deviceToCreate);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.SerialNumber, Is.EqualTo("NEW-001")); // trimmed
        Assert.That(result.Available, Is.True);
        Assert.That(result.TypeId, Is.EqualTo(1));
        Assert.That(result.ProducerId, Is.EqualTo(1));
        Assert.That(result.TypeName, Is.EqualTo("Laptop"));
        Assert.That(result.ProducerName, Is.EqualTo("Dell"));

        var devicesInDatabase = await dbContext.Devices.ToListAsync();
        Assert.That(devicesInDatabase, Has.Count.EqualTo(1));
        Assert.That(devicesInDatabase.Single().SerialNumber, Is.EqualTo("NEW-001"));
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

    [Test, Description("ReadOne should return DeviceReadDto with related data")]
    public async Task ReadOne_ShouldReturnDeviceReadDto_WithRelatedData()
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
            SerialNumber = "SN-001"
        });
        await dbContext.SaveChangesAsync();

        var deviceService = new DeviceService(dbContext);

        // Act
        var result = await deviceService.ReadOne(id: 1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(1));
        Assert.That(result.SerialNumber, Is.EqualTo("SN-001"));
        Assert.That(result.TypeName, Is.EqualTo("Laptop"));
        Assert.That(result.ProducerName, Is.EqualTo("Dell"));
    }

    [Test, Description("ReadAll should return all devices as DeviceReadDto")]
    public async Task ReadAll_ShouldReturnAllDevicesAsDto()
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
        Assert.That(devices.All(d => d.TypeName == "Laptop"), Is.True);
        Assert.That(devices.All(d => d.ProducerName == "Dell"), Is.True);
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

    [Test, Description("Update should return null when SerialNumber is invalid")]
    public async Task Update_ShouldReturnNull_WhenSerialNumberIsInvalid()
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

        var updateDto = new DeviceUpdateDto
        {
            TypeId = 1,
            ProducerId = 1,
            Available = false,
            SerialNumber = "   "
        };

        // Act
        var result = await deviceService.Update(id: 1, dto: updateDto);

        // Assert
        Assert.That(result, Is.Null);

        var deviceInDatabase = await dbContext.Devices.FindAsync(1);
        Assert.That(deviceInDatabase!.Available, Is.True); // unchanged
        Assert.That(deviceInDatabase.SerialNumber, Is.EqualTo("X-1"));
    }

    [Test, Description("Update should return null when Device does not exist")]
    public async Task Update_ShouldReturnNull_WhenDeviceDoesNotExist()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        var dbContextOptions = CreateInMemoryOptions(databaseName);

        await using var dbContext = new AppDbContext(dbContextOptions);
        var deviceService = new DeviceService(dbContext);

        var updateDto = new DeviceUpdateDto
        {
            TypeId = 1,
            ProducerId = 1,
            Available = true,
            SerialNumber = "NEW-999"
        };

        // Act
        var result = await deviceService.Update(id: 999, dto: updateDto);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test, Description("Update should return null when SerialNumber duplicates another Device (case-insensitive)")]
    public async Task Update_ShouldReturnNull_WhenSerialNumberDuplicatesAnotherDevice()
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

        var updateDto = new DeviceUpdateDto
        {
            TypeId = 1,
            ProducerId = 1,
            Available = false,
            SerialNumber = " dup-001 " // duplicate of Id=2
        };

        // Act
        var result = await deviceService.Update(id: 1, dto: updateDto);

        // Assert
        Assert.That(result, Is.Null);

        var deviceInDatabase = await dbContext.Devices.FindAsync(1);
        Assert.That(deviceInDatabase!.SerialNumber, Is.EqualTo("KEEP-001"));
        Assert.That(deviceInDatabase.Available, Is.True);
    }

    [Test, Description("Update should update fields and return DeviceReadDto when data is valid")]
    public async Task Update_ShouldReturnDeviceReadDto_AndUpdateFields_WhenDataIsValid()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        var dbContextOptions = CreateInMemoryOptions(databaseName);

        await using var dbContext = new AppDbContext(dbContextOptions);

        // Seed two producers and two types
        var producer1 = new Producer { Id = 1, Name = "Dell" };
        var producer2 = new Producer { Id = 2, Name = "HP" };
        var type1 = new DeviceType { Id = 1, Name = "Laptop" };
        var type2 = new DeviceType { Id = 2, Name = "Monitor" };

        dbContext.Producers.AddRange(producer1, producer2);
        dbContext.DeviceTypes.AddRange(type1, type2);

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

        var updateDto = new DeviceUpdateDto
        {
            TypeId = 2,
            ProducerId = 2,
            Available = false,
            SerialNumber = "  NEW-001  "
        };

        // Act
        var result = await deviceService.Update(id: 1, dto: updateDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(1));
        Assert.That(result.TypeId, Is.EqualTo(2));
        Assert.That(result.ProducerId, Is.EqualTo(2));
        Assert.That(result.Available, Is.False);
        Assert.That(result.SerialNumber, Is.EqualTo("NEW-001"));
        Assert.That(result.TypeName, Is.EqualTo("Monitor"));
        Assert.That(result.ProducerName, Is.EqualTo("HP"));

        var deviceInDatabase = await dbContext.Devices.FindAsync(1);
        Assert.That(deviceInDatabase, Is.Not.Null);
        Assert.That(deviceInDatabase!.TypeId, Is.EqualTo(2));
        Assert.That(deviceInDatabase.ProducerId, Is.EqualTo(2));
        Assert.That(deviceInDatabase.Available, Is.False);
        Assert.That(deviceInDatabase.SerialNumber, Is.EqualTo("NEW-001"));
    }

    [Test, Description("ReadAvailable should return only devices where Available == true as DeviceReadDto")]
    public async Task ReadAvailable_ShouldReturnOnlyAvailableDevicesAsDto()
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
        Assert.That(devices.Single().Available, Is.True);
        Assert.That(devices.Single().TypeName, Is.EqualTo("Laptop"));
    }

    [Test, Description("ReadByProducer should return all devices for a given ProducerId as DeviceReadDto")]
    public async Task ReadByProducer_ShouldReturnDevicesForProducerAsDto()
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
        Assert.That(devices.All(d => d.ProducerName == "Dell"), Is.True);
    }

    [Test, Description("ReadByType should return all devices for a given TypeId as DeviceReadDto")]
    public async Task ReadByType_ShouldReturnDevicesForTypeAsDto()
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
        Assert.That(devices.All(d => d.TypeName == "Laptop"), Is.True);
    }
}