using System;
using System.Collections.Generic;
using System.Text;

namespace VeeamInterviewProject.Abstractions
{
    public interface IAppLogger
    {
        void InfoMessage(string message);
        void ErrorMessage(string message);
    }
}
