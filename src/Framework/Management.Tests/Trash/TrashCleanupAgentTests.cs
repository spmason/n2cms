﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using N2.Tests;
using N2.Tests.Fakes;
using N2.Persistence.NH;
using N2.Definitions;
using N2.Edit.Trash;
using N2.Persistence;
using N2.Web;
using N2.Engine;
using Rhino.Mocks;
using N2.Configuration;
using NHibernate.Tool.hbm2ddl;

namespace N2.Edit.Tests.Trash
{
	[TestFixture]
	public class TrashCleanupAgentTests : ItemTestsBase
	{
		ContentEngine engine;
		ContentItem root;
		TrashContainerItem trash;
		Func<DateTime> currentTimeBackup;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			engine = new ContentEngine();
			var schemaCreator = new SchemaExport(engine.Resolve<IConfigurationBuilder>().BuildConfiguration());
#if NH2_1
			schemaCreator.Execute(false, true, false, engine.Resolve<ISessionProvider>().OpenSession.Session.Connection, null);
#else
			schemaCreator.Execute(false, true, false, false, engine.Resolve<ISessionProvider>().OpenSession.Session.Connection, null);
#endif

			engine.Initialize();
			engine.SecurityManager.Enabled = false;

			root = new ThrowableItem();
			root.Name = "root";

			engine.Persister.Save(root);
			engine.Resolve<IHost>().DefaultSite.RootItemID = root.ID;
			engine.Resolve<IHost>().DefaultSite.StartPageID = root.ID;

			currentTimeBackup = Utility.CurrentTime;

			trash = ((TrashHandler)engine.Resolve<ITrashHandler>()).GetTrashContainer(true);
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			engine.Persister.Repository.Delete(root);
			engine.Persister.Dispose();

			Utility.CurrentTime = currentTimeBackup;
		}

		[TearDown]
		public override void TearDown()
		{
			while (trash.Children.Count > 0)
			{
				engine.Persister.Delete(trash.Children[0]);
			}
			base.TearDown();
		}

		[TestCase(TrashPurgeInterval.Dayly)]
		[TestCase(TrashPurgeInterval.Weekly)]
		[TestCase(TrashPurgeInterval.Monthly)]
		[TestCase(TrashPurgeInterval.Quarterly)]
		[TestCase(TrashPurgeInterval.Yearly)]
		public void Trash_IsNotPurged_Before_PurgeInterval_HasElapsed(TrashPurgeInterval interval)
		{
			trash.PurgeInterval = interval;
			engine.Persister.Save(trash);

			ContentItem item = new ThrowableItem();
			item.Name = "nullness' avenge";
			item.AddTo(root);
			engine.Persister.Save(item);
			engine.Persister.Delete(item);

			Utility.CurrentTime = () => DateTime.Now.AddDays((int)interval).AddSeconds(-10);

			engine.Resolve<ITrashHandler>().PurgeOldItems();

			Assert.That(trash.Children.Count, Is.EqualTo(1));
		}
		
		public void Trash_IsNotPurged_WhenInterval_IsSetToNever()
		{
			trash.PurgeInterval = TrashPurgeInterval.Never;
			engine.Persister.Save(trash);

			ContentItem item = new ThrowableItem();
			item.Name = "nullness' neverland";
			item.AddTo(root);
			engine.Persister.Save(item);
			engine.Persister.Delete(item);

			Utility.CurrentTime = () => DateTime.Now.AddDays(1000);

			engine.Resolve<ITrashHandler>().PurgeOldItems();

			Assert.That(trash.Children.Count, Is.EqualTo(1));
		}

		[TestCase(TrashPurgeInterval.Dayly)]
		[TestCase(TrashPurgeInterval.Weekly)]
		[TestCase(TrashPurgeInterval.Monthly)]
		[TestCase(TrashPurgeInterval.Quarterly)]
		[TestCase(TrashPurgeInterval.Yearly)]
		public void Trash_CanBePurged_OfOldItems(TrashPurgeInterval interval)
		{
			trash.PurgeInterval = interval;
			engine.Persister.Save(trash);
			
			ContentItem item = new ThrowableItem();
			item.Name = "nullness' destiny";
			item.AddTo(root);
			engine.Persister.Save(item);
			engine.Persister.Delete(item);

			Utility.CurrentTime = () => DateTime.Now.AddDays((int)interval);

			engine.Resolve<ITrashHandler>().PurgeOldItems();

			Assert.That(trash.Children.Count, Is.EqualTo(0));
		}
	}
}
