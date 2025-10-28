using CleanArchitecture.Cmms.Domain.Technicians;
using CleanArchitecture.Cmms.Domain.Technicians.ValueObjects;
using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;

namespace CleanArchitecture.Cmms.Application.Technicians.Commands.AddCertification
{
    internal sealed class AddCertificationCommandHandler
        : ICommandHandler<AddCertificationCommand, Result>
    {
        private readonly IRepository<Technician, Guid> _repository;

        public AddCertificationCommandHandler(IRepository<Technician, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<Result> Handle(AddCertificationCommand request, CancellationToken cancellationToken = default)
        {
            var technician = await _repository.GetByIdAsync(request.TechnicianId, cancellationToken);

            if (technician is null)
                return TechnicianErrors.NotFound;

            var certification = Certification.Create(request.Code, request.IssuedOn, request.ExpiresOn);

            technician.AddCertification(certification);

            return Result.Success();
        }
    }
}
