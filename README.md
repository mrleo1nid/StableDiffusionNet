# StableDiffusionNet

[![CI Build and Test](https://github.com/mrleo1nid/StableDiffusionNet/actions/workflows/ci.yml/badge.svg)](https://github.com/mrleo1nid/StableDiffusionNet/actions/workflows/ci.yml)
[![CodeQL](https://github.com/mrleo1nid/StableDiffusionNet/actions/workflows/codeql.yml/badge.svg)](https://github.com/mrleo1nid/StableDiffusionNet/actions/workflows/codeql.yml)
[![SonarQube](https://github.com/mrleo1nid/StableDiffusionNet/actions/workflows/sonarqube.yml/badge.svg)](https://github.com/mrleo1nid/StableDiffusionNet/actions/workflows/sonarqube.yml)

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=mrleo1nid_StableDiffusionNet&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=mrleo1nid_StableDiffusionNet)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=mrleo1nid_StableDiffusionNet&metric=coverage)](https://sonarcloud.io/summary/new_code?id=mrleo1nid_StableDiffusionNet)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=mrleo1nid_StableDiffusionNet&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=mrleo1nid_StableDiffusionNet)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=mrleo1nid_StableDiffusionNet&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=mrleo1nid_StableDiffusionNet)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=mrleo1nid_StableDiffusionNet&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=mrleo1nid_StableDiffusionNet)

[![NuGet Core](https://img.shields.io/nuget/v/StableDiffusionNet.Core.svg?label=Core)](https://www.nuget.org/packages/StableDiffusionNet.Core/)
[![NuGet DI](https://img.shields.io/nuget/v/StableDiffusionNet.DependencyInjection.svg?label=DI)](https://www.nuget.org/packages/StableDiffusionNet.DependencyInjection/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Мощный и удобный .NET клиент для Stable Diffusion WebUI API с полной поддержкой async/await, retry-логикой и dependency injection.

## 🎯 Выберите подходящий пакет

StableDiffusionNet предлагает два пакета для разных сценариев использования:

### StableDiffusionNet.Core
**Lightweight пакет без Dependency Injection**

[![NuGet](https://img.shields.io/nuget/v/StableDiffusionNet.Core.svg)](https://www.nuget.org/packages/StableDiffusionNet.Core/)

Идеален для:
- ✨ Консольных приложений
- 🚀 Простых скриптов и утилит
- 📦 Проектов без инфраструктуры DI
- ⚡ Минимальных зависимостей

```bash
dotnet add package StableDiffusionNet.Core
```

### StableDiffusionNet.DependencyInjection
**Расширения для Microsoft.Extensions.DependencyInjection**

[![NuGet](https://img.shields.io/nuget/v/StableDiffusionNet.DependencyInjection.svg)](https://www.nuget.org/packages/StableDiffusionNet.DependencyInjection/)

Идеален для:
- 🌐 ASP.NET Core приложений
- 🏗️ Проектов с DI контейнером
- 📊 Интеграции с Microsoft.Extensions.*
- ⚙️ IOptions pattern и конфигурации

```bash
dotnet add package StableDiffusionNet.DependencyInjection
```

## Особенности

- 🎯 **Два варианта использования**: Core (без DI) или DependencyInjection (с полной интеграцией DI)
- 🏗️ **Builder Pattern**: удобное создание клиента в Core пакете
- 🔄 **Надежная retry-логика**: собственная быстрая реализация с экспоненциальной задержкой
- ⚡ **Асинхронные операции**: полная поддержка async/await и CancellationToken
- 📝 **XML документация**: для всех публичных API
- 📊 **Гибкое логирование**: собственная абстракция в Core, интеграция с Microsoft.Extensions.Logging в DI
- 🎨 **Multi-targeting**: .NET Standard 2.0, 2.1, .NET 6.0, .NET 8.0

## 📦 Установка

### Для проектов без DI (Console, Scripts, Utilities)

```bash
dotnet add package StableDiffusionNet.Core
```

### Для проектов с DI (ASP.NET Core, Modern Apps)

```bash
dotnet add package StableDiffusionNet.DependencyInjection
```

Пакет `StableDiffusionNet.DependencyInjection` автоматически установит `StableDiffusionNet.Core` как зависимость.

## 🚀 Быстрый старт

### Вариант 1: StableDiffusionNet.Core (без DI)

```csharp
using StableDiffusionNet;
using StableDiffusionNet.Models.Requests;

// Создание клиента с настройками по умолчанию
var client = StableDiffusionClientBuilder.CreateDefault("http://localhost:7860");

// Или с дополнительными настройками через билдер
var client = new StableDiffusionClientBuilder()
    .WithBaseUrl("http://localhost:7860")
    .WithTimeout(600)
    .WithRetry(retryCount: 3, retryDelayMilliseconds: 1000)
    .WithDetailedLogging()
    .Build();

// Готово! Можно использовать
var request = new TextToImageRequest
{
    Prompt = "a beautiful sunset",
    Width = 512,
    Height = 512
};
var response = await client.TextToImage.GenerateAsync(request);
```

### Вариант 2: StableDiffusionNet.DependencyInjection (с DI)

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StableDiffusionNet.DependencyInjection.Extensions;

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

### ASP.NET Core Integration

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Простая регистрация
builder.Services.AddStableDiffusion("http://localhost:7860");

// Или с полной конфигурацией
builder.Services.AddStableDiffusion(options =>
{
    builder.Configuration.GetSection("StableDiffusion").Bind(options);
});

var app = builder.Build();
```

## 📚 Примеры использования

### Базовая генерация изображения

```csharp
using StableDiffusionNet.Models.Requests;
using StableDiffusionNet.Helpers;

var request = new TextToImageRequest
{
    Prompt = "a beautiful sunset over mountains, highly detailed, 4k",
    NegativePrompt = "blurry, low quality, distorted",
    Width = 512,
    Height = 512,
    Steps = 30
};

var response = await client.TextToImage.GenerateAsync(request);
ImageHelper.Base64ToImage(response.Images[0], "output.png");
```

### 📖 Подробная документация

- **[Справочник API методов](docs/API_REFERENCE.md)** - полный список всех 22 реализованных методов API
- **[Подробные примеры](docs/EXAMPLES.md)** - примеры использования всех сервисов (txt2img, img2img, models, progress, options и др.)
- **[Продвинутые сценарии](docs/ADVANCED.md)** - автоматизация воркфлоу, параллельная генерация, интеграция с БД, RESTful API и многое другое

## 🔧 Конфигурация

### Основные опции

```csharp
services.AddStableDiffusion(options =>
{
    options.BaseUrl = "http://localhost:7860";    // URL вашего Stable Diffusion WebUI
    options.TimeoutSeconds = 300;                 // Таймаут запросов
    options.RetryCount = 3;                       // Количество повторов при ошибке
    options.EnableDetailedLogging = false;        // Детальное логирование
});
```

**Важно**: Никогда не храните API ключи в коде! Используйте User Secrets, переменные окружения или Azure Key Vault.

## 📝 Обработка ошибок

Библиотека предоставляет специализированные исключения для разных типов ошибок:

```csharp
try
{
    var response = await client.TextToImage.GenerateAsync(request);
}
catch (ApiException ex)
{
    Console.WriteLine($"Ошибка API: {ex.StatusCode} - {ex.Message}");
}
catch (StableDiffusionException ex)
{
    Console.WriteLine($"Ошибка: {ex.Message}");
}
```

Библиотека автоматически повторяет запросы при транзитных ошибках (500, 502, 503, 504, 429) с экспоненциальной задержкой.

## 🔌 Реализованные методы API

StableDiffusionNet полностью покрывает основные методы Stable Diffusion WebUI API:

| Группа | Методы | Описание |
|--------|---------|----------|
| **TextToImage** | 1 метод | Генерация изображений из текста (txt2img) |
| **ImageToImage** | 1 метод | Генерация из изображений (img2img, inpainting) |
| **Models** | 4 метода | Управление моделями (список, смена, обновление) |
| **Progress** | 3 метода | Отслеживание прогресса, прерывание, пропуск |
| **Options** | 2 метода | Управление настройками WebUI |
| **Samplers** | 1 метод | Получение списка сэмплеров |
| **Schedulers** | 1 метод | Получение списка планировщиков |
| **Upscalers** | 2 метода | Информация об апскейлерах |
| **PngInfo** | 1 метод | Извлечение метаданных из PNG |
| **Extra** | 1 метод | Постобработка (апскейл, восстановление лиц) |
| **Embeddings** | 2 метода | Работа с текстовыми инверсиями |
| **Loras** | 2 метода | Работа с LoRA моделями |

**Всего: 22 метода API** → [Подробнее в API Reference](docs/API_REFERENCE.md)

## 🎯 Требования

- .NET Standard 2.0+ / .NET 6.0+ / .NET 8.0+
- Stable Diffusion WebUI (AUTOMATIC1111) с включенным API (`--api` флаг)

## 🤝 Вклад в проект

Приветствуются Pull Request'ы! Пожалуйста, добавляйте тесты для новой функциональности.

## 📄 Лицензия

MIT License - см. [LICENSE](LICENSE)

## 🔗 Ссылки

- [Stable Diffusion WebUI](https://github.com/AUTOMATIC1111/stable-diffusion-webui)
- [Официальная документация API](https://github.com/AUTOMATIC1111/stable-diffusion-webui/wiki/API)

