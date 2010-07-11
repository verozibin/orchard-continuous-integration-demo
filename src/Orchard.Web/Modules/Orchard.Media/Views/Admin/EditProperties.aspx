<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<MediaFolderEditPropertiesViewModel>" %>
<%@ Import Namespace="Orchard.Media.Helpers"%>
<%@ Import Namespace="Orchard.Media.Models"%>
<%@ Import Namespace="Orchard.Media.ViewModels"%><%
Html.RegisterStyle("admin.css"); %>
<h1><%: Html.TitleForPage(T("Folder Properties").ToString())%></h1>
<div class="breadCrumbs">
<p><%: Html.ActionLink(T("Media Folders").ToString(), "Index")%> &#62; 
    <%foreach (FolderNavigation navigation in MediaHelpers.GetFolderNavigationHierarchy(Model.MediaPath)) {%>
        <%: Html.ActionLink(navigation.FolderName, "Edit",
                  new {name = navigation.FolderName, mediaPath = navigation.FolderPath})%> &#62;
	    
    <% } %>
    <%: T("Folder Properties")%></p>
</div>    
    
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <%: Html.ValidationSummary() %>
    <fieldset>
        <label for="Name"><%: T("Folder Name:") %></label>
		<input id="MediaPath" name="MediaPath" type="hidden" value="<%: Model.MediaPath %>" />
		<input id="Name" class="textMedium" name="Name" type="text" value="<%: Model.Name %>" />
    </fieldset>
    <fieldset>		
		<input type="submit" class="button primaryAction" name="submit.Save" value="<%: T("Save") %>" />
		<%--<input type="submit" class="button buttonFocus roundCorners" name="submit.Delete" value="<%: T("Remove") %>" />--%>
    </fieldset>
<% } %>