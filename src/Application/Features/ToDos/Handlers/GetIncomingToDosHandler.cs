using Application.Features.ToDos.Dtos;
using Application.Infrastructure.Persistence;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ToDos.Handlers;

// Query record representing a request to get ToDos for a specific period.
// Possible values for Period: "today", "next day", or "current week".
public record GetIncomingToDosQuery(string Period) : IRequest<ErrorOr<IEnumerable<ToDoDto>>>;

// Handler for GetIncomingToDosQuery.
// Responsible for fetching ToDo items within a specified time range.
internal sealed class GetIncomingToDosHandler(ApplicationDbContext context)
    : IRequestHandler<GetIncomingToDosQuery, ErrorOr<IEnumerable<ToDoDto>>>
{
    public async Task<ErrorOr<IEnumerable<ToDoDto>>> Handle(
        GetIncomingToDosQuery request,
        CancellationToken cancellationToken)
    {
        DateTime now = DateTime.UtcNow;

        // 'start' defines the beginning of the search window
        DateTime start = now;

        // 'targetDate' defines the end of the search window
        DateTime targetDate;

        // Determine the time range based on the provided period
        switch (request.Period.ToLower())
        {
            case "today":
                targetDate = now.AddDays(1);
                break;

            case "next day":
                start = now.AddDays(1);
                targetDate = now.AddDays(2);
                break;

            case "current week":
                targetDate = now.AddDays(7);
                break;

            default:
                // If the period string doesn't match any known value, return a validation error
                return Error.Validation(
                    "InvalidPeriod",
                    "The specified period is invalid. Use 'today', 'next day', or 'current week'."
                );
        }

        // Query the database for ToDos within the calculated date range
        var todos = await context.ToDos
            .Where(t => t.ExpiryDate >= start && t.ExpiryDate < targetDate)
            .OrderBy(t => t.ExpiryDate)
            .Select(t => t.ToDto()) // Map entities to DTOs
            .ToListAsync(cancellationToken);

        return todos;
    }
}
