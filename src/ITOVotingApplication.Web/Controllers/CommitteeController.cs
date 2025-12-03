using ITOVotingApplication.Core.DTOs.Committee;
using ITOVotingApplication.Core.DTOs.Common;
using ITOVotingApplication.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITOVotingApplication.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class CommitteeController : ControllerBase
	{
		private readonly ICommitteeService _committeeService;

		public CommitteeController(ICommitteeService committeeService)
		{
			_committeeService = committeeService;
		}

		[HttpGet]
		public async Task<IActionResult> GetAll([FromQuery] PagedRequest request)
		{
			var result = await _committeeService.GetAllAsync(request);
			return Ok(result);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(int id)
		{
			var result = await _committeeService.GetByIdAsync(id);
			if (!result.Success)
			{
				return NotFound(result);
			}
			return Ok(result);
		}

		[HttpPost]
		[Authorize(Roles = "İtop Kullanıcısı")]
		public async Task<IActionResult> Create([FromBody] CreateCommitteeDto dto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var result = await _committeeService.CreateAsync(dto);
			if (!result.Success)
			{
				return BadRequest(result);
			}

			return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result);
		}

		[HttpPut("{id}")]
		[Authorize(Roles = "İtop Kullanıcısı")]
		public async Task<IActionResult> Update(int id, [FromBody] UpdateCommitteeDto dto)
		{
			if (id != dto.Id)
			{
				return BadRequest("ID uyuşmazlığı");
			}

			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var result = await _committeeService.UpdateAsync(dto);
			if (!result.Success)
			{
				return BadRequest(result);
			}

			return Ok(result);
		}

		[HttpDelete("{id}")]
		[Authorize(Roles = "İtop Kullanıcısı")]
		public async Task<IActionResult> Delete(int id)
		{
			var result = await _committeeService.DeleteAsync(id);
			if (!result.Success)
			{
				return BadRequest(result);
			}

			return Ok(result);
		}

		[HttpGet("dropdown")]
		public async Task<IActionResult> GetForDropdown()
		{
			var result = await _committeeService.GetAllForDropdownAsync();
			return Ok(result);
		}
	}
}