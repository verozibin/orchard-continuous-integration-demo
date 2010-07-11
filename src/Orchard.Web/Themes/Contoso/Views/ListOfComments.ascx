﻿<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<IEnumerable<Comment>>" %>
<%@ Import Namespace="Orchard.Comments.Models"%>
<ul class="comments"><%
foreach (var comment in Model) { %>
    <li>
        <div class="comment">
            <p><%: comment.Record.CommentText %></p>
        </div>
        <div class="commentauthor">
<span class="who"><%: Html.LinkOrDefault(comment.Record.UserName, comment.Record.SiteName, new { rel = "nofollow" })%></span>&nbsp;<span>said <%: Html.Link(Html.DateTimeRelative(comment.Record.CommentDateUtc.GetValueOrDefault(), T).ToString(), "#")%></span>
        </div>       
    </li><%
} %>
</ul>