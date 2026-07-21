using API.Domain.Entities;

namespace API.Application.DTO
{
    public class CarteIntersectionResult
    {
        public Apprint Carte { get; set; }
        public Periode Intersection { get; set; }

        public CarteIntersectionResult(Apprint carte, Periode intersection)
        {
            Carte = carte;
            Intersection = intersection;
        }
    }
}