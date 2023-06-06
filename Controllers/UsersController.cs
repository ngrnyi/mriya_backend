using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using MessengerBackend.Models;
using MessengerBackend.Helpers;
using MessengerBackend.ViewModels;
using Microsoft.EntityFrameworkCore;
using MessengerBackend.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace MessengerBackend.Controllers;
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly AppSettings _appSettings;
    private readonly ILogger<UsersController> _logger; // Adding the ILogger service
    private readonly MessengerDbContext _context;
    private readonly TwilioHelper twilioHelper = new TwilioHelper("{YOUR KEY}", "{YOUR KEY}", "+1111111");
    private readonly JwtHelper _jwtHelper;


    public UsersController(UserManager<User> userManager, SignInManager<User> signInManager, IOptions<AppSettings> appSettings, ILogger<UsersController> logger, MessengerDbContext dbContext,JwtHelper jwtHelper)
    {
        _userManager = userManager;
        _appSettings = appSettings.Value;
        _logger = logger;
        _context = dbContext;
        _jwtHelper = jwtHelper;
    }

    [AllowAnonymous]
    [HttpPost("RequestVerification")]
    public async Task<IActionResult> RequestVerification([FromBody] PhoneVerificationRequest request)
    {
        var phoneTaken = _context.PhoneVerificationCodes.Any(x => x.PhoneNumber == request.PhoneNumber);
        if (phoneTaken)
        {
            return BadRequest("Phone number is already taken");

        }
        var verificationCode = new Random().Next(100000, 999999).ToString();

        // Replace this with actual SMS sending implementation
        twilioHelper.SendSms(request.PhoneNumber, $"Your register verification code is: {verificationCode}");
        var phoneVerificationCode = new PhoneVerificationCode
        {
            PhoneNumber = request.PhoneNumber,
            Code = verificationCode,
            ExpirationTime = DateTimeOffset.UtcNow.AddMinutes(10)
        };

        _context.PhoneVerificationCodes.Add(phoneVerificationCode);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("VerifyPhoneNumber")]
    public async Task<IActionResult> VerifyPhoneNumber([FromBody] VerifyPhoneNumberRequest request)
    {
        var phoneVerificationCode = await _context.PhoneVerificationCodes
            .Where(pvc => pvc.PhoneNumber == request.PhoneNumber && pvc.Code == request.Code)
            .FirstOrDefaultAsync();

        if (phoneVerificationCode == null || phoneVerificationCode.ExpirationTime < DateTimeOffset.UtcNow)
        {
            return BadRequest("Invalid verification code or the code has expired.");
        }

        phoneVerificationCode.Verified = true;
        _context.Update(phoneVerificationCode);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("RegisterWithPhoneNumber")]
    public async Task<IActionResult> RegisterWithPhoneNumber([FromBody] RegisterWithPhoneNumberRequest request)
    {

        var phoneVerificationCode = await _context.PhoneVerificationCodes
            .Where(pvc => pvc.PhoneNumber == request.PhoneNumber && pvc.Verified)
            .FirstOrDefaultAsync();
        var usernameVerification = await _context.Users.Where(user => user.UserName == request.UserName).FirstOrDefaultAsync();
        if(usernameVerification != null)
        {
            return BadRequest("User with such username alrady exists");
        }
        if (phoneVerificationCode == null)
        {
            return BadRequest("Phone number is not verified.");
        }

        var user = new User
        {
            UserName = request.UserName,
            PhoneNumber = request.PhoneNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            ConnectionId=Guid.NewGuid().ToString()
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            var token = await _jwtHelper.GenerateJwtTokenAsync(user);
            return Ok(new { token });
        }
        else
        {
            return BadRequest(result.Errors);
        }
    }

   

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginDto)
    {
        // Check if the user exists by phone number
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == loginDto.PhoneNumber || u.UserName == loginDto.UserName);

        if (user == null)
        {
            return BadRequest("Invalid phone number or username.");
        }

        // Check if the password is correct
        if (!await _userManager.CheckPasswordAsync(user, loginDto.Password))
        {
            return BadRequest("Invalid password.");
        }

        // Generate the JWT token and return it in the response
        var token = await _jwtHelper.GenerateJwtTokenAsync(user);
        return Ok(new { token });
    }

    [AllowAnonymous]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        Console.WriteLine("WTF");
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            id = user.Id,
            username = user.UserName,
            phoneNumber = user.PhoneNumber,
            firstName = user.FirstName,
            lastName = user.LastName,
            contacts = user.Contacts
        });
    }
    [HttpGet("getAllUsers")]
    public async Task<ActionResult<IEnumerable<object>>> GetAllUsers([FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 10)
    {
        var users = await _userManager.Users
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                u.Id,
                u.UserName,
                u.Email
            })
            .ToListAsync();

        return Ok(users);
    }
}
