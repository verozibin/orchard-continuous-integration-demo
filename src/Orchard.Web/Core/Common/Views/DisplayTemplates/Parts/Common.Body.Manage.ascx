﻿<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<BodyDisplayViewModel>" %>
<%@ Import Namespace="Orchard.Core.Common.ViewModels"%>
<div class="manage">
    <%: Html.ItemEditLinkWithReturnUrl(T("Edit").ToString(), Model.BodyAspect.ContentItem) %>
</div>