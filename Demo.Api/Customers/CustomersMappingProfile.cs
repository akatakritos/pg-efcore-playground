using AutoMapper;
using Demo.Api.Data;
using Demo.Api.Shared;

namespace Demo.Api.Customers
{
    public class CustomersMappingProfile : Profile
    {
        public CustomersMappingProfile()
        {
            CreateMap<Customer, GetCustomerResponse>()
                .IncludeModelKey();
        }

    }
}
