using AutoMapper;
using ordreChange.DTOs;
using ordreChange.Models;
using OrdreChange.Dtos;

namespace ordreChange.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            //Source => Destination

            // Agent MAPPING
            CreateMap<Agent, AgentDto>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name));

            // Ordre MAPPING
            CreateMap<Ordre, OrdreDto>()
                .ForMember(dest => dest.Agent, opt => opt.MapFrom(src => src.Agent));

            CreateMap<Ordre, OrdreResponseDto>()
                .ForMember(dest => dest.Agent, opt => opt.MapFrom(src => src.Agent));

            CreateMap<CreerOrdreDto, Ordre>()
                .ForMember(dest => dest.DateCreation, opt => opt.Ignore())
                .ForMember(dest => dest.Statut, opt => opt.Ignore())
                .ForMember(dest => dest.Agent, opt => opt.Ignore())
                .ForMember(dest => dest.MontantConverti, opt => opt.Ignore());

            // Historique MAPPING
            CreateMap<HistoriqueOrdre, HistoriqueOrdreDto>();

            CreateMap<Ordre, HistoriqueDto>()
                .ForMember(dest => dest.HistoriqueOrdres, opt => opt.MapFrom(src => src.HistoriqueOrdres))
                .ForMember(dest => dest.Agent, opt => opt.MapFrom(src => src.Agent));
        }
    }
}
