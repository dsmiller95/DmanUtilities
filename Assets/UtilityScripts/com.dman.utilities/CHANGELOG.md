# Changelog



## [0.8.0] - 2025-04-01

### Added

- move async dependent code out to com.dman.task-utilities

## [0.7.0] - 2024-08-13

### Added

- new SingletonLocator, a generic way to find MonoBehavior singletons based on a static generic interface.
- StringDiffUtils provide line-based diffing for strings
- A new WhenAnyCancelAll<T> to allow returning data from WhenAnyCancelAll
- SetGlobalScale to set the global scale of a game object
- 
### Changed

- Logger -> Log , a more full-featured logger which captures information directly from the call site, with optional context

## [0.6.1] - 2024-06-08

### Added

- AsyncFnOnceCell . cs


## [0.6.0] - 2024-05-08

### Removed

- math extensions. use com.dman.math instead.


## [0.3.2] - 2021-02-24

### Added

- fix to the raycast group which breaks it when using the editor

## [0.3.1] - 2021-02-24

### Added

- create asset menu for raycast group

## [0.3.0] - 2021-02-24

mouse over raycast helper. remove isMouseDown check on the RaycastToObject method.

### Added

- RaycastGroup to cache raycasts into the environment

## [0.2.0] - 2021-01-16

serializable vectors.

### Added

- Serializable Vector2, 3, and 4 with implicit casts

## [0.1.0] - 2021-01-10

Initial release.

### Added

- Array extensions
- Game object child destruction helper
- Linq extensions
- Mouse over helpers
- parallel read single-threaded write JobHandle protector
- serializable Scene Reference
- Serializable Matrix4x4
- Vector extensions
