using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;


namespace DependencyViewer{
    public class DependencyGraphView : GraphView{
        Dictionary<string, DependencyNode> _nodes = new Dictionary<string, DependencyNode>();
        public DependencyGraphView(){
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            foreach (var node in GetNodes()){
                AddElement(node);
            }
        }

        List<DependencyNode> GetNodes(){
            List<DependencyNode> nodes = new List<DependencyNode>();
            var AllScrpits = AssetDatabase.FindAssets("t:prefab", new string[]{"Assets"});
            int i = 0;
            foreach (var guid in AllScrpits){
                string path = AssetDatabase.GUIDToAssetPath(guid);
                name = path.Split('/')[^1];
                var node = new DependencyNode(){
                    title = name,
                    GUID = guid
                };
                _nodes.Add(guid, node);
                node.SetPosition(new Rect(200 * i, 200, 100, 150));
                nodes.Add(node);
                i++;
            }

            foreach(var node in _nodes){
                foreach(var dependency in GetDependencies(AssetDatabase.GUIDToAssetPath(node.Key))){
                    if(_nodes.ContainsKey(AssetDatabase.AssetPathToGUID(dependency))){
                        var port = GeneratePort(node.Value, Direction.Output);
                        node.Value.outputContainer.Add(port);
                        var targetNode = _nodes[AssetDatabase.AssetPathToGUID(dependency)];
                        var targetPort = GeneratePort(targetNode, Direction.Input);
                        targetNode.inputContainer.Add(targetPort);
                        var edge = port.ConnectTo(targetPort);
                        AddElement(edge);
                        node.Value.RefreshExpandedState();
                        node.Value.RefreshPorts();
                        targetNode.RefreshExpandedState();
                        targetNode.RefreshPorts();
                    }
                }
            }

            return nodes;
        }
        
        private Port GeneratePort(DependencyNode node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single){
            return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
        }

        private string[] GetDependencies(string path, bool recursive = false){
            return AssetDatabase.GetDependencies(path, false);  // recursive false to remove self Dependency
        }

    }
}
