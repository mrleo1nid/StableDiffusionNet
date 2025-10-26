# Подробные примеры использования StableDiffusionNet

> Практические примеры для всех сервисов библиотеки

## 📋 Оглавление

- [Начало работы](#начало-работы)
- [Text to Image](#text-to-image)
- [Image to Image](#image-to-image)
- [Управление моделями](#управление-моделями)
- [Отслеживание прогресса](#отслеживание-прогресса)
- [Настройки WebUI](#настройки-webui)
- [Получение информации](#получение-информации)
- [PNG метаданные](#png-метаданные)
- [Постобработка](#постобработка)
- [Embeddings и LoRA](#embeddings-и-lora)
- [Обработка ошибок](#обработка-ошибок)

---

## Начало работы

### Создание клиента (Core пакет)

```csharp
using StableDiffusionNet;

// Простой способ
var client = StableDiffusionClientBuilder.CreateDefault("http://localhost:7860");

// С настройками
var client = new StableDiffusionClientBuilder()
    .WithBaseUrl("http://localhost:7860")
    .WithTimeout(600) // 10 минут
    .WithRetry(retryCount: 3, retryDelayMilliseconds: 1000)
    .WithDetailedLogging()
    .Build();
```

### Создание клиента (DI пакет)

```csharp
using Microsoft.Extensions.DependencyInjection;
using StableDiffusionNet.DependencyInjection.Extensions;

var services = new ServiceCollection();
services.AddLogging();
services.AddStableDiffusion(options =>
{
    options.BaseUrl = "http://localhost:7860";
    options.TimeoutSeconds = 600;
    options.RetryCount = 3;
});

var serviceProvider = services.BuildServiceProvider();
var client = serviceProvider.GetRequiredService<IStableDiffusionClient>();
```

### Проверка доступности API

```csharp
var isAvailable = await client.PingAsync();
if (!isAvailable)
{
    Console.WriteLine("API недоступен. Убедитесь, что WebUI запущен с --api флагом.");
    return;
}

Console.WriteLine("✓ API доступен");
```

---

## Text to Image

### Базовая генерация

```csharp
using StableDiffusionNet.Models.Requests;
using StableDiffusionNet.Helpers;

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

// Сохранение изображения
ImageHelper.Base64ToImage(response.Images[0], "output.png");
Console.WriteLine($"Изображение сохранено: output.png");
```

### Генерация с фиксированным seed

```csharp
var request = new TextToImageRequest
{
    Prompt = "a cute cat, professional photo",
    Width = 512,
    Height = 512,
    Steps = 20,
    Seed = 12345 // Фиксированный seed для воспроизводимости
};

var response = await client.TextToImage.GenerateAsync(request);
// При повторном запросе с тем же seed получится идентичное изображение
```

### Батч-генерация

```csharp
var request = new TextToImageRequest
{
    Prompt = "fantasy landscape, concept art",
    Width = 512,
    Height = 512,
    Steps = 25,
    BatchSize = 4,  // 4 изображения за раз
    NIter = 2       // 2 батча (итого 8 изображений)
};

var response = await client.TextToImage.GenerateAsync(request);

// Сохранение всех изображений
for (int i = 0; i < response.Images.Count; i++)
{
    ImageHelper.Base64ToImage(response.Images[i], $"output_{i:D3}.png");
}

Console.WriteLine($"Сгенерировано {response.Images.Count} изображений");
```

### High Resolution (Hires.fix)

```csharp
var request = new TextToImageRequest
{
    Prompt = "epic castle on a mountain, 8k, highly detailed, cinematic lighting",
    Width = 512,
    Height = 512,
    Steps = 30,
    
    // Включение Hires.fix
    EnableHr = true,
    HrScale = 2.0,              // Увеличение в 2 раза (итого 1024x1024)
    HrUpscaler = "Latent",      // Используемый апскейлер
    HrSecondPassSteps = 20,     // Шаги для второго прохода
    DenoisingStrength = 0.7     // Сила шумоподавления
};

var response = await client.TextToImage.GenerateAsync(request);
ImageHelper.Base64ToImage(response.Images[0], "hires_output.png");
```

### Генерация с восстановлением лиц

```csharp
var request = new TextToImageRequest
{
    Prompt = "portrait of a beautiful woman, highly detailed face",
    Width = 512,
    Height = 768,
    Steps = 30,
    RestoreFaces = true  // Автоматическое восстановление лиц
};

var response = await client.TextToImage.GenerateAsync(request);
ImageHelper.Base64ToImage(response.Images[0], "portrait.png");
```

### Tileable текстуры

```csharp
var request = new TextToImageRequest
{
    Prompt = "seamless wood texture, 4k, pbr",
    Width = 512,
    Height = 512,
    Steps = 25,
    Tiling = true  // Генерация бесшовной текстуры
};

var response = await client.TextToImage.GenerateAsync(request);
ImageHelper.Base64ToImage(response.Images[0], "texture.png");
```

### Переопределение настроек для одного запроса

```csharp
var request = new TextToImageRequest
{
    Prompt = "anime style character",
    Steps = 20,
    
    // Временно изменить настройки только для этого запроса
    OverrideSettings = new Dictionary<string, object>
    {
        { "CLIP_stop_at_last_layers", 2 },      // CLIP skip 2
        { "eta_noise_seed_delta", 0 },          // ENSD
        { "sd_vae", "vae-ft-mse-840000-ema-pruned.ckpt" }
    }
};

var response = await client.TextToImage.GenerateAsync(request);
// После выполнения настройки вернутся к исходным
```

### Использование разных сэмплеров

```csharp
var samplers = new[] { "Euler a", "DPM++ 2M", "DPM++ SDE Karras" };
var prompt = "beautiful landscape";

foreach (var sampler in samplers)
{
    var request = new TextToImageRequest
    {
        Prompt = prompt,
        Width = 512,
        Height = 512,
        Steps = 20,
        SamplerName = sampler,
        Seed = 12345  // Одинаковый seed для сравнения
    };
    
    var response = await client.TextToImage.GenerateAsync(request);
    ImageHelper.Base64ToImage(response.Images[0], $"sampler_{sampler.Replace(" ", "_")}.png");
    Console.WriteLine($"Сгенерировано с {sampler}");
}
```

---

## Image to Image

### Базовая трансформация изображения

```csharp
using StableDiffusionNet.Models.Requests;
using StableDiffusionNet.Helpers;

// Загрузка исходного изображения
var initImage = ImageHelper.ImageToBase64("input.png");

var request = new ImageToImageRequest
{
    InitImages = new List<string> { initImage },
    Prompt = "transform into a watercolor painting",
    NegativePrompt = "photo, realistic",
    
    DenoisingStrength = 0.75,  // Сила изменения (0.0 = без изменений, 1.0 = полная перерисовка)
    Width = 512,
    Height = 512,
    Steps = 30,
    CfgScale = 7.5
};

var response = await client.ImageToImage.GenerateAsync(request);
ImageHelper.Base64ToImage(response.Images[0], "watercolor.png");
```

### Вариации одного изображения

```csharp
var initImage = ImageHelper.ImageToBase64("photo.png");

// Небольшая сила - больше похоже на оригинал
for (int i = 0; i < 4; i++)
{
    var request = new ImageToImageRequest
    {
        InitImages = new List<string> { initImage },
        Prompt = "professional photo, high quality",
        DenoisingStrength = 0.3,  // Небольшие изменения
        Steps = 20,
        Seed = -1  // Разный seed для вариаций
    };
    
    var response = await client.ImageToImage.GenerateAsync(request);
    ImageHelper.Base64ToImage(response.Images[0], $"variation_{i}.png");
}
```

### Стилизация фотографии

```csharp
var photo = ImageHelper.ImageToBase64("portrait.png");

var styles = new Dictionary<string, string>
{
    { "van_gogh", "painting in van gogh style, post-impressionism" },
    { "anime", "anime style, studio ghibli" },
    { "cyberpunk", "cyberpunk style, neon lights, futuristic" },
    { "oil_painting", "classical oil painting, renaissance style" }
};

foreach (var style in styles)
{
    var request = new ImageToImageRequest
    {
        InitImages = new List<string> { photo },
        Prompt = style.Value,
        NegativePrompt = "photo, realistic, low quality",
        DenoisingStrength = 0.7,
        Steps = 30
    };
    
    var response = await client.ImageToImage.GenerateAsync(request);
    ImageHelper.Base64ToImage(response.Images[0], $"style_{style.Key}.png");
    Console.WriteLine($"Создан стиль: {style.Key}");
}
```

### Inpainting - рисование по маске

```csharp
var initImage = ImageHelper.ImageToBase64("room.png");
var mask = ImageHelper.ImageToBase64("mask.png");  // Белые области будут перерисованы

var request = new ImageToImageRequest
{
    InitImages = new List<string> { initImage },
    Mask = mask,
    Prompt = "modern sofa, interior design",
    NegativePrompt = "blurry, low quality",
    
    DenoisingStrength = 0.9,
    Steps = 30,
    
    // Параметры inpainting
    InpaintingFill = 1,        // 0: fill, 1: original, 2: latent noise, 3: latent nothing
    InpaintFullRes = true,     // Рисовать только область маски в высоком разрешении
    InpaintFullResPadding = 32, // Отступ вокруг маски
    MaskBlur = 4              // Размытие краев маски
};

var response = await client.ImageToImage.GenerateAsync(request);
ImageHelper.Base64ToImage(response.Images[0], "inpainted.png");
```

### Outpainting - расширение изображения

```csharp
// Создайте маску где нужно дорисовать (белые области)
// Например, расширьте canvas и сделайте новые области белыми

var image = ImageHelper.ImageToBase64("original.png");
var mask = ImageHelper.ImageToBase64("outpaint_mask.png");

var request = new ImageToImageRequest
{
    InitImages = new List<string> { image },
    Mask = mask,
    Prompt = "continue the landscape, natural, seamless",
    
    DenoisingStrength = 0.9,
    Steps = 30,
    InpaintingFill = 2,  // Latent noise для outpainting
    MaskBlur = 8
};

var response = await client.ImageToImage.GenerateAsync(request);
ImageHelper.Base64ToImage(response.Images[0], "outpainted.png");
```

### Изменение размера с разными режимами

```csharp
var image = ImageHelper.ImageToBase64("input.png");

var resizeModes = new Dictionary<int, string>
{
    { 0, "Just resize" },
    { 1, "Crop and resize" },
    { 2, "Resize and fill" },
    { 3, "Just resize (latent upscale)" }
};

foreach (var mode in resizeModes)
{
    var request = new ImageToImageRequest
    {
        InitImages = new List<string> { image },
        Prompt = "high quality, detailed",
        
        Width = 768,
        Height = 768,
        ResizeMode = mode.Key,  // Режим изменения размера
        
        DenoisingStrength = 0.5,
        Steps = 25
    };
    
    var response = await client.ImageToImage.GenerateAsync(request);
    ImageHelper.Base64ToImage(response.Images[0], $"resize_mode_{mode.Key}.png");
    Console.WriteLine($"Режим {mode.Key}: {mode.Value}");
}
```

### Sketch to Image

```csharp
// Превращение наброска в детализированное изображение
var sketch = ImageHelper.ImageToBase64("sketch.png");

var request = new ImageToImageRequest
{
    InitImages = new List<string> { sketch },
    Prompt = "professional concept art, highly detailed, realistic lighting",
    NegativePrompt = "sketch, rough, unfinished",
    
    DenoisingStrength = 0.85,  // Высокая сила для детализации
    Steps = 40,
    CfgScale = 9.0  // Высокий CFG для следования промпту
};

var response = await client.ImageToImage.GenerateAsync(request);
ImageHelper.Base64ToImage(response.Images[0], "detailed_art.png");
```

---

## Управление моделями

### Получение списка доступных моделей

```csharp
var models = await client.Models.GetModelsAsync();

Console.WriteLine("Доступные модели:");
foreach (var model in models)
{
    Console.WriteLine($"  - {model.Title}");
    Console.WriteLine($"    Файл: {model.ModelName}");
    Console.WriteLine($"    Hash: {model.Hash?.Substring(0, 8)}...");
    Console.WriteLine();
}
```

### Получение текущей модели

```csharp
var currentModel = await client.Models.GetCurrentModelAsync();
Console.WriteLine($"Текущая модель: {currentModel}");
```

### Смена модели

```csharp
Console.WriteLine("Смена модели...");
await client.Models.SetModelAsync("sd_xl_base_1.0.safetensors");

// Ожидание загрузки модели
await Task.Delay(5000);

var newModel = await client.Models.GetCurrentModelAsync();
Console.WriteLine($"Активна модель: {newModel}");
```

### Автоматический выбор модели по задаче

```csharp
async Task<string> SelectModelForTask(string task)
{
    var models = await client.Models.GetModelsAsync();
    
    if (task.Contains("anime"))
    {
        // Ищем аниме модель
        var animeModel = models.FirstOrDefault(m => 
            m.Title.Contains("anime", StringComparison.OrdinalIgnoreCase));
        return animeModel?.ModelName ?? models[0].ModelName;
    }
    else if (task.Contains("realistic"))
    {
        // Ищем реалистичную модель
        var realisticModel = models.FirstOrDefault(m => 
            m.Title.Contains("realistic", StringComparison.OrdinalIgnoreCase));
        return realisticModel?.ModelName ?? models[0].ModelName;
    }
    
    return models[0].ModelName;
}

// Использование
var modelName = await SelectModelForTask("generate realistic portrait");
await client.Models.SetModelAsync(modelName);
```

### Обновление списка моделей

```csharp
Console.WriteLine("Сканирование папки с моделями...");
await client.Models.RefreshModelsAsync();

var models = await client.Models.GetModelsAsync();
Console.WriteLine($"Найдено моделей: {models.Count}");
```

### Генерация с разными моделями

```csharp
var models = new[] 
{ 
    "sd_v1-5.safetensors",
    "sd_xl_base_1.0.safetensors"
};

var prompt = "beautiful landscape";

foreach (var model in models)
{
    Console.WriteLine($"Генерация с моделью: {model}");
    
    await client.Models.SetModelAsync(model);
    await Task.Delay(3000); // Ожидание загрузки модели
    
    var request = new TextToImageRequest
    {
        Prompt = prompt,
        Width = 512,
        Height = 512,
        Steps = 20,
        Seed = 12345
    };
    
    var response = await client.TextToImage.GenerateAsync(request);
    ImageHelper.Base64ToImage(response.Images[0], $"model_{model.Replace(".", "_")}.png");
}
```

---

## Отслеживание прогресса

### Простой progress bar

```csharp
var request = new TextToImageRequest
{
    Prompt = "complex detailed scene",
    Width = 512,
    Height = 512,
    Steps = 50
};

// Запуск генерации в отдельной задаче
var generateTask = client.TextToImage.GenerateAsync(request);

// Отслеживание прогресса
while (!generateTask.IsCompleted)
{
    var progress = await client.Progress.GetProgressAsync();
    
    // Процент прогресса
    var percentage = progress.Progress * 100;
    Console.Write($"\rПрогресс: {percentage:F1}%");
    
    await Task.Delay(500);
}

Console.WriteLine("\n✓ Генерация завершена");
var result = await generateTask;
```

### Детальный прогресс с ETA

```csharp
var generateTask = client.TextToImage.GenerateAsync(request);

while (!generateTask.IsCompleted)
{
    var progress = await client.Progress.GetProgressAsync();
    
    if (progress.State != null)
    {
        var eta = TimeSpan.FromSeconds(progress.EtaRelative);
        
        Console.WriteLine($"Прогресс: {progress.Progress:P}");
        Console.WriteLine($"Шаг: {progress.State.SamplingStep}/{progress.State.SamplingSteps}");
        Console.WriteLine($"ETA: {eta:mm\\:ss}");
        Console.WriteLine(new string('-', 50));
    }
    
    await Task.Delay(1000);
}

var result = await generateTask;
```

### Сохранение промежуточных результатов

```csharp
var generateTask = client.TextToImage.GenerateAsync(request);
var previewCount = 0;

while (!generateTask.IsCompleted)
{
    var progress = await client.Progress.GetProgressAsync();
    
    // Если доступно превью
    if (!string.IsNullOrEmpty(progress.CurrentImage))
    {
        ImageHelper.Base64ToImage(
            progress.CurrentImage, 
            $"preview_{previewCount++:D3}.png"
        );
        Console.WriteLine($"Сохранено превью #{previewCount}");
    }
    
    await Task.Delay(2000);
}

var result = await generateTask;
```

### Прерывание генерации пользователем

```csharp
var cts = new CancellationTokenSource();

// Обработчик Ctrl+C
Console.CancelKeyPress += async (sender, e) =>
{
    e.Cancel = true;
    Console.WriteLine("\nПрерывание генерации...");
    await client.Progress.InterruptAsync();
    cts.Cancel();
};

var request = new TextToImageRequest
{
    Prompt = "very complex scene",
    Steps = 100
};

try
{
    var generateTask = client.TextToImage.GenerateAsync(request, cts.Token);
    
    while (!generateTask.IsCompleted)
    {
        var progress = await client.Progress.GetProgressAsync();
        Console.Write($"\rПрогресс: {progress.Progress:P}");
        await Task.Delay(500);
    }
    
    var result = await generateTask;
    Console.WriteLine("\n✓ Генерация завершена");
}
catch (OperationCanceledException)
{
    Console.WriteLine("\n× Генерация прервана пользователем");
}
```

### Пропуск изображения в батче

```csharp
var request = new TextToImageRequest
{
    Prompt = "test prompt",
    BatchSize = 10,
    Steps = 30
};

var generateTask = client.TextToImage.GenerateAsync(request);

// Пропускаем 5-е изображение
var imageCount = 0;
while (!generateTask.IsCompleted)
{
    var progress = await client.Progress.GetProgressAsync();
    
    if (progress.State != null)
    {
        var currentImage = (int)(progress.Progress * 10);
        if (currentImage == 5 && imageCount != currentImage)
        {
            Console.WriteLine("Пропускаем изображение #5...");
            await client.Progress.SkipAsync();
        }
        imageCount = currentImage;
    }
    
    await Task.Delay(500);
}

var result = await generateTask;
```

---

## Настройки WebUI

### Получение текущих настроек

```csharp
var options = await client.Options.GetOptionsAsync();

Console.WriteLine("Текущие настройки:");
Console.WriteLine($"  Модель: {options.SdModelCheckpoint}");
Console.WriteLine($"  CLIP skip: {options.ClipStopAtLastLayers}");
Console.WriteLine($"  xFormers: {options.EnableXformers}");
Console.WriteLine($"  Формат сохранения: {options.SamplesFormat}");
```

### Изменение настроек

```csharp
var options = await client.Options.GetOptionsAsync();

// Изменяем настройки
options.ClipStopAtLastLayers = 2;
options.EnableXformers = true;
options.SamplesFormat = "png";
options.SamplesSave = true;

// Применяем изменения
await client.Options.SetOptionsAsync(options);
Console.WriteLine("✓ Настройки обновлены");
```

### Оптимизация для скорости

```csharp
var options = await client.Options.GetOptionsAsync();

// Настройки для быстрой генерации
options.EnableXformers = true;              // Использовать xFormers
options.AlwaysBatchCondUncond = false;     // Не батчить cond/uncond
options.UseOldHiresFixWidthHeight = false;

await client.Options.SetOptionsAsync(options);
Console.WriteLine("✓ Настроено для скорости");
```

### Оптимизация для качества

```csharp
var options = await client.Options.GetOptionsAsync();

// Настройки для максимального качества
options.NoHalfVae = true;                  // Полная точность VAE
options.EnableXformers = true;
options.SamplesFormat = "png";             // Без потерь

await client.Options.SetOptionsAsync(options);
Console.WriteLine("✓ Настроено для качества");
```

### Временное изменение настроек

```csharp
// Сохраняем текущие настройки
var originalOptions = await client.Options.GetOptionsAsync();

try
{
    // Временно меняем настройки
    var tempOptions = await client.Options.GetOptionsAsync();
    tempOptions.ClipStopAtLastLayers = 2;
    await client.Options.SetOptionsAsync(tempOptions);
    
    // Генерируем с новыми настройками
    var response = await client.TextToImage.GenerateAsync(request);
}
finally
{
    // Восстанавливаем исходные настройки
    await client.Options.SetOptionsAsync(originalOptions);
}
```

---

## Получение информации

### Доступные сэмплеры

```csharp
var samplers = await client.Samplers.GetSamplersAsync();

Console.WriteLine("Доступные сэмплеры:");
foreach (var sampler in samplers)
{
    Console.WriteLine($"  - {sampler}");
}
```

### Доступные планировщики

```csharp
var schedulers = await client.Schedulers.GetSchedulersAsync();

Console.WriteLine("Доступные планировщики:");
foreach (var scheduler in schedulers)
{
    Console.WriteLine($"  - {scheduler}");
}
```

### Доступные апскейлеры

```csharp
var upscalers = await client.Upscalers.GetUpscalersAsync();

Console.WriteLine("Доступные апскейлеры:");
foreach (var upscaler in upscalers)
{
    Console.WriteLine($"  - {upscaler.Name} ({upscaler.Scale}x)");
    if (!string.IsNullOrEmpty(upscaler.ModelPath))
    {
        Console.WriteLine($"    Путь: {upscaler.ModelPath}");
    }
}
```

### Latent upscale режимы

```csharp
var modes = await client.Upscalers.GetLatentUpscaleModesAsync();

Console.WriteLine("Latent upscale режимы:");
foreach (var mode in modes)
{
    Console.WriteLine($"  - {mode}");
}
```

### Доступные embeddings

```csharp
var embeddings = await client.Embeddings.GetEmbeddingsAsync();

Console.WriteLine($"Найдено embeddings: {embeddings.Count}");
foreach (var embedding in embeddings)
{
    Console.WriteLine($"  - {embedding.Key}");
    Console.WriteLine($"    Шаги: {embedding.Value.Step}");
    Console.WriteLine($"    Векторов: {embedding.Value.Vectors}");
}

// Обновление списка
await client.Embeddings.RefreshEmbeddingsAsync();
```

### Доступные LoRA

```csharp
var loras = await client.Loras.GetLorasAsync();

Console.WriteLine($"Найдено LoRA: {loras.Count}");
foreach (var lora in loras)
{
    Console.WriteLine($"  - {lora.Name}");
    Console.WriteLine($"    Путь: {lora.Path}");
}

// Обновление списка
await client.Loras.RefreshLorasAsync();
```

### Создание UI с динамическим выбором

```csharp
// Загружаем все опции для UI
var samplers = await client.Samplers.GetSamplersAsync();
var schedulers = await client.Schedulers.GetSchedulersAsync();
var upscalers = await client.Upscalers.GetUpscalersAsync();
var models = await client.Models.GetModelsAsync();

// Теперь можно построить dropdown'ы в UI
Console.WriteLine("Выберите опции для генерации:");
Console.WriteLine($"Доступно сэмплеров: {samplers.Count}");
Console.WriteLine($"Доступно моделей: {models.Count}");
Console.WriteLine($"Доступно апскейлеров: {upscalers.Count}");
```

---

## PNG метаданные

### Извлечение информации из изображения

```csharp
using StableDiffusionNet.Models.Requests;

var imageBase64 = ImageHelper.ImageToBase64("generated.png");

var request = new PngInfoRequest
{
    Image = imageBase64
};

var response = await client.PngInfo.GetPngInfoAsync(request);

Console.WriteLine("Информация о генерации:");
Console.WriteLine(response.Info);

// Парсинг параметров
if (response.Items != null)
{
    foreach (var item in response.Items)
    {
        Console.WriteLine($"{item.Key}: {item.Value}");
    }
}
```

### Копирование параметров из изображения

```csharp
var sourceImage = ImageHelper.ImageToBase64("source.png");

var pngInfoRequest = new PngInfoRequest { Image = sourceImage };
var pngInfo = await client.PngInfo.GetPngInfoAsync(pngInfoRequest);

// Парсим параметры (это упрощенный пример)
var info = pngInfo.Info;
// Здесь нужно распарсить строку info и извлечь параметры

// Создаем запрос с теми же параметрами
var request = new TextToImageRequest
{
    Prompt = "...extracted from info...",
    Steps = 30, // extracted
    CfgScale = 7.5, // extracted
    // и т.д.
};

var response = await client.TextToImage.GenerateAsync(request);
```

### Анализ чужих изображений

```csharp
async Task AnalyzeImage(string imagePath)
{
    try
    {
        var imageBase64 = ImageHelper.ImageToBase64(imagePath);
        var request = new PngInfoRequest { Image = imageBase64 };
        var response = await client.PngInfo.GetPngInfoAsync(request);
        
        if (!string.IsNullOrEmpty(response.Info))
        {
            Console.WriteLine($"✓ Изображение содержит метаданные генерации");
            Console.WriteLine(response.Info);
            return;
        }
        
        Console.WriteLine("× Изображение не содержит метаданных генерации");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при анализе: {ex.Message}");
    }
}

await AnalyzeImage("downloaded_image.png");
```

---

## Постобработка

### Апскейл изображения

```csharp
using StableDiffusionNet.Models.Requests;

var imageBase64 = ImageHelper.ImageToBase64("input.png");

var request = new ExtraSingleImageRequest
{
    Image = imageBase64,
    ResizeMode = 0,             // 0: по масштабу, 1: до конкретного размера
    UpscalingResize = 4,        // Увеличить в 4 раза
    Upscaler1 = "R-ESRGAN 4x+", // Основной апскейлер
    Upscaler2 = "None"          // Второй апскейлер
};

var response = await client.Extra.ProcessSingleImageAsync(request);
ImageHelper.Base64ToImage(response.Image, "upscaled_4x.png");
Console.WriteLine("✓ Изображение увеличено в 4 раза");
```

### Апскейл до конкретного размера

```csharp
var imageBase64 = ImageHelper.ImageToBase64("small.png");

var request = new ExtraSingleImageRequest
{
    Image = imageBase64,
    ResizeMode = 1,              // До конкретного размера
    UpscalingResizeW = 1920,     // Целевая ширина
    UpscalingResizeH = 1080,     // Целевая высота
    UpscalingCrop = false,       // Не обрезать
    Upscaler1 = "Lanczos"
};

var response = await client.Extra.ProcessSingleImageAsync(request);
ImageHelper.Base64ToImage(response.Image, "resized_1920x1080.png");
```

### Восстановление лиц

```csharp
var imageBase64 = ImageHelper.ImageToBase64("photo.png");

var request = new ExtraSingleImageRequest
{
    Image = imageBase64,
    ResizeMode = 0,
    CodeformerVisibility = 1,    // Сила CodeFormer (0-1)
    CodeformerWeight = 0.5       // Баланс между оригиналом и восстановлением
};

var response = await client.Extra.ProcessSingleImageAsync(request);
ImageHelper.Base64ToImage(response.Image, "faces_restored.png");
```

### Комбинированная обработка

```csharp
var imageBase64 = ImageHelper.ImageToBase64("old_photo.png");

var request = new ExtraSingleImageRequest
{
    Image = imageBase64,
    
    // Апскейл
    ResizeMode = 0,
    UpscalingResize = 2,
    Upscaler1 = "R-ESRGAN 4x+",
    
    // Восстановление лиц
    CodeformerVisibility = 1,
    CodeformerWeight = 0.7,
    
    // Порядок: сначала апскейл, потом восстановление лиц
    UpscaleFirst = true
};

var response = await client.Extra.ProcessSingleImageAsync(request);
ImageHelper.Base64ToImage(response.Image, "enhanced.png");
Console.WriteLine("✓ Изображение улучшено");
```

### Использование двух апскейлеров

```csharp
var imageBase64 = ImageHelper.ImageToBase64("input.png");

var request = new ExtraSingleImageRequest
{
    Image = imageBase64,
    ResizeMode = 0,
    UpscalingResize = 4,
    
    // Первый апскейлер
    Upscaler1 = "R-ESRGAN 4x+",
    
    // Второй апскейлер (смешивается с первым)
    Upscaler2 = "R-ESRGAN 4x+ Anime6B",
    ExtrasUpscaler2Visibility = 0.5  // 50% второго апскейлера
};

var response = await client.Extra.ProcessSingleImageAsync(request);
ImageHelper.Base64ToImage(response.Image, "mixed_upscale.png");
```

### Батч-обработка изображений

```csharp
var inputFiles = Directory.GetFiles("input_folder", "*.png");

foreach (var file in inputFiles)
{
    var imageBase64 = ImageHelper.ImageToBase64(file);
    
    var request = new ExtraSingleImageRequest
    {
        Image = imageBase64,
        ResizeMode = 0,
        UpscalingResize = 2,
        Upscaler1 = "Lanczos"
    };
    
    var response = await client.Extra.ProcessSingleImageAsync(request);
    
    var outputPath = Path.Combine("output_folder", Path.GetFileName(file));
    ImageHelper.Base64ToImage(response.Image, outputPath);
    
    Console.WriteLine($"✓ Обработано: {file}");
}
```

---

## Embeddings и LoRA

### Использование embeddings в промптах

```csharp
// Сначала получаем список доступных embeddings
var embeddings = await client.Embeddings.GetEmbeddingsAsync();

if (embeddings.ContainsKey("my_style"))
{
    var request = new TextToImageRequest
    {
        // Используем embedding в промпте
        Prompt = "a beautiful portrait, <my_style>",
        Steps = 30
    };
    
    var response = await client.TextToImage.GenerateAsync(request);
    ImageHelper.Base64ToImage(response.Images[0], "with_embedding.png");
}
```

### Использование LoRA в промптах

```csharp
// Получаем список LoRA
var loras = await client.Loras.GetLorasAsync();

var request = new TextToImageRequest
{
    // Синтаксис: <lora:name:weight>
    // weight обычно от 0.0 до 1.0
    Prompt = "beautiful landscape, <lora:fantasy_style:0.8>",
    NegativePrompt = "low quality",
    Steps = 30
};

var response = await client.TextToImage.GenerateAsync(request);
ImageHelper.Base64ToImage(response.Images[0], "with_lora.png");
```

### Комбинирование нескольких LoRA

```csharp
var request = new TextToImageRequest
{
    // Можно использовать несколько LoRA одновременно
    Prompt = "anime girl, <lora:anime_style:0.7>, <lora:detailed_eyes:0.5>",
    Steps = 30
};

var response = await client.TextToImage.GenerateAsync(request);
```

### Тестирование разных весов LoRA

```csharp
var loraName = "style_lora";
var weights = new[] { 0.2, 0.5, 0.8, 1.0 };

foreach (var weight in weights)
{
    var request = new TextToImageRequest
    {
        Prompt = $"portrait, <lora:{loraName}:{weight:F1}>",
        Steps = 20,
        Seed = 12345  // Одинаковый seed для сравнения
    };
    
    var response = await client.TextToImage.GenerateAsync(request);
    ImageHelper.Base64ToImage(response.Images[0], $"lora_weight_{weight:F1}.png");
    Console.WriteLine($"Сгенерировано с весом {weight:F1}");
}
```

---

## Обработка ошибок

### Базовая обработка ошибок

```csharp
using StableDiffusionNet.Exceptions;

try
{
    var response = await client.TextToImage.GenerateAsync(request);
}
catch (ApiException ex)
{
    Console.WriteLine($"Ошибка API: {ex.Message}");
    Console.WriteLine($"Код ответа: {ex.StatusCode}");
    Console.WriteLine($"Тело ответа: {ex.ResponseBody}");
}
catch (ConfigurationException ex)
{
    Console.WriteLine($"Ошибка конфигурации: {ex.Message}");
}
catch (StableDiffusionException ex)
{
    Console.WriteLine($"Ошибка StableDiffusion: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Неожиданная ошибка: {ex.Message}");
}
```

### Retry с обработкой ошибок

```csharp
async Task<TextToImageResponse?> GenerateWithRetry(
    TextToImageRequest request, 
    int maxAttempts = 3)
{
    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            Console.WriteLine($"Попытка {attempt}/{maxAttempts}...");
            return await client.TextToImage.GenerateAsync(request);
        }
        catch (ApiException ex) when (ex.StatusCode == 503)
        {
            // Сервер занят
            if (attempt < maxAttempts)
            {
                Console.WriteLine("Сервер занят, ожидание...");
                await Task.Delay(5000);
                continue;
            }
            Console.WriteLine("Сервер недоступен после всех попыток");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            if (attempt == maxAttempts)
                throw;
        }
    }
    
    return null;
}

// Использование
var response = await GenerateWithRetry(request);
if (response != null)
{
    ImageHelper.Base64ToImage(response.Images[0], "output.png");
}
```

### Проверка доступности перед запросом

```csharp
async Task<bool> WaitForApi(int timeoutSeconds = 60)
{
    var stopwatch = Stopwatch.StartNew();
    
    while (stopwatch.Elapsed.TotalSeconds < timeoutSeconds)
    {
        if (await client.PingAsync())
        {
            Console.WriteLine("✓ API доступен");
            return true;
        }
        
        Console.Write(".");
        await Task.Delay(1000);
    }
    
    Console.WriteLine("\n× API недоступен");
    return false;
}

// Использование
if (await WaitForApi())
{
    var response = await client.TextToImage.GenerateAsync(request);
}
```

### Логирование запросов

```csharp
async Task<TextToImageResponse> GenerateWithLogging(TextToImageRequest request)
{
    var sw = Stopwatch.StartNew();
    
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Запуск генерации");
    Console.WriteLine($"  Промпт: {request.Prompt}");
    Console.WriteLine($"  Размер: {request.Width}x{request.Height}");
    Console.WriteLine($"  Шаги: {request.Steps}");
    
    try
    {
        var response = await client.TextToImage.GenerateAsync(request);
        
        sw.Stop();
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✓ Генерация завершена за {sw.Elapsed.TotalSeconds:F1}с");
        Console.WriteLine($"  Сгенерировано изображений: {response.Images.Count}");
        
        return response;
    }
    catch (Exception ex)
    {
        sw.Stop();
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] × Ошибка после {sw.Elapsed.TotalSeconds:F1}с");
        Console.WriteLine($"  {ex.Message}");
        throw;
    }
}
```

---

## Дополнительные примеры

Для продвинутых сценариев использования смотрите [ADVANCED.md](ADVANCED.md).

Полный справочник по API методам: [API_REFERENCE.md](API_REFERENCE.md).

