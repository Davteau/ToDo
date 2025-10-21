using Application.Features.ToDos.Dtos;
using Application.Infrastructure.Persistence;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ToDos.Handlers;

public record GetToDosQuery() : IRequest<ErrorOr<IEnumerable<ToDoDto>>>;

internal sealed class GetToDosHandler(ApplicationDbContext context) : IRequestHandler<GetToDosQuery, ErrorOr<IEnumerable<ToDoDto>>>
{
    public async Task<ErrorOr<IEnumerable<ToDoDto>>> Handle(GetToDosQuery request, CancellationToken cancellationToken)
    {
        var toDos = await context.ToDos
            .Select(t => t.ToDto())
            .ToListAsync(cancellationToken);

        return toDos;
    }
}
