using System;
using System.Collections.Generic;
using System.Text;

namespace VeeamInterviewProject.Abstractions
{
    public interface IFolderSynchronisation
    {
        Task Sync(CancellationToken token);

    }
}
