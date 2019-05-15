using System;
using System.IO;
using System.Linq;

namespace dotnet_container
{
    internal class MsBuildProjectFinder
    {
        private readonly string _directory;

        public MsBuildProjectFinder(string directory)
        {
            if (string.IsNullOrEmpty(directory))
            {
                throw new ArgumentException("Value cannot be null or an empty string.", nameof(directory));
            }

            _directory = directory;
        }

        public string FindMsBuildProject()
        {
            var projectPath = _directory;

            if (Directory.Exists(projectPath))
            {
                var projects = Directory.EnumerateFileSystemEntries(projectPath, "*.*proj", SearchOption.TopDirectoryOnly)
                    .ToList();

                if (projects.Count > 1)
                {
                    throw new FileNotFoundException(projectPath);
                }

                if (projects.Count == 0)
                {
                    throw new FileNotFoundException(projectPath);
                }

                return projects[0];
            }

            if (!File.Exists(projectPath))
            {
                throw new FileNotFoundException(projectPath);
            }

            return projectPath;
        }
    }
}