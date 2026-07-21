using API.Application.DTO;
using API.Domain.Entities;

namespace API.Application.Repository.IRepository
{
    public interface IBkmvtiRepository
    {
        public Task<List<BkmvtiResult>> BkmvtisByMagType(Guid typeMagId);
        public Task<List<BkmvtiSyntheseDto>> GetSyntheseByTypeMagAsync(Guid typeMagId);
        public Task<List<Bkmvti>> SaveBkmvtiAsync(List<Bkmvti> bkmvtis);
        public Task<List<CarteARegulerDto>> CarteAReguler(Guid typeMagId);
        public Task<DashboardSynthese> DashboardResult();
        public Task<CustomerBilling> GetAllCustomerBilling(DateTimeOffset dateDebutFacturation, string ncpf, DateTimeOffset? dateFinFacturation = null);
    }
}