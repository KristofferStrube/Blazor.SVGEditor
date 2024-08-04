export function getJSReference(element) { return element; }

export function targetElement(element) { return element.targetElement; }

export function subscribeToBegin(element, objRef) {
    element.addEventListener("beginEvent", () => objRef.invokeMethod("InvokeOnBegin"));
}

export function subscribeToEnd(element, objRef) {
    element.addEventListener("endEvent", () => objRef.invokeMethod("InvokeOnEnd"));
}

export function subscribeToRepeat(element, objRef) {
    element.addEventListener("repeatEvent", () => objRef.invokeMethod("InvokeOnRepeat"));
}