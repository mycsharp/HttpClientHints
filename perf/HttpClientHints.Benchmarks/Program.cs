// Copyright © https://myCSharp.de - all rights reserved

using BenchmarkDotNet.Running;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
