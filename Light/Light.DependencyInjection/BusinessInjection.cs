using Light.Business;
using Light.IBusiness;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Light.DependencyInjection
{
    public class BusinessInjection
    {
        public static void ConfigureBusiness(IServiceCollection services)
        {
            services.AddSingleton<IUserBusiness, UserBusiness>();
        }

    }
}
