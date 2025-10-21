using Application.Common.Behaviours;
using Application.Features.ToDos.Validators;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests")]

namespace Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(CreateToDoValidator).Assembly, includeInternalTypes: true);

        services.AddMediatR(options =>
        {
            options.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            options.RegisterServicesFromAssembly(typeof(ApplicationServiceRegistration).Assembly);
        });

        return services;
    }
}