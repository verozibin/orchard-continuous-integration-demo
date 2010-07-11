﻿<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Indexing.ViewModels.IndexViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html" %><%
Html.RegisterStyle("admin.css"); %>
<h1><%:Html.TitleForPage(T("Search Index Management").ToString()) %></h1><%
using (Html.BeginForm("update", "admin", FormMethod.Post, new {area = "Orchard.Indexing"})) { %>
    <fieldset>
        <% if (Model.IndexEntry == null) {%>
            <p><%:T("There is currently no search index")%></p>
        <% } else if (Model.IndexEntry.LastUpdateUtc == null) { %>
            <p><%:T("The search index has not been built yet.")%></p>
        <% } else { %>
            <% if (Model.IndexEntry.DocumentCount == 0) { %>
                <p><%:T("The search index does not contain any document.")%></p>
            <% } else { %>
                <p><%:T("The search index contains {0} document(s).", Model.IndexEntry.DocumentCount)%></p>
            <% } %>

            <% if (!Model.IndexEntry.Fields.Any()) { %>
                <p><%:T("The search index does not contain any field.")%></p>
            <% } else { %>
                <p><%:T("The search index contains the following fields: {0}.", string.Join(T(", ").Text, Model.IndexEntry.Fields))%></p>
            <% } %>

            <p><%:T("The search index was last updated {0}.", Html.DateTimeRelative(Model.IndexEntry.LastUpdateUtc.Value, T))%></p>
        <% } %>
        <p><%:T("Update the search index now: ") %><button type="submit" title="<%:T("Update the search index.") %>" class="primaryAction"><%:T("Update")%></button></p>
        <%:Html.AntiForgeryTokenOrchard() %>
    </fieldset><%
}
using (Html.BeginForm("rebuild", "admin", FormMethod.Post, new {area = "Orchard.Search"})) { %>
    <fieldset>
        <p><%:T("Rebuild the search index for a fresh start.") %> 
        <button type="submit" title="<%:T("Rebuild the search index.") %>"><%:T("Rebuild") %></button></p>
        <%:Html.AntiForgeryTokenOrchard() %>
    </fieldset><%
} %>