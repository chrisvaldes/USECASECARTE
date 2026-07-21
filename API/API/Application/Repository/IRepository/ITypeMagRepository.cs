using API.Application.DTO;
using API.Domain.Entities;

namespace API.Application.Repository.IRepository
{
    public interface ITypeMagRepository
    {
        public Task<TypeMag> SaveTypeMagAsync(TypeMag typeMag);
        public Task<IEnumerable<TypeMag>> getAllMag();
        public Task<TypeMagWithSyntheseDto> GetTypeMagWithSyntheseAsync(Guid typeMagId);
        public Task<TypeMag?> IsTypeMagExist(DateTimeOffset startPeriod);
        public Task<bool> IsDownload(Guid typeMagId);
    }
}