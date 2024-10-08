using Base.Domain.Contracts;
using Microsoft.AspNetCore.Identity;

namespace App.Domain.Identity;

public class AppRole: IdentityRole<Guid>, IDomainEntityId<Guid>;