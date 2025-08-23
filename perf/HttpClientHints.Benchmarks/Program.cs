// Copyright © https://myCSharp.de - all rights reserved

using System.Reflection;
using BenchmarkDotNet.Running;

// dotnet run -c Release --framework net80 net90 --runtimes net90
BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args);
