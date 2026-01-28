namespace App.Core.Domain.Extension.ProjectIntegrations;

public static class ProjectIntegrationDefaults
{
	#region Static System Names for Azure

	public const string Azure = "Azure";

    public const string AzureOrganizationName = "azuresettings.organizationname";

    public const string AzureProjectName = "azuresettings.projectname";

    public const string AzureClientId = "azuresettings.clientid";

	public const string AzureClientSecret = "azuresettings.clientsecret";
	
	public const string AzureTenantId = "azuresettings.tenantid";
	
	public const string AzureUserId = "azuresettings.userid";

	public const string AzurePersonalAccessToken = "azuresettings.PAT";

    #endregion

    #region Static System Names for Jira

    public const string Jira = "Jira";

    public const string JiraClientId = "jirasettings.clientid";

    public const string JiraClientSecret = "jirasettings.clientsecret";

    public const string JiraTenantId = "jirasettings.tenantid";

    public const string JiraUserId = "jirasettings.userid";

    #endregion

    #region Static System Names for Asana

    public const string Asana = "Asana";

    public const string AsanaClientId = "asanasettings.clientid";

    public const string AsanaClientSecret = "asanasettings.clientsecret";

    public const string AsanaTenantId = "asanasettings.tenantid";

    public const string AsanaUserId = "asanasettings.userid";

    #endregion
}
