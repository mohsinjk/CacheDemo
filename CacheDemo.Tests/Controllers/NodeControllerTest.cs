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

            var getNodeDto = unitOfWork.Nodes.GetNodeDtoByIdWithChildren(nodeId);
            var childNodeCountBefore = ChildNodeCountBefore(getNodeDto);

            //Act
            AddNodes(unitOfWork, getNodeDto);
            var childNodeCountAfter = unitOfWork.Nodes.GetNodeDtoByIdWithChildren(nodeId).Children.Count();

            //Assert
            Assert.AreEqual(childNodeCountBefore + 2, childNodeCountAfter);
        }

        [TestMethod]
        public void MoveNodes_ParentChildRelationship_ShouldChangeParent()
        {
            // Arrange
            IUnitOfWork unitOfWork = new UnitOfWork(new RepositoryProvider(new RepositoryFactories()));
            var nodeId = 1;
            var getNodeDto = unitOfWork.Nodes.GetNodeDtoByIdWithChildren(nodeId);
            var childNodeCountBefore = ChildNodeCountBefore(getNodeDto);

            //Act
            MoveNode(unitOfWork, getNodeDto);
            var getNodeAfterNewlyAddedChild = unitOfWork.Nodes.GetNodeDtoByIdWithChildren(nodeId);
            var getNodeAfterMovingChildFromAnOtherParent = unitOfWork.Nodes.GetNodeDtoByIdWithChildren(4);
            //Assert
            Assert.AreEqual(childNodeCountBefore + 1, getNodeAfterNewlyAddedChild.Children.Count());
            Assert.AreEqual(0, getNodeAfterMovingChildFromAnOtherParent.Children.Count());
        }

        [TestMethod]
        public void VerifyNodeWithShortcutContents()
        {
            IUnitOfWork unitOfWork = new UnitOfWork(new RepositoryProvider(new RepositoryFactories()));
            var nodeId = 1;

            //var getNode = unitOfWork.Nodes.GetById(nodeId);
            var nodeWithChildren = unitOfWork.Nodes.GetNodeDtoByIdWithChildren(nodeId);

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

        [TestMethod]
        public void DeleteNodes_ParentChildRelationship_ParentShouldSameChildren()
        {
            // Arrange
            IUnitOfWork unitOfWork = new UnitOfWork(new RepositoryProvider(new RepositoryFactories()));
            var nodeId = 1;

            var getNodeDto = unitOfWork.Nodes.GetNodeDtoByIdWithChildren(nodeId);
            var childNodeCountBefore = ChildNodeCountBefore(getNodeDto);

            //Act
            AddNodes(unitOfWork, getNodeDto);
            var childNodeCountAfter = unitOfWork.Nodes.GetNodeDtoByIdWithChildren(nodeId).Children.Count();

            //Assert
            Assert.AreEqual(childNodeCountBefore + 2, childNodeCountAfter);

            var testData = unitOfWork.Nodes.GetAll().Where(x => x.Id > 11);
            foreach (var node in testData)
            {
                unitOfWork.Nodes.Delete(node);
            }

            unitOfWork.Commit();

            childNodeCountAfter = unitOfWork.Nodes.GetNodeDtoByIdWithChildren(nodeId).Children.Count();
            //Assert
            Assert.AreEqual(childNodeCountBefore, childNodeCountAfter);
        }
        
        
        [TestCleanup]
        public void DeleteTestData()
        {
            IUnitOfWork unitOfWork = new UnitOfWork(new RepositoryProvider(new RepositoryFactories()));
            var testData = unitOfWork.Nodes.GetAll().Where(x => x.Id > 11);
            foreach (var node in testData)
            {
                unitOfWork.Nodes.Delete(node);
            }

            unitOfWork.Commit();
        }


        private static void MoveNode(IUnitOfWork unitOfWork, NodeDTO nodeDto)
        {
            var getMovingNode = unitOfWork.Nodes.GetById(9);
            getMovingNode.ParentId = nodeDto.Id; // Change ParentId from 4 to 1

            unitOfWork.Nodes.Update(getMovingNode);
            unitOfWork.Commit();
            unitOfWork.Nodes.UpdateCacheForItsParent(getMovingNode);
        }

        private static int ChildNodeCountBefore(NodeDTO getNode)
        {
            var childNodes = getNode.Children;
            var childNodeCountBefore = 0;
            if (childNodes != null)
            {
                childNodeCountBefore = getNode.Children.Count();
            }
            return childNodeCountBefore;
        }

        private static void AddNodes(IUnitOfWork unitOfWork, NodeDTO getNode)
        {
            var portalId = 1;
            var node1 = new Node { ParentId = getNode.Id, Content = new Content { Type = ContentType.Original, PortalId = portalId } };
            unitOfWork.Nodes.Add(node1);
            var node2 = new Node { ParentId = getNode.Id, Content = new Content { Type = ContentType.Original, PortalId = portalId } };
            unitOfWork.Nodes.Add(node2);

            //Act
            unitOfWork.Commit();
            unitOfWork.Nodes.UpdateCacheForItsParent(node1);
            unitOfWork.Nodes.UpdateCacheForItsParent(node2);
        }
    }
}
