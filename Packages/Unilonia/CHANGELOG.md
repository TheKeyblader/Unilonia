# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.8.0] - 2020-11-17

### Added

- Added support Exprimental Play Mode

### Changed

- Updated Unity Version

## [0.7.0] - 2020-10-31

### Changed

-   Unity Dispatcher is now in an dll to allow to be used in external projects
-   Refactoring code

## [0.6.0] - 2020-10-26

### Added

-   New Samples ControlCatalog from Avalonia

### Fixed

-   Fixed resize of screen using mutex
-   Fixed YFlip of texture using shader
-   Fixed Color space using shader
-   Fixed Missing scroll

## [0.5.0] - 2020-10-24

### Added

-   Added UnityDispatcher allow to run unity function in avalonia
-   Now Avalonia has this own thread.
-   Added draw fps option
-   Added an global option "useDeferredRendering" with default at true
-   Added UniloniaPlatformThreadingInterface to manage threading of Avalonia Thread
-   Added an option "overrideApplicationType" on AvaloiniaView to override default Application type (for samples only)

### Changed

-   Avalonia use this own direct3d device
-   Updated input implementation to work with multithreading.
-   Updated clipboard implementation to work with multithreading.
-   Updated AvaloniaView Component to work with multithreading.
-   Updated AvaloniaApp class to work with multithreading.
-   Rewrite of AvaloniaView and TopLevelImpl to better separate Avalonia and Unity Code
-   Updated Samples
-   Updated Dependencies

### Fixed

-   Added some missing mouse buttons
-   Added some missing keyboard keys

### Removed

-   Removed PlatformThreadingInterface class used for single threading in unity

## [0.4.0] - 2020-10-09

### Added

-   New component AvaloniaView
-   Added global settings for unilonia

### Changed

-   Updated com.solidalloy.type.references from 2.1.0 to 2.2.3

### Removed

-   Removed AvaloniaApp component

## [0.3.0] - 2020-10-01

### Added

-   Added com.solidalloy.type.references for type dropdown

### Changed

-   Moved Unilonia Packages from Assets to Packages
-   Updated new Unilonia Boostrap
-   Updated github workflows

## [0.2.1] - 2020-09-30

-   Corrected package.json version
-   Fixed Samples Previewer

## [0.2.0] - 2020-09-30 [YANKED]

-   Failed build
-   Now using Unity 2019.4.11f1 for development (2020.2.0b4 before)
-   Fixe .meta file errors
-   Remove unused packages
-   Updated README.md
-   Updated Samples
-   Updated gitignore to not ignore AvaloniaUI project files

## [0.1.0] - 2020-09-30

### Added

-   Initial Release
