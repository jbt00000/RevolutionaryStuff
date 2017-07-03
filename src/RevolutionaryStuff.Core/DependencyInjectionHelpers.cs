using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace RevolutionaryStuff.Core
{
    public static class DependencyInjectionHelpers
    {
        public static void Substitute<TInt, TImp>(this IServiceCollection services, ServiceLifetime? newServiceLifetime = null, ServiceLifetime? existingServiceLifetime = null)
        {
            var serviceDescriptors = services.Where(s => typeof(TInt).IsA(s.ServiceType) && (existingServiceLifetime == null || s.Lifetime == existingServiceLifetime.Value)).ToList();
            Requires.Positive(serviceDescriptors.Count, nameof(serviceDescriptors));
            foreach (var oldServiceDescriptor in serviceDescriptors)
            {
                services.Remove(oldServiceDescriptor);
                var newServiceDescriptor = new ServiceDescriptor(oldServiceDescriptor.ServiceType, typeof(TImp), newServiceLifetime.GetValueOrDefault(oldServiceDescriptor.Lifetime));
                services.Add(newServiceDescriptor);
            }
        }

        public static void Substitute<TImp>(this IServiceCollection services, ServiceLifetime? newServiceLifetime = null, ServiceLifetime? existingServiceLifetime = null)
            => services.Substitute<TImp, TImp>(newServiceLifetime, existingServiceLifetime);

        public static T GetRequiredScopedService<T>(this IServiceProvider provider)
            => provider.CreateScope().ServiceProvider.GetRequiredService<T>();
    }
}
