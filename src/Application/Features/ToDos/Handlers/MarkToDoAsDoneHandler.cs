using Application.Infrastructure.Persistence;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ToDos.Handlers;

public record MarkToDoAsDoneCommand(Guid Id) : IRequest<ErrorOr<Unit>>;

internal sealed class MarkToDoAsDoneHandler(ApplicationDbContext context) : IRequestHandler<MarkToDoAsDoneCommand, ErrorOr<Unit>>
{
    public async Task<ErrorOr<Unit>> Handle(MarkToDoAsDoneCommand request, CancellationToken cancellationToken)
    {
        var toDo = await context.ToDos.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (toDo is null)
            return Error.NotFound("ToDo.NotFound", $"ToDo not found");

        toDo.PercentComplete = 100;

        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
