using System;
using System.Collections.Generic;
using System.Text;

namespace VeeamInterviewProject.Abstractions
{
    public interface ICompare
    {
        bool Compare(string target, string source);
    }
}
