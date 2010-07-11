﻿using System.Collections.Generic;
using System.Web.Mvc;

namespace Orchard.ContentManagement.MetaData.Models {
    public class SettingsDictionary : Dictionary<string, string> {
        public SettingsDictionary() { }
        public SettingsDictionary(IDictionary<string, string> dictionary) : base(dictionary) { }

        public T GetModel<T>() where T : class, new() {
            return GetModel<T>(typeof(T).Name);
        }

        public T GetModel<T>(string key) where T : class, new() {
            var binder = new DefaultModelBinder();
            var controllerContext = new ControllerContext();
            var context = new ModelBindingContext {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(T)),
                ModelName = key,
                ValueProvider = new DictionaryValueProvider<string>(this, null)
            };
            return (T)binder.BindModel(controllerContext, context) ?? new T();
        }
    }
}