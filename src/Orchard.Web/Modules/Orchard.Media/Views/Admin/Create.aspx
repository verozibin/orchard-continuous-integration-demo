<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<MediaFolderCreateViewModel>" %>
<%@ Import Namespace="Orchard.Media.Helpers"%>
<%@ Import Namespace="Orchard.Media.Models"%>
<%@ Import Namespace="Orchard.Media.ViewModels"%><%
Html.RegisterStyle("admin.css"); %>
<h1><%: Html.TitleForPage(T("Add a Folder").ToString()) %></h1>
<div class="breadCrumbs">
<p><%: Html.ActionLink(T("Media Folders").ToString(), "Index") %> &#62; 
		<%foreach (FolderNavigation navigation in MediaHelpers.GetFolderNavigationHierarchy(Model.MediaPath)) { %>
		    <%: Html.ActionLink(navigation.FolderName, "Edit",
                      new {name = navigation.FolderName, mediaPath = navigation.FolderPath}) %> &#62;
		<% } %>
		<%: T("Add a Folder") %></p>
</div> 
			
<%using (Html.BeginFormAntiForgeryPost()) { %>
    <%: Html.ValidationSummary() %>
    <fieldset>
        <label for="Name"><%: T("Folder Name") %></label>
		<input id="Name" class="textMedium" name="Name" type="text" />
	    <input type="hidden" id="MediaPath" name="MediaPath" value="<%: Model.MediaPath %>" />
    </fieldset>
	<fieldset>
	    <input type="submit" class="button primaryAction" value="<%: T("Save") %>" />
    </fieldset>
 <% } %>