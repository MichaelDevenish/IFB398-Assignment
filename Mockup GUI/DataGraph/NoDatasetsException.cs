using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGraph
{
    class NoDatasetsException : Exception
    {
        public NoDatasetsException()
        {
        }

        public NoDatasetsException(string message)
        : base(message)
        {
        }
    }
}
