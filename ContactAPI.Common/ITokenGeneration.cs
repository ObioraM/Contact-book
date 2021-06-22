using Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ContactAPI.Common
{
    public interface ITokenGeneration
    {
        public Task<string> GenerateToken(GenUser user);
    }
}
