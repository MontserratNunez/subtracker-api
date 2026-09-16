using SubTracker.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubTracker.Infrastructure.Identity.Helpers
{
    public class PasswordResetTokenProvider : DataProtectorTokenProvider<AppUser>
    {
        public PasswordResetTokenProvider(
            IDataProtectionProvider dataProtectionProvider,
            IOptions<PasswordResetTokenProviderOptions> options,
            ILogger<DataProtectorTokenProvider<AppUser>> logger)
            : base(dataProtectionProvider, options, logger)
        {
        }
    }

    public class PasswordResetTokenProviderOptions : DataProtectionTokenProviderOptions { }
}
