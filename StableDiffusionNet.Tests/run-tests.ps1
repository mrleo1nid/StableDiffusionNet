# PowerShell скрипт для запуска тестов с разными категориями
param(
    [string]$Category = "all",
    [switch]$Verbose
)

$verbosity = if ($Verbose) { "detailed" } else { "normal" }

Write-Host "🧪 Запуск тестов StableDiffusionNet" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

switch ($Category.ToLower()) {
    "unit" {
        Write-Host "▶ Запуск только Unit-тестов..." -ForegroundColor Green
        dotnet test --filter "Category!=Integration" --verbosity $verbosity
    }
    "integration" {
        Write-Host "▶ Запуск всех интеграционных тестов..." -ForegroundColor Yellow
        Write-Host "⚠️  Убедитесь что Stable Diffusion WebUI запущен!" -ForegroundColor Yellow
        Write-Host ""
        dotnet test --filter "Category=Integration" --verbosity $verbosity
    }
    "smoke" {
        Write-Host "▶ Запуск быстрых smoke-тестов..." -ForegroundColor Magenta
        Write-Host "⚠️  Убедитесь что Stable Diffusion WebUI запущен!" -ForegroundColor Yellow
        Write-Host ""
        dotnet test --filter "Category=Smoke" --verbosity $verbosity
    }
    "long" {
        Write-Host "▶ Запуск долгих интеграционных тестов..." -ForegroundColor Red
        Write-Host "⚠️  Убедитесь что Stable Diffusion WebUI запущен!" -ForegroundColor Yellow
        Write-Host "⏱️  Это может занять несколько минут..." -ForegroundColor Yellow
        Write-Host ""
        dotnet test --filter "Category=LongRunning" --verbosity $verbosity
    }
    "fast-integration" {
        Write-Host "▶ Запуск интеграционных тестов без долгих..." -ForegroundColor Blue
        Write-Host "⚠️  Убедитесь что Stable Diffusion WebUI запущен!" -ForegroundColor Yellow
        Write-Host ""
        dotnet test --filter "Category=Integration&Category!=LongRunning" --verbosity $verbosity
    }
    "all" {
        Write-Host "▶ Запуск ВСЕХ тестов..." -ForegroundColor White
        Write-Host "⚠️  Убедитесь что Stable Diffusion WebUI запущен для интеграционных тестов!" -ForegroundColor Yellow
        Write-Host ""
        dotnet test --verbosity $verbosity
    }
    default {
        Write-Host "❌ Неизвестная категория: $Category" -ForegroundColor Red
        Write-Host ""
        Write-Host "Доступные категории:" -ForegroundColor Cyan
        Write-Host "  unit              - Только unit-тесты (быстрые)" -ForegroundColor White
        Write-Host "  integration       - Все интеграционные тесты" -ForegroundColor White
        Write-Host "  smoke             - Быстрые smoke-тесты (проверка API)" -ForegroundColor White
        Write-Host "  fast-integration  - Интеграционные без долгих" -ForegroundColor White
        Write-Host "  long              - Только долгие тесты" -ForegroundColor White
        Write-Host "  all               - Все тесты (по умолчанию)" -ForegroundColor White
        Write-Host ""
        Write-Host "Примеры:" -ForegroundColor Cyan
        Write-Host "  .\run-tests.ps1 -Category unit" -ForegroundColor Gray
        Write-Host "  .\run-tests.ps1 -Category smoke -Verbose" -ForegroundColor Gray
        exit 1
    }
}

Write-Host ""
Write-Host "═══════════════════════════════════════" -ForegroundColor Cyan
Write-Host "✅ Тестирование завершено!" -ForegroundColor Green

