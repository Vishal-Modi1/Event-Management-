using DataModels;

namespace MCM.Web.Service.Interface
{
    public interface IEventService
    {
        List<Event> GetAll();
        bool AddUpdate(Event events);
        bool Delete(Guid id);
        Event FindByID(Guid id);
    }
}
