using McTools.Xrm.Connection;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ServiceModel;
using System.Windows.Forms;
using XrmToolBox.Extensibility;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Crm.Sdk.Messages;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;

namespace paSearch
{
    public partial class MyPluginControl : PluginControlBase
    {
        #region Constants
        private const int COMPONENT_TYPE_WORKFLOW = 29;
        private const int COMPONENT_TYPE_WEBRESOURCE = 61;
        private const int COMPONENT_TYPE_PLUGINTYPE = 90;
        private const int SEARCH_TIMEOUT_SECONDS = 300;
        
        // Search type identifiers
        private const int SEARCH_TYPE_WEBRESOURCE = -2;
        private const int SEARCH_TYPE_PLUGIN = -3;
        
        // Performance settings
        private const int DEFAULT_MAX_RESULTS = 500;
        private const int PROGRESS_UPDATE_INTERVAL = 10;
        #endregion

        #region Fields
        private Settings mySettings;
        public int selectedCategory = -1;
        private string currentEnvironmentId;
        private string searchText = string.Empty;
        
        // Fuzzy search settings
        private bool useFuzzySearch = false;
        private int fuzzyThreshold = 2; // Simple fuzzy matching threshold
        
        // Performance settings
        private int maxResults = DEFAULT_MAX_RESULTS;
        
        // WebResource search settings
        private bool searchWebResourceContent = false; // Default: name search only (FAST)
        
        // Plugin code search settings
        private bool searchPluginCode = false; // Default: metadata only (FAST)
        #endregion

        // Static category mapping to avoid recreation
        private static readonly Dictionary<string, int> CategoryMapping = new Dictionary<string, int>
        {
            { "All", -1 },
            { "Workflow", 0 },
            { "Dialog", 1 },
            { "Business Rule", 2 },
            { "Action", 3 },
            { "Business Process", 4 },
            { "Modern/Cloud", 5 },
            { "Desktop", 6 },
            { "AI", 7 },
            { "Web Resource", SEARCH_TYPE_WEBRESOURCE },
            { "Plugin", SEARCH_TYPE_PLUGIN }
        };

        #region Initialization
        public MyPluginControl()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
        }

        private void MyPluginControl_Load(object sender, EventArgs e)
        {
            ShowInfoNotification("Please feel free to check out the repo and suggest features or contribute!", 
                new Uri("https://github.com/addisonfischer/Power-Automate-Search"));

            if (!SettingsManager.Instance.TryLoad(GetType(), out mySettings))
            {
                mySettings = new Settings();
                LogWarning("Settings not found => a new settings file has been created!");
            }
            else
            {
                LogInfo("Settings found and loaded");
                
                if (mySettings.RememberLastSearch && !string.IsNullOrEmpty(mySettings.LastSearchText))
                {
                    searchTextBox.Text = mySettings.LastSearchText;
                }
                
                var lastCategory = CategoryMapping.FirstOrDefault(x => x.Value == mySettings.DefaultSearchCategory);
                if (!lastCategory.Equals(default(KeyValuePair<string, int>)))
                {
                    comboBox1.SelectedItem = lastCategory.Key;
                }
                else
                {
                    comboBox1.SelectedItem = "All";
                }
                
                // Load performance settings
                if (mySettings.MaxSearchResults > 0)
                {
                    maxResults = mySettings.MaxSearchResults;
                }
                useFuzzySearch = mySettings.UseFuzzySearch;
                fuzzyThreshold = mySettings.FuzzyThreshold;
                searchPluginCode = mySettings.EnablePluginCodeSearch;
                
                // Update UI if fuzzy checkbox exists
                if (fuzzySearchCheckBox != null)
                {
                    fuzzySearchCheckBox.Checked = useFuzzySearch;
                }
                
                // Update UI if plugin code search checkbox exists
                if (pluginCodeSearchCheckBox != null)
                {
                    pluginCodeSearchCheckBox.Checked = searchPluginCode;
                    pluginCodeSearchCheckBox.Visible = false; // Hidden until Plugin category selected
                }
            }

            // Ensure combobox always has a default selection
            if (comboBox1.SelectedItem == null)
            {
                comboBox1.SelectedItem = "All";
                selectedCategory = -1;
            }

            UpdateControlsState();
        }

        private void MyPluginControl_OnCloseTool(object sender, EventArgs e)
        {
            if (mySettings.RememberLastSearch)
            {
                mySettings.LastSearchText = searchTextBox.Text;
                mySettings.DefaultSearchCategory = selectedCategory;
            }
            
            mySettings.MaxSearchResults = maxResults;
            mySettings.UseFuzzySearch = useFuzzySearch;
            mySettings.FuzzyThreshold = fuzzyThreshold;
            mySettings.EnablePluginCodeSearch = searchPluginCode;
            
            SettingsManager.Instance.Save(GetType(), mySettings);
        }
        #endregion

