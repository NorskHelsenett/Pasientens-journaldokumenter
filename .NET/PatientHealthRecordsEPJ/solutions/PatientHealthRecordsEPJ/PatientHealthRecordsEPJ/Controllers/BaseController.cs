using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Options;
using PatientHealthRecordsEPJ.Services; // Needed for SessionOptions

namespace PatientHealthRecordsEPJ.Controllers;

public class BaseController : Controller
{
	private readonly TimeSpan _sessionTimeout;
	protected readonly IUserTokenService _userTokenService;

	public BaseController(IOptions<SessionOptions> sessionOptions, IUserTokenService userTokenService)
	{
		_sessionTimeout = sessionOptions.Value.IdleTimeout;
		_userTokenService = userTokenService;
	}

	public override void OnActionExecuting(ActionExecutingContext context)
	{
		base.OnActionExecuting(context);

		var lastAccessString = HttpContext.Session.GetString("LastAccess");

		if (DateTime.TryParse(lastAccessString, out DateTime lastAccess)) 
		{
			var timeLeft = _sessionTimeout - (DateTime.Now - lastAccess);
			var expiresAt = lastAccess.Add(_sessionTimeout);
			ViewBag.SessionTimeLeft = timeLeft > TimeSpan.Zero ? timeLeft : TimeSpan.Zero;
			ViewBag.SessionExpiresAt = expiresAt;
		}
	}
}