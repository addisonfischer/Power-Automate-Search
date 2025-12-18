# Power Automate Search

This is a tool created to help search Power Automate flows, workflows, web resources, and plugins across your Dataverse environments. It started as a basic search tool and has somewhat grown with several performance optimizations and search features.

Please feel free to suggest commits/changes and new features/bug fixes/optimizations!

## Installation:

##### Option 1:

Download paSearch.dll from the releases tab and move the file into your XrmToolBox plugin directory
(Win: %APPDATA%\MscrmTools\XrmToolBox\Plugins)

##### Option 2:

Wait for the tool to be registered with XrmToolBox and search within the app!

##### Option 3:

Clone the repo and build the project yourself

```bash
git clone https://github.com/AddisonFischer/Power-Automate-Search.git
cd Power-Automate-Search
msbuild paSearch.sln
```

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
