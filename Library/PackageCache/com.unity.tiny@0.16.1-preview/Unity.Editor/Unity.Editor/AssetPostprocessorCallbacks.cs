using System;
using System.Collections.Generic;
using UnityEditor;
using Object = UnityEngine.Object;
using TypeToTypedAssetCallbackMap = System.Collections.Generic.Dictionary<System.Type, Unity.Editor.Persistence.AssetPostprocessorCallbacks.ITypedAssetCallbacks>;

namespace Unity.Editor.Persistence
{
    internal struct PostprocessEventArgs
    {
        public string AssetGuid;
        public string AssetPath;
    }

    internal delegate void TypedAssetHandler<TAsset>(TAsset asset, PostprocessEventArgs args) where TAsset : Object;
    internal delegate void TypedAssetHandler(Object asset, PostprocessEventArgs args);

    internal class AssetPostprocessorCallbacks : AssetPostprocessor
    {
        internal interface ITypedAssetCallbacks
        {
            event TypedAssetHandler AssetHandler;
            void Dispatch(Object asset, PostprocessEventArgs args);
        }

        private class TypedAssetCallbacks<TAsset> : ITypedAssetCallbacks
            where TAsset : Object
        {
            public event TypedAssetHandler AssetHandler = delegate { };
            public event TypedAssetHandler<TAsset> TypedAssetHandler = delegate { };

            public void Dispatch(Object asset, PostprocessEventArgs args)
            {
                UnityEngine.Assertions.Assert.AreEqual(asset.GetType(), typeof(TAsset));

                AssetHandler(asset, args);
                TypedAssetHandler((TAsset)asset, args);
            }
        }

        private static readonly TypeToTypedAssetCallbackMap s_AssetImportedCallbacks = new TypeToTypedAssetCallbackMap();
        private static readonly TypeToTypedAssetCallbackMap s_AssetMovedCallbacks = new TypeToTypedAssetCallbackMap();

        private static event Action PostProcessStarted = delegate { };
        private static event Action PostProcessEnded = delegate { };
        private static event Action<PostprocessEventArgs> AssetImported = delegate { };
        private static event Action<PostprocessEventArgs> AssetDeleted = delegate { };
        private static event Action<PostprocessEventArgs> AssetMoved = delegate { };

        /// <summary>
        /// Registers to receive a callback when the asset postprocessor begins its run.
        /// </summary>
        public static void RegisterToPostProcessStarted(Action handler) => PostProcessStarted += handler;

        /// <summary>
        /// Unregisters a callback for when the asset postprocessor begins its run.
        /// </summary>
        public static void UnregisterFromPostProcessStarted(Action handler) => PostProcessStarted -= handler;

        /// <summary>
        /// Registers to receive a callback when the asset postprocessor ends its run.
        /// </summary>
        public static void RegisterToPostProcessEnded(Action handler) => PostProcessEnded += handler;

        /// <summary>
        /// Unregisters a callback for when the asset postprocessor ends its run.
        /// </summary>
        public static void UnregisterFromPostProcessEnded(Action handler) => PostProcessEnded -= handler;

        /// <summary>
        /// Registers to receive a callback when a specific asset type is imported.
        /// </summary>
        public static void RegisterAssetImportedHandlerForType(Type type, TypedAssetHandler handler)
            => GetOrCreateTypedAssetCallbacks(s_AssetImportedCallbacks, type).AssetHandler += handler;
        public static void RegisterAssetImportedHandlerForType<TAsset>(TypedAssetHandler<TAsset> handler)
            where TAsset : Object
            => GetOrCreateTypedAssetCallbacks<TAsset>(s_AssetImportedCallbacks).TypedAssetHandler += handler;

        /// <summary>
        /// Unregisters a callback for when a specific asset type is imported.
        /// </summary>
        public static void UnregisterAssetImportedHandlerForType(Type type, TypedAssetHandler handler)
            => GetOrCreateTypedAssetCallbacks(s_AssetImportedCallbacks, type).AssetHandler -= handler;
        public static void UnregisterAssetImportedHandlerForType<TAsset>(TypedAssetHandler<TAsset> handler)
            where TAsset : Object
            => GetOrCreateTypedAssetCallbacks<TAsset>(s_AssetImportedCallbacks).TypedAssetHandler -= handler;

        /// <summary>
        /// Registers to receive a callback when ANY asset is imported.
        /// </summary>
        public static void RegisterAssetImportedHandler(Action<PostprocessEventArgs> handler) => AssetImported += handler;

        /// <summary>
        /// Unregisters a callback for when ANY asset is imported.
        /// </summary>
        public static void UnregisterAssetImportedHandler(Action<PostprocessEventArgs> handler) => AssetImported -= handler;

        /// <summary>
        /// Registers to receive a callback when ANY asset is deleted.
        /// </summary>
        public static void RegisterAssetDeletedHandler(Action<PostprocessEventArgs> handler) => AssetDeleted += handler;

        /// <summary>
        /// Unregisters a callback for when ANY asset is deleted.
        /// </summary>
        public static void UnregisterAssetDeletedHandler(Action<PostprocessEventArgs> handler) => AssetDeleted -= handler;

