using AutoMapper;
using Route.C41.G01.DAL.Models;
using Route.C41.G01.PL.ViewModels;

namespace Route.C41.G01.PL.Hepers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<EmployeeViewModel, Employee>().ReverseMap();

            //CreateMap<Employee, EmployeeViewModel>();
        }
    }
}
