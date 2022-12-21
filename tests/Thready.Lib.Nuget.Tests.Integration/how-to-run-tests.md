# How to run Nuget targeting tests
> Thanks to Andrew Lock for his great blog! This test-setup is straight from it, you can find it [here](https://andrewlock.net/creating-a-source-generator-part-3-integration-testing-and-packaging/).

## Why is this project not part of the solution?
As Andrew describes in this blog above this project is handled a little different to not "poison" your Nuget cache.
To achieve this it is not part of the solution and will be executed by using the `dotnet-cli` directly targeting the `csproj`.

## How to run the tests
First you have to restore the `Nuget` packages:
```shell
dotnet restore ./tests/Thready.Lib.Nuget.Tests.Integration --packages ./packages --configfile "nuget.integration-tests.config"
```

Next, build the project:
```shell
dotnet build ./tests/Thready.Lib.Nuget.Tests.Integration -c Release --packages ./packages --no-restore 
```

After that you can run the tests:
```shell
dotnet test ./tests/Thready.Lib.Nuget.Tests.Integration -c Release --no-build --no-restore  
```