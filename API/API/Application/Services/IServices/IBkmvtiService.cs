using API.Application.DTO;

namespace API.Application.Services.IServices
{
    public interface IBkmvtiService
    {
        public Task<List<BkmvtiResult>> BkmvtisByMagType(Guid typeMagId);
        public Task<List<BkmvtiSyntheseDto>> GetSyntheseAsync(Guid typeMagId);

        public Task<List<CarteARegulerDto>> CarteAReguler(Guid typeMagId);
        public Task<DashboardSynthese> DashboardResult();
        public Task<CustomerBilling> GetAllCustomerBilling(DateTimeOffset dateDebutFacturation, string ncpf, DateTimeOffset? dateFinFacturation = null);
    }
}