<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<RoleCreateViewModel>" %>
<%@ Import Namespace="Orchard.Roles.ViewModels"%>
<h1><%: Html.TitleForPage(T("Add Role").ToString()) %></h1>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <%: Html.ValidationSummary()%>
    <fieldset>
	    <legend><%: T("Information") %></legend>
	    <label for="pageTitle"><%: T("Role Name:") %></label>
	    <input id="Name" class="text" name="Name" type="text" value="<%: Model.Name %>" />
    </fieldset>
    <fieldset>
        <legend><%: T("Permissions") %></legend>
        <% foreach (var moduleName in Model.ModulePermissions.Keys) { %>
        <fieldset>
            <legend><%: T("{0} Module", moduleName) %></legend>
            <table class="items">
                <colgroup>
                    <col id="Permission" />
                    <col id="Allow" />
                </colgroup>
                <thead>
                    <tr>
                        <th scope="col"><%: T("Permission") %></th>
                        <th scope="col"><%: T("Allow") %></th>
                    </tr>
                </thead>
                <% foreach (var permission in Model.ModulePermissions[moduleName]) { %>
                <tr>
                    <td><%: permission.Description %></td>
                    <td style="width:60px;/* todo: (heskew) make not inline :( */"><input type="checkbox" value="true" name="<%: T("Checkbox.{0}", permission.Name) %>"/></td>
                </tr>
                <% } %>
            </table>
        </fieldset>
        <% } %>
    </fieldset>
    <fieldset>
       <input type="submit" class="button primaryAction" value="<%: T("Save") %>" />
    </fieldset>
<% } %>