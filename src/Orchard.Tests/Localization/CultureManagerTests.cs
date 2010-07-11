﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web;
using Autofac;
using NHibernate;
using NUnit.Framework;
using Orchard.Caching;
using Orchard.Data;
using Orchard.Localization.Records;
using Orchard.Localization.Services;
using Orchard.Tests.ContentManagement;

namespace Orchard.Tests.Localization {
    [TestFixture]
    public class CultureManagerTests {
        private IContainer _container;
        private ICultureManager _cultureManager;
        private ISessionFactory _sessionFactory;
        private ISession _session;
        private string _databaseFileName;

        [TestFixtureSetUp]
        public void InitFixture() {
            _databaseFileName = System.IO.Path.GetTempFileName();
            _sessionFactory = DataUtility.CreateSessionFactory(
                _databaseFileName,
                typeof(CultureRecord));
        }

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<TestCultureSelector>().As<ICultureSelector>();
            builder.RegisterType<DefaultCultureManager>().As<ICultureManager>();
            builder.RegisterType<Signals>().As<ISignals>();
            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));
            _session = _sessionFactory.OpenSession();
            builder.RegisterInstance(new DefaultContentManagerTests.TestSessionLocator(_session)).As<ISessionLocator>();
            _container = builder.Build();
            _cultureManager = _container.Resolve<ICultureManager>();
        }

        [TearDown]
        public void Term() {
            _session.Close();
        }

        [TestFixtureTearDown]
        public void TermFixture() {
            File.Delete(_databaseFileName);
        }

        [Test]
        public void CultureManagerCanAddAndListValidCultures() {
            _cultureManager.AddCulture("tr-TR");
            _cultureManager.AddCulture("fr-FR");
            _cultureManager.AddCulture("bs-Latn-BA");
            List<string> cultures = new List<string>(_cultureManager.ListCultures());
            Assert.That(cultures.Count, Is.Not.EqualTo(0));
        }

        [Test]
        public void CultureManagerRejectsInvalidCultureNames() {
            Assert.Throws<ArgumentException>(() => _cultureManager.AddCulture("a-b-c"));
        }

        [Test]
        public void CultureManagerAcceptsValidDotNetCultureNames() {
            foreach (var cultureInfo in CultureInfo.GetCultures(CultureTypes.NeutralCultures)) {
                if (!String.IsNullOrEmpty(cultureInfo.Name)) {
                    Assert.DoesNotThrow(() => _cultureManager.AddCulture(cultureInfo.Name));
                }
            }

            foreach (var cultureInfo in CultureInfo.GetCultures(CultureTypes.SpecificCultures)) {
                if (!String.IsNullOrEmpty(cultureInfo.Name)) {
                    Assert.DoesNotThrow(() => _cultureManager.AddCulture(cultureInfo.Name));
                }
            }
        }

        [Test]
        public void CultureManagerReturnsCultureFromSelector() {
            Assert.That(_cultureManager.GetCurrentCulture(null), Is.EqualTo("en-US"));
        }
    }

    public class TestCultureSelector : ICultureSelector {
        public CultureSelectorResult GetCulture(HttpContext context) {
            return new CultureSelectorResult { Priority = 1, CultureName = "en-US" };
        }
    }
}
