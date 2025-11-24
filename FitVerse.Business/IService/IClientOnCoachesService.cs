using FitVerse.Core.ViewModels.Coach;
using FitVerse.Core.ViewModels.Specialist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitVerse.Core.IService
{
    public interface  IClientOnCoachesService :IService
    {
        List<ClientsVM> GetAllClients();
        List<ClientsVM> GetClientsByCoachId(string coachId);
    }
}
