using System;
using System.Linq;
using System.Web.UI.WebControls;
using CacheDemo.Data;
using CacheDemo.Data.Contracts;
using CacheDemo.Data.Helpers;
using CacheDemo.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Content = CacheDemo.Model.Content;
using System.Collections.Generic;

namespace CacheDemo.Tests.Controllers
{
    [TestClass]
    public class NodeControllerTest
    {
        [TestMethod]
        public void AddNodes_ParentChildRelationship_ParentShouldIncreasedBy2Children()
        {
            // Arrange
            IUnitOfWork unitOfWork = new UnitOfWork(new RepositoryProvider(new RepositoryFactories()));
            var nodeId = 1;
            var portalId = 1;

            var getNode = unitOfWork.Nodes.GetById(nodeId);
            var childNodes = getNode.Children;
            var childNodeCountBefore = 0;
            if (childNodes != null)
            {
                childNodeCountBefore = getNode.Children.Count();
            }
            var node = new Node { ParentId = getNode.Id, Content = new Content { Type = ContentType.Original, PortalId = portalId } };
            unitOfWork.Nodes.Add(node);
            node = new Node { ParentId = getNode.Id, Content = new Content { Type = ContentType.Original, PortalId = portalId } };
            unitOfWork.Nodes.Add(node);

            //Act
            unitOfWork.Commit();
            var childNodeCountAfter = unitOfWork.Nodes.GetById(nodeId).Children.Count();

            //var testData = unitOfWork.Nodes.GetAll().Where(x => x.Id > 6);
            //foreach (var n in testData)
            //{
            //    unitOfWork.Nodes.Delete(n);
            //}

            //unitOfWork.Commit();

            //Assert
            Assert.AreEqual(childNodeCountBefore + 2, childNodeCountAfter);
        }

        [TestMethod]
        public void MoveNodes_ParentChildRelationship_ShouldChangeParent()
        {
            // Arrange
            IUnitOfWork unitOfWork = new UnitOfWork(new RepositoryProvider(new RepositoryFactories()));
            var nodeId = 1;
            var portalId = 1;

            var getNode = unitOfWork.Nodes.GetById(nodeId);
            var childNodes = getNode.Children;
            var childNodeCountBefore = 0;
            if (childNodes != null)
            {
                childNodeCountBefore = getNode.Children.Count();
            }
            var node = new Node { ParentId = getNode.Id, Content = new Content { Type = ContentType.Original, PortalId = portalId } };
            unitOfWork.Nodes.Add(node);
            node = new Node { ParentId = getNode.Id, Content = new Content { Type = ContentType.Original, PortalId = portalId } };
            unitOfWork.Nodes.Add(node);

            //Act
            unitOfWork.Commit();
            var childNodeCountAfter = getNode.Children.Count();

            //Assert
            Assert.AreEqual(childNodeCountBefore + 2, childNodeCountAfter);

            var getNodeMoving = unitOfWork.Nodes.GetById(node.Id);
            getNodeMoving.ParentId = 3; // Change ParentId from 1 to 3

            unitOfWork.Nodes.Update(getNodeMoving);
            unitOfWork.Commit();

            var getNodeMoved = unitOfWork.Nodes.GetById(3);

            //Assert
            Assert.AreEqual(1, getNodeMoved.Children.Count());
        }

        //[TestCleanup]
        public void DeleteTestData()
        {
            IUnitOfWork unitOfWork = new UnitOfWork(new RepositoryProvider(new RepositoryFactories()));
            var testData = unitOfWork.Nodes.GetAll().Where(x => x.Id > 6);
            foreach (var node in testData)
            {
                unitOfWork.Nodes.Delete(node);
            }

            unitOfWork.Commit();
        }

        [TestMethod]
        public void VerifyNodeWithShortcutContents()
        {
            IUnitOfWork unitOfWork = new UnitOfWork(new RepositoryProvider(new RepositoryFactories()));
            var nodeId = 1;

            var getNode = unitOfWork.Nodes.GetById(nodeId);
            var nodeWithChildren = unitOfWork.Nodes.GetByIdChildrenAndShortcut(nodeId);

            foreach (var node in nodeWithChildren.Children)
            {
                if (node.Content.Type == ContentType.Shortcut)
                {
                    var originalContent = unitOfWork.Shortcuts.GetOriginalContentByLinkContentId(node.Content.Id);
                    node.Content = originalContent;
                }
            }

            var isAllNodeHaveOriginalContent = nodeWithChildren.Children.Any(x => x.Content.Type == ContentType.Original);

            Assert.IsTrue(isAllNodeHaveOriginalContent);
        }
    }
}
