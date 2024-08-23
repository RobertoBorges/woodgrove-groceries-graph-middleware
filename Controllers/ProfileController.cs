using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using Microsoft.Identity.Web;
using woodgrovedemo.Models;

namespace woodgrove_groceries_graph_middleware.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class ProfileController : ControllerBase
{

    private readonly ILogger<ProfileController> _logger;
    private readonly GraphServiceClient _graphServiceClient;

    public ProfileController(ILogger<ProfileController> logger, GraphServiceClient graphServiceClient)
    {
        _logger = logger;
        _graphServiceClient = graphServiceClient; ;
    }


    [HttpPost]
    public async Task<IActionResult> OnPostAsync([FromForm] UserAttributes att)
    {

        // Get the user unique identifier
        string? userObjectId = User.GetObjectId();

        if (userObjectId == null)
        {
            att.ErrorMessage = "The account cannot be updated since your access token doesn't contain the required 'objectidentifier' claim.";
        }

        try
        {
            // Update user by object ID
            var requestBody = new User
            {
                DisplayName = att.DisplayName,
                GivenName = att.GivenName,
                Surname = att.Surname,
                Country = att.Country,
                City = att.City
            };

            var result = await _graphServiceClient.Me.PatchAsync(requestBody);
        }
        catch (ODataError odataError)
        {
            att.ErrorMessage = $"The account cannot be updated due to the following error: {odataError.Error!.Message} Error code: {odataError.Error.Code}";
            //TrackException(odataError, "OnPostProfileAsync");
        }
        catch (Exception ex)
        {
            string error = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            att.ErrorMessage = $"The account cannot be updated due to the following error: {error}";
            //TrackException(ex, "OnPostProfileAsync");
        }

        return Ok(att);
    }
}
