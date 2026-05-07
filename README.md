# Crosshair Overlay

A high-performance, structurally resilient crosshair overlay built on C# WPF and .NET 8.0. Designed with competitive gaming and minimal system latency in mind, the application bypasses standard rendering overheads by explicitly integrating low-level Win32 APIs, Per-Monitor DPI scaling, and hardware-accelerated WPF Canvas primitives. The architecture is engineered to run seamlessly across multi-monitor setups and borderless windows.

## Core Features

*   **Non-Intrusive Layering:** The overlay securely utilizes `WS_EX_TRANSPARENT` and `WS_EX_LAYERED`, ensuring complete click-through functionality without interfering with window focus hierarchies.
*   **Dynamic Geometry Engine:** Switch natively between dynamically rendered vectors (Classic Cross, Center Dot, Holo Circle, Reflex Triangle) or import local transparent PNG sequences.
*   **Active Window Tracking (Auto-Hide):** Continuously scans for targeted application focus (`TargetProcesses`). If invoked, the overlay extracts physical process bounding-boxes (`GetWindowRect`) and automatically maps scale parameters to follow localized window dimensions across any display monitor.
*   **Global Hook Integrations:**
    *   **Panic Key:** Hardware-level keybinding to instantly terminate and restart the overlay rendering pipe.
    *   **Swap Profile:** Dynamic cyclic progression mapping available localized configurations.
    *   **Smart Hide (ADS):** Conceals the interface natively while holding secondary fire bounds (e.g., aiming down sights).
*   **Dynamic Matrix Inversion:** Utilizing optimized GDI queries (`GetPixel`), the engine actively samples coordinate backdrops at 20Hz increments to dynamically invert the brush RGB values, guaranteeing visual contrast irrespective of game environment lighting.
*   **Profile Ecosystem:** Generate, cache, and cycle through unlimited local `.json` configurations (tied to background game processes). Alternatively, deploy Base64 payload strings to safely share specific visual profiles across systems. 

## Build & Deployment

Developed using standard C# WPF architectures against runtime environment `net8.0-windows`. 

1. Requires **.NET 8.0 SDK**.
2. Build via terminal: `dotnet run --project CrosshairOverlay.csproj`
3. Or compile statically utilizing **Visual Studio 2022**.

---

## [RU] Русский

Высокопроизводительный оверлей-прицел, разработанный на базе C# WPF и .NET 8.0. Приложение сфокусировано на минимизации системных задержек и использует низкоуровневые Win32 API, Per-Monitor DPI масштабирование, а также аппаратное ускорение рендеринга графики. Архитектура адаптирована для работы на нескольких мониторах и в оконных режимах.

## Основной функционал

*   **Абсолютная прозрачность:** Оверлей аппаратно использует флаги `WS_EX_TRANSPARENT` и `WS_EX_LAYERED`, гарантируя кликабельность сквозь окно, не прерывая активный фокус игры.
*   **Генерация векторных форм:** Поддержка нескольких параметров отрисовки (Классический крест, Точка, Круг, Треугольник) с индивидуальной настройкой зазоров (Gap), толщины (Thickness) и смещений (X/Y Offsets), а также загрузка пользовательских PNG изображений.
*   **Автоматическое трекинг-скрытие (Auto-Hide):** Движок сканирует фокус системы на наличие целевых игровых процессов (`TargetProcesses`). Если заданная игра находится в активном фокусе, сканер захватывает ее физические рамки (`GetWindowRect`) и притягивает оверлей строго поверх этого окна даже на вторичных мониторах.
*   **Аппаратные глобальные хуки:**
    *   **Panic Key:** Глобальная горячая клавиша для моментального экстренного скрытия.
    *   **Swap Profile:** Быстрое циклическое переключение всех локальных профилей.
    *   **Smart Hide (ADS):** Отключение вывода на экран при удержании назначенной клавиши (полезно при прицеливании через снайперскую оптику).
*   **Матричная инверсия цвета (Contrast Mode):** С помощью `GDI` сканера (`GetPixel`), механизм считывает контраст изображения из игры на этапе рендера и аппаратно смещает цвета векторной кисти в анти-спектр, обеспечивая идеальную видимость крестика при любой гамме фона.
*   **Экосистема профилей:** Сохраняйте, редактируйте и автоматически переключайте конфигурации привязкой к `.exe` игры. Добавлен функционал дешифрации JSON-объектов с помощью Base64 кодировки (функция Share Profile) для безопасной и быстрой передачи настроек пользователям.

## Сборка и Запуск

Проект написан с использованием классического стека Windows Presentation Foundation.

1. Требуется установка **.NET 8.0 SDK**.
2. Ввод в системном терминале в корневой папке проекта: `dotnet run`
3. Возможна статическая сборка через среды формата **Visual Studio 2022**.
