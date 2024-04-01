export function focus(element) { if (element) { element.focus() } }

export function BBox(element) { return element ? element.getBoundingClientRect() : {}; }

export function unFocus(element) { if (element) { element.blur() } }