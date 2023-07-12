using DataModels;

namespace MCM.Web.Service.Interface
{
    public interface IVenueService
    {
        List<Venue> GetAll();
        bool AddUpdate(Venue venue);
        bool Delete(Guid id);
        Venue FindByID(Guid id);
    }
}
