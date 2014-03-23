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
    public class NodeCacheControllerTest
    {
        [TestMethod]
        public void AddNodeCache_ParentChildRelationship_ParentShouldIncreasedBy2Children()
        {
            // Arrange
            IUnitOfWork unitOfWork = new UnitOfWork(new RepositoryProvider(new RepositoryFactories()));
            var nodeId = 1;

            NodeDTO nodeDto;
            nodeDto = GetNodeFromDBorCache(unitOfWork, nodeId);
            var childNodeCountBefore = ChildNodeCount(nodeDto);
            AddNodeInDBandCache(unitOfWork, nodeDto);

            //Act
            unitOfWork.NodesCache.GetNodeDtoByIdWithChildren(nodeId, out nodeDto);
            var childNodeCountAfter = ChildNodeCount(nodeDto);

            //Assert
            Assert.AreEqual(childNodeCountBefore + 2, childNodeCountAfter);
        }

        [TestMethod]
        public void MoveNodeCache_ParentChildRelationship_ShouldChangeParent()
        {
            // Arrange
            IUnitOfWork unitOfWork = new UnitOfWork(new RepositoryProvider(new RepositoryFactories()));
            var nodeId = 1;
            NodeDTO nodeDto;
            nodeDto = GetNodeFromDBorCache(unitOfWork, nodeId);
            var childNodeCountBefore = ChildNodeCount(nodeDto);

            //Act
            MoveNode(unitOfWork, nodeDto);
            unitOfWork.NodesCache.GetNodeDtoByIdWithChildren(nodeId, out nodeDto);
            var getNodeAfterNewlyAddedChild = nodeDto.Children.Count();

            nodeDto = GetNodeFromDBorCache(unitOfWork, 4);
            var getNodeAfterMovingChildFromAnOtherParent = nodeDto.Children.Count();
            //Assert
            Assert.AreEqual(childNodeCountBefore + 1, getNodeAfterNewlyAddedChild);
            Assert.AreEqual(2, getNodeAfterMovingChildFromAnOtherParent);
        }

        [TestMethod]
        public void GetNodeCache_VerifyNodeWithShortcutContents_ShouldReturnOriginalContents()
        {
            //Arrange
            IUnitOfWork unitOfWork = new UnitOfWork(new RepositoryProvider(new RepositoryFactories()));
            var nodeId = 12;

            NodeDTO nodeDto;
            nodeDto = GetNodeFromDBorCache(unitOfWork, nodeId);
            if (nodeDto.Content.Type == ContentType.Shortcut)
            {
                GetOriginalNode(unitOfWork, nodeDto);
            }
            //Act
            var isNodeHaveOriginalContent = (nodeDto.Content.Type == ContentType.Original);
            
            //Assert
            Assert.IsTrue(isNodeHaveOriginalContent);
        }

        [TestMethod]
        public void DeleteNodeCache_ParentChildRelationship_ParentShouldSameChildren()
        {
            // Arrange
            IUnitOfWork unitOfWork = new UnitOfWork(new RepositoryProvider(new RepositoryFactories()));
            var nodeId = 1;

            NodeDTO nodeDto;
            nodeDto = GetNodeFromDBorCache(unitOfWork, nodeId);
            var childNodeCountBefore = ChildNodeCount(nodeDto);
            AddNodeInDBandCache(unitOfWork, nodeDto);
            
            //Act
            var childNodeCountAfter = ChildNodeCount(nodeDto);

            //Assert
            Assert.AreEqual(childNodeCountBefore + 2, childNodeCountAfter);

            //Act
            var testData = unitOfWork.Nodes.GetAll().Where(x => x.Id > 12);
            foreach (var node in testData)
            {
                var nodeDtoDelete = TransformFromNodeToNodeDto(node);
                unitOfWork.NodesCache.DeleteCacheFromItsParent(nodeDtoDelete, nodeDto);
                unitOfWork.Nodes.simpleDelete(node);
            }
            unitOfWork.Commit();
            childNodeCountAfter = ChildNodeCount(nodeDto);

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
                unitOfWork.Nodes.simpleDelete(node);
            }

            unitOfWork.Commit();
        }

        #region Private Method
        private NodeDTO GetNodeFromDBorCache(IUnitOfWork unitOfWork, int nodeId)
        {
            NodeDTO nodeDto;
            var checkNodeCache = unitOfWork.NodesCache.GetNodeDtoByIdWithChildren(nodeId, out nodeDto);
            if (!checkNodeCache)
            {
                var getNode = unitOfWork.Nodes.GetAll().Where(x => x.Id == nodeId).Include("Content").Include("Parent").Include("Children").Include("Children.Content").First();

                nodeDto = TransformFromNodeToNodeDto(getNode);
                unitOfWork.NodesCache.UpdateCacheForItsParent(nodeDto, nodeDto.Parent);
            }
            return nodeDto;
        }

        private void AddNodeInDBandCache(IUnitOfWork unitOfWork, NodeDTO nodeDto)
        {
            var node1 = new Node { ParentId = nodeDto.Id, Content = new Content { Type = ContentType.Original, PortalId = nodeDto.Content.PortalId } };
            unitOfWork.Nodes.Add(node1);
            var node2 = new Node { ParentId = nodeDto.Id, Content = new Content { Type = ContentType.Original, PortalId = nodeDto.Content.PortalId } };
            unitOfWork.Nodes.Add(node2);
            unitOfWork.Commit();

            unitOfWork.NodesCache.UpdateCacheForItsParent(TransformFromNodeToNodeDto(node1), nodeDto);
            unitOfWork.NodesCache.UpdateCacheForItsParent(TransformFromNodeToNodeDto(node2), nodeDto);
        }

        private void MoveNode(IUnitOfWork unitOfWork, NodeDTO nodeDto)
        {
            //NodeDTO movingNodeDto;
            //movingNodeDto = GetNodeFromDBorCache(unitOfWork, 9);
            var movingNode = unitOfWork.Nodes.GetById(9);
            movingNode.ParentId = nodeDto.Id; // Change ParentId from 4 to 1
            var movingNodeDto = TransformFromNodeToNodeDto(movingNode);
            unitOfWork.Nodes.Update(movingNode);
            unitOfWork.Commit();
            unitOfWork.NodesCache.UpdateCacheForItsParent(movingNodeDto, nodeDto);
        }

        private static int ChildNodeCount(NodeDTO getNode)
        {
            var childNodes = getNode.Children;
            var childNodeCount = 0;
            if (childNodes != null)
            {
                childNodeCount = getNode.Children.Count();
            }
            return childNodeCount;
        }

        private static void AddNodes(IUnitOfWork unitOfWork, NodeDTO getNode)
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

        private NodeDTO TransformFromNodeToNodeDto(Node node)
        {
            NodeDTO nodeDto = new NodeDTO();
            nodeDto.Id = node.Id;
            nodeDto.Description = node.Description;
            nodeDto.ContentId = node.ContentId;
            nodeDto.Content = node.Content;
            nodeDto.ParentId = node.ParentId;
            if (node.Parent != null)
            {
                nodeDto.Parent = new NodeDTO { Id = node.Parent.Id, Description = node.Parent.Description, ContentId = node.Parent.ContentId, Content = node.Parent.Content, ParentId = node.ParentId };
            }

            List<NodeDTO> nodeDTOlist = new List<NodeDTO>();
            if (node.Children != null)
            {
                foreach (var item in node.Children)
                {
                    NodeDTO obj = new NodeDTO();
                    obj.Id = item.Id;
                    obj.Description = item.Description;
                    obj.ContentId = item.ContentId;
                    obj.Content = item.Content;
                    obj.ParentId = item.ParentId;
                    if (item.Parent != null)
                    {
                        obj.Parent = new NodeDTO { Id = item.Parent.Id, Description = item.Parent.Description, ContentId = item.Parent.ContentId, Content = item.Parent.Content, ParentId = item.ParentId };
                    }

                    nodeDTOlist.Add(obj);
                }
            }

            nodeDto.Children = nodeDTOlist;
            return nodeDto;
        }

        private void GetOriginalNode(IUnitOfWork unitOfWork, NodeDTO nodeDto)
        {
            var shortcut = unitOfWork.Shortcuts.GetOriginalContentByLinkContentId(nodeDto.Content.Id);

            var originalNode = unitOfWork.Nodes.GetByContentId(shortcut.OriginalContentId);
            var originalNodeDto = GetNodeFromDBorCache(unitOfWork, originalNode.Id);
            nodeDto.Content = shortcut.OriginalContent;
            nodeDto.Children = originalNodeDto.Children;
        }
        #endregion
    }
}
