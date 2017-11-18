using Godot;

namespace DruggoRTS
{
    public static class NodeExt
    {
        /// <summary>
        /// Gets a child node and casts to T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T GetNodeOf<T>(this Node node, string path) where T : class
        {
            return node.GetNode(path) as T;
        }
    }
}
