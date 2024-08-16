using eBikeSystemDB.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using eBikeSystemDB.BLL.SalesReturns;


namespace eBikeSystemDB
{
    public static class eBikeBackEndExtensions
    {
        public static void eBikeBackEndDependencies(this IServiceCollection services,
            Action<DbContextOptionsBuilder> options)
        {
            services.AddDbContext<eBikeContext>(options);

            services.AddTransient<SalesServices>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<eBikeContext>();
                return new SalesServices(context!);
            });

            services.AddTransient<ReturnServices>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<eBikeContext>();
                return new ReturnServices(context!);
            });
        }


    }
}
