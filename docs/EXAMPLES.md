# Detailed Usage Examples for StableDiffusionNet

**English | [Русский](EXAMPLES.ru.md)**

> Practical examples for all library services

## Table of Contents

- [Getting Started](#getting-started)
- [Text to Image](#text-to-image)
- [Image to Image](#image-to-image)
- [Model Management](#model-management)
- [Progress Tracking](#progress-tracking)
- [WebUI Settings](#webui-settings)
- [Getting Information](#getting-information)
- [PNG Metadata](#png-metadata)
- [Post-processing](#post-processing)
- [Embeddings and LoRA](#embeddings-and-lora)
- [Error Handling](#error-handling)

---

## Getting Started

### Creating Client (Core Package)

```csharp
using StableDiffusionNet;

// Simple way
var client = StableDiffusionClientBuilder.CreateDefault("http://localhost:7860");

// With settings
var client = new StableDiffusionClientBuilder()
    .WithBaseUrl("http://localhost:7860")
    .WithTimeout(600) // 10 minutes
    .WithRetry(retryCount: 3, retryDelayMilliseconds: 1000)
    .WithDetailedLogging()
    .Build();
```

### Creating Client (DI Package)

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

### Checking API Availability

```csharp
var healthCheck = await client.HealthCheckAsync();
if (!healthCheck.IsHealthy)
{
    Console.WriteLine($"API is unavailable: {healthCheck.Error}");
    Console.WriteLine("Make sure WebUI is running with --api flag.");
    return;
}

Console.WriteLine($"✓ API is available (Response time: {healthCheck.ResponseTime?.TotalMilliseconds}ms)");
```

---

## Text to Image

### Basic Generation

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
    Seed = -1 // Random seed
};

var response = await client.TextToImage.GenerateAsync(request);

// Save image
ImageHelper.Base64ToImage(response.Images[0], "output.png");
Console.WriteLine($"Image saved: output.png");
```

### Generation with Fixed Seed

```csharp
var request = new TextToImageRequest
{
    Prompt = "a cute cat, professional photo",
    Width = 512,
    Height = 512,
    Steps = 20,
    Seed = 12345 // Fixed seed for reproducibility
};

var response = await client.TextToImage.GenerateAsync(request);
// Repeated request with same seed will produce identical image
```

### Batch Generation

```csharp
var request = new TextToImageRequest
{
    Prompt = "fantasy landscape, concept art",
    Width = 512,
    Height = 512,
    Steps = 25,
    BatchSize = 4,  // 4 images at once
    NIter = 2       // 2 batches (total 8 images)
};

var response = await client.TextToImage.GenerateAsync(request);

// Save all images
for (int i = 0; i < response.Images.Count; i++)
{
    ImageHelper.Base64ToImage(response.Images[i], $"output_{i:D3}.png");
}

Console.WriteLine($"Generated {response.Images.Count} images");
```

### High Resolution (Hires.fix)

```csharp
var request = new TextToImageRequest
{
    Prompt = "epic castle on a mountain, 8k, highly detailed, cinematic lighting",
    Width = 512,
    Height = 512,
    Steps = 30,
    
    // Enable Hires.fix
    EnableHr = true,
    HrScale = 2.0,              // 2x upscale (total 1024x1024)
    HrUpscaler = "Latent",      // Upscaler to use
    HrSecondPassSteps = 20,     // Steps for second pass
    DenoisingStrength = 0.7     // Denoising strength
};

var response = await client.TextToImage.GenerateAsync(request);
ImageHelper.Base64ToImage(response.Images[0], "hires_output.png");
```

### Generation with Face Restoration

```csharp
var request = new TextToImageRequest
{
    Prompt = "portrait of a beautiful woman, highly detailed face",
    Width = 512,
    Height = 768,
    Steps = 30,
    RestoreFaces = true  // Automatic face restoration
};

var response = await client.TextToImage.GenerateAsync(request);
ImageHelper.Base64ToImage(response.Images[0], "portrait.png");
```

### Tileable Textures

```csharp
var request = new TextToImageRequest
{
    Prompt = "seamless wood texture, 4k, pbr",
    Width = 512,
    Height = 512,
    Steps = 25,
    Tiling = true  // Generate seamless texture
};

var response = await client.TextToImage.GenerateAsync(request);
ImageHelper.Base64ToImage(response.Images[0], "texture.png");
```

### Override Settings for Single Request

```csharp
var request = new TextToImageRequest
{
    Prompt = "anime style character",
    Steps = 20,
    
    // Temporarily change settings only for this request
    OverrideSettings = new Dictionary<string, object>
    {
        { "CLIP_stop_at_last_layers", 2 },      // CLIP skip 2
        { "eta_noise_seed_delta", 0 },          // ENSD
        { "sd_vae", "vae-ft-mse-840000-ema-pruned.ckpt" }
    }
};

var response = await client.TextToImage.GenerateAsync(request);
// After execution, settings return to original
```

### Using Different Samplers

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
        Seed = 12345  // Same seed for comparison
    };
    
    var response = await client.TextToImage.GenerateAsync(request);
    ImageHelper.Base64ToImage(response.Images[0], $"sampler_{sampler.Replace(" ", "_")}.png");
    Console.WriteLine($"Generated with {sampler}");
}
```

---

## Image to Image

### Basic Image Transformation

```csharp
using StableDiffusionNet.Models.Requests;
using StableDiffusionNet.Helpers;

// Load source image
var initImage = ImageHelper.ImageToBase64("input.png");

var request = new ImageToImageRequest
{
    InitImages = new List<string> { initImage },
    Prompt = "transform into a watercolor painting",
    NegativePrompt = "photo, realistic",
    
    DenoisingStrength = 0.75,  // Strength of change (0.0 = no change, 1.0 = full redraw)
    Width = 512,
    Height = 512,
    Steps = 30,
    CfgScale = 7.5
};

var response = await client.ImageToImage.GenerateAsync(request);
ImageHelper.Base64ToImage(response.Images[0], "watercolor.png");
```

### Variations of Single Image

```csharp
var initImage = ImageHelper.ImageToBase64("photo.png");

// Low strength - more similar to original
for (int i = 0; i < 4; i++)
{
    var request = new ImageToImageRequest
    {
        InitImages = new List<string> { initImage },
        Prompt = "professional photo, high quality",
        DenoisingStrength = 0.3,  // Small changes
        Steps = 20,
        Seed = -1  // Different seed for variations
    };
    
    var response = await client.ImageToImage.GenerateAsync(request);
    ImageHelper.Base64ToImage(response.Images[0], $"variation_{i}.png");
}
```

### Photo Stylization

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
    Console.WriteLine($"Created style: {style.Key}");
}
```

### Inpainting - Drawing by Mask

```csharp
var initImage = ImageHelper.ImageToBase64("room.png");
var mask = ImageHelper.ImageToBase64("mask.png");  // White areas will be redrawn

var request = new ImageToImageRequest
{
    InitImages = new List<string> { initImage },
    Mask = mask,
    Prompt = "modern sofa, interior design",
    NegativePrompt = "blurry, low quality",
    
    DenoisingStrength = 0.9,
    Steps = 30,
    
    // Inpainting parameters
    InpaintingFill = 1,        // 0: fill, 1: original, 2: latent noise, 3: latent nothing
    InpaintFullRes = true,     // Draw only mask area in high resolution
    InpaintFullResPadding = 32, // Padding around mask
    MaskBlur = 4              // Mask edge blur
};

var response = await client.ImageToImage.GenerateAsync(request);
ImageHelper.Base64ToImage(response.Images[0], "inpainted.png");
```

### Outpainting - Image Expansion

```csharp
// Create mask where to draw (white areas)
// For example, extend canvas and make new areas white

var image = ImageHelper.ImageToBase64("original.png");
var mask = ImageHelper.ImageToBase64("outpaint_mask.png");

var request = new ImageToImageRequest
{
    InitImages = new List<string> { image },
    Mask = mask,
    Prompt = "continue the landscape, natural, seamless",
    
    DenoisingStrength = 0.9,
    Steps = 30,
    InpaintingFill = 2,  // Latent noise for outpainting
    MaskBlur = 8
};

var response = await client.ImageToImage.GenerateAsync(request);
ImageHelper.Base64ToImage(response.Images[0], "outpainted.png");
```

### Resizing with Different Modes

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
        ResizeMode = mode.Key,  // Resize mode
        
        DenoisingStrength = 0.5,
        Steps = 25
    };
    
    var response = await client.ImageToImage.GenerateAsync(request);
    ImageHelper.Base64ToImage(response.Images[0], $"resize_mode_{mode.Key}.png");
    Console.WriteLine($"Mode {mode.Key}: {mode.Value}");
}
```

### Sketch to Image

```csharp
// Turn sketch into detailed image
var sketch = ImageHelper.ImageToBase64("sketch.png");

var request = new ImageToImageRequest
{
    InitImages = new List<string> { sketch },
    Prompt = "professional concept art, highly detailed, realistic lighting",
    NegativePrompt = "sketch, rough, unfinished",
    
    DenoisingStrength = 0.85,  // High strength for detail
    Steps = 40,
    CfgScale = 9.0  // High CFG to follow prompt
};

var response = await client.ImageToImage.GenerateAsync(request);
ImageHelper.Base64ToImage(response.Images[0], "detailed_art.png");
```

---

## Model Management

### Getting List of Available Models

```csharp
var models = await client.Models.GetModelsAsync();

Console.WriteLine("Available models:");
foreach (var model in models)
{
    Console.WriteLine($"  - {model.Title}");
    Console.WriteLine($"    File: {model.ModelName}");
    Console.WriteLine($"    Hash: {model.Hash?.Substring(0, 8)}...");
    Console.WriteLine();
}
```

### Getting Current Model

```csharp
var currentModel = await client.Models.GetCurrentModelAsync();
Console.WriteLine($"Current model: {currentModel}");
```

### Switching Models

```csharp
Console.WriteLine("Switching model...");
await client.Models.SetModelAsync("sd_xl_base_1.0.safetensors");

// Wait for model to load
await Task.Delay(5000);

var newModel = await client.Models.GetCurrentModelAsync();
Console.WriteLine($"Active model: {newModel}");
```

### Automatic Model Selection by Task

```csharp
async Task<string> SelectModelForTask(string task)
{
    var models = await client.Models.GetModelsAsync();
    
    if (task.Contains("anime"))
    {
        // Find anime model
        var animeModel = models.FirstOrDefault(m => 
            m.Title.Contains("anime", StringComparison.OrdinalIgnoreCase));
        return animeModel?.ModelName ?? models[0].ModelName;
    }
    else if (task.Contains("realistic"))
    {
        // Find realistic model
        var realisticModel = models.FirstOrDefault(m => 
            m.Title.Contains("realistic", StringComparison.OrdinalIgnoreCase));
        return realisticModel?.ModelName ?? models[0].ModelName;
    }
    
    return models[0].ModelName;
}

// Usage
var modelName = await SelectModelForTask("generate realistic portrait");
await client.Models.SetModelAsync(modelName);
```

### Refreshing Model List

```csharp
Console.WriteLine("Scanning models folder...");
await client.Models.RefreshModelsAsync();

var models = await client.Models.GetModelsAsync();
Console.WriteLine($"Found models: {models.Count}");
```

### Generation with Different Models

```csharp
var models = new[] 
{ 
    "sd_v1-5.safetensors",
    "sd_xl_base_1.0.safetensors"
};

var prompt = "beautiful landscape";

foreach (var model in models)
{
    Console.WriteLine($"Generating with model: {model}");
    
    await client.Models.SetModelAsync(model);
    await Task.Delay(3000); // Wait for model to load
    
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

## Progress Tracking

### Simple Progress Bar

```csharp
var request = new TextToImageRequest
{
    Prompt = "complex detailed scene",
    Width = 512,
    Height = 512,
    Steps = 50
};

// Start generation in separate task
var generateTask = client.TextToImage.GenerateAsync(request);

// Track progress
while (!generateTask.IsCompleted)
{
    var progress = await client.Progress.GetProgressAsync();
    
    // Percentage progress
    var percentage = progress.Progress * 100;
    Console.Write($"\rProgress: {percentage:F1}%");
    
    await Task.Delay(500);
}

Console.WriteLine("\n✓ Generation completed");
var result = await generateTask;
```

### Detailed Progress with ETA

```csharp
var generateTask = client.TextToImage.GenerateAsync(request);

while (!generateTask.IsCompleted)
{
    var progress = await client.Progress.GetProgressAsync();
    
    if (progress.State != null)
    {
        var eta = TimeSpan.FromSeconds(progress.EtaRelative);
        
        Console.WriteLine($"Progress: {progress.Progress:P}");
        Console.WriteLine($"Step: {progress.State.SamplingStep}/{progress.State.SamplingSteps}");
        Console.WriteLine($"ETA: {eta:mm\\:ss}");
        Console.WriteLine(new string('-', 50));
    }
    
    await Task.Delay(1000);
}

var result = await generateTask;
```

### Saving Intermediate Results

```csharp
var generateTask = client.TextToImage.GenerateAsync(request);
var previewCount = 0;

while (!generateTask.IsCompleted)
{
    var progress = await client.Progress.GetProgressAsync();
    
    // If preview is available
    if (!string.IsNullOrEmpty(progress.CurrentImage))
    {
        ImageHelper.Base64ToImage(
            progress.CurrentImage, 
            $"preview_{previewCount++:D3}.png"
        );
        Console.WriteLine($"Saved preview #{previewCount}");
    }
    
    await Task.Delay(2000);
}

var result = await generateTask;
```

### User Interruption of Generation

```csharp
var cts = new CancellationTokenSource();

// Ctrl+C handler
Console.CancelKeyPress += async (sender, e) =>
{
    e.Cancel = true;
    Console.WriteLine("\nInterrupting generation...");
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
        Console.Write($"\rProgress: {progress.Progress:P}");
        await Task.Delay(500);
    }
    
    var result = await generateTask;
    Console.WriteLine("\n✓ Generation completed");
}
catch (OperationCanceledException)
{
    Console.WriteLine("\n× Generation interrupted by user");
}
```

### Skipping Image in Batch

```csharp
var request = new TextToImageRequest
{
    Prompt = "test prompt",
    BatchSize = 10,
    Steps = 30
};

var generateTask = client.TextToImage.GenerateAsync(request);

// Skip 5th image
var imageCount = 0;
while (!generateTask.IsCompleted)
{
    var progress = await client.Progress.GetProgressAsync();
    
    if (progress.State != null)
    {
        var currentImage = (int)(progress.Progress * 10);
        if (currentImage == 5 && imageCount != currentImage)
        {
            Console.WriteLine("Skipping image #5...");
            await client.Progress.SkipAsync();
        }
        imageCount = currentImage;
    }
    
    await Task.Delay(500);
}

var result = await generateTask;
```

---

## WebUI Settings

### Getting Current Settings

```csharp
var options = await client.Options.GetOptionsAsync();

Console.WriteLine("Current settings:");
Console.WriteLine($"  Model: {options.SdModelCheckpoint}");
Console.WriteLine($"  CLIP skip: {options.ClipStopAtLastLayers}");
Console.WriteLine($"  xFormers: {options.EnableXformers}");
Console.WriteLine($"  Save format: {options.SamplesFormat}");
```

### Changing Settings

```csharp
var options = await client.Options.GetOptionsAsync();

// Change settings
options.ClipStopAtLastLayers = 2;
options.EnableXformers = true;
options.SamplesFormat = "png";
options.SamplesSave = true;

// Apply changes
await client.Options.SetOptionsAsync(options);
Console.WriteLine("✓ Settings updated");
```

### Optimization for Speed

```csharp
var options = await client.Options.GetOptionsAsync();

// Settings for fast generation
options.EnableXformers = true;              // Use xFormers
options.AlwaysBatchCondUncond = false;     // Don't batch cond/uncond
options.UseOldHiresFixWidthHeight = false;

await client.Options.SetOptionsAsync(options);
Console.WriteLine("✓ Configured for speed");
```

### Optimization for Quality

```csharp
var options = await client.Options.GetOptionsAsync();

// Settings for maximum quality
options.NoHalfVae = true;                  // Full VAE precision
options.EnableXformers = true;
options.SamplesFormat = "png";             // Lossless

await client.Options.SetOptionsAsync(options);
Console.WriteLine("✓ Configured for quality");
```

### Temporary Settings Change

```csharp
// Save current settings
var originalOptions = await client.Options.GetOptionsAsync();

try
{
    // Temporarily change settings
    var tempOptions = await client.Options.GetOptionsAsync();
    tempOptions.ClipStopAtLastLayers = 2;
    await client.Options.SetOptionsAsync(tempOptions);
    
    // Generate with new settings
    var response = await client.TextToImage.GenerateAsync(request);
}
finally
{
    // Restore original settings
    await client.Options.SetOptionsAsync(originalOptions);
}
```

---

## Getting Information

### Available Samplers

```csharp
var samplers = await client.Samplers.GetSamplersAsync();

Console.WriteLine("Available samplers:");
foreach (var sampler in samplers)
{
    Console.WriteLine($"  - {sampler.Name}");
    if (sampler.Aliases.Count > 0)
    {
        Console.WriteLine($"    Aliases: {string.Join(", ", sampler.Aliases)}");
    }
    if (sampler.Options.Count > 0)
    {
        Console.WriteLine($"    Options: {sampler.Options.Count}");
    }
}
```

### Available Schedulers

```csharp
var schedulers = await client.Schedulers.GetSchedulersAsync();

Console.WriteLine("Available schedulers:");
foreach (var scheduler in schedulers)
{
    Console.WriteLine($"  - {scheduler.Name} ({scheduler.Label})");
    if (scheduler.Aliases != null && scheduler.Aliases.Count > 0)
    {
        Console.WriteLine($"    Aliases: {string.Join(", ", scheduler.Aliases)}");
    }
    Console.WriteLine($"    Default Rho: {scheduler.DefaultRho}");
    Console.WriteLine($"    Requires inner model: {scheduler.NeedInnerModel}");
}
```

### Available Upscalers

```csharp
var upscalers = await client.Upscalers.GetUpscalersAsync();

Console.WriteLine("Available upscalers:");
foreach (var upscaler in upscalers)
{
    Console.WriteLine($"  - {upscaler.Name} ({upscaler.Scale}x)");
    if (!string.IsNullOrEmpty(upscaler.ModelPath))
    {
        Console.WriteLine($"    Path: {upscaler.ModelPath}");
    }
}
```

### Latent Upscale Modes

```csharp
var modes = await client.Upscalers.GetLatentUpscaleModesAsync();

Console.WriteLine("Latent upscale modes:");
foreach (var mode in modes)
{
    Console.WriteLine($"  - {mode.Name}");
}
```

### Available Embeddings

```csharp
var embeddings = await client.Embeddings.GetEmbeddingsAsync();

Console.WriteLine($"Found embeddings: {embeddings.Count}");
foreach (var embedding in embeddings)
{
    Console.WriteLine($"  - {embedding.Key}");
    Console.WriteLine($"    Steps: {embedding.Value.Step}");
    Console.WriteLine($"    Vectors: {embedding.Value.Vectors}");
}

// Refresh list
await client.Embeddings.RefreshEmbeddingsAsync();
```

### Available LoRAs

```csharp
var loras = await client.Loras.GetLorasAsync();

Console.WriteLine($"Found LoRAs: {loras.Count}");
foreach (var lora in loras)
{
    Console.WriteLine($"  - {lora.Name}");
    Console.WriteLine($"    Path: {lora.Path}");
}

// Refresh list
await client.Loras.RefreshLorasAsync();
```

### Building UI with Dynamic Selection

```csharp
// Load all options for UI
var samplers = await client.Samplers.GetSamplersAsync();
var schedulers = await client.Schedulers.GetSchedulersAsync();
var upscalers = await client.Upscalers.GetUpscalersAsync();
var models = await client.Models.GetModelsAsync();

// Now can build dropdowns in UI
Console.WriteLine("Select options for generation:");
Console.WriteLine($"Available samplers: {samplers.Count}");
Console.WriteLine($"Available schedulers: {schedulers.Count}");
Console.WriteLine($"Available models: {models.Count}");
Console.WriteLine($"Available upscalers: {upscalers.Count}");

// Example building scheduler list for UI
foreach (var scheduler in schedulers)
{
    // In UI can show Label, use Name
    Console.WriteLine($"Scheduler: {scheduler.Label} (ID: {scheduler.Name})");
}
```

---

## PNG Metadata

### Extracting Information from Image

```csharp
using StableDiffusionNet.Models.Requests;

var imageBase64 = ImageHelper.ImageToBase64("generated.png");

var request = new PngInfoRequest
{
    Image = imageBase64
};

var response = await client.PngInfo.GetPngInfoAsync(request);

Console.WriteLine("Generation information:");
Console.WriteLine(response.Info);

// Parse parameters
if (response.Items != null)
{
    foreach (var item in response.Items)
    {
        Console.WriteLine($"{item.Key}: {item.Value}");
    }
}
```

### Copying Parameters from Image

```csharp
var sourceImage = ImageHelper.ImageToBase64("source.png");

var pngInfoRequest = new PngInfoRequest { Image = sourceImage };
var pngInfo = await client.PngInfo.GetPngInfoAsync(pngInfoRequest);

// Parse parameters (this is simplified example)
var info = pngInfo.Info;
// Here need to parse info string and extract parameters

// Create request with same parameters
var request = new TextToImageRequest
{
    Prompt = "...extracted from info...",
    Steps = 30, // extracted
    CfgScale = 7.5, // extracted
    // etc.
};

var response = await client.TextToImage.GenerateAsync(request);
```

### Analyzing Others' Images

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
            Console.WriteLine($"✓ Image contains generation metadata");
            Console.WriteLine(response.Info);
            return;
        }
        
        Console.WriteLine("× Image doesn't contain generation metadata");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error analyzing: {ex.Message}");
    }
}

await AnalyzeImage("downloaded_image.png");
```

---

## Post-processing

### Image Upscale

```csharp
using StableDiffusionNet.Models.Requests;

var imageBase64 = ImageHelper.ImageToBase64("input.png");

var request = new ExtraSingleImageRequest
{
    Image = imageBase64,
    ResizeMode = 0,             // 0: by scale, 1: to specific size
    UpscalingResize = 4,        // Upscale 4x
    Upscaler1 = "R-ESRGAN 4x+", // Main upscaler
    Upscaler2 = "None"          // Second upscaler
};

var response = await client.Extra.ProcessSingleImageAsync(request);
ImageHelper.Base64ToImage(response.Image, "upscaled_4x.png");
Console.WriteLine("✓ Image upscaled 4x");
```

### Upscale to Specific Size

```csharp
var imageBase64 = ImageHelper.ImageToBase64("small.png");

var request = new ExtraSingleImageRequest
{
    Image = imageBase64,
    ResizeMode = 1,              // To specific size
    UpscalingResizeW = 1920,     // Target width
    UpscalingResizeH = 1080,     // Target height
    UpscalingCrop = false,       // Don't crop
    Upscaler1 = "Lanczos"
};

var response = await client.Extra.ProcessSingleImageAsync(request);
ImageHelper.Base64ToImage(response.Image, "resized_1920x1080.png");
```

### Face Restoration

```csharp
var imageBase64 = ImageHelper.ImageToBase64("photo.png");

var request = new ExtraSingleImageRequest
{
    Image = imageBase64,
    ResizeMode = 0,
    CodeformerVisibility = 1,    // CodeFormer strength (0-1)
    CodeformerWeight = 0.5       // Balance between original and restoration
};

var response = await client.Extra.ProcessSingleImageAsync(request);
ImageHelper.Base64ToImage(response.Image, "faces_restored.png");
```

### Combined Processing

```csharp
var imageBase64 = ImageHelper.ImageToBase64("old_photo.png");

var request = new ExtraSingleImageRequest
{
    Image = imageBase64,
    
    // Upscale
    ResizeMode = 0,
    UpscalingResize = 2,
    Upscaler1 = "R-ESRGAN 4x+",
    
    // Face restoration
    CodeformerVisibility = 1,
    CodeformerWeight = 0.7,
    
    // Order: upscale first, then face restoration
    UpscaleFirst = true
};

var response = await client.Extra.ProcessSingleImageAsync(request);
ImageHelper.Base64ToImage(response.Image, "enhanced.png");
Console.WriteLine("✓ Image enhanced");
```

### Using Two Upscalers

```csharp
var imageBase64 = ImageHelper.ImageToBase64("input.png");

var request = new ExtraSingleImageRequest
{
    Image = imageBase64,
    ResizeMode = 0,
    UpscalingResize = 4,
    
    // First upscaler
    Upscaler1 = "R-ESRGAN 4x+",
    
    // Second upscaler (mixed with first)
    Upscaler2 = "R-ESRGAN 4x+ Anime6B",
    ExtrasUpscaler2Visibility = 0.5  // 50% of second upscaler
};

var response = await client.Extra.ProcessSingleImageAsync(request);
ImageHelper.Base64ToImage(response.Image, "mixed_upscale.png");
```

### Batch Processing Images

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
    
    Console.WriteLine($"✓ Processed: {file}");
}
```

---

## Embeddings and LoRA

### Using Embeddings in Prompts

```csharp
// First get list of available embeddings
var embeddings = await client.Embeddings.GetEmbeddingsAsync();

if (embeddings.ContainsKey("my_style"))
{
    var request = new TextToImageRequest
    {
        // Use embedding in prompt
        Prompt = "a beautiful portrait, <my_style>",
        Steps = 30
    };
    
    var response = await client.TextToImage.GenerateAsync(request);
    ImageHelper.Base64ToImage(response.Images[0], "with_embedding.png");
}
```

### Using LoRA in Prompts

```csharp
// Get LoRA list
var loras = await client.Loras.GetLorasAsync();

var request = new TextToImageRequest
{
    // Syntax: <lora:name:weight>
    // weight usually from 0.0 to 1.0
    Prompt = "beautiful landscape, <lora:fantasy_style:0.8>",
    NegativePrompt = "low quality",
    Steps = 30
};

var response = await client.TextToImage.GenerateAsync(request);
ImageHelper.Base64ToImage(response.Images[0], "with_lora.png");
```

### Combining Multiple LoRAs

```csharp
var request = new TextToImageRequest
{
    // Can use multiple LoRAs simultaneously
    Prompt = "anime girl, <lora:anime_style:0.7>, <lora:detailed_eyes:0.5>",
    Steps = 30
};

var response = await client.TextToImage.GenerateAsync(request);
```

### Testing Different LoRA Weights

```csharp
var loraName = "style_lora";
var weights = new[] { 0.2, 0.5, 0.8, 1.0 };

foreach (var weight in weights)
{
    var request = new TextToImageRequest
    {
        Prompt = $"portrait, <lora:{loraName}:{weight:F1}>",
        Steps = 20,
        Seed = 12345  // Same seed for comparison
    };
    
    var response = await client.TextToImage.GenerateAsync(request);
    ImageHelper.Base64ToImage(response.Images[0], $"lora_weight_{weight:F1}.png");
    Console.WriteLine($"Generated with weight {weight:F1}");
}
```

---

## Error Handling

### Basic Error Handling

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
    Console.WriteLine($"Response Body: {ex.ResponseBody}");
}
catch (ConfigurationException ex)
{
    Console.WriteLine($"Configuration Error: {ex.Message}");
}
catch (StableDiffusionException ex)
{
    Console.WriteLine($"StableDiffusion Error: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected Error: {ex.Message}");
}
```

### Retry with Error Handling

```csharp
async Task<TextToImageResponse?> GenerateWithRetry(
    TextToImageRequest request, 
    int maxAttempts = 3)
{
    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            Console.WriteLine($"Attempt {attempt}/{maxAttempts}...");
            return await client.TextToImage.GenerateAsync(request);
        }
        catch (ApiException ex) when (ex.StatusCode == 503)
        {
            // Server busy
            if (attempt < maxAttempts)
            {
                Console.WriteLine("Server busy, waiting...");
                await Task.Delay(5000);
                continue;
            }
            Console.WriteLine("Server unavailable after all attempts");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            if (attempt == maxAttempts)
                throw;
        }
    }
    
    return null;
}

// Usage
var response = await GenerateWithRetry(request);
if (response != null)
{
    ImageHelper.Base64ToImage(response.Images[0], "output.png");
}
```

### Checking Availability Before Request

```csharp
async Task<bool> WaitForApi(int timeoutSeconds = 60)
{
    var stopwatch = Stopwatch.StartNew();
    
    while (stopwatch.Elapsed.TotalSeconds < timeoutSeconds)
    {
        var healthCheck = await client.HealthCheckAsync();
        if (healthCheck.IsHealthy)
        {
            Console.WriteLine($"✓ API available (Response time: {healthCheck.ResponseTime?.TotalMilliseconds}ms)");
            return true;
        }
        
        Console.Write(".");
        await Task.Delay(1000);
    }
    
    Console.WriteLine("\n× API unavailable");
    return false;
}

// Usage
if (await WaitForApi())
{
    var response = await client.TextToImage.GenerateAsync(request);
}
```

### Request Logging

```csharp
async Task<TextToImageResponse> GenerateWithLogging(TextToImageRequest request)
{
    var sw = Stopwatch.StartNew();
    
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Starting generation");
    Console.WriteLine($"  Prompt: {request.Prompt}");
    Console.WriteLine($"  Size: {request.Width}x{request.Height}");
    Console.WriteLine($"  Steps: {request.Steps}");
    
    try
    {
        var response = await client.TextToImage.GenerateAsync(request);
        
        sw.Stop();
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✓ Generation completed in {sw.Elapsed.TotalSeconds:F1}s");
        Console.WriteLine($"  Generated images: {response.Images.Count}");
        
        return response;
    }
    catch (Exception ex)
    {
        sw.Stop();
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] × Error after {sw.Elapsed.TotalSeconds:F1}s");
        Console.WriteLine($"  {ex.Message}");
        throw;
    }
}
```

---

## Additional Examples

For advanced usage scenarios see [ADVANCED.md](ADVANCED.md).

Complete API methods reference: [API_REFERENCE.md](API_REFERENCE.md).


