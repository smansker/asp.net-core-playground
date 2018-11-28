# ASP.NET Core: Web API Routing

## Create Project

- Use Visual Studio 2017 to create a Web Api
- Run the application and demo the Values Controller

## Routing

- Delete ValuesController
- Add New Empty Api Controller, Named DemoController
- Point Out Some Defaults
  - ```[Route("api/[controller]")]``` - route prefix for all endpoints
  - ```[ApiController]``` - Provides access to some nice http API specific functionality
  - ```public class DemoController: ControllerBase``` - Base Controller class without view related functionality from MVC, good for apis
- Delete the Route and ApiController attributes

[Documentation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-2.1)

### Mapping Routes

- Clear the Launch Browser value in the project's properties. 
- Create a Get method named Default on the Demo controller that accepts an id parameter that is a string.

_DemoController.cs_
```cs
using Microsoft.AspNetCore.Mvc;

namespace demo1.Controllers
{
    public class DemoController : ControllerBase
    {
        [HttpGet]
        public IActionResult Default(string id)
        {
            return Ok($"Hello {id}");
        }
    }
}
```

_Example 1_
```cs
app.UseMvc(routes =>
{
    routes.MapRoute(
        name: "default",
        template: "{controller}/{action}/{id}");
});
```

_Example 2_
```cs
app.UseMvc(routes =>
{
    routes.MapRoute(
        name: "default",
        template: "{controller}/{action}/{id}",
        defaults: new { controller = "Demo", action = "Default", id = "Anonymous" });
});
```

_Example 3_
```cs
app.UseMvc(routes =>
{
    routes.MapRoute(
        name: "default",
        template: "{controller=Demo}/{action=Default}/{id=Anonymous}");
});
```

_Example 4_
```cs
app.UseMvc(routes =>
{
    routes.MapRoute(
        name: "default",
        template: "{controller=Demo}/{action=Default}/{id?}");
});
```

_Example 5_
```cs
app.UseMvc(routes =>
{
    routes.MapRoute(
        name: "default",
        template: "{controller=Demo}/{action=Default}/{id:int}");
});
```

### Attribute Routing

Quote from documentation:

"Web APIs should use attribute routing to model the app's functionality as a set of resources where operations are represented by HTTP verbs. This means that many operations (for example, GET, POST) on the same logical resource will use the same URL. Attribute routing provides a level of control that's needed to carefully design an API's public endpoint layout."

- Remove MapRoute call from Startup.cs

_Example 1_
```cs
[Route("[Controller]/[Action]/{id}")]
[HttpGet()]
public IActionResult Default(string id)
{
    return Ok($"Hello {id}");
}
```

_Example 2_
```cs
[Route("[Controller]/[Action]/{id=Anonymous}")]
[HttpGet()]
public IActionResult Default(string id)
{
    return Ok($"Hello {id}");
}
```

_Example 3_
```cs
[Route("[Controller]/[Action]/{id?}")]
[HttpGet()]
public IActionResult Default(string id)
{
    return Ok($"Hello {id}");
}
```

_Example 4_
```cs
[Route("[Controller]/[Action]/{id:int}")]
[HttpGet()]
public IActionResult Default(int id)
{
    return Ok($"Hello {id}");
}
```

#### Multiple Routes for One Action

```cs
[HttpGet()]
[Route("")]
[Route("[Controller]")]
[Route("[Controller]/[Action]")]
public IActionResult Default()
{
    return Ok("Hello From Default");
}
```

#### Linking Routes to Http Verbs

Allows us to use the same URL for different verbs. Ideal for Restful, resource based endpoints.

```cs
[HttpGet("[Controller]/Default/{id}")]
public IActionResult Get(string id)
{
    return Ok($"Get {id}");
}

[HttpPut("[Controller]/Default/{id}")]
public IActionResult Put(string id)
{
    return Ok($"Put {id}");
}
```

#### Combining Routes

Makes attribute routing less repetitive by placing a shared prefix at the controller level.

```cs
[Route("[Controller]")]
public class DemoController : ControllerBase
{
    [HttpGet("Default1/{id}")]
    public IActionResult Get1(string id)
    {
        return Ok($"Get 1 - {id}");
    }

    [HttpGet("Default2/{id}")]
    public IActionResult Get2(string id)
    {
        return Ok($"Get 2 - {id}");
    }
}
```

#### URL Generation

_Example 1: Controller/Action based_
```cs
[Route("[Controller]")]
public class DemoController : ControllerBase
{
    [HttpGet("Default1/{id}")]
    public IActionResult Get1(string id)
    {
        var url = Url.Action("Get2", "Demo", new { id = "Frank" });
        return Ok($"Get1 ({id}) - {url}");
    }

    [HttpGet("Default2/{id}")]
    public IActionResult Get2(string id)
    {
        var url = Url.Action("Get1", "Demo", new { id = "George" });
        return Ok($"Get2 ({id}) - {url}");
    }
}
```

_Example 2: Named Routes_
```cs
    [Route("[Controller]")]
    public class DemoController : ControllerBase
    {
        [HttpGet("Default1/{id}", Name = "Route1")]
        public IActionResult Get1(string id)
        {
            var url = Url.RouteUrl("Route2", new { id = "Frank" });
            return Ok($"Get1 ({id}) - {url}");
        }

        [HttpGet("Default2/{id}", Name = "Route2")]
        public IActionResult Get2(string id)
        {
            var url = Url.RouteUrl("Route1", new { id = "George" });
            return Ok($"Get2 ({id}) - {url}");
        }
    }
```