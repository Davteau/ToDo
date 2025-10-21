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

public record GetToDoQuery(Guid Id) : IRequest<ErrorOr<ToDoDto>>;

internal sealed class GetToDoHandler(ApplicationDbContext context) : IRequestHandler<GetToDoQuery, ErrorOr<ToDoDto>>
{
    public async Task<ErrorOr<ToDoDto>> Handle(GetToDoQuery request, CancellationToken cancellationToken)
    {
        var toDo = await context.ToDos.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (toDo is null)
        {
            return Error.NotFound("ToDo.NotFound", "ToDo not found");
        }

        return toDo.ToDto();
    }
}
