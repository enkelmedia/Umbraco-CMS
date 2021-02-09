// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Tests.Testing;
using Constants = Umbraco.Cms.Core.Constants;
using Content = Umbraco.Cms.Core.Models.Content;

namespace Umbraco.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Mapper = true, Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class ContentTypeRepositoryTest : UmbracoIntegrationTest
    {
        private ContentType _simpleContentType;
        private ContentType _textpageContentType;

        private IFileSystems FileSystems => GetRequiredService<IFileSystems>();

        private UmbracoMapper Mapper => GetRequiredService<UmbracoMapper>();

        private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

        private IDocumentTypeContainerRepository DocumentTypeContainerRepository => GetRequiredService<IDocumentTypeContainerRepository>();

        private IMediaTypeContainerRepository MediaTypeContainerRepository => GetRequiredService<IMediaTypeContainerRepository>();

        private IMediaTypeRepository MediaTypeRepository => GetRequiredService<IMediaTypeRepository>();

        private IDocumentRepository DocumentRepository => GetRequiredService<IDocumentRepository>();
        private IContentService ContentService => GetRequiredService<IContentService>();

        private ContentTypeRepository ContentTypeRepository => (ContentTypeRepository)GetRequiredService<IContentTypeRepository>();

        [SetUp]
        public void SetUpData() => CreateTestData();

        public void CreateTestData()
        {
            // Create and Save ContentType "umbTextpage" -> (_simpleContentType.Id)
            _simpleContentType = ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateId: 0);

            ContentTypeService.Save(_simpleContentType);

            // Create and Save ContentType "textPage" -> (_textpageContentType.Id)
            _textpageContentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: 0);
            ContentTypeService.Save(_textpageContentType);
        }

        // TODO: Add test to verify SetDefaultTemplates updates both AllowedTemplates and DefaultTemplate(id).

        [Test]
        public void Maps_Templates_Correctly()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                var templateRepo = new TemplateRepository((IScopeAccessor)provider, AppCaches.Disabled, LoggerFactory.CreateLogger<TemplateRepository>(), FileSystems, IOHelper, ShortStringHelper);
                ContentTypeRepository repository = ContentTypeRepository;
                Template[] templates = new[]
                {
                    new Template(ShortStringHelper, "test1", "test1"),
                    new Template(ShortStringHelper, "test2", "test2"),
                    new Template(ShortStringHelper, "test3", "test3")
                };
                foreach (Template template in templates)
                {
                    templateRepo.Save(template);
                }

                ContentType contentType = ContentTypeBuilder.CreateSimpleContentType();
                contentType.AllowedTemplates = new[] { templates[0], templates[1] };
                contentType.SetDefaultTemplate(templates[0]);
                repository.Save(contentType);

                // re-get
                IContentType result = repository.Get(contentType.Id);

                Assert.AreEqual(2, result.AllowedTemplates.Count());
                Assert.AreEqual(templates[0].Id, result.DefaultTemplate.Id);
            }
        }

        [Test]
        public void Can_Move()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                ContentTypeRepository repository = ContentTypeRepository;
                var container1 = new EntityContainer(Constants.ObjectTypes.DocumentType) { Name = "blah1" };
                DocumentTypeContainerRepository.Save(container1);

                var container2 = new EntityContainer(Constants.ObjectTypes.DocumentType) { Name = "blah2", ParentId = container1.Id };
                DocumentTypeContainerRepository.Save(container2);

                var contentType = (IContentType)ContentTypeBuilder.CreateBasicContentType("asdfasdf");
                contentType.ParentId = container2.Id;
                repository.Save(contentType);

                // create a
                var contentType2 = (IContentType)new ContentType(ShortStringHelper, contentType, "hello")
                {
                    Name = "Blahasdfsadf"
                };
                contentType.ParentId = contentType.Id;
                repository.Save(contentType2);

                global::Umbraco.Cms.Core.Events.MoveEventInfo<IContentType>[] result = repository.Move(contentType, container1).ToArray();

                Assert.AreEqual(2, result.Count());

                // re-get
                contentType = repository.Get(contentType.Id);
                contentType2 = repository.Get(contentType2.Id);

                Assert.AreEqual(container1.Id, contentType.ParentId);
                Assert.AreNotEqual(result.Single(x => x.Entity.Id == contentType.Id).OriginalPath, contentType.Path);
                Assert.AreNotEqual(result.Single(x => x.Entity.Id == contentType2.Id).OriginalPath, contentType2.Path);
            }
        }

        [Test]
        public void Can_Create_Container()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                var container = new EntityContainer(Constants.ObjectTypes.DocumentType) { Name = "blah" };
                DocumentTypeContainerRepository.Save(container);

                Assert.That(container.Id, Is.GreaterThan(0));

                EntityContainer found = DocumentTypeContainerRepository.Get(container.Id);
                Assert.IsNotNull(found);
            }
        }

        [Test]
        public void Can_Get_All_Containers()
        {
            EntityContainer container1, container2, container3;

            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                container1 = new EntityContainer(Constants.ObjectTypes.DocumentType) { Name = "container1" };
                DocumentTypeContainerRepository.Save(container1);
                container2 = new EntityContainer(Constants.ObjectTypes.DocumentType) { Name = "container2" };
                DocumentTypeContainerRepository.Save(container2);
                container3 = new EntityContainer(Constants.ObjectTypes.DocumentType) { Name = "container3" };
                DocumentTypeContainerRepository.Save(container3);

                Assert.That(container1.Id, Is.GreaterThan(0));
                Assert.That(container2.Id, Is.GreaterThan(0));
                Assert.That(container3.Id, Is.GreaterThan(0));

                EntityContainer found1 = DocumentTypeContainerRepository.Get(container1.Id);
                Assert.IsNotNull(found1);
                EntityContainer found2 = DocumentTypeContainerRepository.Get(container2.Id);
                Assert.IsNotNull(found2);
                EntityContainer found3 = DocumentTypeContainerRepository.Get(container3.Id);
                Assert.IsNotNull(found3);
                IEnumerable<EntityContainer> allContainers = DocumentTypeContainerRepository.GetMany();
                Assert.AreEqual(3, allContainers.Count());
            }
        }

        [Test]
        public void Can_Delete_Container()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                var container = new EntityContainer(Constants.ObjectTypes.DocumentType) { Name = "blah" };
                DocumentTypeContainerRepository.Save(container);

                // Act
                DocumentTypeContainerRepository.Delete(container);

                EntityContainer found = DocumentTypeContainerRepository.Get(container.Id);
                Assert.IsNull(found);
            }
        }

        [Test]
        public void Can_Create_Container_Containing_Media_Types()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                ContentTypeRepository repository = ContentTypeRepository;
                var container = new EntityContainer(Constants.ObjectTypes.MediaType) { Name = "blah" };
                MediaTypeContainerRepository.Save(container);

                ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("test", "Test", propertyGroupName: "testGroup", defaultTemplateId: 0);
                contentType.ParentId = container.Id;
                repository.Save(contentType);

                Assert.AreEqual(container.Id, contentType.ParentId);
            }
        }

        [Test]
        public void Can_Delete_Container_Containing_Media_Types()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                var container = new EntityContainer(Constants.ObjectTypes.MediaType) { Name = "blah" };
                MediaTypeContainerRepository.Save(container);

                IMediaType contentType = MediaTypeBuilder.CreateSimpleMediaType("test", "Test", propertyGroupName: "testGroup");
                contentType.ParentId = container.Id;
                MediaTypeRepository.Save(contentType);

                // Act
                MediaTypeContainerRepository.Delete(container);

                EntityContainer found = MediaTypeContainerRepository.Get(container.Id);
                Assert.IsNull(found);

                contentType = MediaTypeRepository.Get(contentType.Id);
                Assert.IsNotNull(contentType);
                Assert.AreEqual(-1, contentType.ParentId);
            }
        }

        [Test]
        public void Can_Perform_Add_On_ContentTypeRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                // Act
                ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("test", "Test", propertyGroupName: "testGroup");
                ContentTypeRepository.Save(contentType);

                IContentType fetched = ContentTypeRepository.Get(contentType.Id);

                // Assert
                Assert.That(contentType.HasIdentity, Is.True);
                Assert.That(contentType.PropertyGroups.All(x => x.HasIdentity), Is.True);
                Assert.That(contentType.PropertyTypes.All(x => x.HasIdentity), Is.True);
                Assert.That(contentType.Path.Contains(","), Is.True);
                Assert.That(contentType.SortOrder, Is.GreaterThan(0));

                Assert.That(contentType.PropertyGroups.ElementAt(0).Name == "testGroup", Is.True);
                int groupId = contentType.PropertyGroups.ElementAt(0).Id;
                Assert.That(contentType.PropertyTypes.All(x => x.PropertyGroupId.Value == groupId), Is.True);

                foreach (IPropertyType propertyType in contentType.PropertyTypes)
                {
                    Assert.AreNotEqual(propertyType.Key, Guid.Empty);
                }

                TestHelper.AssertPropertyValuesAreEqual(fetched, contentType, ignoreProperties: new[] { "DefaultTemplate", "AllowedTemplates", "UpdateDate" });
            }
        }

        [Test]
        public void Can_Perform_Add_On_ContentTypeRepository_After_Model_Mapping()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                ContentTypeRepository repository = ContentTypeRepository;

                // Act
                var contentType = (IContentType)ContentTypeBuilder.CreateSimpleContentType2("test", "Test", propertyGroupName: "testGroup");

                Assert.AreEqual(4, contentType.PropertyTypes.Count());

                // remove all templates - since they are not saved, they would break the (!) mapping code
                contentType.AllowedTemplates = new ITemplate[0];

                // there is NO mapping from display to contentType, but only from save
                // to contentType, so if we want to test, let's to it properly!
                DocumentTypeDisplay display = Mapper.Map<DocumentTypeDisplay>(contentType);
                DocumentTypeSave save = MapToContentTypeSave(display);
                IContentType mapped = Mapper.Map<IContentType>(save);

                Assert.AreEqual(4, mapped.PropertyTypes.Count());

                repository.Save(mapped);

                Assert.AreEqual(4, mapped.PropertyTypes.Count());

                // re-get
                contentType = repository.Get(mapped.Id);

                Assert.AreEqual(4, contentType.PropertyTypes.Count());

                // Assert
                Assert.That(contentType.HasIdentity, Is.True);
                Assert.That(contentType.PropertyGroups.All(x => x.HasIdentity), Is.True);
                Assert.That(contentType.PropertyTypes.All(x => x.HasIdentity), Is.True);
                Assert.That(contentType.Path.Contains(","), Is.True);
                Assert.That(contentType.SortOrder, Is.GreaterThan(0));

                Assert.That(contentType.PropertyGroups.ElementAt(0).Name == "testGroup", Is.True);
                int groupId = contentType.PropertyGroups.ElementAt(0).Id;

                IPropertyType[] propertyTypes = contentType.PropertyTypes.ToArray();
                Assert.AreEqual("gen", propertyTypes[0].Alias); // just to be sure
                Assert.IsNull(propertyTypes[0].PropertyGroupId);
                Assert.IsTrue(propertyTypes.Skip(1).All(x => x.PropertyGroupId.Value == groupId));
                Assert.That(propertyTypes.Single(x => x.Alias == "title").LabelOnTop, Is.True);
            }
        }

        [Test]
        public void Can_Perform_Update_On_ContentTypeRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                ContentTypeRepository repository = ContentTypeRepository;

                // Act
                IContentType contentType = repository.Get(_textpageContentType.Id);

                contentType.Thumbnail = "Doc2.png";
                contentType.PropertyGroups["Content"].PropertyTypes.Add(new PropertyType(ShortStringHelper, "test", ValueStorageType.Ntext, "subtitle")
                {
                    Name = "Subtitle",
                    Description = "Optional Subtitle",
                    Mandatory = false,
                    SortOrder = 1,
                    DataTypeId = -88,
                    LabelOnTop = true
                });
                repository.Save(contentType);

                bool dirty = contentType.IsDirty();

                // Assert
                Assert.That(contentType.HasIdentity, Is.True);
                Assert.That(dirty, Is.False);
                Assert.That(contentType.Thumbnail, Is.EqualTo("Doc2.png"));
                Assert.That(contentType.PropertyTypes.Any(x => x.Alias == "subtitle"), Is.True);
                Assert.That(contentType.PropertyTypes.Single(x => x.Alias == "subtitle").LabelOnTop, Is.True);
            }
        }

        // this is for tests only because it makes no sense at all to have such a
        // mapping defined, we only need it for the weird tests that use it
        private DocumentTypeSave MapToContentTypeSave(DocumentTypeDisplay display) =>
            new DocumentTypeSave
            {
                // EntityBasic
                Name = display.Name,
                Icon = display.Icon,
                Trashed = display.Trashed,
                Key = display.Key,
                ParentId = display.ParentId,
                //// Alias = display.Alias,
                Path = display.Path,
                //// AdditionalData = display.AdditionalData,

                // ContentTypeBasic
                Alias = display.Alias,
                UpdateDate = display.UpdateDate,
                CreateDate = display.CreateDate,
                Description = display.Description,
                Thumbnail = display.Thumbnail,

                // ContentTypeSave
                CompositeContentTypes = display.CompositeContentTypes,
                IsContainer = display.IsContainer,
                AllowAsRoot = display.AllowAsRoot,
                AllowedTemplates = display.AllowedTemplates.Select(x => x.Alias),
                AllowedContentTypes = display.AllowedContentTypes,
                DefaultTemplate = display.DefaultTemplate?.Alias,
                Groups = display.Groups.Select(x => new PropertyGroupBasic<PropertyTypeBasic>
                {
                    Inherited = x.Inherited,
                    Id = x.Id,
                    Properties = x.Properties,
                    SortOrder = x.SortOrder,
                    Name = x.Name
                }).ToArray()
            };

        [Test]
        public void Can_Perform_Update_On_ContentTypeRepository_After_Model_Mapping()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                ContentTypeRepository repository = ContentTypeRepository;

                // Act
                IContentType contentType = repository.Get(_textpageContentType.Id);

                // there is NO mapping from display to contentType, but only from save
                // to contentType, so if we want to test, let's to it properly!
                DocumentTypeDisplay display = Mapper.Map<DocumentTypeDisplay>(contentType);
                DocumentTypeSave save = MapToContentTypeSave(display);

                // modify...
                save.Thumbnail = "Doc2.png";
                PropertyGroupBasic<PropertyTypeBasic> contentGroup = save.Groups.Single(x => x.Name == "Content");
                contentGroup.Properties = contentGroup.Properties.Concat(new[]
                {
                    new PropertyTypeBasic
                    {
                        Alias = "subtitle",
                        Label = "Subtitle",
                        Description = "Optional Subtitle",
                        Validation = new PropertyTypeValidation
                        {
                            Mandatory = false,
                            Pattern = string.Empty
                        },
                        SortOrder = 1,
                        DataTypeId = -88,
                        LabelOnTop = true
                    }
                });

                IContentType mapped = Mapper.Map(save, contentType);

                // just making sure
                Assert.AreEqual(mapped.Thumbnail, "Doc2.png");
                Assert.IsTrue(mapped.PropertyTypes.Any(x => x.Alias == "subtitle"));
                Assert.IsTrue(mapped.PropertyTypes.Single(x => x.Alias == "subtitle").LabelOnTop);

                repository.Save(mapped);

                bool dirty = mapped.IsDirty();

                // re-get
                contentType = repository.Get(_textpageContentType.Id);

                // Assert
                Assert.That(contentType.HasIdentity, Is.True);
                Assert.That(dirty, Is.False);
                Assert.That(contentType.Thumbnail, Is.EqualTo("Doc2.png"));
                Assert.That(contentType.PropertyTypes.Any(x => x.Alias == "subtitle"), Is.True);

                Assert.That(contentType.PropertyTypes.Single(x => x.Alias == "subtitle").LabelOnTop, Is.True);

                foreach (IPropertyType propertyType in contentType.PropertyTypes)
                {
                    Assert.IsTrue(propertyType.HasIdentity);
                    Assert.Greater(propertyType.Id, 0);
                }
            }
        }

        [Test]
        public void Can_Perform_Delete_On_ContentTypeRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                ContentTypeRepository repository = ContentTypeRepository;

                // Act
                ContentType contentType = ContentTypeBuilder.CreateSimpleContentType(defaultTemplateId: 0);
                repository.Save(contentType);

                IContentType contentType2 = repository.Get(contentType.Id);
                repository.Delete(contentType2);

                bool exists = repository.Exists(contentType.Id);

                // Assert
                Assert.That(exists, Is.False);
            }
        }

        [Test]
        public void Can_Perform_Delete_With_Heirarchy_On_ContentTypeRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                ContentTypeRepository repository = ContentTypeRepository;
                ContentType ctMain = ContentTypeBuilder.CreateSimpleContentType(defaultTemplateId: 0);
                ContentType ctChild1 = ContentTypeBuilder.CreateSimpleContentType("child1", "Child 1", ctMain, randomizeAliases: true, defaultTemplateId: 0);
                ContentType ctChild2 = ContentTypeBuilder.CreateSimpleContentType("child2", "Child 2", ctChild1, randomizeAliases: true, defaultTemplateId: 0);

                repository.Save(ctMain);
                repository.Save(ctChild1);
                repository.Save(ctChild2);

                // Act
                IContentType resolvedParent = repository.Get(ctMain.Id);
                repository.Delete(resolvedParent);

                // Assert
                Assert.That(repository.Exists(ctMain.Id), Is.False);
                Assert.That(repository.Exists(ctChild1.Id), Is.False);
                Assert.That(repository.Exists(ctChild2.Id), Is.False);
            }
        }

        [Test]
        public void Can_Perform_Query_On_ContentTypeRepository_Sort_By_Name()
        {
            IContentType contentType;

            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                ContentTypeRepository repository = ContentTypeRepository;
                contentType = repository.Get(_textpageContentType.Id);
                ContentType child1 = ContentTypeBuilder.CreateSimpleContentType("abc", "abc", contentType, randomizeAliases: true, defaultTemplateId: 0);
                repository.Save(child1);
                ContentType child3 = ContentTypeBuilder.CreateSimpleContentType("zyx", "zyx", contentType, randomizeAliases: true, defaultTemplateId: 0);
                repository.Save(child3);
                ContentType child2 = ContentTypeBuilder.CreateSimpleContentType("a123", "a123", contentType, randomizeAliases: true, defaultTemplateId: 0);
                repository.Save(child2);

                scope.Complete();
            }

            using (IScope scope = provider.CreateScope())
            {
                ContentTypeRepository repository = ContentTypeRepository;

                // Act
                IEnumerable<IContentType> contentTypes = repository.Get(scope.SqlContext.Query<IContentType>().Where(x => x.ParentId == contentType.Id));

                // Assert
                Assert.That(contentTypes.Count(), Is.EqualTo(3));
                Assert.AreEqual("a123", contentTypes.ElementAt(0).Name);
                Assert.AreEqual("abc", contentTypes.ElementAt(1).Name);
                Assert.AreEqual("zyx", contentTypes.ElementAt(2).Name);
            }
        }

        [Test]
        public void Can_Perform_Get_On_ContentTypeRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                ContentTypeRepository repository = ContentTypeRepository;

                // Act
                IContentType contentType = repository.Get(_textpageContentType.Id);

                // Assert
                Assert.That(contentType, Is.Not.Null);
                Assert.That(contentType.Id, Is.EqualTo(_textpageContentType.Id));
            }
        }

        [Test]
        public void Can_Perform_Get_By_Guid_On_ContentTypeRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                ContentTypeRepository repository = ContentTypeRepository;
                IContentType contentType = repository.Get(_textpageContentType.Id);
                ContentType childContentType = ContentTypeBuilder.CreateSimpleContentType("blah", "Blah", contentType, randomizeAliases: true, defaultTemplateId: 0);
                repository.Save(childContentType);

                // Act
                IContentType result = repository.Get(childContentType.Key);

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Id, Is.EqualTo(childContentType.Id));
            }
        }

        [Test]
        public void Can_Perform_Get_By_Missing_Guid_On_ContentTypeRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                ContentTypeRepository repository = ContentTypeRepository;

                // Act
                IContentType result = repository.Get(Guid.NewGuid());

                // Assert
                Assert.That(result, Is.Null);
            }
        }

        [Test]
        public void Can_Perform_GetAll_On_ContentTypeRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                var repository = (ContentTypeRepository)ContentTypeRepository;

                // Act
                IEnumerable<IContentType> contentTypes = repository.GetMany();
                int count =
                    scope.Database.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM umbracoNode WHERE nodeObjectType = @NodeObjectType",
                        new { NodeObjectType = Constants.ObjectTypes.DocumentType });

                // Assert
                Assert.That(contentTypes.Any(), Is.True);
                Assert.That(contentTypes.Count(), Is.EqualTo(count));
            }
        }

        [Test]
        public void Can_Perform_GetAll_By_Guid_On_ContentTypeRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                var repository = (ContentTypeRepository)ContentTypeRepository;
                Guid[] allGuidIds = repository.GetMany().Select(x => x.Key).ToArray();

                // Act
                IEnumerable<IContentType> contentTypes = ((IReadRepository<Guid, IContentType>)repository).GetMany(allGuidIds);
                int count =
                    scope.Database.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM umbracoNode WHERE nodeObjectType = @NodeObjectType",
                        new { NodeObjectType = Constants.ObjectTypes.DocumentType });

                // Assert
                Assert.That(contentTypes.Any(), Is.True);
                Assert.That(contentTypes.Count(), Is.EqualTo(count));
            }
        }

        [Test]
        public void Can_Perform_Exists_On_ContentTypeRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                ContentTypeRepository repository = ContentTypeRepository;

                // Act
                bool exists = repository.Exists(_simpleContentType.Id);

                // Assert
                Assert.That(exists, Is.True);
            }
        }

        [Test]
        public void Can_Update_ContentType_With_PropertyType_Removed()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                ContentTypeRepository repository = ContentTypeRepository;
                IContentType contentType = repository.Get(_textpageContentType.Id);

                // Act
                contentType.PropertyGroups["Meta"].PropertyTypes.Remove("description");
                repository.Save(contentType);

                IContentType result = repository.Get(_textpageContentType.Id);

                // Assert
                Assert.That(result.PropertyTypes.Any(x => x.Alias == "description"), Is.False);
                Assert.That(contentType.PropertyGroups.Count, Is.EqualTo(result.PropertyGroups.Count));
                Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(result.PropertyTypes.Count()));
            }
        }

        [Test]
        public void Can_Verify_PropertyTypes_On_SimpleTextpage()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                ContentTypeRepository repository = ContentTypeRepository;

                // Act
                IContentType contentType = repository.Get(_simpleContentType.Id);

                // Assert
                Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(3));
                Assert.That(contentType.PropertyGroups.Count(), Is.EqualTo(1));
            }
        }

        [Test]
        public void Can_Verify_PropertyTypes_On_Textpage()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                ContentTypeRepository repository = ContentTypeRepository;

                // Act
                IContentType contentType = repository.Get(_textpageContentType.Id);

                // Assert
                Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(4));
                Assert.That(contentType.PropertyGroups.Count(), Is.EqualTo(2));
            }
        }

        [Test]
        public void Can_Verify_PropertyType_With_No_Group()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                ContentTypeRepository repository = ContentTypeRepository;
                IContentType contentType = repository.Get(_textpageContentType.Id);

                Assert.That(contentType.PropertyGroups.Count, Is.EqualTo(2));
                Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(4));

                // Act
                var urlAlias = new PropertyType(ShortStringHelper, "test", ValueStorageType.Nvarchar, "urlAlias")
                    {
                        Name = "Url Alias",
                        Description = string.Empty,
                        Mandatory = false,
                        SortOrder = 1,
                        DataTypeId = -88
                    };

                bool addedPropertyType = contentType.AddPropertyType(urlAlias);

                Assert.That(contentType.PropertyGroups.Count, Is.EqualTo(2));
                Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(5));

                repository.Save(contentType);

                // Assert
                IContentType updated = repository.Get(_textpageContentType.Id);
                Assert.That(addedPropertyType, Is.True);
                Assert.That(updated.PropertyGroups.Count, Is.EqualTo(2));
                Assert.That(updated.PropertyTypes.Count(), Is.EqualTo(5));
                Assert.That(updated.PropertyTypes.Any(x => x.Alias == "urlAlias"), Is.True);
                Assert.That(updated.PropertyTypes.First(x => x.Alias == "urlAlias").PropertyGroupId, Is.Null);
            }
        }

        [Test]
        public void Can_Verify_AllowedChildContentTypes_On_ContentType()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                ContentTypeRepository repository = ContentTypeRepository;

                ContentType subpageContentType = ContentTypeBuilder.CreateSimpleContentType("umbSubpage", "Subpage");
                ContentType simpleSubpageContentType = ContentTypeBuilder.CreateSimpleContentType("umbSimpleSubpage", "Simple Subpage");
                repository.Save(subpageContentType);
                repository.Save(simpleSubpageContentType);

                // Act
                IContentType contentType = repository.Get(_simpleContentType.Id);
                contentType.AllowedContentTypes = new List<ContentTypeSort>
                    {
                        new ContentTypeSort(new Lazy<int>(() => subpageContentType.Id), 0, subpageContentType.Alias),
                        new ContentTypeSort(new Lazy<int>(() => simpleSubpageContentType.Id), 1, simpleSubpageContentType.Alias)
                    };
                repository.Save(contentType);

                // Assert
                IContentType updated = repository.Get(_simpleContentType.Id);

                Assert.That(updated.AllowedContentTypes.Any(), Is.True);
                Assert.That(updated.AllowedContentTypes.Any(x => x.Alias == subpageContentType.Alias), Is.True);
                Assert.That(updated.AllowedContentTypes.Any(x => x.Alias == simpleSubpageContentType.Alias), Is.True);
            }
        }

        [Test]
        public void Can_Verify_Removal_Of_Used_PropertyType_From_ContentType()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                ContentTypeRepository repository = ContentTypeRepository;
                IContentType contentType = repository.Get(_textpageContentType.Id);

                Content subpage = ContentBuilder.CreateTextpageContent(contentType, "Text Page 1", contentType.Id);
                DocumentRepository.Save(subpage);

                // Act
                contentType.RemovePropertyType("keywords");
                repository.Save(contentType);

                // Assert
                Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(3));
                Assert.That(contentType.PropertyTypes.Any(x => x.Alias == "keywords"), Is.False);
                Assert.That(subpage.Properties.First(x => x.Alias == "description").GetValue(), Is.EqualTo("This is the meta description for a textpage"));
            }
        }

        [Test]
        public void Can_Verify_Addition_Of_PropertyType_After_ContentType_Is_Used()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                ContentTypeRepository repository = ContentTypeRepository;
                IContentType contentType = repository.Get(_textpageContentType.Id);

                Content subpage = ContentBuilder.CreateTextpageContent(contentType, "Text Page 1", contentType.Id);
                DocumentRepository.Save(subpage);

                // Act
                PropertyGroup propertyGroup = contentType.PropertyGroups.First(x => x.Name == "Meta");
                propertyGroup.PropertyTypes.Add(new PropertyType(ShortStringHelper, "test", ValueStorageType.Ntext, "metaAuthor") { Name = "Meta Author", Description = string.Empty, Mandatory = false, SortOrder = 1, DataTypeId = -88 });
                repository.Save(contentType);

                // Assert
                Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(5));
                Assert.That(contentType.PropertyTypes.Any(x => x.Alias == "metaAuthor"), Is.True);
            }
        }

        [Test]
        public void Can_Verify_Usage_Of_New_PropertyType_On_Content()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                ContentTypeRepository repository = ContentTypeRepository;

                IContentType contentType = repository.Get(_textpageContentType.Id);

                Content subpage = ContentBuilder.CreateTextpageContent(contentType, "Text Page 1", contentType.Id);
                DocumentRepository.Save(subpage);

                PropertyGroup propertyGroup = contentType.PropertyGroups.First(x => x.Name == "Meta");
                propertyGroup.PropertyTypes.Add(new PropertyType(ShortStringHelper, "test", ValueStorageType.Ntext, "metaAuthor") { Name = "Meta Author", Description = string.Empty, Mandatory = false, SortOrder = 1, DataTypeId = -88 });
                repository.Save(contentType);

                // Act
                IContent content = DocumentRepository.Get(subpage.Id);
                content.SetValue("metaAuthor", "John Doe");
                DocumentRepository.Save(content);

                // Assert
                IContent updated = DocumentRepository.Get(subpage.Id);
                Assert.That(updated.GetValue("metaAuthor").ToString(), Is.EqualTo("John Doe"));
                Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(5));
                Assert.That(contentType.PropertyTypes.Any(x => x.Alias == "metaAuthor"), Is.True);
            }
        }

        [Test]
        public void Can_Verify_That_A_Combination_Of_Adding_And_Deleting_PropertyTypes_Doesnt_Cause_Issues_For_Content_And_ContentType()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                ContentTypeRepository repository = ContentTypeRepository;

                IContentType contentType = repository.Get(_textpageContentType.Id);

                Content subpage = ContentBuilder.CreateTextpageContent(contentType, "Text Page 1", contentType.Id);
                DocumentRepository.Save(subpage);

                // Remove PropertyType
                contentType.RemovePropertyType("keywords");

                // Add PropertyType
                PropertyGroup propertyGroup = contentType.PropertyGroups.First(x => x.Name == "Meta");
                propertyGroup.PropertyTypes.Add(new PropertyType(ShortStringHelper, "test", ValueStorageType.Ntext, "metaAuthor") { Name = "Meta Author", Description = string.Empty,  Mandatory = false, SortOrder = 1, DataTypeId = -88 });
                repository.Save(contentType);

                // Act
                IContent content = DocumentRepository.Get(subpage.Id);
                content.SetValue("metaAuthor", "John Doe");
                DocumentRepository.Save(content);

                // Assert
                IContent updated = DocumentRepository.Get(subpage.Id);
                Assert.That(updated.GetValue("metaAuthor").ToString(), Is.EqualTo("John Doe"));
                Assert.That(updated.Properties.First(x => x.Alias == "description").GetValue(), Is.EqualTo("This is the meta description for a textpage"));

                Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(4));
                Assert.That(contentType.PropertyTypes.Any(x => x.Alias == "metaAuthor"), Is.True);
                Assert.That(contentType.PropertyTypes.Any(x => x.Alias == "keywords"), Is.False);
            }
        }

        [Test]
        public void Can_Verify_Content_Type_Has_Content_Nodes()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                ContentTypeRepository repository = ContentTypeRepository;

                int contentTypeId = _textpageContentType.Id;
                IContentType contentType = repository.Get(contentTypeId);

                // Act
                bool result = repository.HasContentNodes(contentTypeId);

                Content subpage = ContentBuilder.CreateTextpageContent(contentType, "Test Page 1", contentType.Id);
                DocumentRepository.Save(subpage);

                bool result2 = repository.HasContentNodes(contentTypeId);

                // Assert
                Assert.That(result, Is.False);
                Assert.That(result2, Is.True);
            }
        }

        [Test]
        public void Can_Update_Variation_Of_Element_Type_Property()
        {
            var provider = ScopeProvider;
            using (var scope = provider.CreateScope())
            {
                ContentTypeRepository repository = ContentTypeRepository;
                var contentRepository = DocumentRepository;

                // Create elementType
                var elementType = new ContentType(ShortStringHelper, -1)
                {
                    Alias = "elementType",
                    Name = "Element type",
                    Description = "Element type to use as compositions",
                    Icon = ".sprTreeDoc3",
                    Thumbnail = "doc.png",
                    SortOrder = 1,
                    CreatorId = 0,
                    Trashed = false,
                    IsElement = true,
                    Variations = ContentVariation.Nothing
                };

                var contentCollection = new PropertyTypeCollection(true);
                contentCollection.Add(new PropertyType(ShortStringHelper, "test", ValueStorageType.Ntext)
                {
                    Alias = "title",
                    Name = "Title",
                    Description = "",
                    Mandatory = false,
                    SortOrder = 1,
                    DataTypeId = Constants.DataTypes.Textbox,
                    LabelOnTop = true,
                    Variations = ContentVariation.Nothing
                });
                elementType.PropertyGroups.Add(new PropertyGroup(contentCollection) {Name = "Content", SortOrder = 1});
                elementType.ResetDirtyProperties(false);
                elementType.SetDefaultTemplate(new Template(ShortStringHelper, "ElementType", "elementType"));
                repository.Save(elementType);

                // Create the basic "home" doc type that uses element type as comp
                var docType = new ContentType(ShortStringHelper, -1)
                {
                    Alias = "home",
                    Name = "Home",
                    Description = "Home containing elementType",
                    Icon = ".sprTreeDoc3",
                    Thumbnail = "doc.png",
                    SortOrder = 1,
                    CreatorId = 0,
                    Trashed = false,
                    Variations = ContentVariation.Nothing
                };
                var comp = new List<IContentTypeComposition>();
                comp.Add(elementType);
                docType.ContentTypeComposition = comp;
                repository.Save(docType);

                // Create "home" content
                var content = new Content("Home", -1, docType){Level = 1, SortOrder = 1, CreatorId = 0, WriterId = 0};
                object obj = new {title = "test title"};
                content.PropertyValues(obj);
                content.ResetDirtyProperties(false);
                contentRepository.Save(content);

                // Update variation on element type
                elementType.Variations = ContentVariation.Culture;
                elementType.PropertyTypes.First().Variations = ContentVariation.Culture;
                repository.Save(elementType);

                // Update variation on doc type
                docType.Variations = ContentVariation.Culture;
                repository.Save(docType);

                // Re fetch renewedContent and make sure that the culture has been set.
                var renewedContent = ContentService.GetById(content.Id);
                var hasCulture = renewedContent.Properties["title"].Values.First().Culture != null;
                Assert.That(hasCulture, Is.True);
            }
        }
    }
}
