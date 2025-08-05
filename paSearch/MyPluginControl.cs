using McTools.Xrm.Connection;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Windows.Forms;
using XrmToolBox.Extensibility;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Crm.Sdk.Messages;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace paSearch
{

    public partial class MyPluginControl : PluginControlBase
    {
        private Settings mySettings;
        public int selectedCategory;
        private string currentEnvironmentId;

        public MyPluginControl()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
        }

        private void MyPluginControl_Load(object sender, EventArgs e)
        {
            ExecuteMethod(WhoAmI);
            ShowInfoNotification("Please feel free to check out the repo and suggest features or contribute!", new Uri("https://github.com/addisonfischer/Power-Automate-Search"));

            if (!SettingsManager.Instance.TryLoad(GetType(), out mySettings))
            {
                mySettings = new Settings();
                LogWarning("Settings not found => a new settings file has been created!");
            }
            else
            {
                LogInfo("Settings found and loaded");
            }

            comboBox1.SelectedItem = "All";
        }
        private void WhoAmI()
        {
            Service.Execute(new WhoAmIRequest());
        }

        private void MyPluginControl_OnCloseTool(object sender, EventArgs e)
        {
            SettingsManager.Instance.Save(GetType(), mySettings);
        }

        public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail, string actionName, object parameter)
        {
            base.UpdateConnection(newService, detail, actionName, parameter);

            if (mySettings != null && detail != null)
            {
                mySettings.LastUsedOrganizationWebappUrl = detail.WebApplicationUrl;

                // ✅ Extract environment ID from URL if EnvironmentId is null
                if (!string.IsNullOrWhiteSpace(detail.EnvironmentId))
                {
                    currentEnvironmentId = detail.EnvironmentId?.ToString().Trim('{', '}');
                }
                else
                {
                    var url = detail.WebApplicationUrl;
                    var match = System.Text.RegularExpressions.Regex.Match(url, @"https:\/\/([a-f0-9\-]+)\.crm");

                    if (match.Success)
                    {
                        currentEnvironmentId = match.Groups[1].Value;
                        LogInfo($"Extracted Environment ID from URL: {currentEnvironmentId}");
                    }
                    else
                    {
                        LogError("Failed to extract environment ID from WebApplicationUrl.");
                    }
                }

                LogInfo($"Connection updated. EnvironmentId: {currentEnvironmentId}, WebAppUrl: {detail.WebApplicationUrl}");
            }
        }

        private string searchText = string.Empty;

        private void searchTextBox_TextChanged(object sender, EventArgs e)
        {
            searchText = searchTextBox.Text.Trim();
        }

        private void searchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            ExecuteMethod(WhoAmI);
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                searchButton.PerformClick();
            }
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            paSearchFunction(searchText, selectedCategory);
        }

        private void resultsTextBox_TextChanged(object sender, EventArgs e)
        {
        }

        public class CombinedResult
        {
            public string WorkflowName { get; set; }
            public string WorkflowId { get; set; }
            public string SolutionId { get; set; }
            public string SolutionFriendlyName { get; set; }
        }

        private void paSearchFunction(string searchText, int selectedCategory)
        {
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Searching all those Power Automate objects....",
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

                        if (selectedCategory != -1)
                        {
                            workflowQuery.Criteria.AddCondition("category", ConditionOperator.Equal, selectedCategory);
                        }

                        EntityCollection paObjects = Service.RetrieveMultiple(workflowQuery);

                        foreach (var paObject in paObjects.Entities)
                        {
                            var clientDataRaw = paObject.Contains("clientdata") ? paObject["clientdata"].ToString() : string.Empty;
                            var clientDataJson = clientDataRaw.Replace("\\u0022", "\"");

                            if (SearchFlowDefinition(clientDataJson.ToLower(), searchText.ToLower()))
                            {
                                Guid workflowId = paObject.GetAttributeValue<Guid>("workflowid");

                                QueryExpression solutionComponentQuery = new QueryExpression("solutioncomponent")
                                {
                                    ColumnSet = new ColumnSet("solutionid", "componenttype"),
                                    Criteria = new FilterExpression()
                                    {
                                        Conditions =
                                        {
                                            new ConditionExpression("objectid", ConditionOperator.Equal, workflowId),
                                            new ConditionExpression("componenttype", ConditionOperator.Equal, 29)
                                        }
                                    }
                                };

                                EntityCollection solutionComponents = Service.RetrieveMultiple(solutionComponentQuery);

                                if (solutionComponents.Entities.Count > 0)
                                {
                                    var solutionComponent = solutionComponents.Entities.FirstOrDefault();
                                    if (solutionComponent.Contains("solutionid"))
                                    {
                                        var solutionIdLookup = solutionComponent.GetAttributeValue<EntityReference>("solutionid");
                                        if (solutionIdLookup != null)
                                        {
                                            Guid solutionId = solutionIdLookup.Id;
                                            paObject["solutionid"] = solutionId;

                                            QueryExpression solutionQuery = new QueryExpression("solution")
                                            {
                                                ColumnSet = new ColumnSet("solutionid", "friendlyname"),
                                                Criteria = new FilterExpression()
                                                {
                                                    Conditions =
                                                    {
                                                        new ConditionExpression("solutionid", ConditionOperator.Equal, solutionId)
                                                    }
                                                }
                                            };

                                            EntityCollection solutions = Service.RetrieveMultiple(solutionQuery);

                                            if (solutions.Entities.Count > 0)
                                            {
                                                var solution = solutions.Entities.FirstOrDefault();
                                                if (solution.Contains("friendlyname"))
                                                {
                                                    paObject["solutionname"] = solution["friendlyname"].ToString();
                                                }
                                            }
                                        }
                                    }
                                }

                                paSearchResults.Add(paObject);
                            }
                        }

                        args.Result = paSearchResults;
                    }
                    catch (FaultException<OrganizationServiceFault> ex)
                    {
                        MessageBox.Show($"Error: {ex.Detail.Message}\nErrorCode: {ex.Detail.ErrorCode}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        args.Result = null;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        args.Result = null;
                    }
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Result != null)
                    {
                        resultTextBox.Items.Clear();
                        var paSearchResults = (List<Entity>)args.Result;
                        foreach (var paObject in paSearchResults)
                        {
                            var name = paObject.Contains("name") ? paObject["name"].ToString() : string.Empty;
                            var solutionId = paObject.Contains("solutionid") ? paObject["solutionid"].ToString() : string.Empty;
                            var solutionName = paObject.Contains("solutionname") ? paObject["solutionname"].ToString() : string.Empty;

                            var listViewItem = new ListViewItem(name);
                            listViewItem.SubItems.Add(solutionName);
                            listViewItem.SubItems.Add(solutionId);
                            resultTextBox.Items.Add(listViewItem);
                        }
                    }
                }
            });
        }

        private bool SearchFlowDefinition(string clientDataJson, string searchTerm)
        {
            try
            {
                var parsed = JObject.Parse(clientDataJson);
                var allValues = parsed.Descendants()
                    .OfType<JValue>()
                    .Select(v => v.ToString().ToLower());

                var terms = searchTerm
                    .ToLower()
                    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                return terms.All(term => allValues.Any(v => v.Contains(term)));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JSON parse/search failed: {ex.Message}");
                return false;
            }
        }

        private string GetEnvironmentId()
        {
            try
            {
                var crmServiceClient = Service as CrmServiceClient;
                if (crmServiceClient == null)
                {
                    LogError("Service is not a CrmServiceClient.");
                    return string.Empty;
                }

                WhoAmIRequest request = new WhoAmIRequest();
                WhoAmIResponse response = (WhoAmIResponse)crmServiceClient.Execute(request);

                Guid environmentGuid = response.OrganizationId;
                string environmentId = environmentGuid.ToString();
                LogInfo($"Retrieved Environment ID: {environmentId}");
                return environmentId;
            }
            catch (Exception ex)
            {
                LogError($"Failed to retrieve Environment ID: {ex.Message}");
                return string.Empty;
            }
        }

        private void resultTextBox_DoubleClick(object sender, EventArgs e)
        {
            if (resultTextBox.SelectedItems.Count > 0)
            {
                ListViewItem item = resultTextBox.SelectedItems[0];
                string solutionId = item.SubItems[2].Text;

                if (string.IsNullOrEmpty(solutionId))
                {
                    MessageBox.Show("No Solution ID found", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    Clipboard.SetText(solutionId);
                    MessageBox.Show($"Solution ID containing object copied to clipboard: \"{solutionId}\".", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Dictionary<string, int> valueMapping = new Dictionary<string, int>
            {
                { "All", -1 },
                { "Workflow", 0 },
                { "Dialog", 1 },
                { "Business Rule", 2 },
                { "Action", 3 },
                { "Business Process", 4 },
                { "Modern/Cloud", 5 },
                { "Desktop", 6 },
                { "AI", 7 }
            };

            string selectedLabel = comboBox1.SelectedItem.ToString();
            if (valueMapping.ContainsKey(selectedLabel))
            {
                selectedCategory = valueMapping[selectedLabel];
            }
        }
    }
}
