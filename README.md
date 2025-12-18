# Power Automate Search

This is a tool created to help search Power Automate flows, workflows, web resources, and plugins across your Dataverse environments. It started as a basic search tool and has somewhat grown with several performance optimizations and search features.

Please feel free to suggest commits/changes and new features/bug fixes/optimizations!

## Installation:

## For End Users (Installing the Plugin)

### Option 1: XrmToolBox Plugin Store (Recommended - Coming Soon)

1. Open XrmToolBox
2. Click Tool Library
3. Search for **"Power Automate Search"**
4. Click Install

### Option 2: Manual Installation (Install DLL - Currently Recommended)

#### Step 1: Download the Plugin

- Download the latest `paSearch.dll` from the [Releases page](https://github.com/addisonfischer/Power-Automate-Search/releases)

#### Step 2: Locate XrmToolBox Plugins Folder

Default location:

```
C:\Users\[YourUsername]\AppData\Roaming\MscrmTools\XrmToolBox\Plugins\
```

Alternative locations:

- `%APPDATA%\MscrmTools\XrmToolBox\Plugins\`
- Where you installed XrmToolBox: `[XrmToolBox Install Dir]\Plugins\`

#### Step 3: Copy DLL

1. Copy `paSearch.dll` to the Plugins folder
2. **Important:** Also copy these dependencies if they don't exist:
   - `ICSharpCode.Decompiler.dll` (for code search feature)
   - `System.Reflection.Metadata.dll`
   - `System.Collections.Immutable.dll`

#### Step 4: Restart XrmToolBox

1. Close XrmToolBox completely
2. Reopen XrmToolBox
3. The plugin should appear in the Tools menu

#### Step 5: Open the Plugin

- Click Tools → **Power Automate Search**
- Connect to your Dataverse environment
- Start searching!

---

## For Developers (Building from Source)

### Prerequisites

**Required:**

- Visual Studio 2019 or later
- .NET Framework 4.8 SDK
- Git

**Recommended:**

- XrmToolBox (lol)

### Step 1: Clone the Repository

```bash
git clone https://github.com/addisonfischer/Power-Automate-Search.git
cd Power-Automate-Search
```

### Step 2: Restore NuGet Packages

**Option A: Visual Studio (Recommended)**

1. Open `paSearch.sln` in Visual Studio
2. Right-click solution → **Restore NuGet Packages**
3. Wait for packages to download

**Option B: Command Line**

```bash
nuget restore paSearch.sln
```

**Key Dependencies:**

- `XrmToolBox.PluginBase` - XrmToolBox SDK
- `Microsoft.CrmSdk.CoreAssemblies` - Dynamics SDK
- `ICSharpCode.Decompiler` - Code search decompilation
- `Newtonsoft.Json` - JSON parsing

### Step 3: Build the Project

**Option A: Visual Studio**

1. Open `paSearch.sln`
2. Set configuration to **Release**
3. Build → **Build Solution** (Ctrl+Shift+B)
4. Output DLL: `bin\Release\paSearch.dll`

**Option B: MSBuild (Command Line)**

```bash
msbuild paSearch.sln /p:Configuration=Release
```

**Option C: dotnet CLI** (if using .NET Core SDK)

```bash
dotnet build paSearch.sln --configuration Release
```

### Step 4: Install for Testing

**Copy to XrmToolBox Plugins folder:**

```bash
# Windows (PowerShell)
Copy-Item "bin\Release\paSearch.dll" "$env:APPDATA\MscrmTools\XrmToolBox\Plugins\" -Force

# Also copy dependencies (if not already there)
Copy-Item "bin\Release\ICSharpCode.Decompiler.dll" "$env:APPDATA\MscrmTools\XrmToolBox\Plugins\" -Force
```

**Or manually:**

1. Navigate to `bin\Release\`
2. Copy `paSearch.dll` to XrmToolBox Plugins folder
3. Copy dependency DLLs if needed
4. Restart XrmToolBox

## Usage:

- Connect to your organization
- Search and go!
  - Select a category (Workflow, Cloud Flow, Web Resource, etc.) or search all
  - Leaving the search blank will return everything in the selected category
  - Double clicking a line will copy the Solution ID of the object

### Search Options:

- **Fuzzy Search**: Check this to find results even with typos (e.g., "varCstmrNme" will find "varCustomerName")
- **Search Content**: For Web Resources, check this to search inside file contents. Expect slightly longer search times. Leave unchecked for faster name-only searches.
- **Search Code**: For plugins, check this to decompile plugin code back to dotnet/C#. Expect significantly longer search times. Leave unchecked for faster entity/message/stage-only searches.

### What it Searches:

- **Workflows/Flows**: Searches names and content (variables, expressions, actions inside flows)
- **Web Resources**: Searches names and optionally file contents (HTML, JavaScript, CSS, etc.)
- **Solutions**: Automatically shows which solution contains each object
- **Plugins**: Searches names, entities, messages, stages of plugins, as well as optionally code snippets.

## Features:

- Deep content search inside Power Automate flow definitions
- Web resource search (by name or file content)
- Plugin search (by name/entity/message/stage or code)
- Fuzzy search for typo tolerance
- Optimized batch queries for fast performance

## Future improvements:

- Automatic opening of solutions upon double click (open to suggestions on how to do this)
- Double click plugins and open PluginRegistrationTool (or similar)
- Content preview window
- Export results to CSV/Excel
- Search history
- Additional search filters
