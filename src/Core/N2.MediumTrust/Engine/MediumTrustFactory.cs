using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Configuration;
using N2.Definitions;
using N2.Details;
using N2.Edit;
using N2.Edit.Settings;
using N2.Engine;
using N2.Integrity;
using N2.MediumTrust.Configuration;
using N2.MediumTrust.Persistence.NH;
using N2.Persistence;
using N2.Persistence.Finder;
using N2.Persistence.NH;
using N2.Persistence.NH.Finder;
using N2.Security;
using N2.Serialization;
using N2.Web;

namespace N2.MediumTrust.Engine
{
	public class MediumTrustFactory : IEngine
	{
		private readonly IPersister persister;
		private readonly IUrlParser urlParser;
		private readonly IUrlRewriter rewriter;
		private readonly IDefinitionManager definitions;
		private readonly IIntegrityManager integrityManager;
		private readonly ISecurityManager securityManager;
		private readonly IEditManager editManager;
		private readonly ISessionProvider sessionProvider;
		private readonly IWebContext webContext;
		private readonly ISecurityEnforcer securityEnforcer;
		private readonly Site site;
		private readonly DefaultRequestLifeCycleHandler lifeCycleHandler;
		private readonly ItemFinder finder;
		private readonly IDictionary<Type, object> resolves = new Dictionary<Type, object>();
		private readonly VersionManager versioner;
		private readonly DefaultConfigurationBuilder nhBuilder;
		private readonly DefaultItemNotifier notifier;
		private readonly ITypeFinder typeFinder;
		private readonly IntegrityEnforcer integrityEnforcer;
		private readonly NHRepository<int, ContentItem> itemRepository;
		private readonly NHRepository<int, LinkDetail> linkRepository;
		private readonly ItemXmlReader xmlReader;
		private readonly ItemXmlWriter xmlWriter;
		private readonly DefinitionBuilder definitionBuilder;
		private readonly NHibernate.IInterceptor interceptor;
		private readonly MediumTrustSectionHandler configSection;

