using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualStudio.AndroidX.Migration
{
    class NullProgress : IProgress<string>
    {
        public void Report(string value)
        {
        }
    }
}
