﻿using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Security;
using Orchard.Tags.Helpers;
using Orchard.Tags.Models;
using Orchard.Tags.Services;
using Orchard.Tags.ViewModels;

namespace Orchard.Tags.Drivers {
    [UsedImplicitly]
    public class TagsPartDriver : ContentPartDriver<TagsPart> {
        private const string TemplateName = "Parts/Tags";
        private readonly ITagService _tagService;
        private readonly IAuthorizationService _authorizationService;

        public TagsPartDriver(ITagService tagService,
                             IAuthorizationService authorizationService) {
            _tagService = tagService;
            _authorizationService = authorizationService;
        }

        public virtual IUser CurrentUser { get; set; }

        protected override string Prefix {
            get { return "Tags"; }
        }

        protected override DriverResult Display(TagsPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_Tags_ShowTags",
                            () => shapeHelper.Parts_Tags_ShowTags(ContentPart: part, Tags: part.CurrentTags));
        }

        protected override DriverResult Editor(TagsPart part, dynamic shapeHelper) {
            if (!_authorizationService.TryCheckAccess(Permissions.ApplyTag, CurrentUser, part))
                return null;

            return ContentShape("Parts_Tags_Edit",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: BuildEditorViewModel(part), Prefix: Prefix));
        }

        protected override DriverResult Editor(TagsPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (!_authorizationService.TryCheckAccess(Permissions.ApplyTag, CurrentUser, part))
                return null;

            var model = new EditTagsViewModel();
            updater.TryUpdateModel(model, Prefix, null, null);

            var tagNames = TagHelpers.ParseCommaSeparatedTagNames(model.Tags);
            if (part.ContentItem.Id != 0) {
                _tagService.UpdateTagsForContentItem(part.ContentItem.Id, tagNames);
            }

            return ContentShape("Parts_Tags_Edit",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));
        }

        private static EditTagsViewModel BuildEditorViewModel(TagsPart part) {
            return new EditTagsViewModel {
                Tags = string.Join(", ", part.CurrentTags.Select((t, i) => t.TagName).ToArray())
            };
        }
    }
}