using Microsoft.EntityFrameworkCore;
using UserModel = listly.Features.User.User;

namespace listly.Features.User
{
  public class UserServices
  {
    private readonly ListlyDbContext _context;

    public UserServices(ListlyDbContext context)
    {
      _context = context;
    }

    // Obtener usuario por UId
    public async Task<UserModel?> GetUserByUIdAsync(string uid)
    {
      return await _context.User.FirstOrDefaultAsync(u => u.UId == uid);
    }

    public async Task<UserModel?> GetUserWithActiveSubscriptionAsync(string uid)
    {
      return await _context.User
        .Include(u => u.UserSubscriptions)
        .ThenInclude(us => us.Subscription)
        .Include(u => u.Setting)
        .FirstOrDefaultAsync(u => u.UId == uid);
    }

    // Obtener UId del usuario por email
    public async Task<string?> GetUserUIdByEmailAsync(string email)
    {
      var user = await _context.User.FirstOrDefaultAsync(u => u.Email == email);
      return user?.UId;
    }
  }
}
