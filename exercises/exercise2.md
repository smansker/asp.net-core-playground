# Exercise 2 [WIP]

## Introduction to ASP.NET

At its core, ASP.NET Core is designed to deliver web content. Whether this content is static html, dynamically generated html, web api endpoints, or static files, it is capable of handling it. 

In this exercise, we will explore the very basics of delivering content via ASP.NET Core.

## Links

[Application Startup](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/startup?view=aspnetcore-2.1)
[Dependency Injection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1)

## Exercise

### Prerequisites

- .NET Core SDK 2.1
- Text Editor (Visual Studio Code, or whatever)

### From Console to ASP.NET Core

Start by creating a dotnet console application, using the dotnet cli, in a new directory.

_HINT: dotnet new --help_

Now we need to adjust the project file, switching it from a console application to an asp.net core application:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>netcoreapp2.1</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

</Project>
```
Here, we've identified that we wish to use the Web SDK, and have linked a new package. This package is referred to as a Meta-Package, and contains a core set of asp.net core functionality. This meta package is different from standard NuGet packages, and is not deployed or distributed with the application. Instead, it is part of the ASP.NET Core runtime environment, which must be installed on target machines. For more information on Meta-Packages, go [here](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/metapackage-app?view=aspnetcore-2.1).

Next, we need to define the startup class, and initialize our host settings, in the main Program file:

```cs
public class Program
{
    public static void Main(string[] args)
    {
        CreateWebHostBuilder(args).Build().Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>();
}
```

You'll notice that you get a lot of errors after changing the Program.cs file. It is unable to resolve the IWebHostBuilder type, or locate WebHost. You will need to add using statemenets to indicate where these types may be found:

```cs
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
```

At this point, we have identified a class for the app to use to configure itself on startup, aptly named _Startup_. Let's create that class.

In the same directory as the Program.cs file, create file named Startup.cs, and place the follow contents in it:

```cs
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseStaticFiles();
    }
}
```

Finally, let's provide a placeholder web page for our new site to provide. Create a new folder named _wwwroot_ and place it alongside the Program.cs and Startup.cs files. Inside of it, add a simple index.html page.

Now let's try and run this thing.

Notice that you must type the full url to the index.html page, and that navigating to the root of the application does nothing. Let's inform our app that it should honor default pages when loading up. Modify the Startup.cs file to include the following:

```cs
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseDefaultFiles();
        app.UseStaticFiles();
    }
}
```

Now run the app. You'll notice that it loads your web page automatically.

### Understanding the WebHost stuff

[https://github.com/aspnet/MetaPackages/blob/rel/2.0.0-preview1/src/Microsoft.AspNetCore/WebHost.cs](https://github.com/aspnet/MetaPackages/blob/rel/2.0.0-preview1/src/Microsoft.AspNetCore/WebHost.cs)