        /// <summary>
        /// Registers to receive a callback when ANY asset is moved.
        /// PostprocessEventArgs provided to the handler contain the old asset path.
        /// Use `AssetDatabase.GUIDToAssetPath(args.AssetGuid)` to get the new asset path.
        /// </summary>
        public static void RegisterAssetMovedHandler(Action<PostprocessEventArgs> handler) => AssetMoved += handler;

        /// <summary>
        /// Unregisters a callback for when ANY asset is moved.
        /// </summary>
        public static void UnregisterAssetMovedHandler(Action<PostprocessEventArgs> handler) => AssetMoved -= handler;

        /// <summary>
        /// Registers to receive a callback when a specific asset type is moved.
        /// PostprocessEventArgs provided to the handler contain the old asset path.
        /// Use `AssetDatabase.GUIDToAssetPath(args.AssetGuid)` to get the new asset path.
        /// </summary>
        public static void RegisterAssetMovedHandlerForType(Type type, TypedAssetHandler handler)
            => GetOrCreateTypedAssetCallbacks(s_AssetMovedCallbacks, type).AssetHandler += handler;
        public static void RegisterAssetMovedHandlerForType<TAsset>(TypedAssetHandler<TAsset> handler)
            where TAsset : Object
            => GetOrCreateTypedAssetCallbacks<TAsset>(s_AssetMovedCallbacks).TypedAssetHandler += handler;

        /// <summary>
        /// Unregisters a callback when a specific asset type is moved.
        /// </summary>
        public static void UnregisterAssetMovedHandlerForType(Type type, TypedAssetHandler handler)
            => GetOrCreateTypedAssetCallbacks(s_AssetMovedCallbacks, type).AssetHandler -= handler;
        public static void UnregisterAssetMovedHandlerForType<TAsset>(TypedAssetHandler<TAsset> handler)
            where TAsset : Object
            => GetOrCreateTypedAssetCallbacks<TAsset>(s_AssetMovedCallbacks).TypedAssetHandler -= handler;

        private static ITypedAssetCallbacks GetOrCreateTypedAssetCallbacks(TypeToTypedAssetCallbackMap callbackMap, Type type)
        {
            if (callbackMap.TryGetValue(type, out var untyped))
            {
                return untyped;
            }

            var callback = (ITypedAssetCallbacks) Activator.CreateInstance(typeof(TypedAssetCallbacks<>).MakeGenericType(type));
            callbackMap.Add(type, callback);
            return callback;
        }

        private static TypedAssetCallbacks<TAsset> GetOrCreateTypedAssetCallbacks<TAsset>(TypeToTypedAssetCallbackMap callbackMap) where TAsset : Object
        {
            if (callbackMap.TryGetValue(typeof(TAsset), out var untyped))
            {
                return untyped as TypedAssetCallbacks<TAsset>;
            }

            var callback = new TypedAssetCallbacks<TAsset>();
            callbackMap.Add(typeof(TAsset), callback);
            return callback;
        }

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (Project.Projects.Count == 0)
            {
                return;
            }

            PostProcessStarted();

            foreach (var assetPath in importedAssets)
            {
                var assetGuid = AssetDatabase.AssetPathToGUID(assetPath);

                AssetImported(new PostprocessEventArgs
{
                    AssetPath = assetPath,
                    AssetGuid = assetGuid
                });

                var type = AssetDatabase.GetMainAssetTypeAtPath(assetPath);

                if (null == type)
                {
                    continue;
                }

                if (s_AssetImportedCallbacks.TryGetValue(type, out var callback))
                {
                    callback.Dispatch(AssetDatabase.LoadMainAssetAtPath(assetPath), new PostprocessEventArgs
                    {
                        AssetPath = assetPath,
                        AssetGuid = assetGuid
                    });
                }
            }

            foreach (var assetPath in deletedAssets)
            {
                var assetGuid = AssetDatabase.AssetPathToGUID(assetPath);

                AssetDeleted(new PostprocessEventArgs
                {
                    AssetPath = assetPath,
                    AssetGuid = assetGuid
                });
            }

            Assertions.Assert.IsTrue(movedAssets.Length == movedFromAssetPaths.Length);
            for (int i = 0; i < movedFromAssetPaths.Length; ++i)
            {
                var oldAssetPath = movedFromAssetPaths[i];
                var newAssetPath = movedAssets[i];
                var assetGuid = AssetDatabase.AssetPathToGUID(newAssetPath);

                AssetImported(new PostprocessEventArgs
                {
                    AssetPath = oldAssetPath,
                    AssetGuid = assetGuid
                });

                var type = AssetDatabase.GetMainAssetTypeAtPath(newAssetPath);
                if (null == type)
                {
                    continue;
                }

                if (s_AssetMovedCallbacks.TryGetValue(type, out var callback))
                {
                    callback.Dispatch(AssetDatabase.LoadMainAssetAtPath(newAssetPath), new PostprocessEventArgs
                    {
                        AssetPath = oldAssetPath,
                        AssetGuid = assetGuid
                    });
                }
            }

            PostProcessEnded();
        }
    }
}
