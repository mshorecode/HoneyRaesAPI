using HoneyRaesAPI.Models;

List<HoneyRaesAPI.Models.Customer> customers = new() { 
    new Customer()
    { 
        Id = 1,
        Name = "Tricia Watsica",
        Address = "16942 Hackett Parkways"
    },
    new Customer()
    { 
        Id = 2,
        Name = "Chad Strosin",
        Address = "270 Bode Garden"
    },
    new Customer()
    { 
        Id = 3, 
        Name = "Leo Kris",
        Address = "359 Bonita Street"
    }
};
List<HoneyRaesAPI.Models.Employee> employees = new() { 
    new Employee()
    {
        Id = 1,
        Name = "Johnny Saniat",
        Specialty = "Data Recovery"
    },
    new Employee()
    { 
        Id = 2,
        Name = "Greg Markus",
        Specialty = "Troubleshooting"
    }

};
List<HoneyRaesAPI.Models.ServiceTicket> serviceTickets = new() { 
    new ServiceTicket()
    {
        Id = 1,
        CustomerId = 2,
        Description = "Computer is running slow",
        Emergency = false
    },
    new ServiceTicket()
    {
        Id = 2,
        CustomerId = 1,
        EmployeeId = 1,
        Description = "Can't find a folder with very important files in it!",
        Emergency = true,
        DateCompleted = new DateTime(2024, 01, 22)
    },
    new ServiceTicket() 
    { 
        Id = 3,
        CustomerId = 2,
        EmployeeId = 2,
        Description = "My computer keeps locking up on me randomly and I have to shutdown to fix it",
        Emergency = true,
        DateCompleted = new DateTime(2023, 12, 28)
    },
    new ServiceTicket()
    {
        Id = 4,
        CustomerId = 3,
        Description = "Move data from one harddrive to another",
        Emergency = false
    },
    new ServiceTicket()
    {
        Id = 5,
        CustomerId = 1,
        EmployeeId = 2,
        Description = "I need help getting my printer working with my laptop again",
        Emergency = false,
        DateCompleted = new DateTime(2024, 1, 10)
    }
};



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/servicetickets", () => 
{
    return serviceTickets;
});

app.MapGet("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);
    Customer customer = customers.FirstOrDefault(c => c.Id == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }
    serviceTicket.Employee = employees.FirstOrDefault(e => e.Id == serviceTicket.EmployeeId);
    serviceTicket.Customer = customers.FirstOrDefault(c => c.Id == serviceTicket.CustomerId);
    return Results.Ok(serviceTicket);
});

app.MapGet("/customers", () =>
{
    return customers;
});

app.MapGet("/customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(c => c.Id == id);
    if (customer == null)
    {
        return Results.NotFound();
    }
    customer.ServiceTickets = serviceTickets.Where(st => st.CustomerId == id).ToList();
    return Results.Ok(customer);
});

app.MapGet("/employees", () =>
{
    return employees;
});

app.MapGet("/employees/{id}", (int id) =>
{
    Employee employee = employees.FirstOrDefault(e => e.Id == id);
    if (employee == null)
    {
        return Results.NotFound();
    }
    employee.ServiceTickets = serviceTickets.Where(st => st.EmployeeId == id).ToList();
    return Results.Ok(employee);
});

app.MapPost("/servicetickets", (ServiceTicket serviceTicket) =>
{
    serviceTicket.Id = serviceTickets.Max(st  => st.Id) + 1;
    serviceTickets.Add(serviceTicket);
    return serviceTicket;
});

app.MapDelete("/servicetickets/{id}", (int id) => 
{ 
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st  => st.Id == id);
    serviceTickets.Remove(serviceTicket);
});

app.MapPut("/servicetickets/{id}", (int id, ServiceTicket serviceTicket) =>
{ 
    // ticketToUpdate searches through the ServiceTicket list and finds the first Service Ticket that matches the id provided
    ServiceTicket ticketToUpdate = serviceTickets.FirstOrDefault(st  => st.Id == id);
    // gets the zero based index of ticketToUpdate
    int ticketIndex = serviceTickets.IndexOf(ticketToUpdate);
    // if statements to check the index received
    if (ticketToUpdate == null)
    {
        return Results.NotFound();
    }
    if (id != serviceTicket.Id)
    {
        return Results.BadRequest();
    }
    serviceTickets[ticketIndex] = serviceTicket;
    return Results.Ok();
});

app.MapPost("/servicetickets/{id}/complete", (int id) =>
{
    ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(st => st.Id == id);
    ticketToComplete.DateCompleted = DateTime.Today;
});

app.Run();