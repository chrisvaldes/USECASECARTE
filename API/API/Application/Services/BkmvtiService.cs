using API.Application.DTO;
using API.Application.Repository.IRepository;
using API.Application.Services.IServices;

namespace API.Application.Services
{
    public class BkmvtiService : IBkmvtiService
{
    private readonly IBkmvtiRepository _bkmvtiRepository;
    public BkmvtiService(IBkmvtiRepository bkmvtiRepository) {
        _bkmvtiRepository = bkmvtiRepository;
    }
    public async Task<List<BkmvtiResult>> BkmvtisByMagType(Guid typeMagId)
    {
        return await _bkmvtiRepository.BkmvtisByMagType(typeMagId);
    }
    public async Task<List<BkmvtiSyntheseDto>> GetSyntheseAsync(Guid typeMagId)
    {
        return await _bkmvtiRepository.GetSyntheseByTypeMagAsync(typeMagId);
    }

    public async Task<List<CarteARegulerDto>> CarteAReguler(Guid typeMagId)
    {
        return await _bkmvtiRepository.CarteAReguler(typeMagId);
    }

    public async Task<DashboardSynthese> DashboardResult()
    {
        return await _bkmvtiRepository.DashboardResult();
    }

    public async Task<CustomerBilling> GetAllCustomerBilling(DateTimeOffset dateDebutFacturation, string ncpf, DateTimeOffset? dateFinFacturation = null)

    {
        var result = await _bkmvtiRepository.GetAllCustomerBilling(dateDebutFacturation, ncpf, dateFinFacturation);
        return result;
    }

    //public async Task<TypeMagWithSyntheseDto> GetTypeMagWithSynthese(Guid typeMagId)
    //{
    //    return await _bkmvtiRepository.GetTypeMagWithSyntheseAsync(typeMagId);
    //}
}
}