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
using Utils;

namespace Server.Services;

public class DriverManagementService : IDriverManagementService
{
    private readonly IUserManagementService _userManagementService;
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ISortHelper<ExpandoObject> _userSortHelper;
    private readonly IDataShaper<DriverDto> _userDataShaper;
    private readonly IPager<ExpandoObject> _pager;
    private readonly ISessionUserService _sessionUserService;

    public DriverManagementService(IUserManagementService userManagementService, IMapper mapper,
        UserManager<User> userManager, ApplicationDbContext dbContext,
        ISortHelper<ExpandoObject> userSortHelper, IDataShaper<DriverDto> userDataShaper,
        IPager<ExpandoObject> pager, ISessionUserService sessionUserService)
    {
        _userManagementService = userManagementService;
        _userManager = userManager;
        _dbContext = dbContext;
        _userSortHelper = userSortHelper;
        _userDataShaper = userDataShaper;
        _pager = pager;
        _sessionUserService = sessionUserService;
        _mapper = mapper;
    }

    public async Task<(bool isSucceeded, IActionResult? actionResult, DriverDto driver)> AddDriver(CreateDriverDto createDriverDto)
    {
        
        if (_sessionUserService.GetAuthUserRole() == Identity.Roles.Administrator.ToString())
        {
            if (createDriverDto.CompanyId == null)
            {
                return (false, new BadRequestObjectResult("CompanyId must have a value"), null!);
            }
        }
        else
        {
            var isAuthUserCompanyOwnerResult = await _sessionUserService.IsAuthUserCompanyOwner();
            if (!isAuthUserCompanyOwnerResult.isCompanyOwner)
            {
                return (false, new UnauthorizedResult(), null!);
            }
            createDriverDto.CompanyId = isAuthUserCompanyOwnerResult.companyId;
        }
        
        var createUserDto = _mapper.Map<CreateUserDto>(createDriverDto);
        
        createUserDto.Roles = new List<string> { "Driver" };
        createUserDto.Password = createDriverDto.Password;
        
        var result = await _userManagementService.AddUser(createUserDto);
        
        if (!result.isSucceeded)
        {
            return (false, result.actionResult, null);
        }

        var driverDto = _mapper.Map<DriverDto>(result.user);
        _dbContext.CompanyDrivers.Add(new CompanyDriver { CompanyId = (int) createDriverDto.CompanyId, DriverId = driverDto.Id });
        await _dbContext.SaveChangesAsync();

        driverDto.Roles = result.user.Roles;

        return (true, null, driverDto);
    }

    public async Task<(bool isSucceeded, IActionResult? actionResult, IEnumerable<ExpandoObject> drivers, PagingMetadata<ExpandoObject> pagingMetadata)> GetDrivers(CompanyDriverParameters parameters)
    {
        var dbUsers = _userManager.Users.Include(u => u.Employer)
            .Where(u => u.Employer != null).AsQueryable();
        
        if (_sessionUserService.GetAuthUserRole() != Identity.Roles.Administrator.ToString())
        {
            var result = await _sessionUserService.IsAuthUserCompanyOwner();
            if (!result.isCompanyOwner)
            {
                return (false, new UnauthorizedResult(), null!, null!);
            }
            
            dbUsers = dbUsers.Where(u => u.Employer.CompanyId == result.companyId);
        }

        if (!dbUsers.Any())
        {
            return (false, new NotFoundResult(), null!, null!);
        }

        FilterByCompanyId(ref dbUsers, parameters.CompanyId);
        SearchByAllUserFields(ref dbUsers, parameters.Search);

        var userDtos = _mapper.ProjectTo<DriverDto>(dbUsers);
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

        return (true, null, shapedData, pagingMetadata);

        void FilterByCompanyId(ref IQueryable<User> users, int? compnayId)
        {
            if (!users.Any() || compnayId == null)
            {
                return;
            }

            users = users.Where(u => u.Employer.CompanyId == compnayId);
        }
        
        void SearchByAllUserFields(ref IQueryable<User> users, string? search)
        {
            if (!users.Any() || search == null)
            {
                return;
            }

            users = users.Where(u => 
                u.FirstName.ToLower().Contains(search.ToLower()) ||
                u.LastName.ToLower().Contains(search.ToLower()) ||
                u.Patronymic.ToLower().Contains(search.ToLower()) ||
                u.Email.ToLower().Contains(search.ToLower()) ||
                u.PhoneNumber.ToLower().Contains(search.ToLower()));
        }
    }

    public async Task<(bool isSucceeded, IActionResult? actionResult, ExpandoObject driver)> GetDriver(string id, string? fields)
    {
        if (_sessionUserService.GetAuthUserRole() != Identity.Roles.Administrator.ToString())
        {
            if (!await _sessionUserService.IsAuthUserCompanyDriver(id))
            {
                return (false, new UnauthorizedResult(), null!);
            }
        }
        
        var dbUser = await _userManager.Users.Include(u => u.Employer).
            FirstOrDefaultAsync(u => u.Id == id);
        
        if (dbUser == null)
        {
            return (false, new NotFoundResult(), null!);
        }

        if (String.IsNullOrWhiteSpace(fields))
        {
            fields = CompanyDriverParameters.DefaultFields;
        }
        
        var userDto = _mapper.Map<DriverDto>(dbUser);
        var shapedData = _userDataShaper.ShapeData(userDto, fields);

        return (true, null, shapedData);
    }

    public async Task<(bool isSucceeded, IActionResult? actionResult, DriverDto driver)> UpdateDriver(string id, UpdateDriverDto updateDriverDto)
    {
        if (_sessionUserService.GetAuthUserRole() != Identity.Roles.Administrator.ToString())
        {
            if (!(await _sessionUserService.IsAuthUserCompanyOwner()).isCompanyOwner &&
                !await _sessionUserService.IsAuthUserCompanyDriver(id))
            {
                return (false, new UnauthorizedResult(), null!);
            }
        }
        
        var updateUserDto = _mapper.Map<UpdateUserDto>(updateDriverDto);
        var result = await _userManagementService.UpdateUser(id, updateUserDto);

        return (result.isSucceeded, result.actionResult, _mapper.Map<DriverDto>(result.user));
    }

    public async Task<(bool isSucceed, IActionResult? actionResult)> DeleteDriver(string id)
    {
        if (_sessionUserService.GetAuthUserRole() != Identity.Roles.Administrator.ToString())
        {
            if (!await _sessionUserService.IsAuthUserCompanyDriver(id))
            {
                return (false, new UnauthorizedResult());
            }
        }
        
        return await _userManagementService.DeleteUser(id);
    }
}