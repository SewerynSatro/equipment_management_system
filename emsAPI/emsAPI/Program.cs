using Data;
using Microsoft.EntityFrameworkCore;
using Services;

namespace emsAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        builder.Services.AddControllers()
            .AddApplicationPart(typeof(Controllers.ControllersMarker).Assembly);
        
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("LocalTestDb"));

        builder.Services.AddControllers();
        builder.Services.AddAuthorization();
        builder.Services.AddScoped<IProducerService, ProducerService>();
        builder.Services.AddScoped<IDeviceService, DeviceService>();
        builder.Services.AddScoped<IDeviceTypeService, DeviceTypeService>();
        builder.Services.AddScoped<IBranchService, BranchService>();
        builder.Services.AddScoped<IEmployeeService, EmployeeService>();
        builder.Services.AddScoped<ILoanService, LoanService>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}