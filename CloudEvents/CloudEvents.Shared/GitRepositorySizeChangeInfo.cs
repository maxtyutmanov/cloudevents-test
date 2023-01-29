using System;

namespace CloudEvents.Shared
{
    public class GitRepositorySizeChangeInfo
    {
        public string ProjectId { get; set; }

        public double RepositorySizeInMb { get; set; }

        public GitRepositorySizeStatus SizeStatus { get; set; }
    }
}
