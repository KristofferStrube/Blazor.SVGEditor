# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.3.0] - 2024-03-31
### Fixed
- Fixed that `CompleteWithoutCloseMenuItem` would index out of bounds when used.
- Fixed that the `Text` shape had no estimate for its `SelectionPoints`.
### Added
- Added support for self-closing SVG tags.'
- Added bool parameter `HideElements` to define whether all elements should be hidden.
- Added action parameter `InputRendered` to get a callback every time the input has been rendered.
- Added utility functions `FitViewportToAllShapes`, `FitViewporToSelectedShapes`, and `FitViewportToShapes` for centering the viewport on some select shapes or all shapes.

## [0.2.1] - 2023-11-25
### Fixed
- Fixed that zoom on touch devices de-selected all shapes.

## [0.2.0] - 2023-10-22
### Added
- Added support for pinch-to-zoom on touch devices.

## [0.1.2] - 2023-10-04
### Fixed
- There was an accidental console log left in the code which has now been removed.

## [0.1.1] - 2023-07-18
### Fixed
- Fixed the functionality for moving elements back and forward
- Fixed that shapes be focused while the editor was in the Add EditMode.
