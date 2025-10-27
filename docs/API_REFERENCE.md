# StableDiffusionNet API Methods Reference

**English | [Русский](API_REFERENCE.ru.md)**

> Complete list of implemented Stable Diffusion WebUI API methods

## Table of Contents

- [Overview](#overview)
- [Text to Image (txt2img)](#text-to-image-txt2img)
- [Image to Image (img2img)](#image-to-image-img2img)
- [Models](#models)
- [Progress](#progress)
- [Options](#options)
- [Samplers](#samplers)
- [Schedulers](#schedulers)
- [Upscalers](#upscalers)
- [PNG Info](#png-info)
- [Extra (Post-processing)](#extra-post-processing)
- [Embeddings](#embeddings)
- [LoRA](#lora)
- [Ping](#ping)

---

## Overview

StableDiffusionNet implements the following groups of Stable Diffusion WebUI API methods:

| Service | Description | Methods |
|--------|----------|---------|
| **TextToImage** | Generate images from text | 1 method |
| **ImageToImage** | Generate images from images | 1 method |
| **Models** | Model management | 4 methods |
| **Progress** | Track generation progress | 3 methods |
| **Options** | Manage WebUI settings | 2 methods |
| **Samplers** | Sampler information | 1 method |
| **Schedulers** | Scheduler information | 1 method |
| **Upscalers** | Upscaler information | 2 methods |
| **PngInfo** | Extract metadata from PNG | 1 method |
| **Extra** | Post-process images | 1 method |
| **Embeddings** | Work with textual inversions | 2 methods |
| **Loras** | Work with LoRA models | 2 methods |
| **Client** | General client methods | 1 method |

**Total**: 22 API methods

---

## Text to Image (txt2img)

### `GenerateAsync`

Generates images from text description.

**Signature:**
```csharp
Task<TextToImageResponse> GenerateAsync(
    TextToImageRequest request,
    CancellationToken cancellationToken = default
)
```

**Endpoint**: `POST /sdapi/v1/txt2img`

**Main Parameters:**
- `Prompt` - text description of the image
- `NegativePrompt` - what should not be in the image
- `Width`, `Height` - image dimensions
- `Steps` - number of generation steps
- `CfgScale` - how closely to follow the prompt
- `SamplerName` - sampler to use
- `Seed` - seed for reproducibility
- `BatchSize` - number of images at once
- `NIter` - number of batches

**Advanced Parameters:**
- `EnableHr` - enable Hires.fix
- `HrScale` - scale for Hires.fix
- `HrUpscaler` - upscaler for Hires.fix
- `DenoisingStrength` - denoising strength
- `RestoreFaces` - face restoration
- `Tiling` - generate tileable textures
- `OverrideSettings` - override settings for this request

**Response:**
```csharp
public class TextToImageResponse
{
    public List<string> Images { get; set; }     // Base64 images
    public string Info { get; set; }              // JSON with generation info
    public Dictionary<string, object> Parameters { get; set; }
}
```

---

## Image to Image (img2img)

### `GenerateAsync`

Generates an image based on an existing image.

**Signature:**
```csharp
Task<ImageToImageResponse> GenerateAsync(
    ImageToImageRequest request,
    CancellationToken cancellationToken = default
)
```

**Endpoint**: `POST /sdapi/v1/img2img`

**Main Parameters:**
- `InitImages` - source images in base64
- `Mask` - mask for inpainting (optional)
- `DenoisingStrength` - strength of change to source image (0.0-1.0)
- All parameters from `TextToImageRequest`

**Inpainting Parameters:**
- `InpaintingFill` - what to fill mask with (0: fill, 1: original, 2: latent noise, 3: latent nothing)
- `InpaintFullRes` - paint only mask area
- `InpaintFullResPadding` - padding when `InpaintFullRes`
- `InpaintingMaskInvert` - invert mask
- `MaskBlur` - mask blur

**Resize Parameters:**
- `ResizeMode` - resize mode:
  - 0: Just resize
  - 1: Crop and resize
  - 2: Resize and fill
  - 3: Just resize (latent upscale)

**Response:**
```csharp
public class ImageToImageResponse
{
    public List<string> Images { get; set; }     // Base64 images
    public string Info { get; set; }              // JSON with generation info
    public Dictionary<string, object> Parameters { get; set; }
}
```

---

## Models

Service for managing Stable Diffusion models.

### `GetModelsAsync`

Gets a list of all available models.

**Signature:**
```csharp
Task<IReadOnlyList<SdModel>> GetModelsAsync(
    CancellationToken cancellationToken = default
)
```

**Endpoint**: `GET /sdapi/v1/sd-models`

**Response:**
```csharp
public class SdModel
{
    public string Title { get; set; }          // Model name
    public string ModelName { get; set; }      // Full file name
    public string Hash { get; set; }           // SHA256 hash of model
    public string Sha256 { get; set; }         // SHA256 hash (full)
    public string Filename { get; set; }       // File path
    public string Config { get; set; }         // Config path
}
```

### `GetCurrentModelAsync`

Gets the name of the current active model.

**Signature:**
```csharp
Task<string> GetCurrentModelAsync(
    CancellationToken cancellationToken = default
)
```

**Endpoint**: `GET /sdapi/v1/options` (extracts `sd_model_checkpoint`)

**Response:** String with model name (e.g., "sd_xl_base_1.0.safetensors")

### `SetModelAsync`

Sets the active model.

**Signature:**
```csharp
Task SetModelAsync(
    string modelName,
    CancellationToken cancellationToken = default
)
```

**Endpoint**: `POST /sdapi/v1/options`

**Parameters:**
- `modelName` - name of model to activate

**Note:** Model switching can take time as the model loads into memory.

### `RefreshModelsAsync`

Refreshes the model list (rescans model folders).

**Signature:**
```csharp
Task RefreshModelsAsync(
    CancellationToken cancellationToken = default
)
```

**Endpoint**: `POST /sdapi/v1/refresh-checkpoints`

**Use Case:** After adding new models to folder without restarting WebUI.

---

## Progress

Service for tracking generation progress.

### `GetProgressAsync`

Gets the current generation progress.

**Signature:**
```csharp
Task<GenerationProgress> GetProgressAsync(
    CancellationToken cancellationToken = default
)
```

**Endpoint**: `GET /sdapi/v1/progress`

**Response:**
```csharp
public class GenerationProgress
{
    public double Progress { get; set; }        // Progress 0.0-1.0
    public double EtaRelative { get; set; }     // ETA in seconds
    public ProgressState State { get; set; }    // Progress details
    public string CurrentImage { get; set; }    // Preview (if enabled)
}

public class ProgressState
{
    public int SamplingStep { get; set; }       // Current step
    public int SamplingSteps { get; set; }      // Total steps
    public bool Skipped { get; set; }           // Skipped
    public bool Interrupted { get; set; }       // Interrupted
}
```

**Use Case:** Display progress bar during generation.

### `InterruptAsync`

Interrupts current generation.

**Signature:**
```csharp
Task InterruptAsync(
    CancellationToken cancellationToken = default
)
```

**Endpoint**: `POST /sdapi/v1/interrupt`

**Use Case:** Stop long generation by user.

### `SkipAsync`

Skips current image in batch generation.

**Signature:**
```csharp
Task SkipAsync(
    CancellationToken cancellationToken = default
)
```

**Endpoint**: `POST /sdapi/v1/skip`

**Use Case:** Skip unsuccessful image in batch and move to next.

---

## Options

Service for managing WebUI settings.

### `GetOptionsAsync`

Gets all current WebUI settings.

**Signature:**
```csharp
Task<WebUIOptions> GetOptionsAsync(
    CancellationToken cancellationToken = default
)
```

**Endpoint**: `GET /sdapi/v1/options`

**Response:** `WebUIOptions` object with hundreds of WebUI settings.

**Main Settings:**
- `SdModelCheckpoint` - current model
- `ClipStopAtLastLayers` - CLIP skip
- `EnableXformers` - use xFormers
- `EtaDdim`, `EtaAncestral` - eta parameters for samplers
- `SamplesFormat` - save format (png, jpg, webp)
- `SamplesSave` - whether to save images
- And much more...

### `SetOptionsAsync`

Sets WebUI settings.

**Signature:**
```csharp
Task SetOptionsAsync(
    WebUIOptions options,
    CancellationToken cancellationToken = default
)
```

**Endpoint**: `POST /sdapi/v1/options`

**Parameters:**
- `options` - object with settings (partial changes allowed)

**Use Case:** Programmatically change global WebUI settings.

---

## Samplers

Service for getting sampler information.

### `GetSamplersAsync`

Gets a list of available samplers with full information.

**Signature:**
```csharp
Task<IReadOnlyList<Sampler>> GetSamplersAsync(
    CancellationToken cancellationToken = default
)
```

**Endpoint**: `GET /sdapi/v1/samplers`

**Response:** List of `Sampler` objects with fields:
- `Name` - sampler name
- `Aliases` - list of alternative names
- `Options` - dictionary of additional sampler options

**Sampler Examples:** Euler a, Euler, DPM++ 2M Karras, LMS Karras, and others.

**Use Case:** Dynamic UI building with sampler selection.

---

## Schedulers

Service for getting scheduler information.

### `GetSchedulersAsync`

Gets a list of available step schedulers with full information.

**Signature:**
```csharp
Task<IReadOnlyList<Scheduler>> GetSchedulersAsync(
    CancellationToken cancellationToken = default
)
```

**Endpoint**: `GET /sdapi/v1/schedulers`

**Response:** List of `Scheduler` objects with fields:
- `Name` - internal scheduler name
- `Label` - display scheduler name
- `Aliases` - list of alternative names (may be null)
- `DefaultRho` - default rho value
- `NeedInnerModel` - whether inner model is required

**Scheduler Examples:** Automatic, Karras, Exponential, Normal, Simple, Beta, and others.

**Use Case:** Select scheduler for finer generation tuning.

---

## Upscalers

Service for getting upscaler information.

### `GetUpscalersAsync`

Gets a list of available upscalers.

**Signature:**
```csharp
Task<IReadOnlyList<Upscaler>> GetUpscalersAsync(
    CancellationToken cancellationToken = default
)
```

**Endpoint**: `GET /sdapi/v1/upscalers`

**Response:**
```csharp
public class Upscaler
{
    public string Name { get; set; }           // Upscaler name
    public string Model { get; set; }          // Model (if any)
    public string ModelPath { get; set; }      // Model path (if any)
    public int Scale { get; set; }             // Scale (e.g., 4x)
}
```

**Upscaler Examples:**
- ESRGAN_4x
- Lanczos
- Nearest
- RealESRGAN_x4plus
- ScuNET
- SwinIR_4x

### `GetLatentUpscaleModesAsync`

Gets a list of available latent upscale modes.

**Signature:**
```csharp
Task<IReadOnlyList<LatentUpscaleMode>> GetLatentUpscaleModesAsync(
    CancellationToken cancellationToken = default
)
```

**Endpoint**: `GET /sdapi/v1/latent-upscale-modes`

**Response:** List of `LatentUpscaleMode` objects with `Name` field containing mode name.

**Mode Examples:** Latent, Latent (antialiased), Latent (bicubic), Latent (bicubic antialiased), Latent (nearest), Latent (nearest-exact).

**Use Case:** Used for Hires.fix with latent upscale.

---

## PNG Info

Service for extracting generation metadata from PNG images.

### `GetPngInfoAsync`

Extracts generation parameters from PNG image.

**Signature:**
```csharp
Task<PngInfoResponse> GetPngInfoAsync(
    PngInfoRequest request,
    CancellationToken cancellationToken = default
)
```

**Endpoint**: `POST /sdapi/v1/png-info`

**Parameters:**
```csharp
public class PngInfoRequest
{
    public string Image { get; set; }  // PNG image in base64
}
```

**Response:**
```csharp
public class PngInfoResponse
{
    public string Info { get; set; }   // Text information about generation
    public Dictionary<string, object> Items { get; set; } // Parsed parameters
}
```

**Use Cases:** 
- Extract prompt from generated image
- Copy generation parameters
- Analyze others' images

---

## Extra (Post-processing)

Service for image post-processing (upscale, face restoration).

### `ProcessSingleImageAsync`

Performs post-processing on a single image.

**Signature:**
```csharp
Task<ExtraSingleImageResponse> ProcessSingleImageAsync(
    ExtraSingleImageRequest request,
    CancellationToken cancellationToken = default
)
```

**Endpoint**: `POST /sdapi/v1/extra-single-image`

**Parameters:**
```csharp
public class ExtraSingleImageRequest
{
    public string Image { get; set; }              // Image in base64
    public int ResizeMode { get; set; }            // Resize mode
    public bool ShowExtrasResults { get; set; }    // Show results
    public int GfpganVisibility { get; set; }      // GFPGAN strength (0-1)
    public int CodeformerVisibility { get; set; }  // CodeFormer strength (0-1)
    public double CodeformerWeight { get; set; }   // CodeFormer weight
    public int UpscalingResize { get; set; }       // How much to enlarge
    public double UpscalingResizeW { get; set; }   // Target width
    public double UpscalingResizeH { get; set; }   // Target height
    public bool UpscalingCrop { get; set; }        // Crop to target size
    public string Upscaler1 { get; set; }          // First upscaler
    public string Upscaler2 { get; set; }          // Second upscaler
    public double ExtrasUpscaler2Visibility { get; set; } // Second upscaler visibility
    public bool UpscaleFirst { get; set; }         // Upscale first, then face restoration
}
```

**Response:**
```csharp
public class ExtraSingleImageResponse
{
    public string Image { get; set; }   // Processed image in base64
    public string HtmlInfo { get; set; } // HTML info about process
}
```

**Use Cases:**
- Increase resolution of finished images
- Restore faces in images
- Improve quality of old images

---

## Embeddings

Service for working with embeddings (textual inversions).

### `GetEmbeddingsAsync`

Gets a list of all available embeddings.

**Signature:**
```csharp
Task<IReadOnlyDictionary<string, Embedding>> GetEmbeddingsAsync(
    CancellationToken cancellationToken = default
)
```

**Endpoint**: `GET /sdapi/v1/embeddings`

**Response:**
```csharp
public class Embedding
{
    public int Step { get; set; }              // Training step number
    public string SdCheckpoint { get; set; }   // Checkpoint trained on
    public string SdCheckpointName { get; set; } // Checkpoint name
    public int Shape { get; set; }             // Embedding dimension
    public int Vectors { get; set; }           // Number of vectors
}
```

**Use Cases:** 
- Get list of available textual inversions
- Use in prompts via `<embedding-name>` syntax

### `RefreshEmbeddingsAsync`

Refreshes embeddings list (rescans folder).

**Signature:**
```csharp
Task RefreshEmbeddingsAsync(
    CancellationToken cancellationToken = default
)
```

**Endpoint**: `POST /sdapi/v1/refresh-embeddings`

**Use Case:** After adding new embeddings without restarting WebUI.

---

## LoRA

Service for working with LoRA models.

### `GetLorasAsync`

Gets a list of all available LoRA models.

**Signature:**
```csharp
Task<IReadOnlyList<Lora>> GetLorasAsync(
    CancellationToken cancellationToken = default
)
```

**Endpoint**: `GET /sdapi/v1/loras`

**Response:**
```csharp
public class Lora
{
    public string Name { get; set; }           // LoRA name
    public string Alias { get; set; }          // Alias
    public string Path { get; set; }           // File path
    public Dictionary<string, object> Metadata { get; set; } // Metadata
}
```

**Use Cases:**
- Get list of available LoRAs
- Use in prompts via `<lora:name:weight>` syntax

### `RefreshLorasAsync`

Refreshes LoRA models list (rescans folder).

**Signature:**
```csharp
Task RefreshLorasAsync(
    CancellationToken cancellationToken = default
)
```

**Endpoint**: `POST /sdapi/v1/refresh-loras`

**Use Case:** After adding new LoRAs without restarting WebUI.

---

## Ping

General client method for checking API availability.

### `PingAsync`

Checks API availability.

**Signature:**
```csharp
Task<bool> PingAsync(
    CancellationToken cancellationToken = default
)
```

**Endpoint**: Makes request to base URL

**Response:** `true` if API is available, `false` otherwise.

**Use Cases:**
- Check availability before starting work
- Health check for monitoring
- Wait for WebUI startup

---

## Additional Information

### Image Format

All images are transferred in base64 format. Use `ImageHelper` class for conversion:

```csharp
// Image to base64
var base64 = ImageHelper.ImageToBase64("input.png");

// Base64 to image
ImageHelper.Base64ToImage(base64String, "output.png");
```

### Error Handling

All methods can throw the following exceptions:
- `ApiException` - API errors with response code and body
- `ConfigurationException` - configuration errors
- `StableDiffusionException` - base exception

### CancellationToken

All async methods support `CancellationToken` for canceling operations.

### Retry Policy

The library automatically retries requests on transient errors:
- HTTP 500, 502, 503, 504
- HTTP 429 (Too Many Requests)
- Network errors and timeouts

---

## Examples

See detailed examples of all API methods in [EXAMPLES.md](EXAMPLES.md).

Advanced usage scenarios are described in [ADVANCED.md](ADVANCED.md).


