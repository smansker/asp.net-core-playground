# Exercise 5

## Dependency Injection

In short, Dependency Injection (DI) is a method for decoupling dependent objects from one another. In DI, when one object needs an instance of another object, it is handed an instance from an outside source (a container). This means that the object does not need to know anything about the construction or implementation of the object it needs. By injecting a dependency into an object, it allows us to swap out implementations of that dependency, without needing to touch the object that depends on it. This is especially useful for writing unit tests, because we can pass in object mocks in place of real world implementations of dependencies.

ASP.NET Core supports DI out of the box.

## Links

[Dependency Injection](https://en.wikipedia.org/wiki/Dependency_injection)

[ASP.NET Core DI](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1)

## Exercise

---

### Prerequisites

- .NET Core SDK 2.1
- Text Editor (Visual Studio Code, or whatever)

---

### Create a new empty web api project

Use the dotnet new command to create an empty ASP.NET Core application.

> Hint: ```dotnet new --help```

---

### Why Dependency Injection?

We'll start answering this question by setting up a simple scenario where dependency injection could be applied.

For this example, we'll create a use case class that performs some functionality on some data. To fetch this data, our use case will use a data class, so that it doesn't have to worry about how or where the data is fetched from.

_UseCaseThingie.cs_
```cs
using System;

namespace demos
{
    public class UseCaseThingie
    {
        public void Invoke() {
            var dataThingie = new DataThingie();
            var data = dataThingie.GetData();
            Console.WriteLine($"Data: {data}");
        }
    }
}
```
You can see here that our use case is constructing a specific instance of our data class. This creates a hard dependency between the use case and the data class.

Of course, we need to create our data class.

_DataThingie.cs_
```cs
using System;

namespace demos
{
    public class DataThingie
    {
        public string GetData() {
            return "Some Data";
        }
    }
}
```
While our example here is very simple, it is easy to imagine that it could be doing much more complex operations here, such as accessing a database or performing file I/O.

Now, let's call our use case from within our middleware. Update your _Configure_ method, in the _Startup.cs_ file:

```cs
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.Run(async (context) =>
    {
        var useCase = new UseCase();
        useCase.Invoke();

        await context.Response.WriteAsync("Hello World!");
    });
}
```

Run the app, and check the console for the output from our use case.

At this point, our application does what we expected, so why change it?

The first problem is that our use case is responsible for constructing and maintaining an instance of the data class. Imagine that our data class is used by many different use case classes. Now imagine that we wanted to change the data class implementation, changing its constructor parameters, and even the class. Perhaps we want to keep the original implementation of our data class, but we have a slightly different version that we want to use in some cases. Regardless, all of those hard dependencies now have to be updated to reflect the changes. There can exist scenarios where the pain of changing a class that has widespread use throughout your application can actually prohibit the change.

The more common problem is that you need to unit test your classes. Imagine that we wanted to write a unit test for our use case. Because the use case constructs its own instance of the data class, our unit test will use the real implementation of the data class. Unit Tests should be designed to run quickly, consistently, and often. Unit Tests should also test an isolated "unit" of your code (a class, or a function). If our unit tests run real versions of our test subjects dependencies, they can quickly get out of hand. For example, if a dependency accesses a data, the unit test will cause that access. This means that the database will likely need to be seeded with test data prior to each test run, and can result in inconsistent or slow test results.

Finaly, dependencies can make for code that is difficult to read and maintain. DI promotes good code design.

### Implementing Dependency Injection in ASP.NET Core

ASP.NET Core was built from the ground up with Dependency Injection in mind. Out of the box, it provides a container and other mechanisms for implementating DI. Let's demonstrate this by having our use case and data class take part in DI.

To begin with, let's define interfaces for our classes. This allows us to define a contract for using our classes, which is separate from any implementation.

_IUseCaseThingie.cs_
```cs
namespace demos
{
    public interface IUseCaseThingie
    {
        void Invoke();
    }
}
```

_IDataThingie.cs_
```cs
namespace demos
{
    public interface IDataThingie
    {
        string GetData();
    }
}
```

Now update the data class so that it implements the interface.

```cs
namespace demos {
    public class DataThingie: IDataThingie
    {
        public string GetData() {
            return "Some Data";
        }
    }
}
```

For our use case, in addition to having it implement the new interface, we also need to setup Dependency Injection.

```cs
using System;

namespace demos
{
    public class UseCaseThingie: IUseCaseThingie
    {
        private readonly IDataThingie _dataThingie;

        public UseCaseThingie(IDataThingie dataThingie)
        {
            _dataThingie = dataThingie;
        }

        public void Invoke() {
            var data = _dataThingie.GetData();
            Console.WriteLine($"Data: {data}");
        }
    }
}
```

This particular for of DI is known as _Constructor Injection_. Rather than constructing an instance of the data class on its own, it expects that an instance be handed to it via its constructor. Because of our interface, it doesn't actually care about the specific implementation of the data class. Instead, it just cares about the public api of such a class, as defined by the contract specified by the interface.

Next, let's take a look at our middleware, where it is creating an instance of our use case.

```cs
    app.Run(async (context) =>
    {
        var useCase = new UseCase();
        useCase.Invoke();

        await context.Response.WriteAsync("Hello World!");
    });
```

As you can see, there is still a hard dependency here. Also, there is an error. The use case constructor expects an instance of _IDataThingie_ to be passed in. We could always create one here and pass it in, but that would still represent a hard dependency between our middleware and our classes. 

In ASP.NET Core, they provide us with a DI container and resolver, via Services. Containers are responsible for storing instances of classes used in DI, and a resolver is responsible for analyzing class dependencies and providing appropriate instances from the container.

Let's register our interfaces, and their implementations, with the ASP.NET Core Services. This makes the resolver aware of them. We do this in the **ConfigureServices** method of the _Startup.cs_ class.

```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddTransient<IDataThingie, DataThingie>();
    services.AddTransient<IUseCaseThingie, UseCaseThingie>();
}
```

Now that the services are aware of our two classes, we can have our middleware ask the Services to resolve an instance of IUseCaseThingie:

```cs
app.Run(async (context) =>
{
    var useCase = context.RequestServices.GetService<IUseCaseThingie>();
    useCase.Invoke();

    await context.Response.WriteAsync("Hello World!");
});
```

The call to ```GetService<IUseCaseThingie>``` causes the DI system (Services) to look up a registration for _IUseCaseThingie_. It finds our registration, and sees that we want it to construct an implementation of our class _UseCaseThingie_. It analyzes the constructor for _UseCaseThingie_ and sees that it needs a instance of _IDataThingie_. Luckily, there's also a registration for that, so it creates an instance of _IDataThingie_, and passes that to the _UseCaseThingie_ that it creates for us.

### Container Lifetime Management

So DI uses a container to store instances of the objects that it creates for you. In our case, when an instance of IDataThingie is requested by our use case, the DI system creates a new DataThingie class and stores it for us. The thing it stores it in is known as a container. So what happens if a different class also requests an instance of IDataThingie? Since the DI container already has an instance of IDataThingie stored, it could certainly just use that one, rather than constructing another one. But what if we always want to construct a new instance? Control of how instances are used by the DI system, and for how long, is done via Lifetime Management.

Each service can be registered for one of three different lifetimes: **Transient**, **Scoped**, or **Singleton**.

#### Transient Lifetime

Transient services are created each time they are requested. It is best used for lightweight, stateless services.

#### Scoped Lifetime

Scoped lifetime services are created once per request.

#### Singleton Lifetime

Singleton lifetime creates a service instance the first time it is requested, and then continues to use that instance for all subsequent requests, for the duration of the container it is managed by. Singleton services need to be thread safe, as do any transient services that they use.

#### Gotchas

There are a couple of things to keep in mind about the various lifetimes. These are mentioned in the official documentation. For example, you shouldn't use constructor injection for scoped services in your custom middleware. Also, singleton services should not try to resolve scoped services.

#### Demonstration

Let's play around with these lifetimes. Create the following file:

_Operation.cs_
```cs

using System;

namespace demos
{
    public interface IOperation
    {
        Guid OperationId { get; }
    }

    public interface IOperationTransient : IOperation
    {
    }

    public interface IOperationScoped : IOperation
    {
    }

    public interface IOperationSingleton : IOperation
    {
    }

    public class Operation : IOperationTransient,
        IOperationScoped,
        IOperationSingleton
    {
        public Operation()
        {
            OperationId = Guid.NewGuid();
        }

        public Guid OperationId { get; set; }
    }
}
```

Here we've defined a common interface that contains a single id property. We then defined an interface for each service lifetime type, each of which inherits the common interface. Finally, there's a class that implements the three lifetime interfaces, and constructs a unique id when it is constructed.

Now let's register our implementation with each of the three lifetime interfaces, and see how they operate in action. Register the services in the _ConfigureServices_ method of your _Startup.cs_ file.

```cs
    services.AddTransient<IOperationTransient, Operation>();
    services.AddScoped<IOperationScoped, Operation>();
    services.AddSingleton<IOperationSingleton, Operation>();
```

Next we'll add a middleware to our pipeline that requests our three services, and outputs their ids to the console.

```cs
    app.Use(async (context, next) => {
        var transientOp = context.RequestServices.GetService<IOperationTransient>();
        var scopedOp = context.RequestServices.GetService<IOperationScoped>();
        var singleton = context.RequestServices.GetService<IOperationSingleton>();

        Console.WriteLine($"Transient ID: {transientOp.OperationId}");
        Console.WriteLine($"Scoped ID: {scopedOp.OperationId}");
        Console.WriteLine($"Singleton ID: {singleton.OperationId}");

        await next.Invoke();

        transientOp = context.RequestServices.GetService<IOperationTransient>();
        scopedOp = context.RequestServices.GetService<IOperationScoped>();
        singleton = context.RequestServices.GetService<IOperationSingleton>();

        Console.WriteLine($"Transient ID: {transientOp.OperationId}");
        Console.WriteLine($"Scoped ID: {scopedOp.OperationId}");
        Console.WriteLine($"Singleton ID: {singleton.OperationId}");                
    });
```

In this middleware, we begin by requesting the transient, scoped, and singleton services during the request portion of the middleware. Then, on the way back out, we request them again. Each time, we output the ids. Run the application and use a web browser to hit the endpoint.

What you'll notice is that every transient operation id is different. Within a single request, the scoped id will be the same, though it will change for each request. Finally, you should notice that when you make additional requests (e.g. refresh the browser), the singleton id stays the same. Try stopping the application and re-running it. This time, the singleton id should change, since there's a new container.

### Disposal of Services

If a type the DI container created implements IDisposable, the container will call Dispose on it when cleaning up the services. Note that if you add an instance to the container via code, rather than letting the container construct the instance, it will not call Dispose.

### Built in Service Extensions

When using frameworks like MVC and Entity Framework, they will often need to register services that are used by these frameworks. Many times, they will create a service collection extension method, which does all of the setup and registration. For example, MVC includes a ```services.AddMvc()``` extension method, which registers all of the MVC specific services in your application.