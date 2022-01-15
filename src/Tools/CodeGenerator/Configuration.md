# Roslynator Configuration

Use EditorConfig file to configure analyzers, refactoring and compiler diagnostic fixes.

```editorconfig
# Set severity for all analyzers
dotnet_analyzer_diagnostic.category-roslynator.severity = default|none|silent|suggestion|warning|error

# Set severity for a specific analyzer
dotnet_diagnostic.<ANALYZER_ID>.severity = default|none|silent|suggestion|warning|error

# Enable/disable all refactorings
roslynator.refactorings.enabled = true|false

# Enable/disable specific refactoring
roslynator.refactoring.<REFACTORING_NAME>.enabled = true|false

# Enable/disable all fixes for compiler diagnostics
roslynator.compiler_diagnostic_fixes.enabled = true|false

# Enable/disable fix for a specific compiler diagnostics
roslynator.compiler_diagnostic_fix.<COMPILER_DIAGNOSTIC_ID>.enabled = true|false
```

## Default Configuration

If you want to configure Roslynator on a user-wide basis you have to use Roslynator config file.

## Location of Configuration File

Configuration file is located at `%LOCALAPPDATA%/JosefPihrt/Roslynator/.roslynatorconfig`.
Location of `%LOCALAPPDATA%` depends on the operating system:

| OS | Path |
| -------- | ------- |
| Windows | `C:/Users/<USERNAME>/AppData/Local/.roslynatorconfig` |
| Linux | `/home/<USERNAME>/.local/share/.roslynatorconfig` |
| OSX | `/Users/<USERNAME>/.local/share/.roslynatorconfig` |

Default configuration is loaded once when IDE starts. Therefore, it may be necessary to restart IDE for changes to take effect.
