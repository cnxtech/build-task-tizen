﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace Tizen.NET.TestFramework
{
    public static class FileConstants
    {
        public static readonly string DynamicLibPrefix = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "" : "lib";

        public static readonly string DynamicLibSuffix = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".dll" :
                                                         RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? ".dylib" : ".so";

    }
}
