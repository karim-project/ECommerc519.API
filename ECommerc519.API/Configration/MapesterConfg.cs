using Mapster;

namespace ECommerc519.API.Configration
{
    public static class MapesterConfg
    {

        public static void RegisterMapesterConfg(this IServiceCollection services) 
        {

            //TypeAdapterConfig<ApplicationUser, ApplicationUserVM>
            //            .NewConfig()
            //            .Map(d => d.FullName, s => $"{s.FirstName} {s.LastName}")
            //            .TwoWays();
        }
    }
}