		public MediumTrustFactory()
		{
			configSection = (MediumTrustSectionHandler)WebConfigurationManager.GetSection("n2/mediumTrust");

			Resolves[typeof(Site)] = site = new Site(configSection.RootItemID, configSection.StartPageID);
			Resolves[typeof(IWebContext)] = webContext = new WebContextWrapper();
			Resolves[typeof(IItemNotifier)] = notifier = new DefaultItemNotifier();
			Resolves[typeof(ITypeFinder)] = typeFinder = new MediumTrustTypeFinder();
			Resolves[typeof(DefinitionBuilder)] = definitionBuilder = new DefinitionBuilder(typeFinder);
			Resolves[typeof(IDefinitionManager)] = definitions = new DefaultDefinitionManager(definitionBuilder, notifier);
			Resolves[typeof(IConfigurationBuilder)] = nhBuilder = new MediumTrustNHBuilder(definitions);
			Resolves[typeof(NHibernate.IInterceptor)] = interceptor = new NotifyingInterceptor(notifier);
			Resolves[typeof(ISessionProvider)] = sessionProvider = new DefaultSessionProvider(nhBuilder, interceptor, webContext);
			Resolves[typeof(IItemFinder)] = finder = new ItemFinder(sessionProvider, definitions);
			Resolves[typeof(INHRepository<int, ContentItem>)] = itemRepository = new NHRepository<int, ContentItem>(sessionProvider);
			Resolves[typeof(INHRepository<int, LinkDetail>)] = linkRepository = new NHRepository<int, LinkDetail>(sessionProvider);
			Resolves[typeof(IPersister)] = persister = new DefaultPersister(itemRepository, linkRepository, finder);
			Resolves[typeof(IUrlParser)] = urlParser = new DefaultUrlParser(persister, webContext, notifier, site);
			Resolves[typeof(ISecurityManager)] = securityManager = new DefaultSecurityManager(webContext);
			Resolves[typeof(ISecurityEnforcer)] = securityEnforcer = new SecurityEnforcer(persister, securityManager, urlParser, webContext);
			Resolves[typeof(IVersionManager)] = versioner = new VersionManager(persister, itemRepository);
			Resolves[typeof(IEditManager)] = editManager = new DefaultEditManager(typeFinder, definitions, persister, versioner);
			Resolves[typeof(IIntegrityManager)] = integrityManager = new DefaultIntegrityManager(definitions, urlParser);
			Resolves[typeof(IIntegrityEnforcer)] = integrityEnforcer = new IntegrityEnforcer(persister, integrityManager);
			Resolves[typeof(IUrlRewriter)] = rewriter = new UrlRewriter(persister, urlParser);
			Resolves[typeof(NavigationSettings)] = new NavigationSettings(webContext);
			Resolves[typeof(IRequestLifeCycleHandler)] = lifeCycleHandler = new DefaultRequestLifeCycleHandler(rewriter, securityEnforcer, sessionProvider, webContext);
			Resolves[typeof(ItemXmlReader)] = xmlReader = new ItemXmlReader(definitions);
			Resolves[typeof(Importer)] = new Importer(persister, xmlReader);
			Resolves[typeof(ItemXmlWriter)] = xmlWriter = new ItemXmlWriter(definitions, urlParser);
			Resolves[typeof(Exporter)] = new Exporter(xmlWriter);

			AttributeExplorer<IServiceEditable> serviceExplorer = new AttributeExplorer<IServiceEditable>();
			AttributeExplorer<IEditableContainer> containerExplorer = new AttributeExplorer<IEditableContainer>();
			SettingsManager settingsManager = new SettingsManager(serviceExplorer, containerExplorer);

			foreach (KeyValuePair<Type, object> pair in resolves)
			{
				settingsManager.Handle(pair.Key.Name, pair.Value.GetType());
			}

			EditableHierarchyBuilder<IServiceEditable> hierarchyBuilder = new EditableHierarchyBuilder<IServiceEditable>();
			Resolves[typeof (ISettingsProvider)] = new SettingsProvider( settingsManager, hierarchyBuilder);
			
			securityEnforcer.Start();
			integrityEnforcer.Start();

			//SettingsFacility sf = new SettingsFacility();
		}

		#region Properties
		public IPersister Persister
		{
			get { return persister; }
		}

		public IUrlParser UrlParser
		{
			get { return urlParser; }
		}

		public IUrlRewriter Rewriter
		{
			get { return rewriter; }
		}

		public IDefinitionManager Definitions
		{
			get { return definitions; }
		}

		public IIntegrityManager IntegrityManager
		{
			get { return integrityManager; }
		}

		public ISecurityManager SecurityManager
		{
			get { return securityManager; }
		}

		public IEditManager EditManager
		{
			get { return editManager; }
		}

		public IDictionary<Type, object> Resolves
		{
			get { return resolves; }
		} 
		#endregion

		#region Methods
		public void InitializePlugIns()
		{
			throw new NotImplementedException();
		}

		public void Attach(HttpApplication application)
		{
			lifeCycleHandler.Init(application);
		}

		public T Resolve<T>() where T : class
		{
			return Resolves[typeof(T)] as T;
		}

		public object Resolve(string key)
		{
			foreach (KeyValuePair<Type, object> pair in resolves)
			{
				if (pair.Key.Name == key)
					return pair.Value;
			}
			return null;
		}

		public void AddComponent(string key, Type classType)
		{
			throw new NotImplementedException();
		}

		public void AddComponent(string key, Type serviceType, Type classType)
		{
			throw new NotImplementedException();
		}

		public void AddFacility(string key, object facility)
		{
			throw new NotImplementedException();
		}

		public void Release(object instance)
		{
			throw new NotImplementedException();
		} 

		public void AddComponent(Type serviceType, object instance)
		{
			Resolves[serviceType] = instance;
		}

		#endregion
	}
}