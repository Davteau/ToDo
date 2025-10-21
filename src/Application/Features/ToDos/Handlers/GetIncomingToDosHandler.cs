using Application.Features.ToDos.Dtos;
using Application.Infrastructure.Persistence;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.ToDos.Handlers;

public record GetIncomingToDosQuery(string Period) : IRequest<ErrorOr<IEnumerable<ToDoDto>>>;

internal sealed class GetIncomingToDosHandler(ApplicationDbContext context) : IRequestHandler<GetIncomingToDosQuery, ErrorOr<IEnumerable<ToDoDto>>>
{
    public async Task<ErrorOr<IEnumerable<ToDoDto>>> Handle(GetIncomingToDosQuery request, CancellationToken cancellationToken)
    {
        DateTime now = DateTime.UtcNow;
        DateTime start = now;
        DateTime targetDate;
        
        switch(request.Period.ToLower())
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
                return Error.Validation("InvalidPeriod", "The specified period is invalid. Use 'today', 'next day', or 'current week'.");
        }

        var todos = await context.ToDos
            .Where(t => t.ExpiryDate >= start && t.ExpiryDate < targetDate)
            .OrderBy(t => t.ExpiryDate)
            .Select(t => t.ToDto())
            .ToListAsync(cancellationToken);

        return todos;
    }
}
