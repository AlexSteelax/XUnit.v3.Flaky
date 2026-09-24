# Steelax.XUnit.v3.Flaky

[![Build and test](https://github.com/AlexSteelax/XUnit.v3.Flaky/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/AlexSteelax/XUnit.v3.Flaky/actions/workflows/ci.yml)

![Latest NuGet Version](https://img.shields.io/nuget/v/Steelax.XUnit.v3.Flaky)
![License](https://img.shields.io/github/license/AlexSteelax/XUnit.v3.Flaky)

## Overview

`[FlakyFact]` and `[FlakyTheory]` are test attributes for xUnit v3 that run a test multiple times to detect instability in the implementation under test (for example, a race condition).

Behavior:

- the test is executed up to `n` times, where `n` is the `retriesBeforeFail` constructor argument (default: 3);
- the test is reported as passed only if every attempt passes;
- if any attempt fails, the test is immediately reported as failed.

The attributes do not mask failures: an unstable test continues to fail, making the instability visible.

## Usage

`[FlakyFact(n)]` is used instead of `[Fact]`, `[FlakyTheory(n)]` instead of `[Theory]`.

```cs
using Steelax.XUnit.v3.Flaky.Attributes;

[FlakyFact(42)]
public void SynchronousTest()
{
    // implementation under test
}

[FlakyFact(42)]
public async Task AsynchronousTest()
{
    // implementation under test
}

[FlakyTheory(42)]
[InlineData(true)]
[InlineData(false)]
public async Task TheoryTest(bool value)
{
    // implementation under test
}
```

## Limitations

- if a test mutates shared state, subsequent attempts run with modified state, which can produce misleading results;
- stateful tests should be reviewed before being marked with these attributes.
