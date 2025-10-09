using AutoMapper;
using ITOVotingApplication.Core.DTOs.Company;
using ITOVotingApplication.Core.DTOs.Contact;
using ITOVotingApplication.Core.DTOs.User;
using ITOVotingApplication.Core.DTOs.Vote;
using ITOVotingApplication.Core.Entities;

namespace ITOVotingApplication.Business.Mappings
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			// Company Mappings
			CreateMap<Company, CompanyDto>()
				.ForMember(dest => dest.ActiveContactName,
					opt => opt.MapFrom(src => src.ActiveContact != null ? src.ActiveContact.FullName : null));

			CreateMap<CreateCompanyDto, Company>();
			CreateMap<UpdateCompanyDto, Company>();

			// Contact Mappings
			CreateMap<Contact, ContactDto>()
				.ForMember(dest => dest.CompanyTitle,
					opt => opt.MapFrom(src => src.Company.Title))
				.ForMember(dest => dest.FullName,
					opt => opt.MapFrom(src => src.FullName));

			CreateMap<CreateContactDto, Contact>();
			CreateMap<UpdateContactDto, Contact>();

			// Vote Mappings
			CreateMap<VoteTransaction, VoteDto>()
				.ForMember(dest => dest.CompanyTitle,
					opt => opt.MapFrom(src => src.Company.Title))
				.ForMember(dest => dest.ContactName,
					opt => opt.MapFrom(src => src.Contact.FullName))
				.ForMember(dest => dest.BallotBoxDescription,
					opt => opt.MapFrom(src => src.BallotBox.BallotBoxDescription))
				.ForMember(dest => dest.CreatedUserName,
					opt => opt.MapFrom(src => src.CreatedUser.FullName));

			// User Mappings
			CreateMap<User, UserDto>()
				.ForMember(dest => dest.FullName,
					opt => opt.MapFrom(src => src.FullName))
				.ForMember(dest => dest.Roles,
					opt => opt.MapFrom(src => src.UserRoles
						.Where(ur => ur.IsActive)
						.Select(ur => ur.Role.RoleDescription).ToList()))
				.ForMember(dest => dest.FieldReferenceCategoryId,
					opt => opt.MapFrom(src => src.UserRoles
						.Where(ur => ur.IsActive)
						.FirstOrDefault().FieldReferenceCategoryId))
				.ForMember(dest => dest.FieldReferenceSubCategoryId,
					opt => opt.MapFrom(src => src.UserRoles
						.Where(ur => ur.IsActive)
						.FirstOrDefault().FieldReferenceSubCategoryId))
				.ForMember(dest => dest.FieldReferenceCategoryName,
					opt => opt.MapFrom(src => src.UserRoles
						.Where(ur => ur.IsActive && ur.FieldReferenceCategory != null)
						.FirstOrDefault().FieldReferenceCategory.CategoryName))
				.ForMember(dest => dest.FieldReferenceSubCategoryName,
					opt => opt.MapFrom(src => src.UserRoles
						.Where(ur => ur.IsActive && ur.FieldReferenceSubCategory != null)
						.FirstOrDefault().FieldReferenceSubCategory.SubCategoryName));

			CreateMap<CreateUserDto, User>();
			CreateMap<UpdateUserDto, User>();
		}
	}
}
