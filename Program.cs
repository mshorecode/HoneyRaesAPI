using HoneyRaesAPI.Models;

List<Customer> customers = new()
{
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
List<Employee> employees = new()
{
    new Employee()
    {
        Id = 1,
        Name = "Johnny Saniat",
        Specialty = "Data Recovery"
    },
    new Employee()
    {
        Id = 2,
        Name = "Greg markus",
        Specialty = "Troubleshooting"
    },
    new Employee()
    {
        Id = 3,
        Name = "Alex Sendre",
        Specialty = ""
    }
};
List<ServiceTicket> serviceTickets = new()
{
    new ServiceTicket()
    {
        Id = 1,
        CustomerId = 2,
        EmployeeId = null,
        Description = "Computer is running slow",
        Emergency = false,
        DateCompleted = null
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
        Description =
            "My computer keeps locking up on me randomly and I have to shutdown to fix it",
        Emergency = true,
        DateCompleted = new DateTime(2023, 12, 28)
    },
    new ServiceTicket()
    {
        Id = 4,
        CustomerId = 3,
        Description = "Move data from one hard drive to another",
        Emergency = false,
        DateCompleted = null
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

app.MapGet("/api/servicetickets", () =>
{
    return serviceTickets;
});

app.MapGet("/api/servicetickets/{id}", (int id) =>
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

app.MapGet("/api/customers", () =>
{
    return customers;
});

app.MapGet("/api/customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(c => c.Id == id);
    if (customer == null)
    {
        return Results.NotFound();
    }
    customer.ServiceTickets = serviceTickets.Where(st => st.CustomerId == id).ToList();
    return Results.Ok(customer);
});

app.MapGet("/api/employees", () =>
{
    return employees;
});

app.MapGet("/api/employees/{id}", (int id) =>
{
    Employee employee = employees.FirstOrDefault(e => e.Id == id);
    if (employee == null)
    {
        return Results.NotFound();
    }
    employee.ServiceTickets = serviceTickets.Where(st => st.EmployeeId == id).ToList();
    return Results.Ok(employee);
});

app.MapPost("/api/servicetickets", (ServiceTicket serviceTicket) =>
{
    serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
    serviceTickets.Add(serviceTicket);
    return serviceTicket;
});

app.MapDelete("/api/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);
    serviceTickets.Remove(serviceTicket);
});

app.MapPut("/api/servicetickets/{id}", (int id, ServiceTicket serviceTicket) =>
{
    // ticketToUpdate searches through the ServiceTicket list and finds the first Service Ticket that matches the id provided
    ServiceTicket ticketToUpdate = serviceTickets.FirstOrDefault(st => st.Id == id);
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

app.MapPost("/api/servicetickets/{id}/complete", (int id) =>
{
    ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(st => st.Id == id);
    ticketToComplete.DateCompleted = DateTime.Today;
    return Results.Ok();
});

app.MapGet("/api/servicetickets/emergencies", () =>
{
    List<ServiceTicket> emergencies = serviceTickets.Where(st => st.Emergency == true && st.DateCompleted == null).ToList();

    return Results.Ok(emergencies);

});

app.MapGet("/api/servicetickets/unassigned", () =>
{
    List<ServiceTicket> unassignedTickets = serviceTickets.Where(st => st.EmployeeId == null).ToList();

    return Results.Ok(unassignedTickets); 
});

app.MapGet("/api/customers/inactive", () =>
{
    var inactiveCustomers = customers
        .Where(c =>
            !serviceTickets.Any(st =>
                st.CustomerId == c.Id &&
                st.DateCompleted.HasValue && st.DateCompleted.Value > DateTime.Now.AddYears(-1)
            )
        )
        .ToList();

    return Results.Ok(inactiveCustomers);
});

app.MapGet("/api/employees/available", () =>
{
    List<ServiceTicket> availableEmployees = serviceTickets.Where(st => st.DateCompleted != null && st.EmployeeId >= 0).ToList();

    return Results.Ok(availableEmployees);

});

app.MapGet("/api/employees/{id}/customers", (int id) => 
{
    var employeeCustomers = serviceTickets
        .Where(st => st.EmployeeId == id)
        .Select(st => customers.FirstOrDefault(c => c.Id == st.CustomerId))
        .Distinct()
        .ToList();

    return Results.Ok(employeeCustomers);
});

app.MapGet("/api/employees/employeeofthemonth", () =>
{
    var lastMonth = DateTime.Now.AddMonths(-1);
    var employeeOfTheMonth = employees
        .OrderByDescending(e => serviceTickets.Count(st => st.EmployeeId == e.Id && st.DateCompleted.HasValue && st.DateCompleted.Value.Month == lastMonth.Month))
        .FirstOrDefault();

    return Results.Ok(employeeOfTheMonth);
});

app.MapGet("/api/servicetickets/ticketreview", () =>
{
    var completedTickets = serviceTickets
        .Where(st => st.DateCompleted.HasValue)
        .OrderBy(st => st.DateCompleted)
        .ToList();

    foreach (var ticket in completedTickets)
    {
        ticket.Customer = customers.FirstOrDefault(c => c.Id == ticket.CustomerId);
        ticket.Employee = employees.FirstOrDefault(e => e.Id == ticket.EmployeeId);
    }

    return Results.Ok(completedTickets);
});

app.MapGet("/api/servicetickets/priority", () =>
{
    var priorityTickets = serviceTickets
        .Where(st => !st.DateCompleted.HasValue)
        .OrderByDescending(st => st.Emergency)
        .ThenBy(st => st.EmployeeId.HasValue) 
        .ToList();

    foreach (var ticket in priorityTickets)
    {
        ticket.Customer = customers.FirstOrDefault(c => c.Id == ticket.CustomerId);
        ticket.Employee = employees.FirstOrDefault(e => e.Id == ticket.EmployeeId);
    }

    return Results.Ok(priorityTickets);
});


app.Run();
