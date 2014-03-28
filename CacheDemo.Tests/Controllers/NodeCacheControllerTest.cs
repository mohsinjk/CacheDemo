using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using CacheDemo.Data;
using CacheDemo.Data.Contracts;
using CacheDemo.Data.Helpers;
using CacheDemo.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            int nodeId = 1;

            NodeWithOneLevelChildern nodeWithOneLevelChildern;
            nodeWithOneLevelChildern = GetNodeFromDBorCache(unitOfWork, nodeId);
            int childNodeCountBefore = ChildNodeCount(nodeWithOneLevelChildern);
            AddNodeInDBandCache(unitOfWork, nodeWithOneLevelChildern);

            //Act
            unitOfWork.NodesCache.GetNodeDtoByIdWithChildren(nodeId, out nodeWithOneLevelChildern);
            int childNodeCountAfter = ChildNodeCount(nodeWithOneLevelChildern);

            //Assert
            Assert.AreEqual(childNodeCountBefore + 2, childNodeCountAfter);
        }

        [TestMethod]
        public void MoveNodeCache_ParentChildRelationship_ShouldChangeParent()
        {
            // Arrange
            IUnitOfWork unitOfWork = new UnitOfWork(new RepositoryProvider(new RepositoryFactories()));
            int nodeId = 1;
            NodeWithOneLevelChildern nodeWithOneLevelChildern;
            nodeWithOneLevelChildern = GetNodeFromDBorCache(unitOfWork, nodeId);
            int childNodeCountBefore = ChildNodeCount(nodeWithOneLevelChildern);

            int movingNodeId = 9;
            Node movingNode = unitOfWork.Nodes.GetById(movingNodeId);
            //Act
            MoveNode(unitOfWork, movingNode, nodeWithOneLevelChildern);
            unitOfWork.NodesCache.GetNodeDtoByIdWithChildren(nodeId, out nodeWithOneLevelChildern);
            int getNodeAfterNewlyAddedChild = nodeWithOneLevelChildern.Children.Count();

            int oldParentId = 4;
            nodeWithOneLevelChildern = GetNodeFromDBorCache(unitOfWork, oldParentId);
            int getNodeAfterMovingChildFromAnOtherParent = nodeWithOneLevelChildern.Children.Count();
            //Assert
            Assert.AreEqual(childNodeCountBefore + 1, getNodeAfterNewlyAddedChild);
            Assert.AreEqual(2, getNodeAfterMovingChildFromAnOtherParent);
        }

        [TestMethod]
        public void GetNodeCache_VerifyNodeWithShortcutContents_ShouldReturnOriginalContents()
        {
            //Arrange
            IUnitOfWork unitOfWork = new UnitOfWork(new RepositoryProvider(new RepositoryFactories()));
            int nodeId = 12;

            NodeWithOneLevelChildern nodeWithOneLevelChildern;
            nodeWithOneLevelChildern = GetNodeFromDBorCache(unitOfWork, nodeId);
            if (nodeWithOneLevelChildern.Content.Type == ContentType.Shortcut)
            {
                GetOriginalNode(unitOfWork, nodeWithOneLevelChildern);
            }
            //Act
            bool isNodeHaveOriginalContent = (nodeWithOneLevelChildern.Content.Type == ContentType.Original);

            //Assert
            Assert.IsTrue(isNodeHaveOriginalContent);
        }

        [TestMethod]
        public void DeleteNodeCache_ParentChildRelationship_ParentShouldSameChildren()
        {
            // Arrange
            IUnitOfWork unitOfWork = new UnitOfWork(new RepositoryProvider(new RepositoryFactories()));
            int nodeId = 1;

            NodeWithOneLevelChildern nodeWithOneLevelChildern;
            nodeWithOneLevelChildern = GetNodeFromDBorCache(unitOfWork, nodeId);
            int childNodeCountBefore = ChildNodeCount(nodeWithOneLevelChildern);
            AddNodeInDBandCache(unitOfWork, nodeWithOneLevelChildern);

            //Act
            int childNodeCountAfter = ChildNodeCount(nodeWithOneLevelChildern);

            //Assert
            Assert.AreEqual(childNodeCountBefore + 2, childNodeCountAfter);

            //Act
            IQueryable<Node> testData = unitOfWork.Nodes.GetAll().Where(x => x.Id > 12);
            foreach (Node node in testData)
            {
                NodeWithOneLevelChildern nodeWithOneLevelChildernDelete = TransformFromNodeToNodeDto(node);
                unitOfWork.NodesCache.DeleteCacheFromItsParent(nodeWithOneLevelChildernDelete, nodeWithOneLevelChildern);
                unitOfWork.Nodes.simpleDelete(node);
            }
            unitOfWork.Commit();
            childNodeCountAfter = ChildNodeCount(nodeWithOneLevelChildern);

            //Assert
            Assert.AreEqual(childNodeCountBefore, childNodeCountAfter);
        }

        [TestCleanup]
        public void DeleteTestData()
        {
            IUnitOfWork unitOfWork = new UnitOfWork(new RepositoryProvider(new RepositoryFactories()));
            IQueryable<Node> testData = unitOfWork.Nodes.GetAll().Where(x => x.Id > 12);
            foreach (Node node in testData)
            {
                unitOfWork.Nodes.simpleDelete(node);
            }

            unitOfWork.Commit();
        }

        #region Private Method

        private NodeWithOneLevelChildern GetNodeFromDBorCache(IUnitOfWork unitOfWork, int nodeId)
        {
            NodeWithOneLevelChildern nodeWithOneLevelChildern;
            bool checkNodeCache = unitOfWork.NodesCache.GetNodeDtoByIdWithChildren(nodeId, out nodeWithOneLevelChildern);
            if (!checkNodeCache)
            {
                Node getNode =
                    unitOfWork.Nodes.GetAll()
                        .Where(x => x.Id == nodeId)
                        .Include("Content")
                        .Include("Parent")
                        .Include("Children")
                        .Include("Children.Content")
                        .First();

                nodeWithOneLevelChildern = TransformFromNodeToNodeDto(getNode);
                unitOfWork.NodesCache.UpdateCacheForItsParent(nodeWithOneLevelChildern, nodeWithOneLevelChildern.Parent);
            }
            return nodeWithOneLevelChildern;
        }

        private void AddNodeInDBandCache(IUnitOfWork unitOfWork, NodeWithOneLevelChildern nodeWithOneLevelChildern)
        {
            var node1 = new Node
            {
                ParentId = nodeWithOneLevelChildern.Id,
                Content = new Content {Type = ContentType.Original, PortalId = nodeWithOneLevelChildern.Content.PortalId}
            };
            unitOfWork.Nodes.Add(node1);
            var node2 = new Node
            {
                ParentId = nodeWithOneLevelChildern.Id,
                Content = new Content {Type = ContentType.Original, PortalId = nodeWithOneLevelChildern.Content.PortalId}
            };
            unitOfWork.Nodes.Add(node2);
            unitOfWork.Commit();

            unitOfWork.NodesCache.UpdateCacheForItsParent(TransformFromNodeToNodeDto(node1), nodeWithOneLevelChildern);
            unitOfWork.NodesCache.UpdateCacheForItsParent(TransformFromNodeToNodeDto(node2), nodeWithOneLevelChildern);
        }

        private void MoveNode(IUnitOfWork unitOfWork,Node movingNode, NodeWithOneLevelChildern parentNodeWithOneLevelChildern)
        {
            
            movingNode.ParentId = parentNodeWithOneLevelChildern.Id; // Change ParentId from 4 to 1
            NodeWithOneLevelChildern movingNodeWithOneLevelChildern = TransformFromNodeToNodeDto(movingNode);
            unitOfWork.Nodes.Update(movingNode);
            unitOfWork.Commit();
            unitOfWork.NodesCache.UpdateCacheForItsParent(movingNodeWithOneLevelChildern, parentNodeWithOneLevelChildern);
        }

        private static int ChildNodeCount(NodeWithOneLevelChildern getNode)
        {
            List<NodeWithOneLevelChildern> childNodes = getNode.Children;
            int childNodeCount = 0;
            if (childNodes != null)
            {
                childNodeCount = childNodes.Count();
            }
            return childNodeCount;
        }

        private NodeWithOneLevelChildern TransformFromNodeToNodeDto(Node node)
        {
            var nodeDto = new NodeWithOneLevelChildern();
            nodeDto.Id = node.Id;
            nodeDto.Description = node.Description;
            nodeDto.ContentId = node.ContentId;
            nodeDto.Content = node.Content;
            nodeDto.ParentId = node.ParentId;
            if (node.Parent != null)
            {
                nodeDto.Parent = new NodeWithOneLevelChildern
                {
                    Id = node.Parent.Id,
                    Description = node.Parent.Description,
                    ContentId = node.Parent.ContentId,
                    Content = node.Parent.Content,
                    ParentId = node.ParentId
                };
            }

            var nodeDTOlist = new List<NodeWithOneLevelChildern>();
            if (node.Children != null)
            {
                foreach (Node item in node.Children)
                {
                    var obj = new NodeWithOneLevelChildern();
                    obj.Id = item.Id;
                    obj.Description = item.Description;
                    obj.ContentId = item.ContentId;
                    obj.Content = item.Content;
                    obj.ParentId = item.ParentId;
                    if (item.Parent != null)
                    {
                        obj.Parent = new NodeWithOneLevelChildern
                        {
                            Id = item.Parent.Id,
                            Description = item.Parent.Description,
                            ContentId = item.Parent.ContentId,
                            Content = item.Parent.Content,
                            ParentId = item.ParentId
                        };
                    }

                    nodeDTOlist.Add(obj);
                }
            }

            nodeDto.Children = nodeDTOlist;
            return nodeDto;
        }

        private void GetOriginalNode(IUnitOfWork unitOfWork, NodeWithOneLevelChildern nodeWithOneLevelChildern)
        {
            Shortcut shortcut = unitOfWork.Shortcuts.GetOriginalContentByLinkContentId(nodeWithOneLevelChildern.Content.Id);

            Node originalNode = unitOfWork.Nodes.GetByContentId(shortcut.OriginalContentId);
            NodeWithOneLevelChildern originalNodeWithOneLevelChildern = GetNodeFromDBorCache(unitOfWork, originalNode.Id);
            nodeWithOneLevelChildern.Content = shortcut.OriginalContent;
            nodeWithOneLevelChildern.Children = originalNodeWithOneLevelChildern.Children;
        }

        #endregion
    }
}