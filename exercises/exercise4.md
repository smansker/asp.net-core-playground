# Exercise 4 [WIP]

## Middleware

ASP.NET Core provides a software pipeline for handling HTTP requests and responses. Software inserted into this pipeline will receive Requests from other Middleware that occur prior to them in the pipeline, and will receive responses from the Middleware that occurs after them in the pipeline.

Middleware is able to short-circuit the pipeline. This occurs when a piece of middleware decides to not call the next piece of middleware in the response pipeline.

## Links

[ASP.NET Core Middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-2.1)

## Exercise

### Prerequisites

- .NET Core SDK 2.1
- Text Editor (Visual Studio Code, or whatever)

### Pipeline Example

In this example, we will use simple request delegates to create middleware components to process our requests and responses. The ```app.Use``` method allows us to chain middleware together. The order that the ```app.Use``` function is called determines the order that the middleware occurs in the request pipeline.

The ```app.Run``` method automatically terminates the pipeline, sending the response back through the previous middleware.

Here, we have two pieces of middleware: Middleware A, and Middleware B. Each of them will log a message to the console to demonstrate the timing. If the URL contains a skip=true query parameter, then Middleware B will short-circuit, returning a different response than the ```app.Run``` middleware component we defined at the end.

```cs
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.Use(async (context, next) => {
        
        Console.WriteLine("Middleware A: {0} Request Incoming", context.Request.Method);
        await next.Invoke();
        Console.WriteLine("Middleware A: Response Status Code: {0}", context.Response.StatusCode);
    });

    app.Use(async (context, next) => {
        var skip = false;
        if (context.Request.Query.TryGetValue("skip", out StringValues values)) {
            skip = values[0] == "true";
        }

        if (skip) {
            await context.Response.WriteAsync("Short Circuited Pipeline");
        } else {
            Console.WriteLine("Middleware B: {0} Request Incoming", context.Request.Method);
            await next.Invoke();
            Console.WriteLine("Middleware B: Response Status Code: {0}", context.Response.StatusCode);
        }
    });

    app.Run(async (context) =>
    {
        await context.Response.WriteAsync("Hello World!");
    });
}
```

### Map extensions for Branching Middleware

### Built in Middleware

### Creating Middleware in a Class