# Exercise 1

In this exercise, we will be exploring the dotnet cli.

## Links

[.NET Core CLI Tools](https://docs.microsoft.com/en-us/dotnet/core/tools/?tabs=netcore2x)

## Exercise

### Prerequisites

- .NET Core SDK 2.1
- Text Editor (Visual Studio Code, or whatever)

### Command Prompt

We will be working from within the command prompt for this exercise. Open command prompt, navigate to the /demos directory.

### Basic Commands

\> _dotnet_

Lists basic usage information

\> _dotnet --help_

Context specific help. At root level, lists first level commands. For help on a specific commands, type \> _dotnet [command] --help_.

\> _dotnet --version_

Shows the latest version of the .NET Core SDK that is installed.

\> _dotnet --list-sdks_

It is possible to have multiple SDKs installed side-by-side, on the same machine. The .NET Core SDKs contain everything needed to build and run .NET Core applications.

\> _dotnet --list-runtimes_

Runtimes are required to run .NET Core applications. It is not necessary to install the full SDK on deployment boxes, as long as the appropriate runtime is installed. This command shows you which runtimes are installed. Runtimes can be installed alongside one another.

### Create a Hello World Console App

\> _dotnet new --help_

Using the _new_ command, it is possible to create project and solution files, based off of various templates. To see the list of templates available, view the help for the _new_command.

Create a console app by:
- \> _cd demos_
- \> _mkdir demo1_
- \> _cd demo1_
- \> _dotnet new console -n HelloWorld_
- \> _dotnet run -p HelloWorld_

### Create and Reference Class Library

You can do a lot with the command line tools for .NET Core. Let's add a simple class library project, and reference it from our console application.

- \> _dotnet new classlib --help
- \> _dotnet new classlib -n Messager -f netcoreapp2.1
- \> _dotnet add helloWorld reference Messager
- \> _code ._

Add the following code to Class1.cs in the Messager project:

```cs
using System;

namespace Messager
{
    public static class Class1
    {
        public static string GetMessage() {
            return "Message from Messager!";
        }
    }
}
```

Add the following code to Program.cs in the HelloWorld project:

```cs
using System;
using Messager;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Hello World! {Class1.GetMessage()}");
        }
    }
}
```

- \> dotnet run -p HelloWorld

### You can even create and manage solution files

- \> dotnet new sln
- \> dotnet sln add HelloWorld Messager