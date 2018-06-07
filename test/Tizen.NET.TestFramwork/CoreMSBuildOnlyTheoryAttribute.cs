// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.DotNet.PlatformAbstractions;
using Xunit;

namespace Tizen.NET.TestFramework
{
    public class CoreMSBuildOnlyTheoryAttribute : TheoryAttribute
    {
        public CoreMSBuildOnlyTheoryAttribute()
        {
            string msbuildPath = Environment.GetEnvironmentVariable("DOTNET_SDK_TEST_MSBUILD_PATH");
            bool usingFullFrameworkMSBuild = !string.IsNullOrEmpty(msbuildPath);
            if (usingFullFrameworkMSBuild)
            {
                this.Skip = "This test requires Core MSBuild to run";
            }
        }
    }
}
