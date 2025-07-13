using Law4Hire.Core.Entities;
using Law4Hire.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Law4Hire.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VisaTypeQuestionsController(IVisaTypeQuestionRepository repository) : ControllerBase
{
    private readonly IVisaTypeQuestionRepository _repository = repository;

    [HttpGet("visa/{visaTypeId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<VisaTypeQuestion>>> GetByVisa(Guid visaTypeId)
    {
        var questions = await _repository.GetByVisaTypeIdAsync(visaTypeId);
        return Ok(questions);
    }
}
