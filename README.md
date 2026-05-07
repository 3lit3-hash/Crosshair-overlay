# Crosshair Overlay

A high-performance, hardware-accelerated crosshair overlay built on C# WPF (.NET 8.0). Designed for competitive gaming with zero input lag.

## Features
- **Invisible to Clicks:** Fully transparent overlay (`WS_EX_TRANSPARENT`).
- **Dynamic Shapes & PNGs:** Configurable lines, dots, circles, triangles, and custom PNG imports.
- **Smart Tracking:** Auto-hides when out of game and natively follows window borders across displays.
- **Contrast Mode:** GDI-based real-time pixel inversion to guarantee crosshair visibility anywhere.
- **Global Hotkeys:** Panic key, Profile cycler, and ADS Smart Hide.
- **Profile System:** Save local configs or share them instantly via Base64 code strings.

## Build
```bash
dotnet run --project CrosshairOverlay.csproj
```

---

## [RU] Русский

Высокопроизводительный оверлей-прицел на C# WPF (.NET 8.0) с аппаратным ускорением и нулевой задержкой ввода.

## Возможности
- **Прозрачность:** Окно не перехватывает клики мыши (`WS_EX_TRANSPARENT`).
- **Формы и кастомные PNG:** Поддержка классического креста, точки, круга, треугольника и загрузки `.png`.
- **Умное отслеживание:** Авто-скрытие на Рабочем столе и автоматическая привязка оверлея к окну игры на любом мониторе.
- **Режим контраста:** Аппаратная инверсия пикселей фона в реальном времени. Прицел никогда не сольется с текстурами.
- **Глобальные шорткаты:** Panic Key, быстрое переключение сохраненных профилей и скрытие при зажатии прицела (ADS).
- **Система профилей:** Сохранение конфигов под разные игры и возможность передачи их через Base64-код.

## Запуск
```bash
dotnet run --project CrosshairOverlay.csproj
```
