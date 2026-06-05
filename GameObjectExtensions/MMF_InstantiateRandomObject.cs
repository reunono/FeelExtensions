using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;
using System.Collections.Generic;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// This feedback will instantiate a random object from a list (usually a VFX), optionally creating an object pool for each of them for performance
    /// </summary>
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback allows you to instantiate a random object specified in its list, at the feedback's position (plus an optional offset). You can also optionally (and automatically) create object pools at initialization. In that case you'll need to specify a pool size (this size will be applied to EACH prefab in the list).")]
    [MovedFrom(false, null, "MoreMountains.Feedbacks")]
    [System.Serializable]
    [FeedbackPath("GameObject/Instantiate Random Object")]
    public class MMF_InstantiateRandomObject : MMF_Feedback
    {
       public static bool FeedbackTypeAuthorized = true;
       
       public enum PositionModes { FeedbackPosition, Transform, WorldPosition, Script }

       #if UNITY_EDITOR
       public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
       public override bool EvaluateRequiresSetup() { return (GameObjectsToInstantiate == null || GameObjectsToInstantiate.Count == 0); }
       public override string RequiredTargetText { get { return GameObjectsToInstantiate != null && GameObjectsToInstantiate.Count > 0 ? GameObjectsToInstantiate.Count + " Objects" : "";  } }
       public override string RequiresSetupText { get { return "This feedback requires that at least one GameObject is added to the GameObjectsToInstantiate list."; } }
       #endif

       [MMFInspectorGroup("Instantiate Random Object", true, 37, true)]
       /// the list of objects to randomly pick from and instantiate
       [Tooltip("the list of objects to randomly pick from and instantiate")]
       public List<GameObject> GameObjectsToInstantiate;

       [MMFInspectorGroup("Position", true, 39)]
       public PositionModes PositionMode = PositionModes.FeedbackPosition;
       public bool AlsoApplyRotation = false;
       public bool AlsoApplyScale = false;
       
       [MMFEnumCondition("PositionMode", (int)PositionModes.Transform)]
       public Transform TargetTransform;
       
       [MMFEnumCondition("PositionMode", (int)PositionModes.WorldPosition)]
       public Vector3 TargetPosition;
       
       public Vector3 PositionOffset;

       public bool RandomizePosition = false;
       
       [MMFCondition("RandomizePosition", true)]
       public Vector3 RandomizedPositionMin = Vector3.zero; 
       
       [MMFCondition("RandomizePosition", true)]
       public Vector3 RandomizedPositionMax = Vector3.one;

       [MMFInspectorGroup("Parent", true, 47)]
       public Transform ParentTransform;

       [MMFInspectorGroup("Object Pools", true, 40)]
       public bool CreateObjectPool;
       
       [Tooltip("The initial and planned size of the object pool for EACH prefab in the list.")]
       [MMFCondition("CreateObjectPool", true)]
       public int ObjectPoolSize = 5;
       
       [MMFCondition("CreateObjectPool", true)] 
       public bool MutualizePools = false;
       
       [MMFCondition("CreateObjectPool", true)] 
       public Transform PoolParentTransform;

       public GameObject InstantiatedGameObject { get { return _newGameObject; } }

       protected List<MMMiniObjectPooler> _objectPoolers; 
       protected GameObject _newGameObject;
       protected bool _poolCreatedOrFound = false;
       protected Vector3 _randomizedPosition = Vector3.zero;

       protected override void CustomInitialization(MMF_Player owner)
       {
          base.CustomInitialization(owner);

          if (Active && CreateObjectPool && !_poolCreatedOrFound)
          {
             if (_objectPoolers != null)
             {
                for (int i = 0; i < _objectPoolers.Count; i++)
                {
                   _objectPoolers[i].DestroyObjectPool();
                   owner.ProxyDestroy(_objectPoolers[i].gameObject);
                }
             }

             _objectPoolers = new List<MMMiniObjectPooler>();

             GameObject poolContainerGo = new GameObject();
             poolContainerGo.name = Owner.name + "_RandomObjectPoolers";
             
             if (PoolParentTransform != null)
             {
                poolContainerGo.transform.SetParent(PoolParentTransform);
             }

             // Für jedes Prefab in der Liste erstellen wir einen eigenen Pool
             for (int i = 0; i < GameObjectsToInstantiate.Count; i++)
             {
                 GameObject prefab = GameObjectsToInstantiate[i];
                 if (prefab == null) continue;

                 GameObject objectPoolGo = new GameObject();
                 objectPoolGo.name = "Pool_" + prefab.name;
                 objectPoolGo.transform.SetParent(poolContainerGo.transform);
                 
                 MMMiniObjectPooler pooler = objectPoolGo.AddComponent<MMMiniObjectPooler>();
                 pooler.GameObjectToPool = prefab;
                 pooler.PoolSize = ObjectPoolSize;
                 pooler.MutualizeWaitingPools = MutualizePools;
                 pooler.FillObjectPool();
                 
                 _objectPoolers.Add(pooler);
             }

             if ((Owner != null) && (poolContainerGo.transform.parent == null))
             {
                SceneManager.MoveGameObjectToScene(poolContainerGo, Owner.gameObject.scene);    
             }
             _poolCreatedOrFound = true;
          }
       }

       protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
       {
          if (!Active || !FeedbackTypeAuthorized || GameObjectsToInstantiate == null || GameObjectsToInstantiate.Count == 0)
          {
             return;
          }

          // Wir suchen uns ein zufälliges Element aus der Liste aus
          int randomIndex = UnityEngine.Random.Range(0, GameObjectsToInstantiate.Count);
          GameObject selectedPrefab = GameObjectsToInstantiate[randomIndex];

          if (selectedPrefab == null) return;
            
          if (_objectPoolers != null && _objectPoolers.Count > randomIndex)
          {
             // Wir greifen auf den exakt passenden Pooler für das ausgewählte Prefab zu
             _newGameObject = _objectPoolers[randomIndex].GetPooledGameObject();
             if (_newGameObject != null)
             {
                PositionObject(position);
                _newGameObject.SetActive(true);
             }
          }
          else
          {
             _newGameObject = GameObject.Instantiate(selectedPrefab) as GameObject;
             if (_newGameObject != null)
             {
                SceneManager.MoveGameObjectToScene(_newGameObject, Owner.gameObject.scene);
                PositionObject(position);    
             }
          }
       }

       protected virtual void PositionObject(Vector3 position)
       {
          _newGameObject.transform.position = GetPosition(position);
          if (AlsoApplyRotation)
          {
             _newGameObject.transform.rotation = GetRotation();    
          }
          if (AlsoApplyScale)
          {
             _newGameObject.transform.localScale = GetScale();    
          }
          if (ParentTransform != null)
          {
             _newGameObject.transform.SetParent(ParentTransform);
          }
       }

       protected virtual Vector3 GetPosition(Vector3 position)
       {
          if (RandomizePosition)
          {
             _randomizedPosition.x = UnityEngine.Random.Range(RandomizedPositionMin.x, RandomizedPositionMax.x);
             _randomizedPosition.y = UnityEngine.Random.Range(RandomizedPositionMin.y, RandomizedPositionMax.y);
             _randomizedPosition.z = UnityEngine.Random.Range(RandomizedPositionMin.z, RandomizedPositionMax.z);
          }
            
          switch (PositionMode)
          {
             case PositionModes.FeedbackPosition:
                return Owner.transform.position + PositionOffset + _randomizedPosition;
             case PositionModes.Transform:
                return TargetTransform.position + PositionOffset + _randomizedPosition;
             case PositionModes.WorldPosition:
                return TargetPosition + PositionOffset + _randomizedPosition;
             case PositionModes.Script:
                return position + PositionOffset + _randomizedPosition;
             default:
                return position + PositionOffset + _randomizedPosition;
          }
       }

       protected virtual Quaternion GetRotation()
       {
          switch (PositionMode)
          {
             case PositionModes.FeedbackPosition:
                return Owner.transform.rotation;
             case PositionModes.Transform:
                return TargetTransform.rotation;
             case PositionModes.WorldPosition:
                return Quaternion.identity;
             case PositionModes.Script:
                return Owner.transform.rotation;
             default:
                return Owner.transform.rotation;
          }
       }

       protected virtual Vector3 GetScale()
       {
          switch (PositionMode)
          {
             case PositionModes.FeedbackPosition:
                return Owner.transform.localScale;
             case PositionModes.Transform:
                return TargetTransform.localScale;
             case PositionModes.WorldPosition:
                return Owner.transform.localScale;
             case PositionModes.Script:
                return Owner.transform.localScale;
             default:
                return Owner.transform.localScale;
          }
       }
    }
}