# Thready <img src="./logo.png" height="60" width="60" >
> The little helper for your annoying background tasks.

## General Information
`Thready` helps you with running a function in a background thread with a specified timeout.

## Installation
Apart from this installation technic described below you can use VS22 NuGet Manager or the Rider NuGet UI.
For an IDE-agnostic installation it is best to use the command below to install the NuGet package in your project.
```shell
dotnet add package Thready
```

This will install the latest, stable version.
To install a specific version you can define it like so (replace {VERSION} with the version you want to install):
```shell
dotnet add package Thready --version {VERSION}
```

Example:
```shell
dotnet add package Thready --version 1.0.0-alpha
```

## Usage
*(If you want to see some code you can find a [sample app](https://github.com/MichaelHochriegl/Thready/tree/master/samples/ConsoleSample) right here in this repo)*
After installing the package you can build your first 'worker' with these steps:
```csharp
var worker = new ThreadyWorker(async(ct) => Console.WriteLine("This will get printed periodically"), async() => Console.WriteLine("This will get executed right before stopping"), TimeSpan.FromSeconds(1));

await worker.StartAsync();
// Your other code here...
await worker.StopAsync(forceStop: false);
```

## Contribute
Pull requests are more than welcome! To contribute, it's best to create an issue first and talk about the change you want to bring into this project.
After that, simply fork the repo, pull your forked repo local to your PC, create an appropriate feature-branch (naming example: `feat/999_my-awesome-feature`, this boils down to: `{TYPEOFCHANGE}/{ISSUENUMBER}_{SHORTDESCRIPTION}`).

You can now do your coding in this branch. After you are done, push your changes to your forked repo and create a Pull Request to this repo

## Acknowledgements
Thanks 'Good Ware' for the logo ([Hobbies and free time icons created by Good Ware - Flaticon](https://www.flaticon.com/free-icons/hobbies-and-free-time))