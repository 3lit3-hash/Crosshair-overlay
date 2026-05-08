# Crosshair Overlay

A high-performance, hardware-accelerated crosshair overlay built on C# WPF (.NET 8.0). Designed for competitive gaming with zero input lag.

## Features
- **Invisible to Clicks:** Fully transparent overlay (`WS_EX_TRANSPARENT`).
- **Dynamic Shapes & PNGs:** Configurable lines, dots, circles, triangles, and custom PNG imports.
- **Pixel Builder:** Draw completely custom crosshairs natively inside a 16x16 interactive grid matrix.
- **Dual-Color Engine:** Separate independent color palettes for crosshair fill and vector outlines.
- **RGB Chroma:** Hardware-accelerated rainbow color loops running seamlessly at 60 FPS.
- **Contrast Mode:** GDI-based real-time pixel inversion to guarantee crosshair visibility anywhere.
- **Smart Tracking:** Auto-hides when out of game and natively follows window borders across displays.
- **Global Hotkeys:** Panic key, Profile cycler, and ADS Smart Hide natively bound.
- **Profile System:** Save local configs or share them instantly via Base64 payload strings.

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
- **Пиксельный конструктор (Pixel Builder):** Рисуйте свои уникальные формы прямо в программе на интерактивной сетке 16x16.
- **Кастомные обводки (Dual-Color):** Раздельная независимая настройка цветов для самого прицела и его контурной тени.
- **RGB Chroma:** Аппаратное плавное переливание прицела всеми цветами радуги при стабильных 60 FPS.
- **Режим контраста:** Аппаратная инверсия цвета пикселей фона в реальном времени. Ультимативная видимость.
- **Умное отслеживание:** Авто-скрытие на Рабочем столе и автоматическая привязка оверлея к окну игры на любом мониторе.
- **Глобальные шорткаты:** Panic Key, быстрое переключение профилей через клавиатуру и скрытие при зажатии прицела (ADS).
- **Система профилей:** Сохранение конфигов под разные игры и возможность передачи их через легкий Base64 код.

## Запуск
```bash
dotnet run --project CrosshairOverlay.csproj
```