        #region Connection Management
        public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail, 
            string actionName, object parameter)
        {
            base.UpdateConnection(newService, detail, actionName, parameter);

            if (mySettings != null && detail != null)
            {
                mySettings.LastUsedOrganizationWebappUrl = detail.WebApplicationUrl;

                if (!string.IsNullOrWhiteSpace(detail.EnvironmentId))
                {
                    currentEnvironmentId = detail.EnvironmentId?.ToString().Trim('{', '}');
                }
                else
                {
                    var url = detail.WebApplicationUrl;
                    var match = System.Text.RegularExpressions.Regex.Match(
                        url, @"https:\/\/([a-f0-9\-]+)\.crm");
                    if (match.Success)
                    {
                        currentEnvironmentId = match.Groups[1].Value;
                        LogInfo($"Extracted Environment ID from URL: {currentEnvironmentId}");
                    }
                    else
                    {
                        LogWarning("Failed to extract environment ID from WebApplicationUrl.");
                    }
                }

                LogInfo($"Connection updated. EnvironmentId: {currentEnvironmentId}");
            }

            UpdateControlsState();
        }

        private void UpdateControlsState()
        {
            bool isConnected = Service != null;
            
            searchTextBox.Enabled = isConnected;
            searchButton.Enabled = isConnected;
            comboBox1.Enabled = isConnected;
            
            if (fuzzySearchCheckBox != null)
            {
                fuzzySearchCheckBox.Enabled = isConnected;
            }
            
            if (searchContentCheckBox != null)
            {
                searchContentCheckBox.Enabled = isConnected;
            }
            
            if (!isConnected)
            {
                resultTextBox.Items.Clear();
                LogInfo("Please connect to an organization to start searching");
            }
        }
        #endregion

        #region Search Functionality
        private void searchTextBox_TextChanged(object sender, EventArgs e)
        {
            searchText = searchTextBox.Text.Trim();
        }

        private void searchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                searchButton.PerformClick();
            }
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            if (Service == null)
            {
                MessageBox.Show(
                    "Please connect to an organization before searching.", 
                    "No Connection", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Warning);
                
                ExecuteMethod(ShowConnectionDialog);
                return;
            }

            if (selectedCategory == SEARCH_TYPE_WEBRESOURCE)
            {
                PerformWebResourceSearch(searchText);
            }
            else if (selectedCategory == SEARCH_TYPE_PLUGIN)
            {
                PerformPluginSearch(searchText);
            }
            else
            {
                PerformWorkflowSearch(searchText, selectedCategory);
            }
        }

        private void ShowConnectionDialog()
        {
        }

        private void PerformWorkflowSearch(string searchTerm, int categoryFilter)
        {
            var startTime = DateTime.Now;
            
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Searching Power Automate objects...",
                Work = (worker, args) =>
                {
                    try
                    {
                        var paSearchResults = new List<Entity>();
                        
                        QueryExpression workflowQuery = new QueryExpression("workflow")
                        {
                            ColumnSet = new ColumnSet("workflowid", "name", "clientdata"),
                            Criteria = new FilterExpression()
                        };

                        if (categoryFilter != -1)
                        {
                            workflowQuery.Criteria.AddCondition(
                                "category", ConditionOperator.Equal, categoryFilter);
                        }

                        EntityCollection paObjects = Service.RetrieveMultiple(workflowQuery);
                        
                        worker.ReportProgress(0, $"Processing {paObjects.Entities.Count} workflows...");

                        int processed = 0;
                        int matchCount = 0;
                        
                        foreach (var paObject in paObjects.Entities)
                        {
                            // Check cancellation
                            if (worker.CancellationPending)
                            {
                                args.Cancel = true;
                                return;
                            }
                            
                            processed++;
                            
                            if (processed % PROGRESS_UPDATE_INTERVAL == 0)
                            {
                                worker.ReportProgress(
                                    (int)((processed * 100.0) / paObjects.Entities.Count), 
                                    $"Processed {processed}/{paObjects.Entities.Count} | Found {matchCount} matches");
                            }

                            var workflowName = paObject.Contains("name") 
                                ? paObject["name"].ToString() 
                                : string.Empty;

                            bool matchFound = false;

                            if (string.IsNullOrWhiteSpace(searchTerm))
                            {
                                matchFound = true;
                            }
                            else
                            {
                                // Search in name first (fast)
                                if (MatchesSearchTerm(workflowName, searchTerm))
                                {
                                    matchFound = true;
                                }
                                else
                                {
                                    // Search in content
                                    var clientDataRaw = paObject.Contains("clientdata") 
                                        ? paObject["clientdata"].ToString() 
                                        : string.Empty;
                                    var clientDataJson = clientDataRaw.Replace("\\u0022", "\"");

                                    if (SearchFlowDefinition(clientDataJson.ToLower(), searchTerm.ToLower()))
                                    {
                                        matchFound = true;
                                    }
                                }
                            }

                            if (matchFound)
                            {
                                paSearchResults.Add(paObject);
                                matchCount++;
                                
                                // Stop if we've hit max results
                                if (matchCount >= maxResults)
                                {
                                    worker.ReportProgress(100, $"Reached maximum of {maxResults} results");
                                    break;
                                }
                            }
                        }

                        // PERFORMANCE OPTIMIZATION: Batch solution lookup
                        if (paSearchResults.Count > 0)
                        {
                            worker.ReportProgress(95, "Looking up solutions...");
                            var workflowIds = paSearchResults.Select(e => e.GetAttributeValue<Guid>("workflowid")).ToList();
                            var solutionMap = GetSolutionsForEntitiesBatch(workflowIds, COMPONENT_TYPE_WORKFLOW);
                            
                            foreach (var paObject in paSearchResults)
                            {
                                var workflowId = paObject.GetAttributeValue<Guid>("workflowid");
                                if (solutionMap.ContainsKey(workflowId))
                                {
                                    var (solutionId, solutionName) = solutionMap[workflowId];
                                    paObject["solutionid"] = solutionId;
                                    paObject["solutionname"] = solutionName;
                                }
                            }
                        }

                        args.Result = new SearchResult
                        {
                            Results = paSearchResults,
                            SearchTime = DateTime.Now - startTime,
                            TotalProcessed = processed,
                            WasLimited = matchCount >= maxResults
                        };
                    }
                    catch (FaultException<OrganizationServiceFault> ex)
                    {
                        args.Result = new SearchResult
                        {
                            Error = $"CRM Error: {ex.Detail.Message}\nError Code: {ex.Detail.ErrorCode}"
                        };
                    }
                    catch (Exception ex)
                    {
                        args.Result = new SearchResult
                        {
                            Error = $"Unexpected error: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}"
                        };
                    }
                },
                ProgressChanged = (args) =>
                {
                    SetWorkingMessage(args.UserState?.ToString() ?? "Processing...");
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        MessageBox.Show(
                            $"An error occurred during search:\n\n{args.Error.Message}", 
                            "Error", 
                            MessageBoxButtons.OK, 
                            MessageBoxIcon.Error);
                        return;
                    }

                    if (args.Cancelled)
                    {
                        LogInfo("Search cancelled by user");
                        return;
                    }

                    var searchResult = args.Result as SearchResult;
                    if (searchResult == null)
                        return;

                    if (!string.IsNullOrEmpty(searchResult.Error))
                    {
                        MessageBox.Show(searchResult.Error, "Search Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    resultTextBox.Items.Clear();
                    
                    foreach (var paObject in searchResult.Results)
                    {
                        var name = paObject.Contains("name") 
                            ? paObject["name"].ToString() 
                            : "[No Name]";
                        var solutionId = paObject.Contains("solutionid") 
                            ? paObject["solutionid"].ToString() 
                            : string.Empty;
                        var solutionName = paObject.Contains("solutionname") 
                            ? paObject["solutionname"].ToString() 
                            : "[No Solution]";

                        var listViewItem = new ListViewItem(name);
                        listViewItem.SubItems.Add(solutionName);
                        listViewItem.SubItems.Add(solutionId);
                        resultTextBox.Items.Add(listViewItem);
                    }
                    
                    var message = $"Found {searchResult.Results.Count} result(s) in {searchResult.SearchTime.TotalSeconds:F2} seconds";
                    if (searchResult.WasLimited)
                    {
                        message += $" (limited to {maxResults} results)";
                    }
                    LogInfo(message);
                    
                    if (searchResult.Results.Count == 0)
                    {
                        MessageBox.Show(
                            "No Power Automate objects found matching your search criteria.",
                            "No Results",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else if (searchResult.WasLimited)
                    {
                        MessageBox.Show(
                            $"Search limited to first {maxResults} matches.\n\nTip: Use a more specific search term or category filter for better results.",
                            "Results Limited",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
            });
        }

        private void PerformWebResourceSearch(string searchTerm)
        {
            var startTime = DateTime.Now;
            
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Searching Web Resources...",
                Work = (worker, args) =>
                {
                    try
                    {
                        var webResourceResults = new List<Entity>();
                        
                        QueryExpression webResourceQuery = new QueryExpression("webresource")
                        {
                            ColumnSet = new ColumnSet("webresourceid", "name", "displayname", "content", "webresourcetype"),
                            Criteria = new FilterExpression()
                        };

                        var textBasedTypes = new[] { 1, 2, 3, 4, 9, 11, 12 };
                        var typeFilter = new FilterExpression(LogicalOperator.Or);
                        foreach (var type in textBasedTypes)
                        {
                            typeFilter.AddCondition("webresourcetype", ConditionOperator.Equal, type);
                        }
                        webResourceQuery.Criteria.AddFilter(typeFilter);

                        EntityCollection webResources = Service.RetrieveMultiple(webResourceQuery);
                        
                        worker.ReportProgress(0, $"Processing {webResources.Entities.Count} web resources...");

                        int processed = 0;
                        int matchCount = 0;
                        
                        foreach (var webResource in webResources.Entities)
                        {
                            if (worker.CancellationPending)
                            {
                                args.Cancel = true;
                                return;
                            }
                            
                            processed++;
                            
                            if (processed % PROGRESS_UPDATE_INTERVAL == 0)
                            {
                                worker.ReportProgress(
                                    (int)((processed * 100.0) / webResources.Entities.Count), 
                                    $"Processed {processed}/{webResources.Entities.Count} | Found {matchCount} matches");
                            }

                            var name = webResource.Contains("name") 
                                ? webResource["name"].ToString() 
                                : string.Empty;
                            var displayName = webResource.Contains("displayname") 
                                ? webResource["displayname"].ToString() 
                                : string.Empty;

                            bool matchFound = false;

                            if (string.IsNullOrWhiteSpace(searchTerm))
                            {
                                matchFound = true;
                            }
                            else
                            {
                                if (MatchesSearchTerm(name, searchTerm) || MatchesSearchTerm(displayName, searchTerm))
                                {
                                    matchFound = true;
                                }
                                else
                                {
                                    // Only search content if the option is enabled (performance boost)
                                    if (searchWebResourceContent && webResource.Contains("content"))
                                    {
                                        try
                                        {
                                            var base64Content = webResource["content"].ToString();
                                            var decodedBytes = Convert.FromBase64String(base64Content);
                                            var decodedContent = System.Text.Encoding.UTF8.GetString(decodedBytes);

                                            if (MatchesSearchTerm(decodedContent, searchTerm))
                                            {
                                                matchFound = true;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            LogWarning($"Failed to decode content for {name}: {ex.Message}");
                                        }
                                    }
                                }
                            }

                            if (matchFound)
                            {
                                webResourceResults.Add(webResource);
                                matchCount++;
                                
                                if (matchCount >= maxResults)
                                {
                                    worker.ReportProgress(100, $"Reached maximum of {maxResults} results");
                                    break;
                                }
                            }
                        }

                        // PERFORMANCE OPTIMIZATION: Batch solution lookup
                        if (webResourceResults.Count > 0)
                        {
                            worker.ReportProgress(95, "Looking up solutions...");
                            var webResourceIds = webResourceResults.Select(e => e.GetAttributeValue<Guid>("webresourceid")).ToList();
                            var solutionMap = GetSolutionsForEntitiesBatch(webResourceIds, COMPONENT_TYPE_WEBRESOURCE);
                            
                            foreach (var webResource in webResourceResults)
                            {
                                var webResourceId = webResource.GetAttributeValue<Guid>("webresourceid");
                                if (solutionMap.ContainsKey(webResourceId))
                                {
                                    var (solutionId, solutionName) = solutionMap[webResourceId];
                                    webResource["solutionid"] = solutionId;
                                    webResource["solutionname"] = solutionName;
                                }
                            }
                        }

                        args.Result = new SearchResult
                        {
                            Results = webResourceResults,
                            SearchTime = DateTime.Now - startTime,
                            TotalProcessed = processed,
                            WasLimited = matchCount >= maxResults
                        };
                    }
                    catch (FaultException<OrganizationServiceFault> ex)
                    {
                        args.Result = new SearchResult
                        {
                            Error = $"CRM Error: {ex.Detail.Message}\nError Code: {ex.Detail.ErrorCode}"
                        };
                    }
                    catch (Exception ex)
                    {
                        args.Result = new SearchResult
                        {
                            Error = $"Unexpected error: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}"
                        };
                    }
                },
                ProgressChanged = (args) =>
                {
                    SetWorkingMessage(args.UserState?.ToString() ?? "Processing...");
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        MessageBox.Show(
                            $"An error occurred during search:\n\n{args.Error.Message}", 
                            "Error", 
                            MessageBoxButtons.OK, 
                            MessageBoxIcon.Error);
                        return;
                    }

                    if (args.Cancelled)
                    {
                        LogInfo("Search cancelled by user");
                        return;
                    }

                    var searchResult = args.Result as SearchResult;
                    if (searchResult == null)
                        return;

                    if (!string.IsNullOrEmpty(searchResult.Error))
                    {
                        MessageBox.Show(searchResult.Error, "Search Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    resultTextBox.Items.Clear();
                    
                    foreach (var webResource in searchResult.Results)
                    {
                        var displayName = webResource.Contains("displayname") 
                            ? webResource["displayname"].ToString() 
                            : "[No Display Name]";
                        var solutionId = webResource.Contains("solutionid") 
                            ? webResource["solutionid"].ToString() 
                            : string.Empty;
                        var solutionName = webResource.Contains("solutionname") 
                            ? webResource["solutionname"].ToString() 
                            : "[No Solution]";

                        var listViewItem = new ListViewItem(displayName);
                        listViewItem.SubItems.Add(solutionName);
                        listViewItem.SubItems.Add(solutionId);
                        resultTextBox.Items.Add(listViewItem);
                    }
                    
                    var message = $"Found {searchResult.Results.Count} web resource(s) in {searchResult.SearchTime.TotalSeconds:F2} seconds";
                    if (searchResult.WasLimited)
                    {
                        message += $" (limited to {maxResults} results)";
                    }
                    LogInfo(message);
                    
                    if (searchResult.Results.Count == 0)
                    {
                        MessageBox.Show(
                            "No web resources found matching your search criteria.",
                            "No Results",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else if (searchResult.WasLimited)
                    {
                        MessageBox.Show(
                            $"Search limited to first {maxResults} matches.\n\nTip: Use a more specific search term for better results.",
                            "Results Limited",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
            });
        }
        
        private void PerformPluginSearch(string searchTerm)
        {
            var startTime = DateTime.Now;
            
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Searching Plugins...",
                Work = (worker, args) =>
                {
                    try
                    {
                        var pluginResults = new List<Entity>();
                        var decompilationService = new PluginDecompilationService();
                        
                        // Query plugin types with registration steps
                        QueryExpression pluginQuery = new QueryExpression("plugintype")
                        {
                            ColumnSet = new ColumnSet("plugintypeid", "typename", "friendlyname", "assemblyname", "pluginassemblyid"),
                            Criteria = new FilterExpression()
                        };
                        
                        // Link to get assembly information
                        var assemblyLink = pluginQuery.AddLink(
                            "pluginassembly",
                            "pluginassemblyid",
                            "pluginassemblyid",
                            JoinOperator.LeftOuter);
                        assemblyLink.Columns = new ColumnSet("name", "version");
                        assemblyLink.EntityAlias = "assembly";
                        
                        // Link to get registration steps (for filtering and metadata)
                        var stepLink = pluginQuery.AddLink(
                            "sdkmessageprocessingstep",
                            "plugintypeid",
                            "plugintypeid",
                            JoinOperator.LeftOuter);
                        stepLink.Columns = new ColumnSet("name", "description", "stage", "mode");
                        stepLink.EntityAlias = "step";
                        
                        // Link to get message information
                        var messageLink = stepLink.AddLink(
                            "sdkmessage",
                            "sdkmessageid",
                            "sdkmessageid",
                            JoinOperator.LeftOuter);
                        messageLink.Columns = new ColumnSet("name");
                        messageLink.EntityAlias = "message";
                        
                        // Link to get filter (entity) information
                        var filterLink = stepLink.AddLink(
                            "sdkmessagefilter",
                            "sdkmessagefilterid",
                            "sdkmessagefilterid",
                            JoinOperator.LeftOuter);
                        filterLink.Columns = new ColumnSet("primaryobjecttypecode");
                        filterLink.EntityAlias = "filter";

                        EntityCollection plugins = Service.RetrieveMultiple(pluginQuery);
                        
                        worker.ReportProgress(0, $"Processing {plugins.Entities.Count} plugin registrations...");

                        int processed = 0;
                        int matchCount = 0;
                        
                        // Group by plugintypeid to deduplicate (a plugin can have multiple steps)
                        var pluginGroups = plugins.Entities
                            .GroupBy(p => p.GetAttributeValue<Guid>("plugintypeid"))
                            .Select(g => new
                            {
                                PluginType = g.First(),
                                Steps = g.ToList()
                            })
                            .ToList();
                        
                        foreach (var pluginGroup in pluginGroups)
                        {
                            if (worker.CancellationPending)
                            {
                                args.Cancel = true;
                                return;
                            }
                            
                            processed++;
                            
                            if (processed % PROGRESS_UPDATE_INTERVAL == 0)
                            {
                                worker.ReportProgress(
                                    (int)((processed * 100.0) / pluginGroups.Count), 
                                    $"Processed {processed}/{pluginGroups.Count} | Found {matchCount} matches");
                            }

                            var plugin = pluginGroup.PluginType;
                            
                            var typeName = plugin.Contains("typename") 
                                ? plugin["typename"].ToString() 
                                : string.Empty;
                            var friendlyName = plugin.Contains("friendlyname") 
                                ? plugin["friendlyname"].ToString() 
                                : string.Empty;
                            var assemblyName = plugin.Contains("assemblyname") 
                                ? plugin["assemblyname"].ToString() 
                                : string.Empty;

                            // Get assembly details from alias
                            var fullAssemblyName = plugin.Contains("assembly.name") 
                                ? plugin.GetAttributeValue<AliasedValue>("assembly.name").Value.ToString()
                                : assemblyName;

                            bool matchFound = false;

                            if (string.IsNullOrWhiteSpace(searchTerm))
                            {
                                matchFound = true;
                            }
                            else
                            {
                                // Search in plugin metadata
                                if (MatchesSearchTerm(typeName, searchTerm) ||
                                    MatchesSearchTerm(friendlyName, searchTerm) ||
                                    MatchesSearchTerm(assemblyName, searchTerm) ||
                                    MatchesSearchTerm(fullAssemblyName, searchTerm))
                                {
                                    matchFound = true;
                                }
                                else
                                {
                                    // Search in registration metadata (messages, entities, step names)
                                    foreach (var step in pluginGroup.Steps)
                                    {
                                        var stepName = step.Contains("step.name")
                                            ? step.GetAttributeValue<AliasedValue>("step.name")?.Value?.ToString() ?? string.Empty
                                            : string.Empty;
                                        
                                        var messageName = step.Contains("message.name")
                                            ? step.GetAttributeValue<AliasedValue>("message.name")?.Value?.ToString() ?? string.Empty
                                            : string.Empty;
                                        
                                        var entityName = step.Contains("filter.primaryobjecttypecode")
                                            ? step.GetAttributeValue<AliasedValue>("filter.primaryobjecttypecode")?.Value?.ToString() ?? string.Empty
                                            : string.Empty;

                                        if (MatchesSearchTerm(stepName, searchTerm) ||
                                            MatchesSearchTerm(messageName, searchTerm) ||
                                            MatchesSearchTerm(entityName, searchTerm))
                                        {
                                            matchFound = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            
                            // CODE SEARCH: If metadata didn't match but code search is enabled, try decompiling and searching
                            if (!matchFound && searchPluginCode && !string.IsNullOrWhiteSpace(searchTerm))
                            {
                                try
                                {
                                    // Get the plugin type ID to retrieve the assembly reference
                                    var pluginTypeId = plugin.GetAttributeValue<Guid>("plugintypeid");
                                    
                                    // Retrieve the plugintype record to get the pluginassemblyid lookup
                                    var pluginTypeRecord = Service.Retrieve("plugintype", pluginTypeId, new ColumnSet("pluginassemblyid"));
                                    
                                    // Get the assembly reference
                                    var assemblyReference = pluginTypeRecord.GetAttributeValue<EntityReference>("pluginassemblyid");
                                    
                                    if (assemblyReference != null && assemblyReference.Id != Guid.Empty)
                                    {
                                        var assemblyId = assemblyReference.Id;
                                        
                                        worker.ReportProgress(-1, $"Decompiling and searching code for {typeName}...");
                                        
                                        // Retrieve assembly content
                                        var assembly = Service.Retrieve("pluginassembly", assemblyId, new ColumnSet("content", "name"));
                                        
                                        if (assembly.Contains("content"))
                                        {
                                            var base64Content = assembly["content"].ToString();
                                            var assemblyBytes = Convert.FromBase64String(base64Content);
                                            var actualAssemblyName = assembly.Contains("name") ? assembly["name"].ToString() : fullAssemblyName;
                                            
                                            // Decompile the assembly
                                            var decompiledResult = decompilationService.DecompilePlugin(
                                                assemblyBytes, 
                                                assemblyId, 
                                                actualAssemblyName,
                                                useCache: mySettings.CacheDecompiledCode);
                                            
                                            if (decompiledResult.Success)
                                            {
                                                // Search in decompiled code
                                                if (MatchesSearchTerm(decompiledResult.DecompiledCode, searchTerm))
                                                {
                                                    matchFound = true;
                                                    plugin["matchsource"] = "Code Content"; // Tag for user visibility
                                                }
                                            }
                                            else
                                            {
                                                // Log decompilation failure but continue
                                                LogWarning($"Failed to decompile {actualAssemblyName}: {decompiledResult.ErrorMessage}");
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogWarning($"Error during code search for {typeName}: {ex.Message}");
                                }
                            }

                            if (matchFound)
                            {
                                // Build registration summary for display
                                var registrations = new List<string>();
                                foreach (var step in pluginGroup.Steps)
                                {
                                    var messageName = step.Contains("message.name")
                                        ? step.GetAttributeValue<AliasedValue>("message.name")?.Value?.ToString() ?? "Unknown"
                                        : "Unknown";
                                    
                                    var entityName = step.Contains("filter.primaryobjecttypecode")
                                        ? step.GetAttributeValue<AliasedValue>("filter.primaryobjecttypecode")?.Value?.ToString() ?? "None"
                                        : "None";
                                    
                                    var stage = step.Contains("step.stage")
                                        ? step.GetAttributeValue<AliasedValue>("step.stage")?.Value?.ToString() ?? ""
                                        : "";
                                    
                                    // Convert stage number to text (C# 7.3 compatible)
                                    string stageText;
                                    switch (stage)
                                    {
                                        case "10":
                                            stageText = "PreValidation";
                                            break;
                                        case "20":
                                            stageText = "PreOperation";
                                            break;
                                        case "40":
                                            stageText = "PostOperation";
                                            break;
                                        default:
                                            stageText = "Stage " + stage;
                                            break;
                                    }
                                    
                                    registrations.Add($"{messageName} on {entityName} ({stageText})");
                                }
                                
                                // Store registration summary as custom attribute
                                plugin["registrations"] = string.Join("; ", registrations.Take(3)); // Limit to 3 for display
                                if (registrations.Count > 3)
                                {
                                    plugin["registrations"] += $" ... and {registrations.Count - 3} more";
                                }
                                
                                // Store assembly name for display
                                plugin["displayassembly"] = fullAssemblyName;
                                
                                pluginResults.Add(plugin);
                                matchCount++;
                                
                                if (matchCount >= maxResults)
                                {
                                    worker.ReportProgress(100, $"Reached maximum of {maxResults} results");
                                    break;
                                }
                            }
                        }

                        // PERFORMANCE OPTIMIZATION: Batch solution lookup
                        if (pluginResults.Count > 0)
                        {
                            worker.ReportProgress(95, "Looking up solutions...");
                            var pluginTypeIds = pluginResults.Select(e => e.GetAttributeValue<Guid>("plugintypeid")).ToList();
                            var solutionMap = GetSolutionsForEntitiesBatch(pluginTypeIds, COMPONENT_TYPE_PLUGINTYPE);
                            
                            foreach (var plugin in pluginResults)
                            {
                                var pluginTypeId = plugin.GetAttributeValue<Guid>("plugintypeid");
                                if (solutionMap.ContainsKey(pluginTypeId))
                                {
                                    var (solutionId, solutionName) = solutionMap[pluginTypeId];
                                    plugin["solutionid"] = solutionId;
                                    plugin["solutionname"] = solutionName;
                                }
                            }
                        }

                        args.Result = new SearchResult
                        {
                            Results = pluginResults,
                            SearchTime = DateTime.Now - startTime,
                            TotalProcessed = processed,
                            WasLimited = matchCount >= maxResults
                        };
                    }
                    catch (FaultException<OrganizationServiceFault> ex)
                    {
                        args.Result = new SearchResult
                        {
                            Error = $"CRM Error: {ex.Detail.Message}\nError Code: {ex.Detail.ErrorCode}"
                        };
                    }
                    catch (Exception ex)
                    {
                        args.Result = new SearchResult
                        {
                            Error = $"Unexpected error: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}"
                        };
                    }
                },
                ProgressChanged = (args) =>
                {
                    SetWorkingMessage(args.UserState?.ToString() ?? "Processing...");
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        MessageBox.Show(
                            $"An error occurred during search:\n\n{args.Error.Message}", 
                            "Error", 
                            MessageBoxButtons.OK, 
                            MessageBoxIcon.Error);
                        return;
                    }

                    if (args.Cancelled)
                    {
                        LogInfo("Search cancelled by user");
                        return;
                    }

                    var searchResult = args.Result as SearchResult;
                    if (searchResult == null)
                        return;

                    if (!string.IsNullOrEmpty(searchResult.Error))
                    {
                        MessageBox.Show(searchResult.Error, "Search Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    resultTextBox.Items.Clear();
                    
                    foreach (var plugin in searchResult.Results)
                    {
                        // Display: Plugin Type Name | Assembly Name | Registrations
                        var typeName = plugin.Contains("typename") 
                            ? plugin["typename"].ToString() 
                            : "[No Type Name]";
                        
                        var assemblyName = plugin.Contains("displayassembly")
                            ? plugin["displayassembly"].ToString()
                            : plugin.Contains("assemblyname") 
                                ? plugin["assemblyname"].ToString()
                                : "[No Assembly]";
                        
                        var registrations = plugin.Contains("registrations")
                            ? plugin["registrations"].ToString()
                            : "[No Registrations]";
                        
                        var solutionName = plugin.Contains("solutionname") 
                            ? plugin["solutionname"].ToString() 
                            : "[No Solution]";
                        
                        var solutionId = plugin.Contains("solutionid") 
                            ? plugin["solutionid"].ToString() 
                            : string.Empty;

                        // Column 1: Type Name + Assembly (combined for readability)
                        var displayName = $"{typeName} ({assemblyName})";
                        
                        var listViewItem = new ListViewItem(displayName);
                        listViewItem.SubItems.Add(solutionName);
                        listViewItem.SubItems.Add(solutionId);
                        
                        // Store registration info in tooltip or tag
                        listViewItem.ToolTipText = $"Registrations: {registrations}";
                        
                        resultTextBox.Items.Add(listViewItem);
                    }
                    
                    var message = $"Found {searchResult.Results.Count} plugin(s) in {searchResult.SearchTime.TotalSeconds:F2} seconds";
                    if (searchResult.WasLimited)
                    {
                        message += $" (limited to {maxResults} results)";
                    }
                    LogInfo(message);
                    
                    if (searchResult.Results.Count == 0)
                    {
                        MessageBox.Show(
                            "No plugins found matching your search criteria.",
                            "No Results",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else if (searchResult.WasLimited)
                    {
                        MessageBox.Show(
                            $"Search limited to first {maxResults} matches.\n\nTip: Use a more specific search term for better results.",
                            "Results Limited",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
            });
        }
        #endregion

        #region Search Helper Methods
        
        /// <summary>
        /// PERFORMANCE OPTIMIZATION: Batch solution lookup - 10-50x faster than individual queries
        /// Prioritizes non-Default solutions (shows custom solutions first)
        /// </summary>
        private Dictionary<Guid, (Guid solutionId, string solutionName)> GetSolutionsForEntitiesBatch(
            List<Guid> entityIds, int componentType)
        {
            var solutionMap = new Dictionary<Guid, (Guid, string)>();
            
            if (entityIds == null || entityIds.Count == 0) 
                return solutionMap;
            
            try
            {
                // Query ALL solution components at once (instead of one by one)
                QueryExpression componentQuery = new QueryExpression("solutioncomponent")
                {
                    ColumnSet = new ColumnSet("objectid", "solutionid"),
                    Criteria = new FilterExpression()
                };
                
                // Add OR condition for all entity IDs
                var idFilter = new FilterExpression(LogicalOperator.Or);
                foreach (var id in entityIds)
                {
                    idFilter.AddCondition("objectid", ConditionOperator.Equal, id);
                }
                componentQuery.Criteria.AddFilter(idFilter);
                componentQuery.Criteria.AddCondition("componenttype", ConditionOperator.Equal, componentType);
                
                var components = Service.RetrieveMultiple(componentQuery);
                
                // Collect unique solution IDs
                var solutionIds = components.Entities
                    .Where(c => c.Contains("solutionid"))
                    .Select(c => c.GetAttributeValue<EntityReference>("solutionid").Id)
                    .Distinct()
                    .ToList();
                
                if (solutionIds.Count == 0) 
                    return solutionMap;
                
                // Query ALL solutions at once (instead of one by one)
                QueryExpression solutionQuery = new QueryExpression("solution")
                {
                    ColumnSet = new ColumnSet("solutionid", "friendlyname", "uniquename", "ismanaged"),
                    Criteria = new FilterExpression()
                };
                
                var solutionIdFilter = new FilterExpression(LogicalOperator.Or);
                foreach (var solId in solutionIds)
                {
                    solutionIdFilter.AddCondition("solutionid", ConditionOperator.Equal, solId);
                }
                solutionQuery.Criteria.AddFilter(solutionIdFilter);
                
                var solutions = Service.RetrieveMultiple(solutionQuery);
                var solutionLookup = solutions.Entities.ToDictionary(
                    s => s.Id,
                    s => new {
                        Id = s.Id,
                        FriendlyName = s.GetAttributeValue<string>("friendlyname") ?? "[No Name]",
                        UniqueName = s.GetAttributeValue<string>("uniquename") ?? "",
                        IsManaged = s.GetAttributeValue<bool>("ismanaged")
                    }
                );
                
                // Group components by objectid to find ALL solutions for each entity
                var componentsByObject = components.Entities
                    .Where(c => c.Contains("objectid") && c.Contains("solutionid"))
                    .GroupBy(c => c.GetAttributeValue<Guid>("objectid"));
                
                foreach (var objectGroup in componentsByObject)
                {
                    var objectId = objectGroup.Key;
                    
                    // Get all solutions for this entity
                    var entitySolutions = objectGroup
                        .Select(c => c.GetAttributeValue<EntityReference>("solutionid").Id)
                        .Where(solId => solutionLookup.ContainsKey(solId))
                        .Select(solId => solutionLookup[solId])
                        .ToList();
                    
                    if (entitySolutions.Count == 0)
                        continue;
                    
                    // Prioritize solutions (BEST solution wins):
                    // 1. Non-Default, non-managed solutions (custom solutions)
                    // 2. Non-Default, managed solutions
                    // 3. Default Solution (only if nothing else available)
                    
                    var bestSolution = entitySolutions
                        .OrderByDescending(s => !s.UniqueName.Equals("Default", StringComparison.OrdinalIgnoreCase)) // Non-Default first
                        .ThenByDescending(s => !s.UniqueName.Equals("Active", StringComparison.OrdinalIgnoreCase)) // Non-Active first
                        .ThenByDescending(s => !s.IsManaged) // Unmanaged first
                        .ThenBy(s => s.FriendlyName) // Alphabetical as tiebreaker
                        .First();
                    
                    solutionMap[objectId] = (bestSolution.Id, bestSolution.FriendlyName);
                }
            }
            catch (Exception ex)
            {
                LogWarning($"Batch solution lookup failed: {ex.Message}");
            }
            
            return solutionMap;
        }

        /// <summary>
        /// Unified search method with simple fuzzy matching support
        /// </summary>
        private bool MatchesSearchTerm(string content, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return true;
            
            if (string.IsNullOrWhiteSpace(content))
                return false;
            
            var contentLower = content.ToLower();
            var searchLower = searchTerm.ToLower();
            
            // Exact match first (fastest)
            if (contentLower.Contains(searchLower))
                return true;
            
            // Simple fuzzy match if enabled
            if (useFuzzySearch)
            {
                // Split content into words and check each
                var words = contentLower.Split(new[] { ' ', '_', '-', '.', '/', '\\', '(', ')', '[', ']', '{', '}' }, 
                    StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var word in words)
                {
                    if (word.Length < 3) continue; // Skip very short words
                    
                    // Check if word starts with search term or vice versa
                    if (word.StartsWith(searchLower) || searchLower.StartsWith(word))
                        return true;
                    
                    // Check edit distance for longer words
                    if (word.Length >= searchLower.Length - fuzzyThreshold && 
                        word.Length <= searchLower.Length + fuzzyThreshold)
                    {
                        int distance = LevenshteinDistance(word, searchLower);
                        if (distance <= fuzzyThreshold)
                            return true;
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Calculate Levenshtein distance between two strings
        /// </summary>
        private int LevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            
            if (n == 0) return m;
            if (m == 0) return n;
            
            int[,] d = new int[n + 1, m + 1];
            
            for (int i = 0; i <= n; i++) d[i, 0] = i;
            for (int j = 0; j <= m; j++) d[0, j] = j;
            
            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            
            return d[n, m];
        }

        private bool SearchFlowDefinition(string clientDataJson, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return true;
            }

            try
            {
                // Quick rejection first
                if (!clientDataJson.Contains(searchTerm))
                {
                    // Try fuzzy if enabled
                    if (useFuzzySearch)
                    {
                        // Simple fuzzy matching using MatchesSearchTerm
                        if (MatchesSearchTerm(clientDataJson, searchTerm))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                var parsed = JObject.Parse(clientDataJson);
                
                var allValues = parsed.Descendants()
                    .OfType<JValue>()
                    .Select(v => v.ToString().ToLower())
                    .ToList();

                var allPropertyNames = parsed.Descendants()
                    .OfType<JProperty>()
                    .Select(p => p.Name.ToLower())
                    .ToList();

                var allSearchableContent = allValues.Concat(allPropertyNames).ToList();

                var terms = searchTerm
                    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                return terms.All(term => allSearchableContent.Any(content => content.Contains(term)));
            }
            catch (Newtonsoft.Json.JsonReaderException ex)
            {
                LogWarning($"Invalid JSON in clientdata for flow, using simple search: {ex.Message}");
                return MatchesSearchTerm(clientDataJson, searchTerm);
            }
            catch (Exception ex)
            {
                LogWarning($"JSON parse/search failed: {ex.Message}");
                return MatchesSearchTerm(clientDataJson, searchTerm);
            }
        }
        #endregion

        #region Results Handling
        private void resultTextBox_DoubleClick(object sender, EventArgs e)
        {
            if (resultTextBox.SelectedItems.Count == 0)
                return;

            ListViewItem item = resultTextBox.SelectedItems[0];
            string solutionId = item.SubItems[2].Text;

            if (string.IsNullOrEmpty(solutionId))
            {
                MessageBox.Show(
                    "No Solution ID found for this object.\n\nThis object may not be in a solution.", 
                    "No Solution ID", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Clipboard.SetText(solutionId);
                MessageBox.Show(
                    $"Solution ID copied to clipboard:\n\n{solutionId}", 
                    "Copied to Clipboard", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Information);
            }
            catch (System.Runtime.InteropServices.ExternalException ex)
            {
                MessageBox.Show(
                    $"Failed to access clipboard:\n\n{ex.Message}", 
                    "Clipboard Error", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An unexpected error occurred:\n\n{ex.Message}", 
                    "Error", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Category Selection
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedLabel = comboBox1.SelectedItem?.ToString();
            
            if (!string.IsNullOrEmpty(selectedLabel) && CategoryMapping.ContainsKey(selectedLabel))
            {
                selectedCategory = CategoryMapping[selectedLabel];
                LogInfo($"Category filter changed to: {selectedLabel} (value: {selectedCategory})");
                
                if (categoryTipLabel != null)
                {
                    categoryTipLabel.Visible = (selectedLabel == "All");
                }
                
                // Show/hide plugin code search checkbox based on category
                if (pluginCodeSearchCheckBox != null)
                {
                    pluginCodeSearchCheckBox.Visible = (selectedCategory == SEARCH_TYPE_PLUGIN);
                }
                
                // Hide web resource content search when not on web resource
                if (searchContentCheckBox != null)
                {
                    searchContentCheckBox.Visible = (selectedCategory == SEARCH_TYPE_WEBRESOURCE || selectedLabel == "All");
                }
            }
        }
        
        private void fuzzySearchCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            useFuzzySearch = fuzzySearchCheckBox.Checked;
            LogInfo($"Fuzzy search {(useFuzzySearch ? "enabled" : "disabled")}");
        }
        
        private void searchContentCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            searchWebResourceContent = searchContentCheckBox.Checked;
            LogInfo($"WebResource content search {(searchWebResourceContent ? "enabled" : "disabled")}");
        }
        
        private void pluginCodeSearchCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            searchPluginCode = pluginCodeSearchCheckBox.Checked;
            
            // Show warning on first use
            if (searchPluginCode && !mySettings.PluginCodeSearchWarningShown)
            {
                var result = MessageBox.Show(
                    "Plugin code search decompiles assemblies which can take 30-120 seconds (or longer) on first use.\n\n" +
                    "Results are cached for this session to improve subsequent searches.\n\n" +
                    "Note: This will not work with obfuscated code.\n\n" +
                    "Continue?",
                    "Performance Warning",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                
                if (result == DialogResult.No)
                {
                    pluginCodeSearchCheckBox.Checked = false;
                    searchPluginCode = false;
                    return;
                }
                
                mySettings.PluginCodeSearchWarningShown = true;
            }
            
            LogInfo($"Plugin code search {(searchPluginCode ? "enabled" : "disabled")}");
        }
        #endregion

        #region Helper Classes
        private class SearchResult
        {
            public List<Entity> Results { get; set; } = new List<Entity>();
            public TimeSpan SearchTime { get; set; }
            public string Error { get; set; }
            public int TotalProcessed { get; set; }
            public bool WasLimited { get; set; }
        }
        #endregion
    }
}
