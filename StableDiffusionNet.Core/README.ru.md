# StableDiffusionNet.Core

**[English](README.md) | Русский**

Core библиотека для работы с Stable Diffusion WebUI API.

## Особенности

- **Минимальные зависимости**: только Newtonsoft.Json для сериализации
- **Без DI**: простой в использовании без инфраструктуры DI
- **Встроенный Retry**: собственная реализация retry логики
- **Асинхронные операции**: поддержка async/await и CancellationToken
- **XML документация**: для всех публичных API
- **Builder Pattern**: удобное создание клиента
- **Собственное логирование**: минималистичная абстракция без Microsoft.Extensions
- **Multi-targeting**: поддержка .NET Standard 2.0, 2.1, .NET 6.0, .NET 8.0

## Установка

```bash
dotnet add package StableDiffusionNet.Core
```

Или через NuGet Package Manager:

```
Install-Package StableDiffusionNet.Core
```

## Быстрый старт

### Простейший вариант

```csharp
using StableDiffusionNet;
using StableDiffusionNet.Models.Requests;

// Создание клиента с настройками по умолчанию
var client = StableDiffusionClientBuilder.CreateDefault("http://localhost:7860");

// Генерация изображения
var request = new TextToImageRequest
{
    Prompt = "a beautiful sunset over mountains, highly detailed, 4k",
    NegativePrompt = "blurry, low quality",
    Width = 512,
    Height = 512,
    Steps = 30
};

var response = await client.TextToImage.GenerateAsync(request);
```

### Использование Builder для настройки

```csharp
using StableDiffusionNet;
using StableDiffusionNet.Logging;

// Создание клиента с дополнительными настройками
var client = new StableDiffusionClientBuilder()
    .WithBaseUrl("http://localhost:7860")
    .WithTimeout(600)
    .WithRetry(retryCount: 3, retryDelayMilliseconds: 1000)
    .WithApiKey("your-api-key-if-needed")
    .WithDetailedLogging()
    .Build();
```

### Использование с собственным логированием

```csharp
// Реализуйте IStableDiffusionLogger
public class ConsoleLogger : IStableDiffusionLogger
{
    public void Log(LogLevel logLevel, string message)
    {
        Console.WriteLine($"[{logLevel}] {message}");
    }

    public void Log(LogLevel logLevel, Exception exception, string message)
    {
        Console.WriteLine($"[{logLevel}] {message}: {exception}");
    }

    public bool IsEnabled(LogLevel logLevel) => true;
}

// Реализуйте IStableDiffusionLoggerFactory
public class ConsoleLoggerFactory : IStableDiffusionLoggerFactory
{
    public IStableDiffusionLogger CreateLogger<T>() => new ConsoleLogger();
    public IStableDiffusionLogger CreateLogger(string categoryName) => new ConsoleLogger();
}

// Используйте с Builder
var client = new StableDiffusionClientBuilder()
    .WithBaseUrl("http://localhost:7860")
    .WithLoggerFactory(new ConsoleLoggerFactory())
    .Build();
```

## Основные возможности

### Text-to-Image генерация

```csharp
var request = new TextToImageRequest
{
    Prompt = "a cute cat, highly detailed",
    Width = 512,
    Height = 512,
    Steps = 20,
    CfgScale = 7.5
};

var response = await client.TextToImage.GenerateAsync(request);
ImageHelper.Base64ToImage(response.Images[0], "output.png");
```

### Image-to-Image генерация

```csharp
var initImage = ImageHelper.ImageToBase64("input.png");

var request = new ImageToImageRequest
{
    InitImages = new List<string> { initImage },
    Prompt = "transform into van gogh style",
    DenoisingStrength = 0.7
};

var response = await client.ImageToImage.GenerateAsync(request);
```

### Работа с моделями

```csharp
// Получить список моделей
var models = await client.Models.GetModelsAsync();

// Получить текущую модель
var currentModel = await client.Models.GetCurrentModelAsync();

// Установить модель
await client.Models.SetModelAsync("sd_xl_base_1.0.safetensors");
```

### Мониторинг прогресса

```csharp
var progress = await client.Progress.GetProgressAsync();
Console.WriteLine($"Progress: {progress.Progress:P}");
```

## Retry логика

Библиотека включает надежную собственную реализацию retry с экспоненциальной задержкой:

- Автоматические повторы для транзитных ошибок (500, 502, 503, 504)
- Специальная обработка rate limiting (HTTP 429)
- Экспоненциальный backoff с jitter для избежания thundering herd
- Настраиваемое количество попыток и задержки

## Обработка ошибок

```csharp
using StableDiffusionNet.Exceptions;

try
{
    var response = await client.TextToImage.GenerateAsync(request);
}
catch (ApiException ex)
{
    Console.WriteLine($"API Error: {ex.Message}");
    Console.WriteLine($"Status Code: {ex.StatusCode}");
    Console.WriteLine($"Response: {ex.ResponseBody}");
}
catch (ConfigurationException ex)
{
    Console.WriteLine($"Configuration Error: {ex.Message}");
}
catch (StableDiffusionException ex)
{
    Console.WriteLine($"General Error: {ex.Message}");
}
```

## Нужен Dependency Injection?

Если вам нужна интеграция с Microsoft.Extensions.DependencyInjection, используйте пакет `StableDiffusionNet.DependencyInjection`:

```bash
dotnet add package StableDiffusionNet.DependencyInjection
```

```csharp
services.AddStableDiffusion(options =>
{
    options.BaseUrl = "http://localhost:7860";
});
```

## Лицензия

MIT License

## Ссылки

- [GitHub Repository](https://github.com/mrleo1nid/StableDiffusionNet)
- [StableDiffusionNet.DependencyInjection](https://www.nuget.org/packages/StableDiffusionNet.DependencyInjection/)
- [Stable Diffusion WebUI](https://github.com/AUTOMATIC1111/stable-diffusion-webui)

