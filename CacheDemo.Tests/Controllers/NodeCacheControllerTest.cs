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

            NodeDTO nodeDto;
            nodeDto = GetNodeFromDBorCache(unitOfWork, nodeId);
            int childNodeCountBefore = ChildNodeCount(nodeDto);
            AddNodeInDBandCache(unitOfWork, nodeDto);

            //Act
            unitOfWork.NodesCache.GetNodeDtoByIdWithChildren(nodeId, out nodeDto);
            int childNodeCountAfter = ChildNodeCount(nodeDto);

            //Assert
            Assert.AreEqual(childNodeCountBefore + 2, childNodeCountAfter);
        }

        [TestMethod]
        public void MoveNodeCache_ParentChildRelationship_ShouldChangeParent()
        {
            // Arrange
            IUnitOfWork unitOfWork = new UnitOfWork(new RepositoryProvider(new RepositoryFactories()));
            int nodeId = 1;
            NodeDTO nodeDto;
            nodeDto = GetNodeFromDBorCache(unitOfWork, nodeId);
            int childNodeCountBefore = ChildNodeCount(nodeDto);

            int movingNodeId = 9;
            Node movingNode = unitOfWork.Nodes.GetById(movingNodeId);
            //Act
            MoveNode(unitOfWork, movingNode, nodeDto);
            unitOfWork.NodesCache.GetNodeDtoByIdWithChildren(nodeId, out nodeDto);
            int getNodeAfterNewlyAddedChild = nodeDto.Children.Count();

            int oldParentId = 4;
            nodeDto = GetNodeFromDBorCache(unitOfWork, oldParentId);
            int getNodeAfterMovingChildFromAnOtherParent = nodeDto.Children.Count();
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

            NodeDTO nodeDto;
            nodeDto = GetNodeFromDBorCache(unitOfWork, nodeId);
            if (nodeDto.Content.Type == ContentType.Shortcut)
            {
                GetOriginalNode(unitOfWork, nodeDto);
            }
            //Act
            bool isNodeHaveOriginalContent = (nodeDto.Content.Type == ContentType.Original);

            //Assert
            Assert.IsTrue(isNodeHaveOriginalContent);
        }

        [TestMethod]
        public void DeleteNodeCache_ParentChildRelationship_ParentShouldSameChildren()
        {
            // Arrange
            IUnitOfWork unitOfWork = new UnitOfWork(new RepositoryProvider(new RepositoryFactories()));
            int nodeId = 1;

            NodeDTO nodeDto;
            nodeDto = GetNodeFromDBorCache(unitOfWork, nodeId);
            int childNodeCountBefore = ChildNodeCount(nodeDto);
            AddNodeInDBandCache(unitOfWork, nodeDto);

            //Act
            int childNodeCountAfter = ChildNodeCount(nodeDto);

            //Assert
            Assert.AreEqual(childNodeCountBefore + 2, childNodeCountAfter);

            //Act
            IQueryable<Node> testData = unitOfWork.Nodes.GetAll().Where(x => x.Id > 12);
            foreach (Node node in testData)
            {
                NodeDTO nodeDtoDelete = TransformFromNodeToNodeDto(node);
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
            IQueryable<Node> testData = unitOfWork.Nodes.GetAll().Where(x => x.Id > 12);
            foreach (Node node in testData)
            {
                unitOfWork.Nodes.simpleDelete(node);
            }

            unitOfWork.Commit();
        }

        #region Private Method

        private NodeDTO GetNodeFromDBorCache(IUnitOfWork unitOfWork, int nodeId)
        {
            NodeDTO nodeDto;
            bool checkNodeCache = unitOfWork.NodesCache.GetNodeDtoByIdWithChildren(nodeId, out nodeDto);
            if (!checkNodeCache)
            {
                Node getNode =
                    unitOfWork.Nodes.GetAll()
                        .Where(x => x.Id == nodeId)
                        //.Include("Content")
                        //.Include("Parent")
                        //.Include("Children")
                        //.Include("Children.Content")
                        .First();

                nodeDto = TransformFromNodeToNodeDto(getNode);
                unitOfWork.NodesCache.UpdateCacheForItsParent(nodeDto, nodeDto.Parent);
            }
            return nodeDto;
        }

        private void AddNodeInDBandCache(IUnitOfWork unitOfWork, NodeDTO nodeDto)
        {
            var node1 = new Node
            {
                ParentId = nodeDto.Id,
                Content = new Content {Type = ContentType.Original, PortalId = nodeDto.Content.PortalId}
            };
            unitOfWork.Nodes.Add(node1);
            var node2 = new Node
            {
                ParentId = nodeDto.Id,
                Content = new Content {Type = ContentType.Original, PortalId = nodeDto.Content.PortalId}
            };
            unitOfWork.Nodes.Add(node2);
            unitOfWork.Commit();

            unitOfWork.NodesCache.UpdateCacheForItsParent(TransformFromNodeToNodeDto(node1), nodeDto);
            unitOfWork.NodesCache.UpdateCacheForItsParent(TransformFromNodeToNodeDto(node2), nodeDto);
        }

        private void MoveNode(IUnitOfWork unitOfWork,Node movingNode, NodeDTO parentNodeDto)
        {
            
            movingNode.ParentId = parentNodeDto.Id; // Change ParentId from 4 to 1
            NodeDTO movingNodeDto = TransformFromNodeToNodeDto(movingNode);
            unitOfWork.Nodes.Update(movingNode);
            unitOfWork.Commit();
            unitOfWork.NodesCache.UpdateCacheForItsParent(movingNodeDto, parentNodeDto);
        }

        private static int ChildNodeCount(NodeDTO getNode)
        {
            List<NodeDTO> childNodes = getNode.Children;
            int childNodeCount = 0;
            if (childNodes != null)
            {
                childNodeCount = childNodes.Count();
            }
            return childNodeCount;
        }

        private NodeDTO TransformFromNodeToNodeDto(Node node)
        {
            var nodeDto = new NodeDTO();
            nodeDto.Id = node.Id;
            nodeDto.Description = node.Description;
            nodeDto.ContentId = node.ContentId;
            nodeDto.Content = node.Content;
            nodeDto.ParentId = node.ParentId;
            if (node.Parent != null)
            {
                nodeDto.Parent = new NodeDTO
                {
                    Id = node.Parent.Id,
                    Description = node.Parent.Description,
                    ContentId = node.Parent.ContentId,
                    Content = node.Parent.Content,
                    ParentId = node.ParentId
                };
            }

            var nodeDTOlist = new List<NodeDTO>();
            if (node.Children != null)
            {
                foreach (Node item in node.Children)
                {
                    var obj = new NodeDTO();
                    obj.Id = item.Id;
                    obj.Description = item.Description;
                    obj.ContentId = item.ContentId;
                    obj.Content = item.Content;
                    obj.ParentId = item.ParentId;
                    if (item.Parent != null)
                    {
                        obj.Parent = new NodeDTO
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

        private void GetOriginalNode(IUnitOfWork unitOfWork, NodeDTO nodeDto)
        {
            Shortcut shortcut = unitOfWork.Shortcuts.GetOriginalContentByLinkContentId(nodeDto.Content.Id);

            Node originalNode = unitOfWork.Nodes.GetByContentId(shortcut.OriginalContentId);
            NodeDTO originalNodeDto = GetNodeFromDBorCache(unitOfWork, originalNode.Id);
            nodeDto.Content = shortcut.OriginalContent;
            nodeDto.Children = originalNodeDto.Children;
        }

        #endregion
    }
}