using Light.IRepository;
using Light.Repository;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Light.DependencyInjection
{
    public class RepositoryInjection
    {

        public static void ConfigureRepository(IServiceCollection services)
        {
            services.AddSingleton<IUserRepository, UserRepository>();
        }

    }
}
