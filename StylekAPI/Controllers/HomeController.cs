using Microsoft.AspNetCore.Mvc;
using StylekAPI.DTOs.Home;
using StylekAPI.Helpers;
using StylekAPI.Services;

namespace StylekAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HomeController : ControllerBase
{
    private readonly HomeService _homeService;

    public HomeController(HomeService homeService)
    {
        _homeService = homeService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<HomePageDto>>> GetHomePage()
    {
        var data = await _homeService.GetHomePageAsync();
        return Ok(ApiResponse<HomePageDto>.Ok(data));
    }
}
