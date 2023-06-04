using System.Dynamic;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Helpers;
using Server.Models;
using SharedModels.DataTransferObjects.Model;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;
using SharedModels.Requests;
using SharedModels.Requests.Account;
using Utils;

namespace Server.Services;

public class DriverManagementService : IDriverManagementService
{
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private readonly ISortHelper<ExpandoObject> _userSortHelper;
    private readonly IDataShaper<DriverDto> _userDataShaper;
    private readonly IPager<ExpandoObject> _pager;
    private readonly ISessionUserService _sessionUserService;
    private readonly IEmailSenderService _emailSender;

    public DriverManagementService( IMapper mapper, UserManager<User> userManager,
        ISortHelper<ExpandoObject> userSortHelper, IDataShaper<DriverDto> userDataShaper,
        IPager<ExpandoObject> pager, ISessionUserService sessionUserService,
        IEmailSenderService emailSenderService)
    {
        _userManager = userManager;
        _userSortHelper = userSortHelper;
        _userDataShaper = userDataShaper;
        _pager = pager;
        _sessionUserService = sessionUserService;
        _emailSender = emailSenderService;
        _mapper = mapper;
        
        _userManager.UserValidators.Clear();
    }

    public async Task<(bool isSucceeded, IActionResult actionResult)> RegisterDriver(DriverRegistrationRequest request)
    {
        if (_sessionUserService.GetAuthUserRole() == Identity.Roles.Administrator.ToString())
        {
            if (request.CompanyId == null)
            {
                return (false, new BadRequestObjectResult("CompanyId must have a value"));
            }
        }
        else
        {
            var isAuthUserCompanyOwnerResult = await _sessionUserService.IsAuthUserCompanyOwner();
            if (!isAuthUserCompanyOwnerResult.isCompanyOwner)
            {
                return (false, new UnauthorizedResult());
            }
            request.CompanyId = isAuthUserCompanyOwnerResult.companyId;
        }

        var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
        if (userWithSameEmail != null)
        {
            return (false, new BadRequestObjectResult("Email is already registered."));
        }

        var userWithSamePhone = await _userManager.Users
            .SingleOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);
        if (userWithSamePhone != null)
        {
            return (false, new BadRequestObjectResult("Phone is already registered."));
        }

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Patronymic = request.Patronymic,
            BirthDate = new DateTime(request.BirthDate.Year, request.BirthDate.Month, request.BirthDate.Day, 0, 0, 0, DateTimeKind.Utc),
            Email = request.Email,
            PhoneNumber = request.PhoneNumber
        };

        var createUserResult = await _userManager.CreateAsync(user, request.Password);
        if (!createUserResult.Succeeded)
        {
            return (false, new BadRequestObjectResult(createUserResult.Errors));
        }

        await _userManager.AddToRoleAsync(user, Identity.Roles.Driver.ToString());

        user.Employer = new CompanyDriver { CompanyId = (int)request.CompanyId };

        user.Gender = request.Gender;
        user.Document = request.Document;
        user.DocumentDetails = request.DocumentDetails;

        await _userManager.UpdateAsync(user);

        var emailMessage = "You have been registered as a driver.\n\n" +
                           $"Your login: ${request.Email}\nYour password: ${request.Password}";
        
        try { await _emailSender.SendMail(request.Email, "Driver Registration", emailMessage); }
        catch (Exception) {  /* ignored */ }
        
        return (true, null!);
    }

    public async Task<(bool isSucceeded, IActionResult actionResult, IEnumerable<ExpandoObject> drivers, PagingMetadata<ExpandoObject> pagingMetadata)>
        GetDrivers(CompanyDriverParameters parameters)
    {
        var dbDriverUsers = _userManager.Users.Include(u => u.Employer)
            .Where(u => u.Employer != null).AsQueryable();
        
        if (_sessionUserService.GetAuthUserRole() != Identity.Roles.Administrator.ToString())
        {
            var result = await _sessionUserService.IsAuthUserCompanyOwner();
            if (!result.isCompanyOwner)
            {
                return (false, new UnauthorizedResult(), null!, null!);
            }
            
            dbDriverUsers = dbDriverUsers.Where(u => u.Employer!.CompanyId == result.companyId);
        }

        if (!dbDriverUsers.Any())
        {
            return (false, new NotFoundResult(), null!, null!);
        }

        FilterByCompanyId(ref dbDriverUsers, parameters.CompanyId);
        SearchByAllUserFields(ref dbDriverUsers, parameters.Search);

        var userDtos = _mapper.ProjectTo<DriverDto>(dbDriverUsers);
        var shapedData = _userDataShaper.ShapeData(userDtos, parameters.Fields).AsQueryable();
        
        try
        {
            shapedData = _userSortHelper.ApplySort(shapedData, parameters.Sort);
        }
        catch (Exception)
        {
            return (false, new BadRequestObjectResult("Invalid sorting string"), null!, null!);
        }
        
        var pagingMetadata = _pager.ApplyPaging(ref shapedData, parameters.PageNumber, parameters.PageSize);

        return (true, null!, shapedData, pagingMetadata);

        void FilterByCompanyId(ref IQueryable<User> users, int? companyId)
        {
            if (!users.Any() || companyId == null)
            {
                return;
            }

            users = users.Where(u => u.Employer!.CompanyId == companyId);
        }
        
        void SearchByAllUserFields(ref IQueryable<User> users, string? search)
        {
            if (!users.Any() || search == null)
            {
                return;
            }

            var t = users.ToArray().Where(u =>
                (u.LastName.ToLower() + u.FirstName.ToLower() + u.Patronymic.ToLower()).Contains(search.ToLower()) ||
                u.Email.ToLower().Contains(search.ToLower()) ||
                u.PhoneNumber.ToLower().Contains(search.ToLower()));

            users = t
                .AsQueryable();
        }
    }

    public async Task<(bool isSucceeded, IActionResult actionResult, ExpandoObject driver)>
        GetDriver(string driverId, string? fields)
    {
        if (_sessionUserService.GetAuthUserRole() != Identity.Roles.Administrator.ToString())
        {
            if (!await _sessionUserService.IsAuthUserCompanyDriver(driverId))
            {
                return (false, new UnauthorizedResult(), null!);
            }
        }
        
        var dbDriverUser = await _userManager.Users.Include(u => u.Employer).
            FirstOrDefaultAsync(u => u.Id == driverId && u.Employer != null);
        
        if (dbDriverUser == null)
        {
            return (false, new NotFoundResult(), null!);
        }

        if (String.IsNullOrWhiteSpace(fields))
        {
            fields = CompanyDriverParameters.DefaultFields;
        }
        
        var userDto = _mapper.Map<DriverDto>(dbDriverUser);
        var shapedData = _userDataShaper.ShapeData(userDto, fields);

        return (true, null!, shapedData);
    }

    public async Task<(bool isSucceed, IActionResult actionResult)> DeleteDriver(string driverId)
    {
        if (_sessionUserService.GetAuthUserRole() != Identity.Roles.Administrator.ToString())
        {
            if (!await _sessionUserService.IsAuthUserCompanyDriver(driverId))
            {
                return (false, new UnauthorizedResult());
            }
        }

        await _userManager.DeleteAsync(await _userManager.FindByIdAsync(driverId));
            
        return (true, null!);
    }
}