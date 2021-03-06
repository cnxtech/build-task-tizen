﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.DotNet.Cli.Utils;
using System;
using System.IO;
using System.Linq;
using Xunit.Abstractions;

namespace Tizen.NET.TestFramework.Commands
{
    public class MSBuildCommand : TestCommand
    { 
        public MSBuildTest MSBuild { get; }
        public string Target { get;  }

        private readonly string _projectRootPath;
        public string ProjectRootPath => _projectRootPath;

        public string ProjectFile { get; }

        public string FullPathProjectFile => Path.Combine(ProjectRootPath, ProjectFile);

        public MSBuildCommand(ITestOutputHelper log, string target, string projectRootPath, string relativePathToProject = null, MSBuildTest msbuild = null)
            : base(log)
        {
            MSBuild = msbuild ?? MSBuildTest.Stage0MSBuild;
            Target = target;

            _projectRootPath = projectRootPath;
            ProjectFile = FindProjectFile(ref _projectRootPath, relativePathToProject);
        }

        internal static string FindProjectFile(ref string projectRootPath, string relativePathToProject)
        {
            if (File.Exists(projectRootPath) && string.IsNullOrEmpty(relativePathToProject))
            {
                return projectRootPath;
            }

            if (!string.IsNullOrEmpty(relativePathToProject))
            {
                string fullPathToProject = Path.Combine(projectRootPath, relativePathToProject);
                if (File.Exists(fullPathToProject))
                {
                    //  If a file exists at the specified relative path, it's the project file
                    return fullPathToProject;
                }
                else
                {
                    //  Otherwise, treat the relative path as the root path for the project and search for the project file under that path
                    projectRootPath = fullPathToProject;
                }
            }

            var buildProjectFiles = Directory.GetFiles(projectRootPath, "*.csproj");

            if (buildProjectFiles.Length != 1)
            {
                var errorMsg = $"Found {buildProjectFiles.Length} .csproj files under {projectRootPath} instead of just 1.";
                throw new ArgumentException(errorMsg);
            }

            return buildProjectFiles[0];
        }

        public virtual DirectoryInfo GetOutputDirectory(string targetFramework, string configuration = "Debug", string runtimeIdentifier = "")
        {
            targetFramework = targetFramework ?? string.Empty;
            configuration = configuration ?? string.Empty;
            runtimeIdentifier = runtimeIdentifier ?? string.Empty;

            string output = Path.Combine(ProjectRootPath, "bin", configuration, targetFramework, runtimeIdentifier);
            return new DirectoryInfo(output);
        }

        public virtual DirectoryInfo GetNonSDKOutputDirectory(string configuration = "Debug")
        {
            configuration = configuration ?? string.Empty;

            string output = Path.Combine(ProjectRootPath, "bin", configuration);
            return new DirectoryInfo(output);
        }

        public DirectoryInfo GetBaseIntermediateDirectory()
        {
            string output = Path.Combine(ProjectRootPath, "obj");
            return new DirectoryInfo(output);
        }

        protected override ICommand CreateCommand(params string[] args)
        {
            var newArgs = args.ToList();
            newArgs.Insert(0, FullPathProjectFile);
            return MSBuild.CreateCommandForTarget(Target, newArgs.ToArray());
        }
    }
}