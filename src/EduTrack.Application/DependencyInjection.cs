using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Services;
using EduTrack.Application.Common.Services.Validators;
using EduTrack.Domain.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EduTrack.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Register Domain Services
        services.AddScoped<IUserDomainService, UserDomainService>();
        services.AddScoped<ICourseDomainService, CourseDomainService>();
        services.AddScoped<IProgressDomainService, ProgressDomainService>();

        // Register Page Title Section Services
        services.AddScoped<IPageTitleSectionService, PageTitleSectionService>();
        services.AddScoped<IPageTitleSectionFactory, PageTitleSectionFactory>();

        // Register Block Answer Validators
        services.AddScoped<IBlockAnswerValidator, OrderingBlockValidator>();
        services.AddScoped<IBlockAnswerValidator, MultipleChoiceBlockValidator>();
        services.AddScoped<BlockAnswerValidatorFactory>();

        return services;
    }
}
