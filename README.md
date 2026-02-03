# Puzzle Gallery

![simple-art_screen-record](https://github.com/user-attachments/assets/975d8088-754f-4607-aea9-e711406a7123)

[![Unity Version](https://img.shields.io/badge/Unity-2022.3.62f3%20LTS-blue)](https://unity.com)
[![Platform](https://img.shields.io/badge/Platform-Android%20%7C%20iOS-green)](https://unity.com)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

Unity mobile puzzle gallery application.

## Overview

Mobile gallery demo showcasing MVP, Service Locator, State Machine, and Event Bus patterns. Optimized for mobile with scalable architecture.

## Key Features

### User-Facing Features
- **Banner Carousel** - Auto-scroll with 5s intervals
- **Filterable Gallery** - 66 remote images from GitHub Pages, tab filtering (All, Odd, Even)
- **Adaptive Layouts** - Phone (2 cols) / Tablet (3 cols), 7" threshold
- **Full-Screen Viewer** - Click to view images
- **Premium System** - Popup and subscription features (every 4th image)
- **Animations** - DOTween transitions

### Technical Highlights
- **Virtualized Scroll** - 1000+ items at 60fps, renders only visible cells
- **LRU Cache** - 100 sprite max, 4 concurrent downloads
- **Object Pooling** - Eliminates GC spikes
- **Lazy Loading** - On-demand via Addressables
- **UniTask** - Zero-allocation async
- **Remote Images** - HTTPS loading with retry logic

## Architecture

- **MVP** - Separation of UI, logic, and data
- **Service Locator** - DI container with lifecycle management
- **State Machine** - `Splash → Menu ↔ Puzzle` with lazy loading
- **Event Bus** - Publish-subscribe for loose coupling
- **Factory Pattern** - Presenter creation with dependency resolution
- **Bootstrap Pipeline** - Topological service initialization
- **Command Queue** - Sequential async execution

See [ARCHITECTURE.md](ARCHITECTURE.md) for details.

## Tech Stack

**Framework:** Unity 2022.3.62f3 LTS
**Platform:** Mobile (Android/iOS), IL2CPP, 60fps

**Libraries:**
- **UniTask** - Zero-allocation async
- **DOTween** - Animations
- **Addressables** - Asset management
- **URP** - Universal Render Pipeline
- **TextMeshPro** - Text rendering

## Project Structure

```
Assets/PuzzleGallery/
├── Scripts/Runtime/
│   ├── ApplicationLayer/       # Application-specific code
│   │   ├── Bootstrap/          # Entry point, initialization
│   │   ├── Configs/            # Config interfaces
│   │   └── Features/           # Splash, MainMenu (Carousel, Gallery, TabBar), Puzzle, Premium
│   ├── Core/                   # MVP, ServiceLocator, StateMachine, EventBus, Bootstrap abstractions
│   ├── Services/               # Asset, Logging, Popup, RemoteImage, ResourcesAccess, Save, Screen
│   └── UI/                     # VirtualizedScroll, Animations, Components
├── Configs/                    # ScriptableObject configs
├── Prefabs/                    # UI prefabs
├── Scenes/                     # Bootstrapper.unity
└── Art/                        # Sprites, fonts, icons
```

## Getting Started

**Requirements:** Unity 2022.3.62f3+, Git

```bash
git clone <repository-url>
cd puzzle-gallery-unity-dev
```

1. Open in Unity
2. Open [Assets/PuzzleGallery/Scenes/Bootstrapper.unity](Assets/PuzzleGallery/Scenes/Bootstrapper.unity)
3. Press Play or build via `File > Build Settings`

**Note:** Rebuild Addressables after modifications: `Window > Asset Management > Addressables > Build > Default Build Script`

## Key Systems

### Virtualized Scroll
- Renders ~10-15 visible cells for any data size
- 1000+ items at 60fps, RecyclePool eliminates GC spikes
- [UI/VirtualizedScroll/](Assets/PuzzleGallery/Scripts/Runtime/UI/VirtualizedScroll/)

### Remote Image Loading
- LRU cache (100 sprite max), max 4 concurrent downloads, 30s timeout
- Loading indicators, fade-in animations, batch display
- [UI/Components/RemoteImage.cs](Assets/PuzzleGallery/Scripts/Runtime/UI/Components/RemoteImage.cs), [Services/RemoteImage/](Assets/PuzzleGallery/Scripts/Runtime/Services/RemoteImage/)

### State Machine
- Flow: `SplashState → MainMenuState ↔ PuzzleState`
- Views load on first entry, stay loaded for instant transitions
- [Core/StateMachine/](Assets/PuzzleGallery/Scripts/Runtime/Core/StateMachine/)

### Bootstrap System
- Topological dependency sorting, 7 services
- [ApplicationLayer/Bootstrap/Bootstrapper.cs](Assets/PuzzleGallery/Scripts/Runtime/ApplicationLayer/Bootstrap/Bootstrapper.cs), [BOOTSTRAP_SETUP.md](BOOTSTRAP_SETUP.md)

### Service Locator
- DI container with `Initialize()` and `Dispose()` lifecycle
- Safe access via `TryGet<T>()`
- [Core/ServiceLocator/ServiceLocator.cs](Assets/PuzzleGallery/Scripts/Runtime/Core/ServiceLocator/ServiceLocator.cs)

## Configuration

ScriptableObject-based configs in [Assets/PuzzleGallery/Configs/](Assets/PuzzleGallery/Configs/)

Main: [AppConfig.asset](Assets/PuzzleGallery/Configs/AppConfig.asset)

Available configs: GalleryConfig, CarouselConfig, ScreenConfig, RemoteImageConfig, PremiumPopupConfig, MenuLayoutConfig, TabBarLayoutConfig, LogsConfig

Adjustable: image URLs, cache sizes, download limits, animation speeds, layout ratios, premium intervals, retry logic.

## Performance

- ~10-15 cells rendered (1000+ items supported)
- LRU cache: 100 sprite max
- Max 4 concurrent downloads, 30s timeout, 3 retries
- Object pooling: zero GC spikes
- Target: 60fps mobile
- UniTask: zero-allocation async

## License

MIT License

Copyright (c) 2026

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
