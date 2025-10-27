# Справочник API методов StableDiffusionNet

**[English](API_REFERENCE.md) | Русский**

> Полный список реализованных методов Stable Diffusion WebUI API

## Оглавление

- [Обзор](#обзор)
- [Text to Image (txt2img)](#text-to-image-txt2img)
- [Image to Image (img2img)](#image-to-image-img2img)
- [Models](#models)
- [Progress](#progress)
- [Options](#options)
- [Samplers](#samplers)
- [Schedulers](#schedulers)
- [Upscalers](#upscalers)
- [PNG Info](#png-info)
- [Extra (Постобработка)](#extra-постобработка)
- [Embeddings](#embeddings)
- [LoRA](#lora)
- [Ping](#ping)

---

## Обзор

StableDiffusionNet реализует следующие группы методов Stable Diffusion WebUI API:

| Сервис | Описание | Методы |
|--------|----------|---------|
| **TextToImage** | Генерация изображений из текста | 1 метод |
| **ImageToImage** | Генерация изображений из изображений | 1 метод |
| **Models** | Управление моделями | 4 метода |
| **Progress** | Отслеживание прогресса генерации | 3 метода |
| **Options** | Управление настройками WebUI | 2 метода |
| **Samplers** | Информация о сэмплерах | 1 метод |
| **Schedulers** | Информация о планировщиках | 1 метод |
| **Upscalers** | Информация об апскейлерах | 2 метода |
| **PngInfo** | Извлечение метаданных из PNG | 1 метод |
| **Extra** | Постобработка изображений | 1 метод |
| **Embeddings** | Работа с текстовыми инверсиями | 2 метода |
| **Loras** | Работа с LoRA моделями | 2 метода |
| **Client** | Общие методы клиента | 1 метод |

**Всего**: 22 метода API

---

## Text to Image (txt2img)

### `GenerateAsync`

Генерирует изображения из текстового описания.

**Сигнатура:**
```csharp
Task<TextToImageResponse> GenerateAsync(
    TextToImageRequest request,
    CancellationToken cancellationToken = default
)
```

**Эндпоинт**: `POST /sdapi/v1/txt2img`

**Основные параметры:**
- `Prompt` - текстовое описание изображения
- `NegativePrompt` - что не должно быть на изображении
- `Width`, `Height` - размеры изображения
- `Steps` - количество шагов генерации
- `CfgScale` - степень следования промпту
- `SamplerName` - используемый сэмплер
- `Seed` - seed для воспроизводимости
- `BatchSize` - количество изображений за раз
- `NIter` - количество батчей

**Расширенные параметры:**
- `EnableHr` - включить Hires.fix
- `HrScale` - масштаб для Hires.fix
- `HrUpscaler` - апскейлер для Hires.fix
- `DenoisingStrength` - сила шумоподавления
- `RestoreFaces` - восстановление лиц
- `Tiling` - генерация tileable текстур
- `OverrideSettings` - переопределение настроек для этого запроса

**Ответ:**
```csharp
public class TextToImageResponse
{
    public List<string> Images { get; set; }     // Base64 изображения
    public string Info { get; set; }              // JSON с информацией о генерации
    public Dictionary<string, object> Parameters { get; set; }
}
```

---

## Image to Image (img2img)

### `GenerateAsync`

Генерирует изображение на основе существующего изображения.

**Сигнатура:**
```csharp
Task<ImageToImageResponse> GenerateAsync(
    ImageToImageRequest request,
    CancellationToken cancellationToken = default
)
```

**Эндпоинт**: `POST /sdapi/v1/img2img`

**Основные параметры:**
- `InitImages` - исходные изображения в base64
- `Mask` - маска для inpainting (опционально)
- `DenoisingStrength` - сила изменения исходного изображения (0.0-1.0)
- Все параметры из `TextToImageRequest`

**Параметры для Inpainting:**
- `InpaintingFill` - чем заполнять маску (0: fill, 1: original, 2: latent noise, 3: latent nothing)
- `InpaintFullRes` - рисовать только область маски
- `InpaintFullResPadding` - отступ при `InpaintFullRes`
- `InpaintingMaskInvert` - инвертировать маску
- `MaskBlur` - размытие маски

**Параметры для изменения размера:**
- `ResizeMode` - режим изменения размера:
  - 0: Just resize (просто изменить размер)
  - 1: Crop and resize (обрезать и изменить размер)
  - 2: Resize and fill (изменить размер и заполнить)
  - 3: Just resize (latent upscale)

**Ответ:**
```csharp
public class ImageToImageResponse
{
    public List<string> Images { get; set; }     // Base64 изображения
    public string Info { get; set; }              // JSON с информацией о генерации
    public Dictionary<string, object> Parameters { get; set; }
}
```

---

## Models

Сервис для управления моделями Stable Diffusion.

### `GetModelsAsync`

Получает список всех доступных моделей.

**Сигнатура:**
```csharp
Task<IReadOnlyList<SdModel>> GetModelsAsync(
    CancellationToken cancellationToken = default
)
```

**Эндпоинт**: `GET /sdapi/v1/sd-models`

**Ответ:**
```csharp
public class SdModel
{
    public string Title { get; set; }          // Имя модели
    public string ModelName { get; set; }      // Полное имя файла
    public string Hash { get; set; }           // SHA256 хеш модели
    public string Sha256 { get; set; }         // SHA256 хеш (полный)
    public string Filename { get; set; }       // Путь к файлу
    public string Config { get; set; }         // Путь к конфигу
}
```

### `GetCurrentModelAsync`

Получает название текущей активной модели.

**Сигнатура:**
```csharp
Task<string> GetCurrentModelAsync(
    CancellationToken cancellationToken = default
)
```

**Эндпоинт**: `GET /sdapi/v1/options` (извлекает `sd_model_checkpoint`)

**Ответ:** Строка с названием модели (например, "sd_xl_base_1.0.safetensors")

### `SetModelAsync`

Устанавливает активную модель.

**Сигнатура:**
```csharp
Task SetModelAsync(
    string modelName,
    CancellationToken cancellationToken = default
)
```

**Эндпоинт**: `POST /sdapi/v1/options`

**Параметры:**
- `modelName` - название модели для активации

**Примечание:** Смена модели может занять время, так как модель загружается в память.

### `RefreshModelsAsync`

Обновляет список моделей (сканирует папки с моделями заново).

**Сигнатура:**
```csharp
Task RefreshModelsAsync(
    CancellationToken cancellationToken = default
)
```

**Эндпоинт**: `POST /sdapi/v1/refresh-checkpoints`

**Применение:** После добавления новых моделей в папку без перезапуска WebUI.

---

## Progress

Сервис для отслеживания прогресса генерации.

### `GetProgressAsync`

Получает текущий прогресс генерации.

**Сигнатура:**
```csharp
Task<GenerationProgress> GetProgressAsync(
    CancellationToken cancellationToken = default
)
```

**Эндпоинт**: `GET /sdapi/v1/progress`

**Ответ:**
```csharp
public class GenerationProgress
{
    public double Progress { get; set; }        // Прогресс 0.0-1.0
    public double EtaRelative { get; set; }     // ETA в секундах
    public ProgressState State { get; set; }    // Детали прогресса
    public string CurrentImage { get; set; }    // Превью (если включено)
}

public class ProgressState
{
    public int SamplingStep { get; set; }       // Текущий шаг
    public int SamplingSteps { get; set; }      // Всего шагов
    public bool Skipped { get; set; }           // Пропущено
    public bool Interrupted { get; set; }       // Прервано
}
```

**Применение:** Показ progress bar'а во время генерации.

### `InterruptAsync`

Прерывает текущую генерацию.

**Сигнатура:**
```csharp
Task InterruptAsync(
    CancellationToken cancellationToken = default
)
```

**Эндпоинт**: `POST /sdapi/v1/interrupt`

**Применение:** Остановка долгой генерации пользователем.

### `SkipAsync`

Пропускает текущее изображение при батч-генерации.

**Сигнатура:**
```csharp
Task SkipAsync(
    CancellationToken cancellationToken = default
)
```

**Эндпоинт**: `POST /sdapi/v1/skip`

**Применение:** Пропустить неудачное изображение в батче и перейти к следующему.

---

## Options

Сервис для управления настройками WebUI.

### `GetOptionsAsync`

Получает все текущие настройки WebUI.

**Сигнатура:**
```csharp
Task<WebUIOptions> GetOptionsAsync(
    CancellationToken cancellationToken = default
)
```

**Эндпоинт**: `GET /sdapi/v1/options`

**Ответ:** Объект `WebUIOptions` с сотнями настроек WebUI.

**Основные настройки:**
- `SdModelCheckpoint` - текущая модель
- `ClipStopAtLastLayers` - CLIP skip
- `EnableXformers` - использование xFormers
- `EtaDdim`, `EtaAncestral` - параметры eta для сэмплеров
- `SamplesFormat` - формат сохранения (png, jpg, webp)
- `SamplesSave` - сохранять ли изображения
- И многое другое...

### `SetOptionsAsync`

Устанавливает настройки WebUI.

**Сигнатура:**
```csharp
Task SetOptionsAsync(
    WebUIOptions options,
    CancellationToken cancellationToken = default
)
```

**Эндпоинт**: `POST /sdapi/v1/options`

**Параметры:**
- `options` - объект с настройками (можно передать частичные изменения)

**Применение:** Изменение глобальных настроек WebUI программно.

---

## Samplers

Сервис для получения информации о сэмплерах.

### `GetSamplersAsync`

Получает список доступных сэмплеров с полной информацией.

**Сигнатура:**
```csharp
Task<IReadOnlyList<Sampler>> GetSamplersAsync(
    CancellationToken cancellationToken = default
)
```

**Эндпоинт**: `GET /sdapi/v1/samplers`

**Ответ:** Список объектов `Sampler` с полями:
- `Name` - название сэмплера
- `Aliases` - список альтернативных имён
- `Options` - словарь дополнительных опций сэмплера

**Примеры сэмплеров:** Euler a, Euler, DPM++ 2M Karras, LMS Karras и другие.

**Применение:** Динамическое построение UI с выбором сэмплера.

---

## Schedulers

Сервис для получения информации о планировщиках (schedulers).

### `GetSchedulersAsync`

Получает список доступных планировщиков шагов с полной информацией.

**Сигнатура:**
```csharp
Task<IReadOnlyList<Scheduler>> GetSchedulersAsync(
    CancellationToken cancellationToken = default
)
```

**Эндпоинт**: `GET /sdapi/v1/schedulers`

**Ответ:** Список объектов `Scheduler` с полями:
- `Name` - внутреннее имя планировщика
- `Label` - отображаемое имя планировщика
- `Aliases` - список альтернативных имён (может быть null)
- `DefaultRho` - значение rho по умолчанию
- `NeedInnerModel` - требуется ли внутренняя модель

**Примеры планировщиков:** Automatic, Karras, Exponential, Normal, Simple, Beta и другие.

**Применение:** Выбор scheduler'а для более тонкой настройки генерации.

---

## Upscalers

Сервис для получения информации об апскейлерах.

### `GetUpscalersAsync`

Получает список доступных апскейлеров.

**Сигнатура:**
```csharp
Task<IReadOnlyList<Upscaler>> GetUpscalersAsync(
    CancellationToken cancellationToken = default
)
```

**Эндпоинт**: `GET /sdapi/v1/upscalers`

**Ответ:**
```csharp
public class Upscaler
{
    public string Name { get; set; }           // Название апскейлера
    public string Model { get; set; }          // Модель (если есть)
    public string ModelPath { get; set; }      // Путь к модели (если есть)
    public int Scale { get; set; }             // Масштаб (например, 4x)
}
```

**Примеры апскейлеров:**
- ESRGAN_4x
- Lanczos
- Nearest
- RealESRGAN_x4plus
- ScuNET
- SwinIR_4x

### `GetLatentUpscaleModesAsync`

Получает список доступных режимов latent upscale.

**Сигнатура:**
```csharp
Task<IReadOnlyList<LatentUpscaleMode>> GetLatentUpscaleModesAsync(
    CancellationToken cancellationToken = default
)
```

**Эндпоинт**: `GET /sdapi/v1/latent-upscale-modes`

**Ответ:** Список объектов `LatentUpscaleMode` с полем `Name`, содержащим название режима.

**Примеры режимов:** Latent, Latent (antialiased), Latent (bicubic), Latent (bicubic antialiased), Latent (nearest), Latent (nearest-exact).

**Применение:** Используется для Hires.fix с latent upscale.

---

## PNG Info

Сервис для извлечения метаданных генерации из PNG изображений.

### `GetPngInfoAsync`

Извлекает параметры генерации из PNG изображения.

**Сигнатура:**
```csharp
Task<PngInfoResponse> GetPngInfoAsync(
    PngInfoRequest request,
    CancellationToken cancellationToken = default
)
```

**Эндпоинт**: `POST /sdapi/v1/png-info`

**Параметры:**
```csharp
public class PngInfoRequest
{
    public string Image { get; set; }  // PNG изображение в base64
}
```

**Ответ:**
```csharp
public class PngInfoResponse
{
    public string Info { get; set; }   // Текстовая информация о генерации
    public Dictionary<string, object> Items { get; set; } // Распарсенные параметры
}
```

**Применение:** 
- Извлечение промпта из сгенерированного изображения
- Копирование параметров генерации
- Анализ чужих изображений

---

## Extra (Постобработка)

Сервис для постобработки изображений (апскейл, восстановление лиц).

### `ProcessSingleImageAsync`

Выполняет постобработку одного изображения.

**Сигнатура:**
```csharp
Task<ExtraSingleImageResponse> ProcessSingleImageAsync(
    ExtraSingleImageRequest request,
    CancellationToken cancellationToken = default
)
```

**Эндпоинт**: `POST /sdapi/v1/extra-single-image`

**Параметры:**
```csharp
public class ExtraSingleImageRequest
{
    public string Image { get; set; }              // Изображение в base64
    public int ResizeMode { get; set; }            // Режим изменения размера
    public bool ShowExtrasResults { get; set; }    // Показывать результаты
    public int GfpganVisibility { get; set; }      // Сила GFPGAN (0-1)
    public int CodeformerVisibility { get; set; }  // Сила CodeFormer (0-1)
    public double CodeformerWeight { get; set; }   // Вес CodeFormer
    public int UpscalingResize { get; set; }       // Во сколько раз увеличить
    public double UpscalingResizeW { get; set; }   // Целевая ширина
    public double UpscalingResizeH { get; set; }   // Целевая высота
    public bool UpscalingCrop { get; set; }        // Обрезать до целевого размера
    public string Upscaler1 { get; set; }          // Первый апскейлер
    public string Upscaler2 { get; set; }          // Второй апскейлер
    public double ExtrasUpscaler2Visibility { get; set; } // Видимость второго апскейлера
    public bool UpscaleFirst { get; set; }         // Сначала апскейл, потом face restoration
}
```

**Ответ:**
```csharp
public class ExtraSingleImageResponse
{
    public string Image { get; set; }   // Обработанное изображение в base64
    public string HtmlInfo { get; set; } // HTML информация о процессе
}
```

**Применение:**
- Увеличение разрешения готовых изображений
- Восстановление лиц на изображениях
- Улучшение качества старых изображений

---

## Embeddings

Сервис для работы с embeddings (textual inversions).

### `GetEmbeddingsAsync`

Получает список всех доступных embeddings.

**Сигнатура:**
```csharp
Task<IReadOnlyDictionary<string, Embedding>> GetEmbeddingsAsync(
    CancellationToken cancellationToken = default
)
```

**Эндпоинт**: `GET /sdapi/v1/embeddings`

**Ответ:**
```csharp
public class Embedding
{
    public int Step { get; set; }              // Номер шага обучения
    public string SdCheckpoint { get; set; }   // Чекпоинт, на котором обучено
    public string SdCheckpointName { get; set; } // Имя чекпоинта
    public int Shape { get; set; }             // Размерность эмбеддинга
    public int Vectors { get; set; }           // Количество векторов
}
```

**Применение:** 
- Получение списка доступных текстовых инверсий
- Использование в промптах через синтаксис `<embedding-name>`

### `RefreshEmbeddingsAsync`

Обновляет список embeddings (сканирует папку заново).

**Сигнатура:**
```csharp
Task RefreshEmbeddingsAsync(
    CancellationToken cancellationToken = default
)
```

**Эндпоинт**: `POST /sdapi/v1/refresh-embeddings`

**Применение:** После добавления новых embeddings без перезапуска WebUI.

---

## LoRA

Сервис для работы с LoRA моделями.

### `GetLorasAsync`

Получает список всех доступных LoRA моделей.

**Сигнатура:**
```csharp
Task<IReadOnlyList<Lora>> GetLorasAsync(
    CancellationToken cancellationToken = default
)
```

**Эндпоинт**: `GET /sdapi/v1/loras`

**Ответ:**
```csharp
public class Lora
{
    public string Name { get; set; }           // Имя LoRA
    public string Alias { get; set; }          // Алиас
    public string Path { get; set; }           // Путь к файлу
    public Dictionary<string, object> Metadata { get; set; } // Метаданные
}
```

**Применение:**
- Получение списка доступных LoRA
- Использование в промптах через синтаксис `<lora:name:weight>`

### `RefreshLorasAsync`

Обновляет список LoRA моделей (сканирует папку заново).

**Сигнатура:**
```csharp
Task RefreshLorasAsync(
    CancellationToken cancellationToken = default
)
```

**Эндпоинт**: `POST /sdapi/v1/refresh-loras`

**Применение:** После добавления новых LoRA без перезапуска WebUI.

---

## Ping

Общий метод клиента для проверки доступности API.

### `PingAsync`

Проверяет доступность API.

**Сигнатура:**
```csharp
Task<bool> PingAsync(
    CancellationToken cancellationToken = default
)
```

**Эндпоинт**: Выполняет запрос к базовому URL

**Ответ:** `true` если API доступен, `false` в противном случае.

**Применение:**
- Проверка доступности перед началом работы
- Health check для мониторинга
- Ожидание запуска WebUI

---

## Дополнительная информация

### Формат изображений

Все изображения передаются в формате base64. Для конвертации используйте класс `ImageHelper`:

```csharp
// Изображение в base64
var base64 = ImageHelper.ImageToBase64("input.png");

// Base64 в изображение
ImageHelper.Base64ToImage(base64String, "output.png");
```

### Обработка ошибок

Все методы могут выбросить следующие исключения:
- `ApiException` - ошибки API с кодом ответа и телом
- `ConfigurationException` - ошибки конфигурации
- `StableDiffusionException` - базовое исключение

### CancellationToken

Все асинхронные методы поддерживают `CancellationToken` для отмены операций.

### Retry Policy

Библиотека автоматически повторяет запросы при транзитных ошибках:
- HTTP 500, 502, 503, 504
- HTTP 429 (Too Many Requests)
- Ошибки сети и таймауты

---

## Примеры

Подробные примеры использования всех методов API смотрите в [EXAMPLES.ru.md](EXAMPLES.ru.md).

Продвинутые сценарии использования описаны в [ADVANCED.ru.md](ADVANCED.ru.md).

