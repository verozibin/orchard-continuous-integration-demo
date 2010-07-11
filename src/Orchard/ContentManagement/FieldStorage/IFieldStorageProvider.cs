﻿using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentManagement.FieldStorage {
    public interface IFieldStorageProvider : IDependency {
        string ProviderName { get; }
        
        IFieldStorage BindStorage(
            ContentPart contentPart, 
            ContentPartDefinition.Field partFieldDefinition);
    }
}