namespace FoosLadder.Api.Context
open Microsoft.AspNet.Identity.EntityFramework

type AuthContext() = 
    inherit IdentityDbContext<IdentityUser>("AuthContext")





