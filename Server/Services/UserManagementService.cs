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

namespace Server.Services;

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IMapper _mapper;
    private readonly ISortHelper<ExpandoObject> _userSortHelper;
    private readonly IDataShaper<UserDto> _userDataShaper;
    private readonly IPager<ExpandoObject> _pager;

    public UserManagementService(IMapper mapper, UserManager<User> userManager, RoleManager<IdentityRole> roleManager,
        ISortHelper<ExpandoObject> userSortHelper, IDataShaper<UserDto> userDataShaper, IPager<ExpandoObject> pager,
        ApplicationDbContext dbContext)
    {
        _mapper = mapper;
        _userManager = userManager;
        _roleManager = roleManager;
        _userSortHelper = userSortHelper;
        _userDataShaper = userDataShaper;
        _pager = pager;

        _userManager.UserValidators.Clear();
    }

    public async Task<(bool isSucceeded, IActionResult? actionResult, UserDto user)> AddUser(CreateUserDto createUserDto)
    {
        var user = _mapper.Map<User>(createUserDto);
        user.BirthDate = user.BirthDate == null ? null : new DateTime(user.BirthDate.Value.Ticks, DateTimeKind.Utc);

        var userDto = _mapper.Map<UserDto>(user);
        userDto.Roles = new List<string>();

        if (await _userManager.FindByEmailAsync(user.Email) != null)
        {
            return (false, new BadRequestObjectResult("Email already registered"), null);
        }
        
        if (user.PhoneNumber != null && await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == user.PhoneNumber) != null)
        {
            return (false, new BadRequestObjectResult("Phone number already registered"), null);
        }

        if (createUserDto.Roles != null!)
        {
            foreach (var role in createUserDto.Roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    return (false, new BadRequestObjectResult($"Roles \"{role}\" doesn't exist"), null);
                }

                userDto.Roles.Add(role);
            }
        }
        
        await _userManager.CreateAsync(user, createUserDto.Password);
        await _userManager.AddToRolesAsync(user, createUserDto.Roles);

        userDto.Id = user.Id;
    
        return (true, null, userDto);
    }

    public async Task<(bool isSucceeded, IActionResult? actionResult, IEnumerable<ExpandoObject> users,
        PagingMetadata<ExpandoObject> pagingMetadata)> GetUsers(UserParameters parameters)
    {
        var dbUsers = _userManager.Users.Include(u => u.Company)
            .Include(u => u.Reviews).Include(u => u.TicketGroups)
            .ThenInclude(tg => tg.Tickets).AsQueryable();

        SearchByAllUserFields(ref dbUsers, parameters.Search);

        var userDtos = _mapper.ProjectTo<UserDto>(dbUsers);
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

        if ((bool)parameters.Fields?.Contains("roles"))
        {
            foreach (var user in shapedData)
            {
                dynamic dynamicUser = user as IDictionary<string, object>;

                var roles = await _userManager.GetRolesAsync(new User { Id = dynamicUser.Id });
                dynamicUser.Roles = roles;
            }
        }
        
        return (true, null, shapedData, pagingMetadata);
        
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

    public async Task<(bool isSucceeded, IActionResult? actionResult, ExpandoObject user)>
        GetUser(string id, string? fields)
    {
        var dbUser = await _userManager.Users.Include(u => u.Employer).
            FirstOrDefaultAsync(u => u.Id == id);
        
        if (dbUser == null)
        {
            return (false, new NotFoundResult(), null!);
        }

        if (String.IsNullOrWhiteSpace(fields))
        {
            fields = UserParameters.DefaultFields;
        }
        
        var userDto = _mapper.Map<DriverDto>(dbUser);
        var shapedData = _userDataShaper.ShapeData(userDto, fields);

        return (true, null, shapedData);
    }

    public async Task<(bool isSucceeded, IActionResult? actionResult, UserDto user)>
        UpdateUser(string id, UpdateUserDto updateUserDto)
    {
        if (id != updateUserDto.Id)
        {
            return (false, new BadRequestObjectResult("Object and query ids don't match"), null);
        }

        if (!await _userManager.Users.AnyAsync(u => u.Id == id))
        {
            
            return (false, new NotFoundResult(), null);
        }

        var dbUser = await _userManager.FindByIdAsync(id);
        var user = _mapper.Map<User>(updateUserDto);

        dbUser.FirstName = user.FirstName;
        dbUser.LastName = user.LastName;
        dbUser.Patronymic = user.Patronymic;
        dbUser.BirthDate = new DateTime(user.BirthDate.Value.Ticks, DateTimeKind.Utc);
        dbUser.Gender = user.Gender;
        dbUser.Document = user.Document;
        dbUser.DocumentDetails = user.DocumentDetails;
        dbUser.Email = user.Email;
        dbUser.EmailConfirmed = user.EmailConfirmed;
        dbUser.PhoneNumber = user.PhoneNumber;
        dbUser.PhoneNumberConfirmed = user.PhoneNumberConfirmed;

        var userDto = _mapper.Map<UserDto>(user);
        userDto.Roles = new List<string>();

        updateUserDto.Roles = updateUserDto.Roles == null ? new List<string>() : updateUserDto.Roles;
        var roles = await _userManager.GetRolesAsync(user);
        var rolesToDelete = roles.Except(updateUserDto.Roles).ToArray();
        var rolesToAdd = updateUserDto.Roles.Except(roles).ToArray();
        userDto.Roles = roles.Except(rolesToDelete).Union(rolesToAdd).ToList();

        foreach (var role in rolesToDelete)
        {
            if (await _roleManager.RoleExistsAsync(role))
            {
                await _userManager.RemoveFromRoleAsync(dbUser, role);
            }
        }

        foreach (var role in rolesToAdd)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                return (false, new BadRequestObjectResult($"Roles \"{role}\" doesn't exist"), null);
            }
        }

        await _userManager.AddToRolesAsync(dbUser, rolesToAdd);

        if (updateUserDto.Password != null)
        {
            await _userManager.RemovePasswordAsync(dbUser);
            await _userManager.AddPasswordAsync(dbUser, updateUserDto.Password);
        }

        await _userManager.UpdateAsync(dbUser);
    
        return (true, null, userDto);
    }

    public async Task<(bool isSucceed, IActionResult? actionResult)> DeleteUser(string id)
    {
        var dbUser = await _userManager.FindByIdAsync(id);
    
        if (dbUser == null)
        {
            return (false, new NotFoundResult());
        }

        await _userManager.DeleteAsync(dbUser);
    
        return (true, null);
    }
}