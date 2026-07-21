using API.Domain.Entities;

namespace API.Application.Repository.IRepository
{
    public interface ICartesRepository
    {
        public Dictionary<string, ComptesActifsResponse> ComptesActifs();
        public Dictionary<string, ComptesOuvert> ComptesOuverts();
        public Dictionary<string, DateDsouPackEchuResponse> DateDernSouscripPackEchu();
        public Dictionary<string, PackagesActifsResponse> PackagesActifs();
        public Dictionary<string, HistCptDebiteRedevCarteResponse> HistCptDebiteRedevCarte();
    }
}