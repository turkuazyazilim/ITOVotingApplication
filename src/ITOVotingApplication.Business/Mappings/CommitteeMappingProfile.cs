using AutoMapper;
using ITOVotingApplication.Core.DTOs.Committee;
using ITOVotingApplication.Core.Entities;

namespace ITOVotingApplication.Business.Mappings
{
	public class CommitteeMappingProfile : Profile
	{
		public CommitteeMappingProfile()
		{
			CreateMap<Committee, CommitteeDto>();
			CreateMap<CreateCommitteeDto, Committee>();
			CreateMap<UpdateCommitteeDto, Committee>();
		}
	}
}