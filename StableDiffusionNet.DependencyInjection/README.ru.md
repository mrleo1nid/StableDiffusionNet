# StableDiffusionNet.DependencyInjection

**[English](README.md) | Русский**

Расширения для Microsoft.Extensions.DependencyInjection для работы с StableDiffusionNet.Core.

## Особенности

- **Microsoft.Extensions.DependencyInjection**: интеграция с DI контейнером
- **Microsoft.Extensions.Logging**: автоматическая интеграция с логированием
- **IOptions Pattern**: конфигурация через IOptions<T>
- **IHttpClientFactory**: управление HttpClient через фабрику
- **Все возможности Core**: доступ к функционалу StableDiffusionNet.Core

## Установка

```bash
dotnet add package StableDiffusionNet.DependencyInjection
```

Пакет автоматически установит `StableDiffusionNet.Core` как зависимость.

Или через NuGet Package Manager:

```
Install-Package StableDiffusionNet.DependencyInjection
```

## Быстрый старт

### Простая регистрация

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StableDiffusionNet.DependencyInjection.Extensions;

var services = new ServiceCollection();

// Добавление логирования (опционально, но рекомендуется)
services.AddLogging(builder => builder.AddConsole());

// Регистрация StableDiffusion клиента (простейший вариант)
services.AddStableDiffusion("http://localhost:7860");

var serviceProvider = services.BuildServiceProvider();
var client = serviceProvider.GetRequiredService<IStableDiffusionClient>();
```

### Расширенная конфигурация

```csharp
services.AddStableDiffusion(options =>
{
    options.BaseUrl = "http://localhost:7860";
    options.TimeoutSeconds = 600;
    options.RetryCount = 5;
    options.RetryDelayMilliseconds = 2000;
    options.ApiKey = "your-api-key";
    options.EnableDetailedLogging = true; // Только для отладки
});
```

### ASP.NET Core Integration

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Регистрация из конфигурации
builder.Services.AddStableDiffusion(options =>
{
    builder.Configuration.GetSection("StableDiffusion").Bind(options);
});

var app = builder.Build();
```

```json
// appsettings.json
{
  "StableDiffusion": {
    "BaseUrl": "http://localhost:7860",
    "TimeoutSeconds": 300,
    "RetryCount": 3
  }
}
```

### Использование в контроллерах

```csharp
using Microsoft.AspNetCore.Mvc;
using StableDiffusionNet.Interfaces;
using StableDiffusionNet.Models.Requests;

[ApiController]
[Route("api/[controller]")]
public class ImageGenerationController : ControllerBase
{
    private readonly IStableDiffusionClient _sdClient;
    private readonly ILogger<ImageGenerationController> _logger;

    public ImageGenerationController(
        IStableDiffusionClient sdClient,
        ILogger<ImageGenerationController> logger)
    {
        _sdClient = sdClient;
        _logger = logger;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] TextToImageRequest request)
    {
        try
        {
            var response = await _sdClient.TextToImage.GenerateAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Image generation failed");
            return StatusCode(500, "Generation failed");
        }
    }
}
```

### Использование отдельных сервисов

Вы также можете внедрять отдельные сервисы:

```csharp
public class MyService
{
    private readonly ITextToImageService _textToImage;
    private readonly IModelService _models;
    
    public MyService(
        ITextToImageService textToImage,
        IModelService models)
    {
        _textToImage = textToImage;
        _models = models;
    }
    
    public async Task DoSomething()
    {
        var models = await _models.GetModelsAsync();
        // ...
    }
}
```

## Безопасность API ключей

**Важно**: Никогда не храните API ключи в коде или публичных репозиториях!

### User Secrets (для разработки)

```bash
dotnet user-secrets set "StableDiffusion:ApiKey" "your-secret-key"
```

```csharp
services.AddStableDiffusion(options =>
{
    options.ApiKey = builder.Configuration["StableDiffusion:ApiKey"];
});
```

### Переменные окружения

```bash
# Windows PowerShell
$env:SD_API_KEY="your-secret-key"

# Linux/Mac
export SD_API_KEY="your-secret-key"
```

```csharp
services.AddStableDiffusion(options =>
{
    options.ApiKey = Environment.GetEnvironmentVariable("SD_API_KEY");
});
```

### Azure Key Vault (для продакшена)

```csharp
var keyVaultUri = new Uri(builder.Configuration["KeyVaultUri"]);
builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());
```

## Логирование

Библиотека автоматически интегрируется с `Microsoft.Extensions.Logging`:

```csharp
// Настройка логирования
builder.Services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.AddDebug();
    builder.SetMinimumLevel(LogLevel.Information);
});
```

Логи включают:
- Информацию о запросах и ответах
- Ошибки и исключения
- Retry попытки
- Прогресс операций

**Внимание**: Опция `EnableDetailedLogging` может логировать чувствительные данные (промпты, base64 изображения). Используйте только для отладки в безопасном окружении!

## Доступные опции

| Опция | Тип | По умолчанию | Описание |
|-------|-----|--------------|----------|
| `BaseUrl` | `string` | `"http://localhost:7860"` | Базовый URL API |
| `TimeoutSeconds` | `int` | `300` | Таймаут запросов в секундах |
| `RetryCount` | `int` | `3` | Количество повторов при ошибке |
| `RetryDelayMilliseconds` | `int` | `1000` | Задержка между повторами в мс |
| `ApiKey` | `string?` | `null` | API ключ (если требуется) |
| `EnableDetailedLogging` | `bool` | `false` | Детальное логирование (только для отладки) |

## Нужен lightweight вариант без DI?

Если вам не нужна интеграция с Dependency Injection, используйте базовый пакет `StableDiffusionNet.Core`:

```bash
dotnet add package StableDiffusionNet.Core
```

```csharp
var client = StableDiffusionClientBuilder.CreateDefault("http://localhost:7860");
```

## Лицензия

MIT License

## Ссылки

- [GitHub Repository](https://github.com/mrleo1nid/StableDiffusionNet)
- [StableDiffusionNet.Core](https://www.nuget.org/packages/StableDiffusionNet.Core/)
- [Stable Diffusion WebUI](https://github.com/AUTOMATIC1111/stable-diffusion-webui)

