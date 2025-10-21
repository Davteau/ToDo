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

public record SetToDoPercentCommand(Guid Id, int PercentComplete) : IRequest<ErrorOr<Unit>>;

internal sealed class SetToDoPercentHandler(ApplicationDbContext context) : IRequestHandler<SetToDoPercentCommand, ErrorOr<Unit>>
{
    public async Task<ErrorOr<Unit>> Handle(SetToDoPercentCommand request, CancellationToken cancellationToken)
    {
        var toDo = await context.ToDos.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (toDo is null)
            return Error.NotFound("ToDo.NotFound", $"ToDo not found");

        toDo.PercentComplete = request.PercentComplete;

        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
