using Application.Infrastructure.Persistence;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ToDos.Handlers;

public record DeleteToDoCommand(Guid Id) : IRequest<ErrorOr<Unit>>;

internal sealed class DeleteToDoHandler(ApplicationDbContext context) : IRequestHandler<DeleteToDoCommand, ErrorOr<Unit>>
{
    public async Task<ErrorOr<Unit>> Handle(DeleteToDoCommand request, CancellationToken cancellationToken)
    {
        var toDo = await context.ToDos.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (toDo is null)
        {
            return Error.NotFound("ToDo.NotFound", "ToDo not found");
        }

        context.ToDos.Remove(toDo);

        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
