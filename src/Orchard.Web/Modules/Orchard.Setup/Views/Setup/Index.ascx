﻿<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<SetupViewModel>" %>
<%@ Import Namespace="Orchard.Setup.ViewModels"%>
<h1><%: Html.TitleForPage(T("Get Started").ToString())%></h1>
<%
using (Html.BeginFormAntiForgeryPost()) { %>
<%: Html.ValidationSummary() %>
<h2><%: T("Please answer a few questions to configure your site.")%></h2>
<fieldset class="site">
    <div>
        <label for="SiteName"><%: T("What is the name of your site?") %></label>
        <%: Html.TextBoxFor(svm => svm.SiteName, new { autofocus = "autofocus" })%>
    </div>
    <div>
        <label for="AdminUsername"><%: T("Choose a user name:") %></label>
        <%: Html.EditorFor(svm => svm.AdminUsername)%>
    </div>
    <div>
        <label for="AdminPassword"><%: T("Choose a password:") %></label>
        <%: Html.PasswordFor(svm => svm.AdminPassword) %>
    </div>
    <div>
        <label for="ConfirmAdminPassword"><%: T("Confirm the password:") %></label>
        <%: Html.PasswordFor(svm => svm.ConfirmPassword)%>
    </div>
</fieldset><%
if (!Model.DatabaseIsPreconfigured) { %>
<fieldset class="data">
    <legend><%: T("How would you like to store your data?") %></legend>
    <%: Html.ValidationMessage("DatabaseOptions", "Unable to setup data storage") %>
    <div>
        <%: Html.RadioButtonFor(svm => svm.DatabaseOptions, true, new { id = "builtin" })%>
        <label for="builtin" class="forcheckbox"><%: T("Use built-in data storage (SQLite)") %></label>
    </div>
    <div>
        <%: Html.RadioButtonFor(svm => svm.DatabaseOptions, false, new { id = "sql" })%>
        <label for="sql" class="forcheckbox"><%: T("Use an existing SQL Server (or SQL Express) database") %></label>
        <div data-controllerid="sql">
            <label for="DatabaseConnectionString"><%: T("Connection string") %></label>
            <%: Html.EditorFor(svm => svm.DatabaseConnectionString)%>
            <span class="hint"><%: T("Example:") %><br /><%: T("Data Source=sqlServerName;Initial Catalog=dbName;Persist Security Info=True;User ID=userName;Password=password") %></span>
        </div>
        <div data-controllerid="sql">
            <label for="DatabaseTablePrefix"><%: T("Database Table Prefix") %></label>
            <%: Html.EditorFor(svm => svm.DatabaseTablePrefix)%>
        </div>
    </div>
</fieldset><%
} %>
<fieldset>
    <input class="button" type="submit" value="<%: T("Finish Setup") %>" />
</fieldset><%
} %>