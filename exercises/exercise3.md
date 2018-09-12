# Exercise 3

## Understanding the Web Host

In ASP.NET Core, apps are run and managed by a host.

There are currently two host APIs available in .NET Core: Web Host, and Generic Host. The Web Host is designed for hosting web applications. For the purposes of this exercise, we will be exploring the Web Host.

We have control over the hosts configuration, and how it is launched.

## Links

[ASP.NET Core Web Host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/web-host?view=aspnetcore-2.1)

[WebHost Source Code](https://github.com/aspnet/MetaPackages/blob/master/src/Microsoft.AspNetCore/WebHost.cs)

## Exercise

### Prerequisites

- .NET Core SDK 2.1
- Text Editor (Visual Studio Code, or whatever)
- Output from Exercise 2

### Investigating Program.cs

In this exercise, we will build off of the ASP.NET Core project that we started in Exercise 2.

Open the ```Program.cs``` file at the root of the ASP.NET Core project.

Let's start with the entry point for the application:

```cs
public static void Main(string[] args)
{
    CreateWebHostBuilder(args).Build().Run();
}
```

We also defined another method, which is returning an instance of IWebHostBuilder. The command line arguments are passed to the web host builder method.

The ```CreateWebHostBuilder(args)``` returns an instance of a IWebHostBuilder object.

Next, we construct an instance of a WebHost from the IWebHostBuilder, by calling ```Build()```.

Finally, we tell the host to begin running our application, by calling ```Run()``` on the WebHost instance.

If we look inside of the ```CreateWebHostBuilder(string[] args)``` function, we call a static method on the _WebHost_ class, to create a Default Builder. It looks like this:

```cs
public static IWebHostBuilder CreateWebHostBuilder(string[] args) 
{
    return WebHost.CreateDefaultBuilder<Startup>(args);
}
```

### Picking apart WebHost.CreateDefaultBuilder<Startup>()

The CreateDefaultBuilder function is a convenience method that configures the web host with recommended settings. The microsoft documentation lists what kinds of things are configured for you when using this function, which you can see here: [Web Host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/web-host?view=aspnetcore-2.1)

Because ASP.NET Core is an open source project, we can actually get at the source code, and see what this function is doing exactly. Here's a [link](https://github.com/aspnet/MetaPackages/blob/master/src/Microsoft.AspNetCore/WebHost.cs).

It is possible to override these default settings. This allows us to continue to use the _CreateDefaultBuilder_ convenience method, while tailoring the web host settings specifically to our application.

Let's demonstrate this.

Add the following code to our _CreateWebHostBuilder_ method:

```cs
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) 
        {
            return WebHost.CreateDefaultBuilder<Startup>(args)
                .UseUrls("http://*:5003", "http://localhost:5004", "https://localhost:5005");
        }
```

Run the app and you'll see that the console lists all of these ports as being listend too. You can type any of them into a browser window, and should see the website.

Obviously, if you want more control over the web host's configuration, you do not need to call the CreateDefaultBuilder function, and can instead manually configure the Web Host.

### Using Command Line Parameters to Override Configuration

It is possible to tell the web host builder to load its configuration from multiple places. One such mechanism is from command line parameters. In fact, the _CreateWebHostBuilder_ method sets up the WebHostBuilder's configuration to honor command line parameters, as long as some are applied. You can see this in the source code (look for .AddCommandLine).

The way it works is that you must supply a parameter that is named the same as a WebHostBuilder key (for example "urls"). For example, we could override the the urls that we configured above, by typing the following:

```cs
dotnet run --urls "http://*:5006;http://localhost:5007;https://localhost:5008"
```

If you type this, without changing the ```.UseUrls``` code from the earlier demonstration, you'll notice that it does not use the command line parameters. This is because the order of methods called in the configuration matters. The last called configuration method takes precedence. Go ahead and remove the ```.UseUrls``` call from _Program.cs_ and re-run the dotent run command with the --urls parameters.

### Using Environment Variables to Override Configuration

All of the Host Configuration values will also honor Environment Variables. For example, we can specify the urls to listen to via the ASPNETCORE_URLS.

```
SET ASPNETCORE_URLS=http://*:5006;http://localhost:5007;https://localhost:5008
```

Run the application, by typing ```dotnet run```, and you'll see it listening on the specified URLs in the environment variable.

### Using a Configuration JSON File to Configure Web Host

In addition to Environment Variables, and setting values in code, we can load Web Host configuration values from external JSON files. Here's an example:

```cs
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) 
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("hostsettings.json", optional: true)
                .AddCommandLine(args)
                .Build();

            return WebHost.CreateDefaultBuilder<Startup>(args)
                .UseConfiguration(config);
        }
```

In this example, there is a _hostsettings.json_ file at the root of the project folder, that looks like:

```json
{
    "urls": "http://*:5003;http://localhost:5004;https://localhost:5005"
}
```

### Multiple Web Host Configuration Sources

We can put all of the Web Host Configuration Override methods together. Remember, the last configuration value loaded is the one that sticks.

```cs
    public static IWebHostBuilder CreateWebHostBuilder(string[] args) 
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("hostsettings.json", optional: true)
            .AddCommandLine(args)
            .AddEnvironmentVariables()
            .Build();

        return WebHost.CreateDefaultBuilder<Startup>(args)
            .UseUrls("http://*:5000", "http://localhost:5001", "https://localhost:5002")
            .UseConfiguration(config);
    }
```

Note: I expected that the .AddEnvironmentVariables() call would cause the ASPNETCORE_ environment variables to load at that time, overriding settings from the json file, command line, and .UseUrls() method, but that isn't what happened. Instead, all of the other settings will override the environment variables, which might be intended, though I think that sucks.