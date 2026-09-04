using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Activadis.Domain.Interfaces.Helpers
{
    public interface IPassword
    {
        bool Validate(string hash, string password);
        string Hash(string password);
    }
}
