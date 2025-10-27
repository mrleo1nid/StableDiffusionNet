# StableDiffusionNet

**English | [Русский](README.ru.md)**

[![CI Build](https://github.com/mrleo1nid/StableDiffusionNet/actions/workflows/ci.yml/badge.svg)](https://github.com/mrleo1nid/StableDiffusionNet/actions/workflows/ci.yml)
[![CodeQL](https://github.com/mrleo1nid/StableDiffusionNet/actions/workflows/codeql.yml/badge.svg)](https://github.com/mrleo1nid/StableDiffusionNet/actions/workflows/codeql.yml)
[![SonarQube](https://github.com/mrleo1nid/StableDiffusionNet/actions/workflows/sonarqube.yml/badge.svg)](https://github.com/mrleo1nid/StableDiffusionNet/actions/workflows/sonarqube.yml)

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=mrleo1nid_StableDiffusionNet&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=mrleo1nid_StableDiffusionNet)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=mrleo1nid_StableDiffusionNet&metric=coverage)](https://sonarcloud.io/summary/new_code?id=mrleo1nid_StableDiffusionNet)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=mrleo1nid_StableDiffusionNet&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=mrleo1nid_StableDiffusionNet)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=mrleo1nid_StableDiffusionNet&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=mrleo1nid_StableDiffusionNet)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=mrleo1nid_StableDiffusionNet&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=mrleo1nid_StableDiffusionNet)

[![NuGet Core](https://img.shields.io/nuget/v/StableDiffusionNet.Core.svg?label=Core)](https://www.nuget.org/packages/StableDiffusionNet.Core/)
[![NuGet Core Downloads](https://img.shields.io/nuget/dt/StableDiffusionNet.Core.svg?label=Core%20Downloads)](https://www.nuget.org/packages/StableDiffusionNet.Core/)
[![NuGet DI](https://img.shields.io/nuget/v/StableDiffusionNet.DependencyInjection.svg?label=DI)](https://www.nuget.org/packages/StableDiffusionNet.DependencyInjection/)
[![NuGet DI Downloads](https://img.shields.io/nuget/dt/StableDiffusionNet.DependencyInjection.svg?label=DI%20Downloads)](https://www.nuget.org/packages/StableDiffusionNet.DependencyInjection/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A .NET client library for Stable Diffusion WebUI API with async/await support, retry logic, and dependency injection.

## Package Selection

StableDiffusionNet offers two packages for different use cases:

### StableDiffusionNet.Core
**Lightweight package without Dependency Injection**

[![NuGet](https://img.shields.io/nuget/v/StableDiffusionNet.Core.svg)](https://www.nuget.org/packages/StableDiffusionNet.Core/)

Ideal for:
- Console applications
- Simple scripts and utilities
- Projects without DI infrastructure
- Minimal dependencies

```bash
dotnet add package StableDiffusionNet.Core
```

### StableDiffusionNet.DependencyInjection
**Extensions for Microsoft.Extensions.DependencyInjection**

[![NuGet](https://img.shields.io/nuget/v/StableDiffusionNet.DependencyInjection.svg)](https://www.nuget.org/packages/StableDiffusionNet.DependencyInjection/)

Ideal for:
- ASP.NET Core applications
- Projects with DI container
- Integration with Microsoft.Extensions.*
- IOptions pattern and configuration

```bash
dotnet add package StableDiffusionNet.DependencyInjection
```

## Features

- **Two usage options**: Core (without DI) or DependencyInjection (with DI integration)
- **Builder Pattern**: convenient client creation in Core package
- **Retry logic**: custom implementation with exponential backoff
- **Asynchronous operations**: async/await and CancellationToken support
- **XML documentation**: for all public APIs
- **Flexible logging**: custom abstraction in Core, Microsoft.Extensions.Logging integration in DI
- **Multi-targeting**: .NET Standard 2.0, 2.1, .NET 6.0, .NET 8.0

## Installation

### For projects without DI (Console, Scripts, Utilities)

```bash
dotnet add package StableDiffusionNet.Core
```

### For projects with DI (ASP.NET Core, Modern Apps)

```bash
dotnet add package StableDiffusionNet.DependencyInjection
```

The `StableDiffusionNet.DependencyInjection` package will automatically install `StableDiffusionNet.Core` as a dependency.

## Quick Start

### Option 1: StableDiffusionNet.Core (without DI)

```csharp
using StableDiffusionNet;
using StableDiffusionNet.Models.Requests;

var client = StableDiffusionClientBuilder.CreateDefault("http://localhost:7860");

var request = new TextToImageRequest
{
    Prompt = "a beautiful sunset",
    Width = 512,
    Height = 512
};
var response = await client.TextToImage.GenerateAsync(request);
```

See [StableDiffusionNet.Core README](StableDiffusionNet.Core/README.md) for detailed configuration options.

### Option 2: StableDiffusionNet.DependencyInjection (with DI)

```csharp
using Microsoft.Extensions.DependencyInjection;
using StableDiffusionNet.DependencyInjection.Extensions;

var services = new ServiceCollection();
services.AddStableDiffusion(options =>
{
    options.BaseUrl = "http://localhost:7860";
});

var serviceProvider = services.BuildServiceProvider();
var client = serviceProvider.GetRequiredService<IStableDiffusionClient>();
```

See [StableDiffusionNet.DependencyInjection README](StableDiffusionNet.DependencyInjection/README.md) for ASP.NET Core integration.

## Usage Examples

```csharp
using StableDiffusionNet.Models.Requests;
using StableDiffusionNet.Helpers;

var request = new TextToImageRequest
{
    Prompt = "a beautiful sunset over mountains",
    Width = 512,
    Height = 512,
    Steps = 30
};

var response = await client.TextToImage.GenerateAsync(request);
ImageHelper.Base64ToImage(response.Images[0], "output.png");
```

### Documentation

- **[API Methods Reference](docs/API_REFERENCE.md)** - list of all 22 implemented API methods
- **[Examples](docs/EXAMPLES.md)** - examples of using all services (txt2img, img2img, models, progress, options, etc.)
- **[Advanced Scenarios](docs/ADVANCED.md)** - workflow automation, parallel generation, database integration, RESTful API

## Configuration

### Main Options

```csharp
services.AddStableDiffusion(options =>
{
    options.BaseUrl = "http://localhost:7860";    // Your Stable Diffusion WebUI URL
    options.TimeoutSeconds = 300;                 // Request timeout
    options.RetryCount = 3;                       // Number of retries on error
    options.EnableDetailedLogging = false;        // Detailed logging
});
```

**Important**: Never store API keys in code! Use User Secrets, environment variables, or Azure Key Vault.

## Error Handling

The library provides specialized exceptions for different error types:

```csharp
try
{
    var response = await client.TextToImage.GenerateAsync(request);
}
catch (ApiException ex)
{
    Console.WriteLine($"API Error: {ex.StatusCode} - {ex.Message}");
}
catch (StableDiffusionException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

The library automatically retries requests on transient errors (500, 502, 503, 504, 429) with exponential backoff.

## Implemented API Methods

The library implements 22 API methods across 12 service groups, covering text-to-image generation, image-to-image processing, model management, progress tracking, and more.

See the [API Reference](docs/API_REFERENCE.md) for the complete list of methods and their documentation.

## Requirements

- .NET Standard 2.0+ / .NET 6.0+ / .NET 8.0+
- Stable Diffusion WebUI (AUTOMATIC1111) with enabled API (`--api` flag)

## Contributing

Pull Requests are welcome! Please add tests for new functionality.

## License

MIT License - see [LICENSE](LICENSE)

## Links

- [Stable Diffusion WebUI](https://github.com/AUTOMATIC1111/stable-diffusion-webui)
- [Official API Documentation](https://github.com/AUTOMATIC1111/stable-diffusion-webui/wiki/API)

