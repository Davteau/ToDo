using Application.Features.ToDos.Dtos;
using Application.Infrastructure.Persistence;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ToDos.Handlers;

public record UpdateToDoCommand() : IRequest<ErrorOr<Unit>>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public int PercentComplete { get; set; }
}

internal sealed class UpdateToDoHandler(ApplicationDbContext context) : IRequestHandler<UpdateToDoCommand, ErrorOr<Unit>>
{
    public async Task<ErrorOr<Unit>> Handle(UpdateToDoCommand request, CancellationToken cancellationToken)
    {

        var toDo = await context.ToDos.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (toDo is null)
            return Error.NotFound("ToDo.NotFound", $"ToDo not found");

        toDo.UpdateFromDto(request);

        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
