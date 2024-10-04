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
using System.Linq;

namespace paSearch
{
	public partial class MyPluginControl : PluginControlBase
	{
		private Settings mySettings;
		public int selectedCategory;

		public MyPluginControl()
		{
			InitializeComponent();
		}

		private void MyPluginControl_Load(object sender, EventArgs e)
		{
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

		/// <summary>
		/// This event occurs when the plugin is closed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MyPluginControl_OnCloseTool(object sender, EventArgs e)
		{
			// Before leaving, save the settings
			SettingsManager.Instance.Save(GetType(), mySettings);
		}

		/// <summary>
		/// This event occurs when the connection has been updated in XrmToolBox
		/// </summary>
		public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail, string actionName, object parameter)
		{
			base.UpdateConnection(newService, detail, actionName, parameter);

			if (mySettings != null && detail != null)
			{
				mySettings.LastUsedOrganizationWebappUrl = detail.WebApplicationUrl;
				LogInfo("Connection has changed to: {0}", detail.WebApplicationUrl);
			}
		}

		private string searchText = string.Empty;

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
						var paSearchResults = new List<Entity>(); // List to store the search results

						// First Query: Query workflows (Power Automates)
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

						// For each workflow, query solutioncomponent to find the related solutionid
						foreach (var paObject in paObjects.Entities)
						{
							var clientDataRaw = paObject.Contains("clientdata") ? paObject["clientdata"].ToString() : string.Empty;
							var clientDataJson = clientDataRaw.Replace("\\u0022", "\"");

							if (SearchFlowDefinition(clientDataJson, searchText))
							{
								// Query solutioncomponent for solutionid based on the workflowid
								Guid workflowId = paObject.GetAttributeValue<Guid>("workflowid");

								QueryExpression solutionComponentQuery = new QueryExpression("solutioncomponent")
								{
									ColumnSet = new ColumnSet("solutionid", "componenttype"),
									Criteria = new FilterExpression()
									{
										Conditions =
								{
									new ConditionExpression("objectid", ConditionOperator.Equal, workflowId),
									new ConditionExpression("componenttype", ConditionOperator.Equal, 29) // 29 is the componenttype for workflows
                                }
									}
								};

								// Retrieve solution component
								EntityCollection solutionComponents = Service.RetrieveMultiple(solutionComponentQuery);

								// If a solution component is found, add the solutionid to the workflow result
								if (solutionComponents.Entities.Count > 0)
								{
									var solutionComponent = solutionComponents.Entities.FirstOrDefault();

									// Check if the solutionid is a lookup and retrieve the Id
									if (solutionComponent.Contains("solutionid"))
									{
										var solutionIdLookup = solutionComponent.GetAttributeValue<EntityReference>("solutionid");
										if (solutionIdLookup != null)
										{
											Guid solutionId = solutionIdLookup.Id; // Get the solution Id
											paObject["solutionid"] = solutionId; // Add solutionid to the paObject

											// Now query the solutions entity for additional details
											QueryExpression solutionQuery = new QueryExpression("solution")
											{
												ColumnSet = new ColumnSet("solutionid", "friendlyname"), // Specify the columns you want to retrieve
												Criteria = new FilterExpression()
												{
													Conditions =
											{
												new ConditionExpression("solutionid", ConditionOperator.Equal, solutionId)
											}
												}
											};

											// Retrieve solution details
											EntityCollection solutions = Service.RetrieveMultiple(solutionQuery);

											// If a solution is found, add the friendlyname to the paObject
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
						resultTextBox.Items.Clear(); // Clear existing items
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
			return clientDataJson.IndexOf(searchTerm.Replace(' ', '_'), StringComparison.OrdinalIgnoreCase) >= 0;
		}

		// Function to get the CRM URL dynamically
		private string GetEnvironmentId()
		{
			var crmServiceClient = (CrmServiceClient)Service;
			// Create the WhoAmI request
			WhoAmIRequest request = new WhoAmIRequest();

			// Execute the request
			WhoAmIResponse response = (WhoAmIResponse)crmServiceClient.Execute(request);

			// Extract the OrganizationId (Environment GUID)
			Guid environmentGuid = response.OrganizationId;
			return environmentGuid.ToString();
		}

		// Handle the DoubleClick event to open the URL
		private void resultTextBox_DoubleClick_TODO(object sender, EventArgs e)
		{
			if (resultTextBox.SelectedItems.Count > 0)
			{
				ListViewItem item = resultTextBox.SelectedItems[0]; // Access the first selected item
				string solutionId = item.SubItems[1].Text; // Assuming the second column contains the SolutionId (GUID)
				string paUrl = GetEnvironmentId(); // Get the CRM URL dynamically
				string url = $"https://make.powerapps.com/{paUrl}/solutions/{solutionId}"; // Construct the URL

				// Log the URL for debugging
				Console.WriteLine($"URL: {url}");

				try
				{
					// Validate the URL
					Uri uriResult;
					bool isValidUrl = Uri.TryCreate(url, UriKind.Absolute, out uriResult) &&
									  (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

					if (isValidUrl)
					{
						System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
						{
							FileName = url,
							UseShellExecute = true
						});
					}
					else
					{
						MessageBox.Show($"The constructed URL is not valid: {url}");
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show($"An error occurred: {ex.Message}");
				}
			}
		}

		private void resultTextBox_DoubleClick(object sender, EventArgs e)
		{
			if (resultTextBox.SelectedItems.Count > 0)
			{
				ListViewItem item = resultTextBox.SelectedItems[0]; 
				string objectName = item.SubItems[0].Text; 

				Clipboard.SetText(objectName);

				MessageBox.Show($"Object Name {objectName} copied to clipboard.");
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
