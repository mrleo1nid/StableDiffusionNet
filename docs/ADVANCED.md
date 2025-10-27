# Advanced Usage Examples for StableDiffusionNet

**English | [Русский](ADVANCED.ru.md)**

> Complex scenarios and best practices for professional use

## Table of Contents

- [Workflow Automation](#workflow-automation)
- [Parallel Generation](#parallel-generation)
- [Performance Optimization](#performance-optimization)
- [Advanced Inpainting](#advanced-inpainting)
- [Working with Large Batches](#working-with-large-batches)
- [Database Integration](#database-integration)
- [RESTful API Service](#restful-api-service)
- [Queue Processing](#queue-processing)
- [Caching and Optimization](#caching-and-optimization)
- [Testing and Custom Services](#testing-and-custom-services)
- [Monitoring and Metrics](#monitoring-and-metrics)

---

## Workflow Automation

### Complete Generation and Post-processing Pipeline

```csharp
using StableDiffusionNet;
using StableDiffusionNet.Models.Requests;
using StableDiffusionNet.Helpers;

public class ImageGenerationPipeline
{
    private readonly IStableDiffusionClient _client;
    
    public ImageGenerationPipeline(IStableDiffusionClient client)
    {
        _client = client;
    }
    
    public async Task<string> GenerateAndEnhanceAsync(
        string prompt,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        // 1. Generate base image
        Console.WriteLine("Step 1/3: Generating base image...");
        var generateRequest = new TextToImageRequest
        {
            Prompt = prompt,
            Width = 512,
            Height = 512,
            Steps = 30,
            CfgScale = 7.5,
            SamplerName = "DPM++ 2M Karras"
        };
        
        var generated = await _client.TextToImage.GenerateAsync(
            generateRequest, 
            cancellationToken
        );
        
        // 2. Upscale with face restoration
        Console.WriteLine("Step 2/3: Upscaling and restoring details...");
        var upscaleRequest = new ExtraSingleImageRequest
        {
            Image = generated.Images[0],
            ResizeMode = 0,
            UpscalingResize = 2,
            Upscaler1 = "R-ESRGAN 4x+",
            CodeformerVisibility = 1,
            CodeformerWeight = 0.7,
            UpscaleFirst = true
        };
        
        var upscaled = await _client.Extra.ProcessSingleImageAsync(
            upscaleRequest, 
            cancellationToken
        );
        
        // 3. Final refinement via img2img
        Console.WriteLine("Step 3/3: Final detail refinement...");
        var refineRequest = new ImageToImageRequest
        {
            InitImages = new List<string> { upscaled.Image },
            Prompt = prompt + ", highly detailed, sharp, professional",
            DenoisingStrength = 0.2,  // Small correction
            Steps = 20,
            Width = 1024,
            Height = 1024
        };
        
        var refined = await _client.ImageToImage.GenerateAsync(
            refineRequest, 
            cancellationToken
        );
        
        // 4. Save result
        ImageHelper.Base64ToImage(refined.Images[0], outputPath);
        Console.WriteLine($"✓ Image saved: {outputPath}");
        
        return outputPath;
    }
}

// Usage
var pipeline = new ImageGenerationPipeline(client);
await pipeline.GenerateAndEnhanceAsync(
    "a beautiful fantasy landscape",
    "output/final.png"
);
```

### Automatic Optimal Parameter Selection

```csharp
public class SmartGenerationService
{
    private readonly IStableDiffusionClient _client;
    
    public async Task<TextToImageRequest> OptimizeRequestAsync(
        string prompt, 
        string category = "general")
    {
        var request = new TextToImageRequest
        {
            Prompt = prompt
        };
        
        // Determine optimal parameters by category
        switch (category.ToLower())
        {
            case "portrait":
                request.Width = 512;
                request.Height = 768;
                request.Steps = 40;
                request.CfgScale = 7.5;
                request.SamplerName = "DPM++ SDE Karras";
                request.RestoreFaces = true;
                break;
                
            case "landscape":
                request.Width = 768;
                request.Height = 512;
                request.Steps = 30;
                request.CfgScale = 7.0;
                request.SamplerName = "DPM++ 2M Karras";
                break;
                
            case "anime":
                request.Width = 512;
                request.Height = 768;
                request.Steps = 28;
                request.CfgScale = 8.0;
                request.SamplerName = "Euler a";
                
                // Switch to anime model
                var models = await _client.Models.GetModelsAsync();
                var animeModel = models.FirstOrDefault(m => 
                    m.Title.Contains("anime", StringComparison.OrdinalIgnoreCase));
                
                if (animeModel != null)
                {
                    await _client.Models.SetModelAsync(animeModel.ModelName);
                }
                break;
                
            case "concept_art":
                request.Width = 768;
                request.Height = 512;
                request.Steps = 50;
                request.CfgScale = 9.0;
                request.SamplerName = "DDIM";
                request.EnableHr = true;
                request.HrScale = 1.5;
                break;
                
            default:
                request.Width = 512;
                request.Height = 512;
                request.Steps = 30;
                request.CfgScale = 7.5;
                request.SamplerName = "Euler a";
                break;
        }
        
        return request;
    }
}

// Usage
var service = new SmartGenerationService(client);
var request = await service.OptimizeRequestAsync(
    "beautiful woman portrait", 
    "portrait"
);
var response = await client.TextToImage.GenerateAsync(request);
```

### Generating Variations with Evolution

```csharp
public class ImageEvolutionService
{
    private readonly IStableDiffusionClient _client;
    
    public async Task<List<string>> EvolveImageAsync(
        string initialPrompt,
        int generations = 5,
        int variantsPerGeneration = 4,
        string outputFolder = "evolution")
    {
        Directory.CreateDirectory(outputFolder);
        var results = new List<string>();
        
        // Generate initial generation
        Console.WriteLine($"Generation 0: Initial generation");
        var request = new TextToImageRequest
        {
            Prompt = initialPrompt,
            BatchSize = variantsPerGeneration,
            Steps = 30,
            Width = 512,
            Height = 512
        };
        
        var response = await _client.TextToImage.GenerateAsync(request);
        string bestImage = response.Images[0];
        
        // Save initial variants
        for (int i = 0; i < response.Images.Count; i++)
        {
            var path = Path.Combine(outputFolder, $"gen0_var{i}.png");
            ImageHelper.Base64ToImage(response.Images[i], path);
            results.Add(path);
        }
        
        // Evolution via img2img
        for (int gen = 1; gen < generations; gen++)
        {
            Console.WriteLine($"Generation {gen}: Evolution...");
            
            var evolutionRequest = new ImageToImageRequest
            {
                InitImages = new List<string> { bestImage },
                Prompt = initialPrompt + ", improved, enhanced, refined",
                DenoisingStrength = 0.4 - (gen * 0.05),  // Reduce strength each generation
                BatchSize = variantsPerGeneration,
                Steps = 25
            };
            
            var evolutionResponse = await _client.ImageToImage.GenerateAsync(evolutionRequest);
            
            // Save variants of current generation
            for (int i = 0; i < evolutionResponse.Images.Count; i++)
            {
                var path = Path.Combine(outputFolder, $"gen{gen}_var{i}.png");
                ImageHelper.Base64ToImage(evolutionResponse.Images[i], path);
                results.Add(path);
            }
            
            // Select best for next generation (in reality - through quality assessment)
            bestImage = evolutionResponse.Images[0];
        }
        
        Console.WriteLine($"✓ Evolution completed: {results.Count} images");
        return results;
    }
}

// Usage
var evolution = new ImageEvolutionService(client);
var images = await evolution.EvolveImageAsync(
    "a futuristic city",
    generations: 5,
    variantsPerGeneration: 4
);
```

---

## Parallel Generation

### Generating Multiple Prompts in Parallel

```csharp
public async Task BatchGenerateAsync(List<string> prompts, string outputFolder)
{
    Directory.CreateDirectory(outputFolder);
    
    // Configure parallelism (depends on GPU memory)
    var options = new ParallelOptions
    {
        MaxDegreeOfParallelism = 2  // 2 generations simultaneously
    };
    
    await Parallel.ForEachAsync(prompts, options, async (prompt, ct) =>
    {
        try
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Starting: {prompt}");
            
            var request = new TextToImageRequest
            {
                Prompt = prompt,
                Width = 512,
                Height = 512,
                Steps = 20
            };
            
            var response = await client.TextToImage.GenerateAsync(request, ct);
            
            // Safe file name
            var fileName = string.Join("_", 
                prompt.Split(Path.GetInvalidFileNameChars()))
                .Substring(0, Math.Min(50, prompt.Length));
            
            var path = Path.Combine(outputFolder, $"{fileName}.png");
            ImageHelper.Base64ToImage(response.Images[0], path);
            
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✓ Done: {prompt}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] × Error for '{prompt}': {ex.Message}");
        }
    });
    
    Console.WriteLine("✓ Batch generation completed");
}

// Usage
var prompts = new List<string>
{
    "a beautiful sunset",
    "a cute cat",
    "a fantasy castle",
    "a cyberpunk city"
};

await BatchGenerateAsync(prompts, "batch_output");
```

### Parallel Processing with Different Models

```csharp
public async Task CompareModelsAsync(string prompt, List<string> modelNames)
{
    var tasks = new List<Task<(string model, byte[] image)>>();
    
    // Create separate client for each model (if multiple GPUs)
    // Or execute sequentially
    foreach (var modelName in modelNames)
    {
        tasks.Add(Task.Run(async () =>
        {
            // Switch model
            await client.Models.SetModelAsync(modelName);
            await Task.Delay(2000); // Wait for model to load
            
            var request = new TextToImageRequest
            {
                Prompt = prompt,
                Width = 512,
                Height = 512,
                Steps = 20,
                Seed = 12345  // Same seed for comparison
            };
            
            var response = await client.TextToImage.GenerateAsync(request);
            var imageData = Convert.FromBase64String(response.Images[0]);
            
            return (modelName, imageData);
        }));
    }
    
    var results = await Task.WhenAll(tasks);
    
    // Save results
    foreach (var (model, imageData) in results)
    {
        var fileName = $"model_{model.Replace(".", "_")}.png";
        File.WriteAllBytes(fileName, imageData);
        Console.WriteLine($"✓ Saved: {fileName}");
    }
}
```

### Asynchronous Queue Processing

```csharp
using System.Threading.Channels;

public class GenerationQueueService
{
    private readonly IStableDiffusionClient _client;
    private readonly Channel<GenerationTask> _queue;
    
    public GenerationQueueService(IStableDiffusionClient client)
    {
        _client = client;
        _queue = Channel.CreateUnbounded<GenerationTask>();
    }
    
    public async Task EnqueueAsync(GenerationTask task)
    {
        await _queue.Writer.WriteAsync(task);
        Console.WriteLine($"✓ Task added to queue: {task.Id}");
    }
    
    public async Task ProcessQueueAsync(CancellationToken cancellationToken)
    {
        await foreach (var task in _queue.Reader.ReadAllAsync(cancellationToken))
        {
            try
            {
                Console.WriteLine($"Processing task {task.Id}...");
                
                var response = await _client.TextToImage.GenerateAsync(
                    task.Request, 
                    cancellationToken
                );
                
                ImageHelper.Base64ToImage(response.Images[0], task.OutputPath);
                
                task.CompletionSource.SetResult(task.OutputPath);
                Console.WriteLine($"✓ Task {task.Id} completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"× Task {task.Id} error: {ex.Message}");
                task.CompletionSource.SetException(ex);
            }
        }
    }
}

public class GenerationTask
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public TextToImageRequest Request { get; set; }
    public string OutputPath { get; set; }
    public TaskCompletionSource<string> CompletionSource { get; set; } = new();
}

// Usage
var queueService = new GenerationQueueService(client);

// Start queue processor in background
var cts = new CancellationTokenSource();
var processingTask = Task.Run(() => queueService.ProcessQueueAsync(cts.Token));

// Add tasks
for (int i = 0; i < 10; i++)
{
    var task = new GenerationTask
    {
        Request = new TextToImageRequest
        {
            Prompt = $"test prompt {i}",
            Steps = 20
        },
        OutputPath = $"output_{i}.png"
    };
    
    await queueService.EnqueueAsync(task);
    
    // Wait for task completion
    _ = task.CompletionSource.Task.ContinueWith(t =>
    {
        if (t.IsCompletedSuccessfully)
            Console.WriteLine($"Result received: {t.Result}");
    });
}

// When done, stop processing
// cts.Cancel();
```

---

## Performance Optimization

### Request Reuse

```csharp
public class OptimizedGenerator
{
    private readonly IStableDiffusionClient _client;
    private TextToImageRequest _baseRequest;
    
    public OptimizedGenerator(IStableDiffusionClient client)
    {
        _client = client;
        
        // Base request to reuse
        _baseRequest = new TextToImageRequest
        {
            Width = 512,
            Height = 512,
            Steps = 30,
            CfgScale = 7.5,
            SamplerName = "DPM++ 2M Karras",
            NegativePrompt = "low quality, blurry, distorted"
        };
    }
    
    public async Task<TextToImageResponse> QuickGenerateAsync(string prompt)
    {
        // Clone base request and change only prompt
        var request = new TextToImageRequest
        {
            Prompt = prompt,
            NegativePrompt = _baseRequest.NegativePrompt,
            Width = _baseRequest.Width,
            Height = _baseRequest.Height,
            Steps = _baseRequest.Steps,
            CfgScale = _baseRequest.CfgScale,
            SamplerName = _baseRequest.SamplerName
        };
        
        return await _client.TextToImage.GenerateAsync(request);
    }
}
```

### Result Caching

```csharp
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

public class CachedGenerationService
{
    private readonly IStableDiffusionClient _client;
    private readonly string _cacheFolder;
    
    public CachedGenerationService(IStableDiffusionClient client, string cacheFolder = "cache")
    {
        _client = client;
        _cacheFolder = cacheFolder;
        Directory.CreateDirectory(_cacheFolder);
    }
    
    private string GetCacheKey(TextToImageRequest request)
    {
        // Create unique key based on request parameters
        var json = JsonSerializer.Serialize(request);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(hash).ToLower();
    }
    
    public async Task<TextToImageResponse> GenerateWithCacheAsync(
        TextToImageRequest request)
    {
        var cacheKey = GetCacheKey(request);
        var cachePath = Path.Combine(_cacheFolder, $"{cacheKey}.png");
        var metadataPath = Path.Combine(_cacheFolder, $"{cacheKey}.json");
        
        // Check cache
        if (File.Exists(cachePath))
        {
            Console.WriteLine("✓ Result from cache");
            var cachedImage = ImageHelper.ImageToBase64(cachePath);
            var metadata = File.Exists(metadataPath) 
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(
                    File.ReadAllText(metadataPath))
                : new Dictionary<string, object>();
            
            return new TextToImageResponse
            {
                Images = new List<string> { cachedImage },
                Parameters = metadata
            };
        }
        
        // Generate and cache
        Console.WriteLine("Generating new image...");
        var response = await _client.TextToImage.GenerateAsync(request);
        
        ImageHelper.Base64ToImage(response.Images[0], cachePath);
        if (response.Parameters != null)
        {
            File.WriteAllText(metadataPath, 
                JsonSerializer.Serialize(response.Parameters));
        }
        
        Console.WriteLine("✓ Result saved to cache");
        return response;
    }
    
    public void ClearCache()
    {
        if (Directory.Exists(_cacheFolder))
        {
            Directory.Delete(_cacheFolder, true);
            Directory.CreateDirectory(_cacheFolder);
            Console.WriteLine("✓ Cache cleared");
        }
    }
}

// Usage
var cachedService = new CachedGenerationService(client);

var request = new TextToImageRequest
{
    Prompt = "test prompt",
    Steps = 20
};

// First call - generation
var response1 = await cachedService.GenerateWithCacheAsync(request);

// Second call - from cache
var response2 = await cachedService.GenerateWithCacheAsync(request);
```

### Resource Preloading

```csharp
public class PreloadedGenerationService
{
    private readonly IStableDiffusionClient _client;
    private IReadOnlyList<string> _samplers;
    private IReadOnlyList<Upscaler> _upscalers;
    private IReadOnlyDictionary<string, Embedding> _embeddings;
    
    public async Task InitializeAsync()
    {
        Console.WriteLine("Preloading resources...");
        
        // Load all lists ahead of time
        var tasks = new[]
        {
            _client.Samplers.GetSamplersAsync()
                .ContinueWith(t => _samplers = t.Result),
            _client.Upscalers.GetUpscalersAsync()
                .ContinueWith(t => _upscalers = t.Result),
            _client.Embeddings.GetEmbeddingsAsync()
                .ContinueWith(t => _embeddings = t.Result)
        };
        
        await Task.WhenAll(tasks);
        
        Console.WriteLine("✓ Resources preloaded");
        Console.WriteLine($"  Samplers: {_samplers.Count}");
        Console.WriteLine($"  Upscalers: {_upscalers.Count}");
        Console.WriteLine($"  Embeddings: {_embeddings.Count}");
    }
    
    public bool IsSamplerAvailable(string samplerName)
    {
        return _samplers?.Contains(samplerName) ?? false;
    }
    
    public bool IsUpscalerAvailable(string upscalerName)
    {
        return _upscalers?.Any(u => u.Name == upscalerName) ?? false;
    }
}

// Usage
var service = new PreloadedGenerationService(client);
await service.InitializeAsync();

if (service.IsSamplerAvailable("DPM++ 2M Karras"))
{
    Console.WriteLine("Sampler available");
}
```

---

## Advanced Inpainting

### Automatic Mask Creation

```csharp
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

public class MaskGenerator
{
    // Create mask by color
    public string CreateColorMask(string imagePath, Rgb24 targetColor, int tolerance = 30)
    {
        using var image = Image.Load<Rgb24>(imagePath);
        using var mask = new Image<Rgb24>(image.Width, image.Height);
        
        image.ProcessPixelRows(imageAccessor =>
        {
            mask.ProcessPixelRows(maskAccessor =>
            {
                for (int y = 0; y < imageAccessor.Height; y++)
                {
                    var imageRow = imageAccessor.GetRowSpan(y);
                    var maskRow = maskAccessor.GetRowSpan(y);
                    
                    for (int x = 0; x < imageRow.Length; x++)
                    {
                        var pixel = imageRow[x];
                        var distance = Math.Sqrt(
                            Math.Pow(pixel.R - targetColor.R, 2) +
                            Math.Pow(pixel.G - targetColor.G, 2) +
                            Math.Pow(pixel.B - targetColor.B, 2)
                        );
                        
                        // White = editable area
                        maskRow[x] = distance <= tolerance 
                            ? new Rgb24(255, 255, 255) 
                            : new Rgb24(0, 0, 0);
                    }
                }
            });
        });
        
        var maskPath = "mask_temp.png";
        mask.SaveAsPng(maskPath);
        return maskPath;
    }
    
    // Create rectangular area mask
    public string CreateRectangleMask(
        int imageWidth, 
        int imageHeight,
        Rectangle area)
    {
        using var mask = new Image<Rgb24>(imageWidth, imageHeight);
        
        mask.Mutate(ctx =>
        {
            // Black background
            ctx.Fill(Color.Black);
            // White rectangle
            ctx.Fill(Color.White, area);
        });
        
        var maskPath = "mask_rect.png";
        mask.SaveAsPng(maskPath);
        return maskPath;
    }
}

// Usage
var maskGen = new MaskGenerator();

// Replace red areas
var mask = maskGen.CreateColorMask("photo.png", new Rgb24(255, 0, 0), tolerance: 50);

var initImage = ImageHelper.ImageToBase64("photo.png");
var maskImage = ImageHelper.ImageToBase64(mask);

var request = new ImageToImageRequest
{
    InitImages = new List<string> { initImage },
    Mask = maskImage,
    Prompt = "beautiful flowers",
    DenoisingStrength = 0.9,
    Steps = 30,
    InpaintingFill = 1,
    MaskBlur = 4
};

var response = await client.ImageToImage.GenerateAsync(request);
```

### Multi-Step Inpainting

```csharp
public class MultiStepInpaintingService
{
    private readonly IStableDiffusionClient _client;
    
    public async Task<string> InpaintMultipleAreasAsync(
        string imagePath,
        List<(Rectangle area, string prompt)> tasks)
    {
        using var image = Image.Load(imagePath);
        string currentImageBase64 = ImageHelper.ImageToBase64(imagePath);
        
        foreach (var (area, prompt) in tasks)
        {
            Console.WriteLine($"Processing area: {prompt}");
            
            // Create mask for current area
            var maskGen = new MaskGenerator();
            var maskPath = maskGen.CreateRectangleMask(
                image.Width, 
                image.Height, 
                area
            );
            var maskBase64 = ImageHelper.ImageToBase64(maskPath);
            
            // Inpaint current area
            var request = new ImageToImageRequest
            {
                InitImages = new List<string> { currentImageBase64 },
                Mask = maskBase64,
                Prompt = prompt,
                DenoisingStrength = 0.85,
                Steps = 30,
                InpaintingFill = 1,
                InpaintFullRes = true,
                MaskBlur = 4
            };
            
            var response = await _client.ImageToImage.GenerateAsync(request);
            currentImageBase64 = response.Images[0];
            
            // Delete temporary mask
            File.Delete(maskPath);
        }
        
        // Save final result
        var outputPath = "multi_inpaint_result.png";
        ImageHelper.Base64ToImage(currentImageBase64, outputPath);
        
        return outputPath;
    }
}

// Usage
var service = new MultiStepInpaintingService(client);

var tasks = new List<(Rectangle, string)>
{
    (new Rectangle(100, 100, 200, 200), "a red apple"),
    (new Rectangle(400, 150, 150, 150), "a blue vase"),
    (new Rectangle(200, 400, 250, 200), "wooden table")
};

var result = await service.InpaintMultipleAreasAsync("room.png", tasks);
```

---

## Working with Large Batches

### Generating Image Dataset

```csharp
public class DatasetGenerator
{
    private readonly IStableDiffusionClient _client;
    
    public async Task GenerateDatasetAsync(
        List<string> prompts,
        int imagesPerPrompt,
        string outputFolder)
    {
        Directory.CreateDirectory(outputFolder);
        
        int totalImages = prompts.Count * imagesPerPrompt;
        int processedImages = 0;
        
        var progressBar = new Progress<int>(value =>
        {
            var percentage = (value * 100.0) / totalImages;
            Console.Write($"\rProgress: {value}/{totalImages} ({percentage:F1}%)");
        });
        
        foreach (var prompt in prompts)
        {
            // Safe folder name
            var folderName = string.Join("_", 
                prompt.Split(Path.GetInvalidFileNameChars()))
                .Substring(0, Math.Min(30, prompt.Length));
            
            var promptFolder = Path.Combine(outputFolder, folderName);
            Directory.CreateDirectory(promptFolder);
            
            for (int i = 0; i < imagesPerPrompt; i++)
            {
                var request = new TextToImageRequest
                {
                    Prompt = prompt,
                    Width = 512,
                    Height = 512,
                    Steps = 20,
                    Seed = -1  // Different seeds
                };
                
                var response = await _client.TextToImage.GenerateAsync(request);
                
                var imagePath = Path.Combine(promptFolder, $"image_{i:D4}.png");
                ImageHelper.Base64ToImage(response.Images[0], imagePath);
                
                // Save metadata
                var metadataPath = Path.Combine(promptFolder, $"image_{i:D4}.json");
                File.WriteAllText(metadataPath, response.Info);
                
                processedImages++;
                ((IProgress<int>)progressBar).Report(processedImages);
            }
        }
        
        Console.WriteLine($"\n✓ Dataset created: {totalImages} images");
    }
}

// Usage
var generator = new DatasetGenerator(client);

var prompts = new List<string>
{
    "a red car",
    "a blue house",
    "a green tree",
    "a yellow flower"
};

await generator.GenerateDatasetAsync(prompts, imagesPerPrompt: 100, "dataset");
```

### Smart Memory Management for Large Batches

```csharp
public class MemoryEfficientBatchGenerator
{
    private readonly IStableDiffusionClient _client;
    private readonly int _maxMemoryMb;
    
    public MemoryEfficientBatchGenerator(
        IStableDiffusionClient client, 
        int maxMemoryMb = 1024)
    {
        _client = client;
        _maxMemoryMb = maxMemoryMb;
    }
    
    public async Task GenerateLargeBatchAsync(
        List<string> prompts,
        string outputFolder)
    {
        Directory.CreateDirectory(outputFolder);
        
        // Determine sub-batch size based on available memory
        var estimatedImageSizeMb = 2; // Approximate size of one image in memory
        var batchSize = Math.Max(1, _maxMemoryMb / estimatedImageSizeMb / 2);
        
        Console.WriteLine($"Sub-batch size: {batchSize}");
        
        for (int i = 0; i < prompts.Count; i += batchSize)
        {
            var batch = prompts.Skip(i).Take(batchSize).ToList();
            Console.WriteLine($"Processing batch {i / batchSize + 1}...");
            
            foreach (var prompt in batch)
            {
                var request = new TextToImageRequest
                {
                    Prompt = prompt,
                    Steps = 20
                };
                
                var response = await _client.TextToImage.GenerateAsync(request);
                var path = Path.Combine(outputFolder, $"image_{i}.png");
                ImageHelper.Base64ToImage(response.Images[0], path);
                
                // Free memory
                response = null;
            }
            
            // Force garbage collection between batches
            GC.Collect();
            GC.WaitForPendingFinalizers();
            
            Console.WriteLine($"✓ Batch {i / batchSize + 1} completed");
        }
    }
}
```

---

## Database Integration

### Saving Generation History in SQLite

```csharp
using Microsoft.Data.Sqlite;
using System.Text.Json;

public class GenerationHistoryService
{
    private readonly string _connectionString;
    private readonly IStableDiffusionClient _client;
    
    public GenerationHistoryService(
        IStableDiffusionClient client,
        string dbPath = "generation_history.db")
    {
        _client = client;
        _connectionString = $"Data Source={dbPath}";
        InitializeDatabase();
    }
    
    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        
        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS generations (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                prompt TEXT NOT NULL,
                negative_prompt TEXT,
                width INTEGER,
                height INTEGER,
                steps INTEGER,
                cfg_scale REAL,
                sampler TEXT,
                seed INTEGER,
                image_path TEXT,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                metadata TEXT
            )";
        command.ExecuteNonQuery();
    }
    
    public async Task<long> GenerateAndSaveAsync(
        TextToImageRequest request,
        string outputFolder = "outputs")
    {
        Directory.CreateDirectory(outputFolder);
        
        // Generation
        var response = await _client.TextToImage.GenerateAsync(request);
        
        // Save image
        var fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}.png";
        var imagePath = Path.Combine(outputFolder, fileName);
        ImageHelper.Base64ToImage(response.Images[0], imagePath);
        
        // Save to DB
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        
        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO generations (
                prompt, negative_prompt, width, height, steps, 
                cfg_scale, sampler, seed, image_path, metadata
            ) VALUES (
                @prompt, @negative_prompt, @width, @height, @steps,
                @cfg_scale, @sampler, @seed, @image_path, @metadata
            );
            SELECT last_insert_rowid();";
        
        command.Parameters.AddWithValue("@prompt", request.Prompt ?? "");
        command.Parameters.AddWithValue("@negative_prompt", request.NegativePrompt ?? "");
        command.Parameters.AddWithValue("@width", request.Width);
        command.Parameters.AddWithValue("@height", request.Height);
        command.Parameters.AddWithValue("@steps", request.Steps);
        command.Parameters.AddWithValue("@cfg_scale", request.CfgScale);
        command.Parameters.AddWithValue("@sampler", request.SamplerName ?? "");
        command.Parameters.AddWithValue("@seed", request.Seed);
        command.Parameters.AddWithValue("@image_path", imagePath);
        command.Parameters.AddWithValue("@metadata", response.Info ?? "");
        
        var id = (long)command.ExecuteScalar();
        
        Console.WriteLine($"✓ Generation saved: ID {id}");
        return id;
    }
    
    public List<GenerationRecord> SearchByPrompt(string searchTerm)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM generations 
            WHERE prompt LIKE @search 
            ORDER BY created_at DESC 
            LIMIT 100";
        command.Parameters.AddWithValue("@search", $"%{searchTerm}%");
        
        var results = new List<GenerationRecord>();
        using var reader = command.ExecuteReader();
        
        while (reader.Read())
        {
            results.Add(new GenerationRecord
            {
                Id = reader.GetInt64(0),
                Prompt = reader.GetString(1),
                ImagePath = reader.GetString(9),
                CreatedAt = reader.GetDateTime(10)
            });
        }
        
        return results;
    }
}

public class GenerationRecord
{
    public long Id { get; set; }
    public string Prompt { get; set; }
    public string ImagePath { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Usage
var historyService = new GenerationHistoryService(client);

var request = new TextToImageRequest
{
    Prompt = "beautiful sunset",
    Steps = 30
};

var id = await historyService.GenerateAndSaveAsync(request);

// Search history
var results = historyService.SearchByPrompt("sunset");
foreach (var record in results)
{
    Console.WriteLine($"ID: {record.Id}, Prompt: {record.Prompt}");
}
```

---

## RESTful API Service

### Creating ASP.NET Core API

```csharp
// Program.cs
using Microsoft.AspNetCore.Mvc;
using StableDiffusionNet;
using StableDiffusionNet.DependencyInjection.Extensions;
using StableDiffusionNet.Models.Requests;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register StableDiffusion client
builder.Services.AddStableDiffusion(options =>
{
    options.BaseUrl = builder.Configuration["StableDiffusion:BaseUrl"] 
        ?? "http://localhost:7860";
    options.TimeoutSeconds = 600;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Generation endpoint
app.MapPost("/api/generate", async (
    [FromBody] GenerateRequest request,
    [FromServices] IStableDiffusionClient client) =>
{
    try
    {
        var sdRequest = new TextToImageRequest
        {
            Prompt = request.Prompt,
            NegativePrompt = request.NegativePrompt,
            Width = request.Width ?? 512,
            Height = request.Height ?? 512,
            Steps = request.Steps ?? 30,
            Seed = request.Seed ?? -1
        };
        
        var response = await client.TextToImage.GenerateAsync(sdRequest);
        
        return Results.Ok(new
        {
            success = true,
            images = response.Images,
            info = response.Info
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new
        {
            success = false,
            error = ex.Message
        });
    }
})
.WithName("Generate")
.WithOpenApi();

// Get models endpoint
app.MapGet("/api/models", async ([FromServices] IStableDiffusionClient client) =>
{
    var models = await client.Models.GetModelsAsync();
    return Results.Ok(models.Select(m => new
    {
        title = m.Title,
        name = m.ModelName,
        hash = m.Hash
    }));
})
.WithName("GetModels")
.WithOpenApi();

// Health check endpoint
app.MapGet("/api/health", async ([FromServices] IStableDiffusionClient client) =>
{
    var isAvailable = await client.PingAsync();
    return Results.Ok(new
    {
        status = isAvailable ? "healthy" : "unhealthy",
        timestamp = DateTime.UtcNow
    });
})
.WithName("HealthCheck")
.WithOpenApi();

app.Run();

// DTO models
public record GenerateRequest(
    string Prompt,
    string? NegativePrompt = null,
    int? Width = null,
    int? Height = null,
    int? Steps = null,
    int? Seed = null
);
```

---

## Testing and Custom Services

### Using Custom Service Factory for Testing

The library supports custom service factories, making it easy to test your code with mock services:

```csharp
using Moq;
using StableDiffusionNet;
using StableDiffusionNet.Interfaces;

public class CustomServiceFactory : IStableDiffusionServiceFactory
{
    private readonly ITextToImageService _mockTextToImage;
    
    public CustomServiceFactory(ITextToImageService mockTextToImage)
    {
        _mockTextToImage = mockTextToImage;
    }
    
    public ITextToImageService CreateTextToImageService(
        IHttpClientWrapper httpClient,
        IStableDiffusionLogger logger)
    {
        return _mockTextToImage;
    }
    
    // Implement other methods with default implementations or mocks
    // ...
}

// In your test
[Fact]
public async Task TestImageGeneration()
{
    // Arrange
    var mockTextToImage = new Mock<ITextToImageService>();
    mockTextToImage
        .Setup(s => s.GenerateAsync(It.IsAny<TextToImageRequest>(), default))
        .ReturnsAsync(new TextToImageResponse 
        { 
            Images = new List<string> { "base64image" } 
        });
    
    var factory = new CustomServiceFactory(mockTextToImage.Object);
    
    var client = new StableDiffusionClientBuilder()
        .WithBaseUrl("http://localhost:7860")
        .WithServiceFactory(factory)
        .Build();
    
    // Act
    var request = new TextToImageRequest { Prompt = "test" };
    var response = await client.TextToImage.GenerateAsync(request);
    
    // Assert
    Assert.NotNull(response);
    Assert.Single(response.Images);
    mockTextToImage.Verify(
        s => s.GenerateAsync(It.IsAny<TextToImageRequest>(), default),
        Times.Once
    );
}
```

### Integration Testing with Real API

```csharp
public class StableDiffusionIntegrationTests : IAsyncLifetime
{
    private IStableDiffusionClient _client;
    
    public async Task InitializeAsync()
    {
        _client = new StableDiffusionClientBuilder()
            .WithBaseUrl("http://localhost:7860")
            .WithTimeout(600)
            .Build();
            
        // Check if API is available
        var isAvailable = await _client.PingAsync();
        if (!isAvailable)
        {
            throw new Exception("Stable Diffusion API is not available");
        }
    }
    
    public Task DisposeAsync()
    {
        _client?.Dispose();
        return Task.CompletedTask;
    }
    
    [Fact]
    public async Task Generate_SimplePrompt_ReturnsImage()
    {
        // Arrange
        var request = new TextToImageRequest
        {
            Prompt = "a red apple",
            Steps = 10, // Fast test
            Width = 256,
            Height = 256
        };
        
        // Act
        var response = await _client.TextToImage.GenerateAsync(request);
        
        // Assert
        Assert.NotNull(response);
        Assert.NotEmpty(response.Images);
        Assert.NotNull(response.Info);
    }
}
```

### Custom Service Implementation

You can implement custom services to add additional functionality:

```csharp
public class CachedTextToImageService : ITextToImageService
{
    private readonly ITextToImageService _innerService;
    private readonly IMemoryCache _cache;
    
    public CachedTextToImageService(
        ITextToImageService innerService,
        IMemoryCache cache)
    {
        _innerService = innerService;
        _cache = cache;
    }
    
    public async Task<TextToImageResponse> GenerateAsync(
        TextToImageRequest request,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GenerateCacheKey(request);
        
        // Try get from cache
        if (_cache.TryGetValue<TextToImageResponse>(cacheKey, out var cached))
        {
            Console.WriteLine("Cache hit!");
            return cached;
        }
        
        // Generate new image
        var response = await _innerService.GenerateAsync(request, cancellationToken);
        
        // Cache for 1 hour
        _cache.Set(cacheKey, response, TimeSpan.FromHours(1));
        
        return response;
    }
    
    private string GenerateCacheKey(TextToImageRequest request)
    {
        return $"{request.Prompt}_{request.Width}_{request.Height}_{request.Steps}_{request.Seed}";
    }
}

// Usage with custom factory
public class CachedServiceFactory : IStableDiffusionServiceFactory
{
    private readonly IMemoryCache _cache;
    private readonly DefaultServiceFactory _defaultFactory = new();
    
    public CachedServiceFactory(IMemoryCache cache)
    {
        _cache = cache;
    }
    
    public ITextToImageService CreateTextToImageService(
        IHttpClientWrapper httpClient,
        IStableDiffusionLogger logger)
    {
        var defaultService = _defaultFactory.CreateTextToImageService(httpClient, logger);
        return new CachedTextToImageService(defaultService, _cache);
    }
    
    // Other services use default implementation
    public IImageToImageService CreateImageToImageService(
        IHttpClientWrapper httpClient,
        IStableDiffusionLogger logger) 
        => _defaultFactory.CreateImageToImageService(httpClient, logger);
    
    // ... implement other methods
}

// Create client with caching
var cache = new MemoryCache(new MemoryCacheOptions());
var factory = new CachedServiceFactory(cache);

var client = new StableDiffusionClientBuilder()
    .WithBaseUrl("http://localhost:7860")
    .WithServiceFactory(factory)
    .Build();
```

---

## Monitoring and Metrics

### Collecting Generation Metrics

```csharp
using System.Diagnostics;

public class MetricsCollector
{
    private readonly List<GenerationMetrics> _metrics = new();
    
    public async Task<(TextToImageResponse response, GenerationMetrics metrics)> 
        GenerateWithMetricsAsync(
            IStableDiffusionClient client,
            TextToImageRequest request)
    {
        var metrics = new GenerationMetrics
        {
            Timestamp = DateTime.UtcNow,
            Prompt = request.Prompt,
            Width = request.Width,
            Height = request.Height,
            Steps = request.Steps
        };
        
        var sw = Stopwatch.StartNew();
        
        try
        {
            var response = await client.TextToImage.GenerateAsync(request);
            
            sw.Stop();
            metrics.DurationSeconds = sw.Elapsed.TotalSeconds;
            metrics.Success = true;
            metrics.ImageCount = response.Images.Count;
            
            _metrics.Add(metrics);
            
            return (response, metrics);
        }
        catch (Exception ex)
        {
            sw.Stop();
            metrics.DurationSeconds = sw.Elapsed.TotalSeconds;
            metrics.Success = false;
            metrics.ErrorMessage = ex.Message;
            
            _metrics.Add(metrics);
            throw;
        }
    }
    
    public void PrintStatistics()
    {
        if (_metrics.Count == 0)
        {
            Console.WriteLine("No data for statistics");
            return;
        }
        
        var successful = _metrics.Where(m => m.Success).ToList();
        
        Console.WriteLine("\n=== Generation Statistics ===");
        Console.WriteLine($"Total generations: {_metrics.Count}");
        Console.WriteLine($"Successful: {successful.Count}");
        Console.WriteLine($"Errors: {_metrics.Count - successful.Count}");
        
        if (successful.Any())
        {
            Console.WriteLine($"\nGeneration time:");
            Console.WriteLine($"  Average: {successful.Average(m => m.DurationSeconds):F2}s");
            Console.WriteLine($"  Minimum: {successful.Min(m => m.DurationSeconds):F2}s");
            Console.WriteLine($"  Maximum: {successful.Max(m => m.DurationSeconds):F2}s");
            
            Console.WriteLine($"\nTotal images generated: {successful.Sum(m => m.ImageCount)}");
        }
    }
    
    public void ExportToCsv(string filePath)
    {
        using var writer = new StreamWriter(filePath);
        writer.WriteLine("Timestamp,Prompt,Width,Height,Steps,Duration,Success,ImageCount,Error");
        
        foreach (var m in _metrics)
        {
            writer.WriteLine($"{m.Timestamp:yyyy-MM-dd HH:mm:ss}," +
                           $"\"{m.Prompt}\"," +
                           $"{m.Width},{m.Height},{m.Steps}," +
                           $"{m.DurationSeconds:F2}," +
                           $"{m.Success}," +
                           $"{m.ImageCount}," +
                           $"\"{m.ErrorMessage}\"");
        }
        
        Console.WriteLine($"✓ Metrics exported: {filePath}");
    }
}

public class GenerationMetrics
{
    public DateTime Timestamp { get; set; }
    public string Prompt { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int Steps { get; set; }
    public double DurationSeconds { get; set; }
    public bool Success { get; set; }
    public int ImageCount { get; set; }
    public string ErrorMessage { get; set; }
}

// Usage
var collector = new MetricsCollector();

for (int i = 0; i < 10; i++)
{
    var request = new TextToImageRequest
    {
        Prompt = $"test prompt {i}",
        Steps = 20
    };
    
    var (response, metrics) = await collector.GenerateWithMetricsAsync(client, request);
    Console.WriteLine($"Generation completed in {metrics.DurationSeconds:F2}s");
}

collector.PrintStatistics();
collector.ExportToCsv("metrics.csv");
```

---

For basic examples see [EXAMPLES.md](EXAMPLES.md).

Complete API reference: [API_REFERENCE.md](API_REFERENCE.md).


