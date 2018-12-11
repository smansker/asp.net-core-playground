# ASP.NET Core: Model Binding

## Requirements

- Visual Studio 2017 (15.9+)
- .NET Core SDK (2.2.100)
- Postman

## Model Binding

Model Binding is how ASP.NET Core maps data from HTTP requests to action method parameters in controllers.

[Model Binding Docs](https://docs.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-2.2)

The ApiController attribute applies certain conventions to the model binding for Web APIs. For more information, see [ApiController Attribute](https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-2.2).

## Parameter Location Inference Rules

When determining where an action's parameter data should come from, ASP.NET Core will first see if the location is explicity set. If not, 
it will make some assumptions about how the parameter should be populated.

If the [ApiController] attribute is applied to the controller, then the rules are:

1. If the parameter is of type IFormFile, it will look in the body for the file in form data.
1. If it is a complex data type, it will populate the type from the body.
1. Next, it will check for matching route information.
1. Lastly, it will look for data in the query string.

### Example: Implicitly loading parameters from request data

To see the inference rules in action, lets send a request to an endpoint, and specify the id parameter in multiple locations. For the following code, send a multipart/form-data request with _id_ in the form data, and _id_ in the query string (e.g. http://localhost/api/demo/inference/one?id=two). Notice that it pulls in the route data for id.

```cs
[HttpPost("inference/{id}")]
public IActionResult Inference(string id)
{
    return Ok(id);
}
```

## Explicitly Declaring Parameter Locations

It is also possible, and in some cases necessary, to specify where in the request a parameter's data should come from.

This is done via attributes. Valid attributes include:

- [FromBody]
- [FromForm]
- [FromHeader]
- [FromQuery]
- [FromRoute]
- [FromServices]

### Example: Explicitly loading a parameter from a location

Let's modify the code from the ealier Example to specify that we want to pull the id from the form data.

```cs
[HttpPost("inference/{id}")]
public IActionResult Inference([FormData] string id)
{
    return Ok(id);
}
```

Try changing it to load the id from the query string.

### Example: Explicitly loading multiple parameters from different locations

We can specify multiple different locations in the same action.

```cs
[HttpPost("explicit/{route}")]
public IActionResult Explicit([FromRoute] string route, [FromQuery] string query, [FromBody] string body)
{
    var results = new { route, query, body };

    return Ok(results);
}
```

## Loading Parameters From the Body

Only one parameter can be set to pull its data from the body. Note that this rule does not apply when loading from Form Data.

### Example: Loading simple types from body

If you want to read a value type parameter from the body, you must explicitly indicate that the parameter comes from the body. Here's we are passing up application/json data in the body, where it has a string as its value.

```cs
[HttpPost("body")]
public IActionResult Example([FromBody] string stuff)
{
    return Ok(stuff);
}
```

If you don't specify the [FromBody] attribute, the _stuff_ parameter will be null.

### Example: Loading complex types from body

If you are loading a complex type from the body, there's no need to explicitly set the [FromBody] attribute.

```cs
public class Pet {
    public string Name { get; set; }
    public int Age { get; set; }
}
```

```cs
[HttpPost("pets")]
public IActionResult AddPet(Pet pet) {
    return Ok(pet);
}
```

### Example: Multiple complex types not supported

If you attempt to include more than one complex parameter data type, you will get an error. The following code breaks at run time:

```cs
[HttpPost("bad")]
public IActionResult Example(SomeObject one, SomeObject two)
{
    return NoContent();
}
```

## Loading Parameters From DI Services

Action parameters can get their data from registered Services. This allows us to populate parameters using a form of Dependency Injection that isn't Constructor Injection. To demonstrate this, let's first register a new service in our demo application.

```cs
public class Startup
{
    private readonly IHostingEnvironment _env;

    public Startup(IConfiguration configuration, IHostingEnvironment env)
    {
        Configuration = configuration;
        _env = env;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        services.AddSingleton(_env.ContentRootFileProvider);
    }

    ...
}
```

Now, in the controller, let's setup a parameter to use this method. To see actual output, put an _Images_ folder at the root of the application, and place an image or two in it.

```cs
[HttpGet("services")]
public IActionResult DemoServices([FromServices] IFileProvider fileProvider)
{
    return Ok(fileProvider.GetDirectoryContents("Images"));
}
```

## Loading Form Data

Using the [FromForm] attribute, you can specify that parameters should be loaded from form data in the request body. You can also load one or more files using the IFormFile type. Note that when using IFormFile, or IFormFileCollection, the name of the parameter must match the name of the field that contains the file in the form collection.

### Example: Loading Form Data

```cs
[HttpPost("form")]
public IActionResult FormDemo([FromForm] string first, [FromForm] string last, IFormFile file)
{
    var results = new
    {
        first,
        last,
        file = file.FileName
    };

    return Ok(results);
}
```

## Model Validation

When loading in complex types from a request body, you can annotate those types with validation attributes. When using the [ApiController] attribute, model validation errors automatically return a Bad Request (400) response.

Given the following **Pet** class:

```cs
public class Pet
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    [Range(0, 30)]
    public int Age { get; set; }
}
```

Create an endpoint that expects this class as a parameter:

```cs
        [HttpPost("owners/{id}/pets")]
        public IActionResult AddPet(string id, Pet pet)
        {
            return Created(Url.Action("AddPet", new { id }), pet);
        }
```

Play around with sending different values for the pet's name and age.

By using the [ApiController] attribute, it elminates the need for us to check for model state errors. Without the [ApiController] attribute, we would have to change the above endpoint like so:

```cs
        [HttpPost("owners/{id}/pets")]
        public IActionResult AddPet(string id, Pet pet)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Created(Url.Action("AddPet", new { id }), pet);
        }
```

## Fun Stuff

### Pulling Query String Parameters into a Dictionary

```cs
[HttpGet("parameters/{id}")]
public IActionResult DictionaryTest(string id, [FromQuery] Dictionary<string, string> parameters)
{
    return Ok(parameters);
}
```

### Endpoints for Uploading and Downloading an Image

This code assumes that a _wwwroot_ folder exists at the root of the application, and that it contains an _Images_ folder.

```cs
[HttpPost("images")]
public async Task<IActionResult> UploadImage([FromServices] IHostingEnvironment _env, IFormFile file)
{
    if (file == null)
    {
        return BadRequest("No File Specified to Upload");
    }

    if (file.ContentType != MediaTypeNames.Image.Jpeg)
    {
        return BadRequest("Unsupported Content Type. Only Jpegs may be uploaded here.");
    }

    var fileId = Guid.NewGuid();

    var fileName = $"{fileId}.jpg";

    var filePath = Path.Combine(_env.WebRootPath, "Images", fileName);

    using (var fileStream = new FileStream(filePath, FileMode.Create))
    {
        await file.CopyToAsync(fileStream);
    }

    var resourceUrl = Url.RouteUrl("GetImage", new { fileId });

    return Created(resourceUrl, fileId);
}

[HttpGet("images/{fileId}", Name = "GetImage")]
public IActionResult GetImage([FromServices] IHostingEnvironment _env, string fileId)
{
    var fileInfo = _env.WebRootFileProvider.GetFileInfo($"Images/{fileName}.jpg");

    if (fileInfo.Exists)
    {
        var fileStream = fileInfo.CreateReadStream();

        return File(fileStream, "image/jpeg");
    }
    else
    {
        return NotFound();
    }
}
```

For more fun, let's modify the GetImage method so that a query parameter can be passed in to indicate that the image should be saved when downloaded, rather than displayed in the browser.

```cs
[HttpGet("images/{fileId}", Name = "GetImage")]
public IActionResult GetFile([FromServices] IHostingEnvironment _env, string fileId, [FromQuery] string saveAs = null)
{
    var fileInfo = _env.WebRootFileProvider.GetFileInfo($"Images/{fileId}.jpg");

    if (fileInfo.Exists)
    {
        var fileStream = fileInfo.CreateReadStream();

        if (string.IsNullOrEmpty(saveAs))
        {
            return File(fileStream, "image/jpeg");
        }
        else
        {
            return File(fileStream, "image/jpeg", saveAs);
        }
    }
    else
    {
        return NotFound();
    }
}
```