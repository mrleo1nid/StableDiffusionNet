# StableDiffusionNet

[![CI Build and Test](https://github.com/mrleo1nid/StableDiffusionNet/actions/workflows/ci.yml/badge.svg)](https://github.com/mrleo1nid/StableDiffusionNet/actions/workflows/ci.yml)
[![CodeQL](https://github.com/mrleo1nid/StableDiffusionNet/actions/workflows/codeql.yml/badge.svg)](https://github.com/mrleo1nid/StableDiffusionNet/actions/workflows/codeql.yml)
[![NuGet Version](https://img.shields.io/nuget/v/StableDiffusionNet.svg)](https://www.nuget.org/packages/StableDiffusionNet/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/StableDiffusionNet.svg)](https://www.nuget.org/packages/StableDiffusionNet/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

.NET клиент для Stable Diffusion WebUI API

## Особенности

- Поддержка Dependency Injection
- Встроенная retry-логика с использованием Polly
- Асинхронные операции с поддержкой async/await и CancellationToken
- XML документация для всех публичных API
- Интеграция с Microsoft.Extensions.Logging
- Поддержка .NET Standard 2.0, 2.1, .NET 6.0, .NET 8.0

## 📦 Установка

```bash
dotnet add package StableDiffusionNet
```

Или через NuGet Package Manager:

```
Install-Package StableDiffusionNet
```

## 🚀 Быстрый старт

### Настройка с Dependency Injection

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StableDiffusionNet.Extensions;

// Настройка DI контейнера
var services = new ServiceCollection();

// Добавление логирования
services.AddLogging(builder => builder.AddConsole());

// Регистрация StableDiffusion клиента
services.AddStableDiffusion(options =>
{
    options.BaseUrl = "http://localhost:7860";
    options.TimeoutSeconds = 300;
    options.RetryCount = 3;
    options.EnableDetailedLogging = true;
});

var serviceProvider = services.BuildServiceProvider();
var client = serviceProvider.GetRequiredService<IStableDiffusionClient>();
```

### Простая настройка

```csharp
// Для быстрого старта с настройками по умолчанию
services.AddStableDiffusion("http://localhost:7860");
```

## 📚 Примеры использования

### 1. Генерация изображения из текста (txt2img)

```csharp
using StableDiffusionNet.Models.Requests;

var request = new TextToImageRequest
{
    Prompt = "a beautiful sunset over mountains, highly detailed, 4k",
    NegativePrompt = "blurry, low quality, distorted",
    Width = 512,
    Height = 512,
    Steps = 30,
    CfgScale = 7.5,
    SamplerName = "Euler a",
    Seed = -1 // Случайный seed
};

var response = await client.TextToImage.GenerateAsync(request);

// Сохранение первого изображения
var base64Image = response.Images[0];
ImageHelper.Base64ToImage(base64Image, "output.png");

Console.WriteLine($"Сгенерировано изображений: {response.Images.Count}");
Console.WriteLine($"Информация: {response.Info}");
```

### 2. Генерация изображения из изображения (img2img)

```csharp
using StableDiffusionNet.Models.Requests;
using StableDiffusionNet.Helpers;

// Загрузка исходного изображения
var initImageBase64 = ImageHelper.ImageToBase64("input.png");

var request = new ImageToImageRequest
{
    InitImages = new List<string> { initImageBase64 },
    Prompt = "transform into a painting in van gogh style",
    NegativePrompt = "photo, realistic",
    DenoisingStrength = 0.7,
    Steps = 30,
    CfgScale = 7.5
};

var response = await client.ImageToImage.GenerateAsync(request);
ImageHelper.Base64ToImage(response.Images[0], "output.png");
```

### 3. Работа с моделями

```csharp
// Получение списка доступных моделей
var models = await client.Models.GetModelsAsync();
foreach (var model in models)
{
    Console.WriteLine($"Модель: {model.Title}");
}

// Получение текущей модели
var currentModel = await client.Models.GetCurrentModelAsync();
Console.WriteLine($"Текущая модель: {currentModel}");

// Смена модели
await client.Models.SetModelAsync("sd_xl_base_1.0.safetensors");

// Обновление списка моделей
await client.Models.RefreshModelsAsync();
```

### 4. Отслеживание прогресса генерации

```csharp
// Запуск генерации в отдельной задаче
var generateTask = client.TextToImage.GenerateAsync(request);

// Мониторинг прогресса
while (!generateTask.IsCompleted)
{
    var progress = await client.Progress.GetProgressAsync();
    
    Console.WriteLine($"Прогресс: {progress.Progress:P}");
    
    if (progress.State != null)
    {
        Console.WriteLine($"Шаг: {progress.State.SamplingStep}/{progress.State.SamplingSteps}");
        Console.WriteLine($"ETA: {progress.EtaRelative:F1} сек");
    }
    
    await Task.Delay(1000);
}

var result = await generateTask;
```

### 5. Прерывание генерации

```csharp
// Прерывание текущей генерации
await client.Progress.InterruptAsync();

// Пропуск текущего изображения при батч-генерации
await client.Progress.SkipAsync();
```

### 6. Работа с настройками WebUI

```csharp
// Получение текущих настроек
var options = await client.Options.GetOptionsAsync();
Console.WriteLine($"Текущий CLIP skip: {options.ClipStopAtLastLayers}");

// Изменение настроек
options.ClipStopAtLastLayers = 2;
options.EnableXformers = true;
await client.Options.SetOptionsAsync(options);
```

### 7. Получение списка sampler'ов

```csharp
var samplers = await client.Samplers.GetSamplersAsync();
foreach (var sampler in samplers)
{
    Console.WriteLine($"Sampler: {sampler}");
}
```

### 8. Проверка доступности API

```csharp
var isAvailable = await client.PingAsync();
if (isAvailable)
{
    Console.WriteLine("API доступен");
}
else
{
    Console.WriteLine("API недоступен");
}
```

### 9. Батч-генерация изображений

```csharp
var request = new TextToImageRequest
{
    Prompt = "a cute cat, highly detailed",
    BatchSize = 4,  // 4 изображения за раз
    NIter = 2,      // 2 батча
    Steps = 20
};

var response = await client.TextToImage.GenerateAsync(request);
// Получим 8 изображений (4 × 2)

for (int i = 0; i < response.Images.Count; i++)
{
    ImageHelper.Base64ToImage(response.Images[i], $"output_{i}.png");
}
```

### 10. High Resolution (Hires.fix)

```csharp
var request = new TextToImageRequest
{
    Prompt = "a beautiful landscape, 8k, highly detailed",
    Width = 512,
    Height = 512,
    EnableHr = true,
    HrScale = 2.0,  // Увеличение в 2 раза (итого 1024×1024)
    HrUpscaler = "Latent",
    HrSecondPassSteps = 20,
    DenoisingStrength = 0.7
};

var response = await client.TextToImage.GenerateAsync(request);
```

### 11. Inpainting (рисование по маске)

```csharp
var initImage = ImageHelper.ImageToBase64("input.png");
var mask = ImageHelper.ImageToBase64("mask.png"); // Белые области будут перерисованы

var request = new ImageToImageRequest
{
    InitImages = new List<string> { initImage },
    Mask = mask,
    Prompt = "a red apple",
    DenoisingStrength = 0.75,
    InpaintingFill = 1, // 0: fill, 1: original, 2: latent noise, 3: latent nothing
    InpaintFullRes = true,
    MaskBlur = 4
};

var response = await client.ImageToImage.GenerateAsync(request);
ImageHelper.Base64ToImage(response.Images[0], "inpainted.png");
```

## 🔧 Конфигурация

### 🔒 Безопасность API ключей

**Важно**: Никогда не храните API ключи в коде или публичных репозиториях!

Рекомендуемые способы хранения ключей:

```csharp
// ✅ Использование User Secrets (для разработки)
// dotnet user-secrets set "StableDiffusion:ApiKey" "your-secret-key"
services.AddStableDiffusion(options =>
{
    options.ApiKey = builder.Configuration["StableDiffusion:ApiKey"];
});

// ✅ Использование переменных окружения
services.AddStableDiffusion(options =>
{
    options.ApiKey = Environment.GetEnvironmentVariable("SD_API_KEY");
});

// ✅ Использование Azure Key Vault (для продакшена)
// var keyVaultUri = new Uri(builder.Configuration["KeyVaultUri"]);
// builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());
```

### Доступные опции

```csharp
services.AddStableDiffusion(options =>
{
    // Базовый URL API (обязательно)
    options.BaseUrl = "http://localhost:7860";
    
    // Таймаут для запросов в секундах (по умолчанию: 300)
    options.TimeoutSeconds = 600;
    
    // Количество повторов при ошибке (по умолчанию: 3)
    options.RetryCount = 5;
    
    // Задержка между повторами в мс (по умолчанию: 1000)
    options.RetryDelayMilliseconds = 2000;
    
    // API ключ (если требуется)
    options.ApiKey = "your-api-key";
    
    // Детальное логирование (по умолчанию: false)
    // ⚠️ ВНИМАНИЕ: Включайте только для отладки в безопасном окружении!
    // Логи могут содержать промпты, base64 изображения и другие данные.
    options.EnableDetailedLogging = true;
});
```

## 🧪 Тестирование

Все компоненты тестируются с использованием mock-объектов:

```csharp
var mockHttpClient = new Mock<IHttpClientWrapper>();
mockHttpClient
    .Setup(x => x.PostAsync<TextToImageRequest, TextToImageResponse>(
        It.IsAny<string>(), 
        It.IsAny<TextToImageRequest>(), 
        It.IsAny<CancellationToken>()))
    .ReturnsAsync(new TextToImageResponse 
    { 
        Images = new List<string> { "base64data" } 
    });

var service = new TextToImageService(
    mockHttpClient.Object, 
    Mock.Of<ILogger<TextToImageService>>());
```

## 📝 Обработка ошибок

Библиотека предоставляет специализированные исключения:

- `StableDiffusionException` - базовое исключение
- `ApiException` - ошибки API (содержит StatusCode и ResponseBody)
- `ConfigurationException` - ошибки конфигурации

```csharp
try
{
    var response = await client.TextToImage.GenerateAsync(request);
}
catch (ApiException ex)
{
    Console.WriteLine($"Ошибка API: {ex.Message}");
    Console.WriteLine($"Статус код: {ex.StatusCode}");
    Console.WriteLine($"Тело ответа: {ex.ResponseBody}");
}
catch (StableDiffusionException ex)
{
    Console.WriteLine($"Общая ошибка: {ex.Message}");
}
```

## 🔄 Retry Policy

Библиотека использует Polly для автоматических повторов при временных ошибках:

- Транзитные HTTP ошибки
- Ошибки сети
- HTTP 429 (Too Many Requests)

Повторы выполняются с экспоненциальной задержкой.

## 🎯 Требования

- .NET Standard 2.0+ / .NET Framework 4.7.2+ / .NET Core 2.0+ / .NET 5.0+
- Stable Diffusion WebUI (AUTOMATIC1111) с включенным API

## 🤝 Вклад в проект

Мы приветствуем вклад в развитие проекта! Пожалуйста:

1. Создайте форк репозитория
2. Создайте ветку для вашей функции
3. Добавьте тесты для новой функциональности
4. Создайте Pull Request

## 📄 Лицензия

MIT License

## 🔗 Полезные ссылки

- [Stable Diffusion WebUI](https://github.com/AUTOMATIC1111/stable-diffusion-webui)
- [API Documentation](https://github.com/AUTOMATIC1111/stable-diffusion-webui/wiki/API)

## 📧 Поддержка

При возникновении проблем создайте issue в репозитории проекта.

