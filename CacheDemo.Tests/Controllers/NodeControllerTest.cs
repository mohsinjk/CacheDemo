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
using System.Data.Entity;
using AutoMapper;

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
            Assert.AreEqual(2, getNodeAfterMovingChildFromAnOtherParent.Children.Count());
        }

        [TestMethod]
        public void GetNode_VerifyNodeWithShortcutContents_ShouldReturnOriginalContents()
        {
            IUnitOfWork unitOfWork = new UnitOfWork(new RepositoryProvider(new RepositoryFactories()));
            var nodeId = 1;

            var nodeWithChildren = unitOfWork.Nodes.GetNodeDtoByIdWithChildren(nodeId);

            foreach (var node in nodeWithChildren.Children)
            {
                if (node.Content.Type == ContentType.Shortcut)
                {
                    var shortcut = unitOfWork.Shortcuts.GetOriginalContentByLinkContentId(node.Content.Id);
                    node.Content = shortcut.OriginalContent;
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

            var testData = unitOfWork.Nodes.GetAll().Where(x => x.Id > 12);
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
            var testData = unitOfWork.Nodes.GetAll().Where(x => x.Id > 12);
            foreach (var node in testData)
            {
                unitOfWork.Nodes.Delete(node);
            }

            unitOfWork.Commit();
        }

        private static void MoveNode(IUnitOfWork unitOfWork, NodeWithOneLevelChildern nodeWithOneLevelChildern)
        {
            var getMovingNode = unitOfWork.Nodes.GetById(9);
            getMovingNode.ParentId = nodeWithOneLevelChildern.Id; // Change ParentId from 4 to 1

            unitOfWork.Nodes.Update(getMovingNode);
            unitOfWork.Commit();
            unitOfWork.Nodes.UpdateCacheForItsParent(getMovingNode);
        }

        private static int ChildNodeCountBefore(NodeWithOneLevelChildern getNode)
        {
            var childNodes = getNode.Children;
            var childNodeCountBefore = 0;
            if (childNodes != null)
            {
                childNodeCountBefore = getNode.Children.Count();
            }
            return childNodeCountBefore;
        }

        private static void AddNodes(IUnitOfWork unitOfWork, NodeWithOneLevelChildern getNode)
        {
            var node1 = new Node { ParentId = getNode.Id, Content = new Content { Type = ContentType.Original, PortalId = getNode.Content.PortalId } };
            unitOfWork.Nodes.Add(node1);
            var node2 = new Node { ParentId = getNode.Id, Content = new Content { Type = ContentType.Original, PortalId = getNode.Content.PortalId } };
            unitOfWork.Nodes.Add(node2);

            //Act
            unitOfWork.Commit();
            unitOfWork.Nodes.UpdateCacheForItsParent(node1);
            unitOfWork.Nodes.UpdateCacheForItsParent(node2);
        }

        private NodeWithOneLevelChildern TransformFromNodeToNodeDTO(Node node)
        {
            Mapper.CreateMap<Node, NodeWithOneLevelChildern>();
            var d = Mapper.Map<Node, NodeWithOneLevelChildern>(node);
            NodeWithOneLevelChildern nodeWithOneLevelChildern = new NodeWithOneLevelChildern();
            nodeWithOneLevelChildern.Id = node.Id;
            nodeWithOneLevelChildern.Description = node.Description;
            nodeWithOneLevelChildern.ContentId = node.ContentId;
            nodeWithOneLevelChildern.Content = node.Content;
            nodeWithOneLevelChildern.ParentId = node.ParentId;
            if (node.Parent != null)
            {
                nodeWithOneLevelChildern.Parent = new NodeWithOneLevelChildern { Id = node.Parent.Id, Description = node.Parent.Description, ContentId = node.Parent.ContentId, Content = node.Parent.Content, ParentId = node.ParentId };
            }

            List<NodeWithOneLevelChildern> nodeDTOlist = new List<NodeWithOneLevelChildern>();
            if (node.Children != null)
            {
                foreach (var item in node.Children)
                {
                    NodeWithOneLevelChildern obj = new NodeWithOneLevelChildern();
                    obj.Id = item.Id;
                    obj.Description = item.Description;
                    obj.ContentId = item.ContentId;
                    obj.Content = item.Content;
                    obj.ParentId = item.ParentId;
                    nodeDTOlist.Add(obj);
                }
            }

            nodeWithOneLevelChildern.Children = nodeDTOlist;
            return nodeWithOneLevelChildern;
        }
    }
}
