using System;
using System.Collections.Generic;
using System.Text;
using VeeamInterviewProject.Models;

namespace VeeamInterviewProject.Abstractions
{
    public interface ICommandLineParser
    {
        Task<Config> ParseAsync(string[] args);
    }
}
