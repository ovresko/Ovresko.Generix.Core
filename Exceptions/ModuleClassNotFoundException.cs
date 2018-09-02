using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ovresko.Generix.Core.Exceptions
{
    public class ModuleClassNotFoundException : Exception
    {
        public ModuleClassNotFoundException()
        {
        }

        public ModuleClassNotFoundException(string message)
            : base(message)
        {
        }

        public ModuleClassNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
