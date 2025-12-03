using ITOVotingApplication.Business.Interfaces;
using ITOVotingApplication.Core.DTOs.Common;
using ITOVotingApplication.Core.DTOs.Contact;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITOVotingApplication.Web.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class ContactController : ControllerBase
	{
		private readonly IContactService _contactService;

		public ContactController(IContactService contactService)
		{
			_contactService = contactService;
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(int id)
		{
			var result = await _contactService.GetByIdAsync(id);

			if (!result.Success)
				return NotFound(result);

			return Ok(result);
		}

		[HttpGet]
		public async Task<IActionResult> GetAll([FromQuery] PagedRequest request)
		{
			var result = await _contactService.GetAllAsync(request);
			return Ok(result);
		}

		[HttpGet("company/{companyId}")]
		public async Task<IActionResult> GetByCompanyId(int companyId)
		{
			var result = await _contactService.GetByCompanyIdAsync(companyId);
			return Ok(result);
		}

		[HttpGet("eligible/{ballotBoxId}")]
		public async Task<IActionResult> GetEligibleVoters(int ballotBoxId)
		{
			var result = await _contactService.GetEligibleVotersAsync(ballotBoxId);
			return Ok(result);
		}

		[HttpGet("identity/{identityNum}")]
		public async Task<IActionResult> GetByIdentityNum(string identityNum)
		{
			var result = await _contactService.GetByIdentityNumAsync(identityNum);

			if (!result.Success)
				return NotFound(result);

			return Ok(result);
		}

		[HttpGet("by-registration-number/{registrationNumber}")]
		public async Task<IActionResult> GetByRegistrationNumber(string registrationNumber)
		{
			var result = await _contactService.GetByCompanyRegistrationNumberAsync(registrationNumber);

			if (!result.Success)
				return NotFound(result);

			return Ok(result);
		}

		[HttpPost]
		[Authorize(Roles = "İtop Kullanıcısı")]
		public async Task<IActionResult> Create([FromBody] CreateContactDto dto)
		{
			var result = await _contactService.CreateAsync(dto);

			if (!result.Success)
				return BadRequest(result);

			return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result);
		}

		[HttpPut("{id}")]
		[Authorize(Roles = "İtop Kullanıcısı")]
		public async Task<IActionResult> Update(int id, [FromBody] UpdateContactDto dto)
		{
			if (id != dto.Id)
				return BadRequest("ID uyuşmazlığı");

			var result = await _contactService.UpdateAsync(dto);

			if (!result.Success)
				return BadRequest(result);

			return Ok(result);
		}

		[HttpDelete("{id}")]
		[Authorize(Roles = "İtop Kullanıcısı")]
		public async Task<IActionResult> Delete(int id)
		{
			var result = await _contactService.DeleteAsync(id);

			if (!result.Success)
				return BadRequest(result);

			return Ok(result);
		}
	}
}