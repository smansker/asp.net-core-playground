# Exercise 4

## Middleware

ASP.NET Core provides a software pipeline for handling HTTP requests and responses. Software inserted into this pipeline will receive Requests from other Middleware that occur prior to them in the pipeline, and will receive responses from the Middleware that occurs after them in the pipeline.

Middleware is able to short-circuit the pipeline. This occurs when a piece of middleware decides to not call the next piece of middleware in the response pipeline.

## Links

[ASP.NET Core Middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-2.1)

## Exercise

---

### Prerequisites

- .NET Core SDK 2.1
- Text Editor (Visual Studio Code, or whatever)

---

### Create a new empty web api project

For this exercise, we will be modifying the Startup.cs file of an ASP.NET Core application. To begin with, use the dotnet new command to create an empty ASP.NET Core application.

> Hint: ```dotnet new --help```

---

### A Simple, Single Request Delegate

To start with, we will demonstrated how to use a simple [delegate]https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/delegates/) function to handle every HTTP request into our application. Delete the **ConfigureServices** method from the _Startup.cs_ file in your demo project, and replace the contents of the **Configure** method with the following code. 

```cs
public void Configure(IApplicationBuilder app)
{
    app.Run(async context => {
        await context.Response.WriteAsync("Hello, World!");
    });
}
```

Now run the application and browse to it in a web browser. 

---

### Pipeline Example

In the previous example, we added a single middleware delegate to our pipeline, using the ```app.Run``` command. With only one middleware delegate in our pipeline, it isn't much of a pipeline. Let's add some more middleware delegates next.

In this example, we will use simple request delegates to create middleware components to process our requests and responses. The ```app.Use``` method allows us to chain middleware together. The order that the ```app.Use``` function is called determines the order that the middleware occurs in the request pipeline.

The ```app.Run``` method automatically terminates the pipeline, sending the response back through the previous middleware, so there's no point in having additional middleware declared after the run.

Update the **Configure** method of your _Startup.cs_ file, with the following 2 ```app.Use``` commands:

```cs
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.Use(async (context, next) => {
        Console.WriteLine("Middleware A: {0} Request Incoming", context.Request.Method);
        await next.Invoke();
        Console.WriteLine("Middleware A: Response Status Code: {0}", context.Response.StatusCode);
    });

    app.Use(async (context, next) => {
        Console.WriteLine("Middleware B: {0} Request Incoming", context.Request.Method);
        await next.Invoke();
        Console.WriteLine("Middleware B: Response Status Code: {0}", context.Response.StatusCode);
    });

    app.Run(async (context) =>
    {
        await context.Response.WriteAsync("Hello, World!");
    });
}
```

Here, we have two pieces of middleware: Middleware A, and Middleware B. Each of them will log a message to the console to demonstrate the timing. Each middleware invokes the next middleware, via the next object passed in to the delegate.

To demonstrate that the ```app.Run``` method terminates the pipeline, try adding a third middleware delegate after the ```app.Run``` statement. Have it write to the console like the others, naming itself Middleware C.

---

### Short-Circuiting the Pipeline

One very important concept of the Pipeline design, is the ability to short-circut the rest of the request pipeline from any middleware, and initiate a response early. For example, imagine a security scenario, where an Authorization token is required to access your api. You could use a piece of middleware to analyze the request headers, and in the event that the Authorization token is missing, invalid, or expired, it could immediately return an Unauthorized response, without engaging any of the middleware further up the pipeline.

Let's demonstrate short-circuiting using our example code. In our second middleware, middleware B, let's check the incomping url to see if it contains a Query Parameter named "skip". If it does have a "skip" parameter, and its value is "true", we will short-circuit the request pipeline, and return a response that indicates this happened.

Short-circuiting the pipeline is pretty simple. Simply don't invoke the next middleware. It probably also makes sense to create a response at this point, which indicates the error or problem encountered.

Update the second ```app.Use``` statement in your **Configure** method, with the following code:

```cs

app.Use(async (context, next) => {
    var skip = false;
    if (context.Request.Query.TryGetValue("skip", out StringValues values)) {
        skip = values[0] == "true";
    }

    if (skip) {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Short Circuited Pipeline");
    } else {
        Console.WriteLine("Middleware B: {0} Request Incoming", context.Request.Method);
        await next.Invoke();
        Console.WriteLine("Middleware B: Response Status Code: {0}", context.Response.StatusCode);
    }
});
```

---

### Sanity Break: dotnet watch run

Are you getting tired of restarting the webhost everytime you make a file change? If you are, then there is good news for you. Try using the ```dotnet watch run``` command.

Caveats:
- it doesn't support live reload of the browser
- it might not reconnect your debugger to the latest running instace (I'm not 100% sure on this, as I read about this problem in 1 year old documentation)

---

### Map extensions for Branching Middleware

The ```app.Map``` method allows you to branch the pipeline based on the request path coming in. It analyzes the path, checks for a match, and then branches off to a new set of middleware.

Create a new method in the _Startup.cs_ class, named **HandleMap1**.

```cs
private static void HandleMap1(IApplicationBuilder app)
{
    app.Use(async (context, next) =>
    {
        Console.WriteLine("Map Middleware 1: {0} Request Incoming", context.Request.Method);
        await next.Invoke();
        Console.WriteLine("Map Middleware 1: Response Status Code: {0}", context.Response.StatusCode);
    });

    app.Run(async context =>
    {
        await context.Response.WriteAsync("Map 1 Response");
    });
}
```

Now insert an ```app.Map``` middleware to **Configure** method, which calls our map handler method whenever the path contains "/map1". Here's the code:

```cs
app.Map("/map1", HandleMap1);
```

Now run the app. Investigate how the pipeline behaves in the following scenarios:
- _http://localhost:5001_
- _http://localhost:5001/map1_
- _http://localhost:5001?skip=true_
- _http://localhost:5001/map1?skip=true_

There is also a ```app.MapWhen```, which conditionally branches the pipeline. For example:

```cs
app.MapWhen(context => context.Request.Query.ContainsKey("something"), HandleBranch);
```

Can you think of how to use the ```app.MapWhen``` to handle our skip short-circuiting from earlier in the demo.

---

### Creating Middleware in a Class

It is also possible to write your middleware in a class, and to provide access to your middleware via extension methods. The microsoft documentation goes into more detail on this topic [here](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-2.1#write-middleware).

---

### Built in Middleware

Luckily, we don't need to write our own middleware for typical scenarios. Check out this [link](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-2.1#built-in-middleware) for a list of middleware that ships with ASP.NET Core.