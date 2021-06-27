# KristofferStrube.Blazor.SVGEditor
A basic HTML SVG Editor written in Blazor WASM.

![Showcase](./docs/showcase.gif?raw=true)

## Demo
The project can be demoed at [https://kristofferstrube.github.io/Blazor.SVGEditor/](https://kristofferstrube.github.io/Blazor.SVGEditor/)

## Tag type support and attributes
- RECT (x, y, width, height, fill, stroke, stroke-width)
- CIRCLE (cx, cy, r, fill, stroke, stroke-width)
- POLYGON (points, fill, stroke, stroke-width)
- PATH (d, fill, stroke, stroke-width)
    - Movements
    - Lines
    - Vertical Lines
    - Horizontal Lines
    - Close Path
    - Cubic Bézier Curve
        - Shorthand aswell
    - Quadratic Bézier Curve
        - Shorthand aswell
    - Elliptical Arc Curve
        - Needs more work for radi interaction

## Current goals
- Add support for touch devices
- Add support for scroll-zoom
- Add support for contextmenu-zoom
- Implement path creation action
- Show text in SVG.
- Implement text edit in SVG.
- Support more browsers using style attribute.
