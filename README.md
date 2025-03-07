# Money Burned: mb-dotnet-console

This repository is one of several reference implementations of the "Money Burned" application to illustrate the use of a specific development technology/platform. To learn more about it, check out the [organization profile](https://github.com/Money-Burned).  

This [.NET](https://dotnet.microsoft.com/en-us/learn/dotnet/what-is-dotnet) based console application is intended to be cross-platform designed and shows how to implement the requirements with a very basic user interface at command prompt level.  

## Quick facts

- Application type: **Desktop**
- Available for: **Cross-Platform** (Windows/Linux/Mac)
- Framework/Technology used: **[.NET 9.0](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)** (pronounced "Dotnet")
- Programming Language used: **C#**
- User interaction: **CLI** 

> Degree of difficulty: **moderate**

## Getting started

### Prerequisites

- [Download](https://git-scm.com/downloads) and install a current version of Git
- [Download](https://dotnet.microsoft.com/en-us/download) and Install .NET SDK (at least Version 9.0)
- Create a development folder into which you clone the required repositories
    - Clone the dependency project [repository "mb-dotnet-lib"](https://github.com/Money-Burned/mb-dotnet-lib)
    - Clone this [repository](https://github.com/Money-Burned/mb-dotnet-console)

If you are working on Windows, you can use the following PowerShell commands to get started:  

```powershell
winget install Git.Git -e
winget install Microsoft.DotNet.SDK.9 -e
md ~\Money-Burned
cd ~\Money-Burned
git clone https://github.com/Money-Burned/mb-dotnet-console.git
git clone https://github.com/Money-Burned/mb-dotnet-lib.git
```

### How to run

Once you have met all the requirements, you are good to go:  

```powershell
cd ~\Money-Burned\mb-dotnet-console\src
dotnet run
```

### How to develop

For information about the development process of this appliacation please refer to the [development approach documentation](./doc/dev-approach.md).  

## Usage

To run the application please use `dotnet run [-- options]` or, if using the executable, you can use `MoneyBurned.Cli [options]`.

**Options**  
- `-r <resource string>`; `--resources <resource string>`
    - Starts the tool including a set of pre-defined resources, given as string
    - A resource string is separated by a semicolon or plus sign for  cost. If you need to assign names, use a colon as an additional each resource separator before the cost value. (e. g. for 3 resources: _"24,99;Manager:89;11"_)
    - You are allowed to use common interval types to specify costs  scoped not only to hourly bases (e. g. MD = man days, d = days)
- `-c`; `--cost-types`                 
    - Lists all available cost interval types
- `n`; `--nice` 
    - Enables a more interactive and nice looking user experience
- `-?`; `-h`; `--help` 
    - Show help and usage information

## More things to know

### Have a more convenient user interaction

You may have noticed the `--nice` option... this is a special feature because compromises had to be made for cross-platform reasons.  
It looks way nicer when you use this option field. The application then feels more like a dialog, and when the stopwatch is running, only the latest cost value is displayed.  
However, this display mode is not used by default, so it is compatible with several terminal interpreters.  

### Use pre-defined ressources

To deviate from the standard time scope (hours) for a cost, use a slash, the word `per` or the letter `à` after the value. Then use the codes for the respective interval type.  
        
Standard Cost Types:  

- Minute: m, min
- Day: d, day, Tag
- Week: w, wk, Week, Woche
- Month: mth, month, mo, mon, Monat
- Year: y, yr, Year, j, Jahr

Labor-based Cost Types:  

- Work Day: MD (man day), PD, PT, Arbeitstag [default: 8h/d]
- Work Week: ww, wwk, MW, PW, AW  [default: 8h * 5d /w]
- Work Month: wmth, workmonth, MM, PM, Arbeitsmonat [8h * 5d * 4.35w / m]
- Work Year: wy, MY, PY, AJ, PJ [8h * 5d * 4.35w * 12 / y]

To decide whether it is a resource based on labor-related or total costs, consider whether or not the resource would be available to you for the 24-hour period per day.  

#### Example

The command...  

```powershell
MoneyBurned.Cli -r "Consultant:1100 per MD; Rental:92/d; Dev:63500/wy; Junior-Dev:55200/wy; Co-Working-Space:35"
```
...leads to the following pre-defined resources:  

- Resource Consultant at $137,50/h
- Resource Rental at $3,83/h
- Resource Dev at $30,41/h
- Resource Junior-Dev at $26,44/h
- Resource Co-Working-Space at $35,00/h
