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

For this exercise, we will be modifying the Startup.cs file of an ASP.NET Core application. To begin with, use the dotnet new command to create an empty ASP.NET Core application.

> Hint: ```dotnet new --help```

---

### Dependency Injection Overview

Let's start by creating a dependency chain. Create the following files in the root of your web api project.

_Primary.cs_
```cs
using System;

namespace demos
{
    public class Primary
    {
        public void Invoke() {
            Console.WriteLine("Hello from Primary");

            var secondary = new Secondary();
            secondary.Invoke();
        }
    }
}
```

_Secondary.cs_
```cs
using System;

namespace demos
{
    public class Secondary
    {
        public void Invoke() {
            Console.WriteLine("Hello from Secondary");

            var tertiary = new Tertiary();
            tertiary.Invoke();
        }
    }
}
```

_Tertiary.cs_
```cs
using System;

namespace demos 
{
    public class Tertiary {
        public void Invoke() {
            Console.WriteLine("Hello from Tertiary");
        }
    }
}
```

Now update your _Configure_ method, in the _Startup.cs_ file, to call our Primary class:

```cs
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                var primary = new Primary();
                primary.Invoke();

                await context.Response.WriteAsync("Hello World!");
            });
        }
```

Run the app, and check the console for messages.