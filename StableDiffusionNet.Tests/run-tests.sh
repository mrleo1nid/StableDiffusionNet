#!/bin/bash

# Bash скрипт для запуска тестов с разными категориями

CATEGORY="${1:-all}"
VERBOSE="${2:-false}"

if [ "$VERBOSE" == "-v" ] || [ "$VERBOSE" == "--verbose" ]; then
    VERBOSITY="detailed"
else
    VERBOSITY="normal"
fi

echo "🧪 Запуск тестов StableDiffusionNet"
echo "═══════════════════════════════════════"
echo ""

case "$CATEGORY" in
    unit)
        echo "▶ Запуск только Unit-тестов..."
        dotnet test --filter "Category!=Integration" --verbosity $VERBOSITY
        ;;
    integration)
        echo "▶ Запуск всех интеграционных тестов..."
        echo "⚠️  Убедитесь что Stable Diffusion WebUI запущен!"
        echo ""
        dotnet test --filter "Category=Integration" --verbosity $VERBOSITY
        ;;
    smoke)
        echo "▶ Запуск быстрых smoke-тестов..."
        echo "⚠️  Убедитесь что Stable Diffusion WebUI запущен!"
        echo ""
        dotnet test --filter "Category=Smoke" --verbosity $VERBOSITY
        ;;
    long)
        echo "▶ Запуск долгих интеграционных тестов..."
        echo "⚠️  Убедитесь что Stable Diffusion WebUI запущен!"
        echo "⏱️  Это может занять несколько минут..."
        echo ""
        dotnet test --filter "Category=LongRunning" --verbosity $VERBOSITY
        ;;
    fast-integration)
        echo "▶ Запуск интеграционных тестов без долгих..."
        echo "⚠️  Убедитесь что Stable Diffusion WebUI запущен!"
        echo ""
        dotnet test --filter "Category=Integration&Category!=LongRunning" --verbosity $VERBOSITY
        ;;
    all)
        echo "▶ Запуск ВСЕХ тестов..."
        echo "⚠️  Убедитесь что Stable Diffusion WebUI запущен для интеграционных тестов!"
        echo ""
        dotnet test --verbosity $VERBOSITY
        ;;
    *)
        echo "❌ Неизвестная категория: $CATEGORY"
        echo ""
        echo "Доступные категории:"
        echo "  unit              - Только unit-тесты (быстрые)"
        echo "  integration       - Все интеграционные тесты"
        echo "  smoke             - Быстрые smoke-тесты (проверка API)"
        echo "  fast-integration  - Интеграционные без долгих"
        echo "  long              - Только долгие тесты"
        echo "  all               - Все тесты (по умолчанию)"
        echo ""
        echo "Примеры:"
        echo "  ./run-tests.sh unit"
        echo "  ./run-tests.sh smoke -v"
        exit 1
        ;;
esac

echo ""
echo "═══════════════════════════════════════"
echo "✅ Тестирование завершено!"

