# Project Initializer

A utility that helps initializing new unity projects.

## Installation
1. Open "Package Manager"
2. Choose "Add package from git URL..."
3. Use the HTTPS URL of this repository:
```
4. Enter `https://github.com/yanicksenn/com.yanicksenn.projectinitializer.git`
```
4. Click "Add"

## Features

### Project Initialization

The project initializer helps setting up a new project by creating a standard folder structure, installing common packages and copying resources.

### Violation Scanner

The violation scanner helps keeping the project clean by enforcing naming conventions and folder structures.


## How to Use

### Initialize Project

1. Open the project initializer window via `Tools/Project Setup/Initialize Project`
2. Select the folders to create
3. Select the packages to install
4. Select the resources to copy
5. Click **Initialize Project**

### Scan/Fix Violations

1. Open the violation scanner window via `Tools/Project Setup/Violation Scanner`
2. Click **Scan for Violations**
3. Select the violations to fix
4. Click **Fix Selected**

## Creating Custom Violations

You can extend the violation scanner by creating custom detectors and handlers.

### 1. Define the Violation

Create a class that implements `IViolation` to hold the violation data.

```csharp
public class MyCustomViolation : IViolation {
    public string Title => "My Custom Violation";
    public string SubTitle => "Asset Path";
    public string Description => "This asset violates a custom rule.";
    public bool IsSelected { get; set; }
    
    // Add custom data properties here
    public string AssetPath { get; set; }
}
```

### 2. Create a Detector

Implement `IViolationDetector` and add the `[ViolationDetector]` attribute.

```csharp
[ViolationDetector]
public class MyCustomViolationDetector : IViolationDetector {
    public IEnumerable<IViolation> Detect() {
        // Logic to find violations
        yield return new MyCustomViolation {
            AssetPath = "Assets/SomeFile.txt"
        };
    }
}
```

### 3. Create a Handler

Implement `IViolationHandler` and add the `[ViolationHandler]` attribute targeting your violation type.

```csharp
[ViolationHandler(typeof(MyCustomViolation))]
public class MyCustomViolationHandler : IViolationHandler {
    public void Fix(IViolation violation) {
        if (violation is MyCustomViolation myViolation) {
            // Logic to fix the violation
            Debug.Log($"Fixing {myViolation.AssetPath}");
        }
    }
}
```