using System;
using System.Collections.Generic;
using System.Text;

namespace VeeamInterviewProject.Models
{
    /// <summary>
    /// Configuration class used for storing and pass arguments from command line parse to sync
    /// </summary>
    public class Config
    {
        public string? Source { get; set; }
        public string? Target { get; set; }
        public int Interval { get; set; }
        public string? LogPath { get; set; }
    }
}
