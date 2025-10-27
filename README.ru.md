# StableDiffusionNet

**[English](README.md) | Русский**

[![CI Build](https://github.com/mrleo1nid/StableDiffusionNet/actions/workflows/ci.yml/badge.svg)](https://github.com/mrleo1nid/StableDiffusionNet/actions/workflows/ci.yml)
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

.NET библиотека-клиент для Stable Diffusion WebUI API с поддержкой async/await, retry-логикой и dependency injection.

## Выбор пакета

StableDiffusionNet предлагает два пакета для разных сценариев использования:

### StableDiffusionNet.Core
**Lightweight пакет без Dependency Injection**

[![NuGet](https://img.shields.io/nuget/v/StableDiffusionNet.Core.svg)](https://www.nuget.org/packages/StableDiffusionNet.Core/)

Идеален для:
- Консольных приложений
- Простых скриптов и утилит
- Проектов без инфраструктуры DI
- Минимальных зависимостей

```bash
dotnet add package StableDiffusionNet.Core
```

### StableDiffusionNet.DependencyInjection
**Расширения для Microsoft.Extensions.DependencyInjection**

[![NuGet](https://img.shields.io/nuget/v/StableDiffusionNet.DependencyInjection.svg)](https://www.nuget.org/packages/StableDiffusionNet.DependencyInjection/)

Идеален для:
- ASP.NET Core приложений
- Проектов с DI контейнером
- Интеграции с Microsoft.Extensions.*
- IOptions pattern и конфигурации

```bash
dotnet add package StableDiffusionNet.DependencyInjection
```

## Особенности

- **Два варианта использования**: Core (без DI) или DependencyInjection (с интеграцией DI)
- **Builder Pattern**: удобное создание клиента в Core пакете
- **Retry-логика**: собственная реализация с экспоненциальной задержкой
- **Асинхронные операции**: поддержка async/await и CancellationToken
- **XML документация**: для всех публичных API
- **Гибкое логирование**: собственная абстракция в Core, интеграция с Microsoft.Extensions.Logging в DI
- **Multi-targeting**: .NET Standard 2.0, 2.1, .NET 6.0, .NET 8.0

## Установка

### Для проектов без DI (Console, Scripts, Utilities)

```bash
dotnet add package StableDiffusionNet.Core
```

### Для проектов с DI (ASP.NET Core, Modern Apps)

```bash
dotnet add package StableDiffusionNet.DependencyInjection
```

Пакет `StableDiffusionNet.DependencyInjection` автоматически установит `StableDiffusionNet.Core` как зависимость.

## Быстрый старт

### Вариант 1: StableDiffusionNet.Core (без DI)

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

См. [StableDiffusionNet.Core README](StableDiffusionNet.Core/README.ru.md) для детальных опций конфигурации.

### Вариант 2: StableDiffusionNet.DependencyInjection (с DI)

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

См. [StableDiffusionNet.DependencyInjection README](StableDiffusionNet.DependencyInjection/README.ru.md) для интеграции с ASP.NET Core.

## Примеры использования

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

### Документация

- **[Справочник API методов](docs/API_REFERENCE.ru.md)** - список всех 22 реализованных методов API
- **[Примеры](docs/EXAMPLES.ru.md)** - примеры использования всех сервисов (txt2img, img2img, models, progress, options и др.)
- **[Продвинутые сценарии](docs/ADVANCED.ru.md)** - автоматизация воркфлоу, параллельная генерация, интеграция с БД, RESTful API

## Конфигурация

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

## Обработка ошибок

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

## Реализованные методы API

Библиотека реализует 22 метода API в 12 группах сервисов, покрывая генерацию text-to-image, обработку image-to-image, управление моделями, отслеживание прогресса и многое другое.

См. [Справочник API](docs/API_REFERENCE.ru.md) для списка всех методов и их документации.

## Требования

- .NET Standard 2.0+ / .NET 6.0+ / .NET 8.0+
- Stable Diffusion WebUI (AUTOMATIC1111) с включенным API (`--api` флаг)

## Вклад в проект

Приветствуются Pull Request'ы! Пожалуйста, добавляйте тесты для новой функциональности.

## Лицензия

MIT License - см. [LICENSE](LICENSE)

## Ссылки

- [Stable Diffusion WebUI](https://github.com/AUTOMATIC1111/stable-diffusion-webui)
- [Официальная документация API](https://github.com/AUTOMATIC1111/stable-diffusion-webui/wiki/API)


